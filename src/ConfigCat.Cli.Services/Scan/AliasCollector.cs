using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ConfigCat.Cli.Models.Api;
using System.Threading;
using Trybot;
using Trybot.Timeout.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using ConfigCat.Cli.Models.Scan;
using System.Collections.Concurrent;

namespace ConfigCat.Cli.Services.Scan
{
    public interface IAliasCollector
    {
        Task<AliasScanResult> CollectAsync(IEnumerable<FlagModel> flags,
            FileInfo fileToScan,
            CancellationToken token);
    }

    public class AliasCollector : IAliasCollector
    {
        private readonly IBotPolicy<AliasScanResult> botPolicy;
        private readonly IOutput output;

        public AliasCollector(IBotPolicy<AliasScanResult> botPolicy,
            IOutput output)
        {
            this.botPolicy = botPolicy;
            this.output = output;

            this.botPolicy.Configure(p => p.Timeout(t => t.After(TimeSpan.FromSeconds(10))));
        }

        public async Task<AliasScanResult> CollectAsync(IEnumerable<FlagModel> flags, FileInfo fileToScan, CancellationToken token)
        {
            try
            {
                return await this.botPolicy.ExecuteAsync(async (ctx, cancellation) =>
                {
                    await using var stream = fileToScan.OpenRead();
                    if (await stream.IsBinaryAsync(cancellation))
                    {
                        this.output.Verbose($"{fileToScan.FullName} is binary, skipping.", ConsoleColor.Yellow);
                        return null;
                    }

                    this.output.Verbose($"{fileToScan.FullName} searching aliases...");

                    var flagKeys = flags.Select(f => f.Key).ToArray();
                    var keys = string.Join('|', flagKeys);

                    var result = new AliasScanResult { ScannedFile = fileToScan };

                    Parallel.ForEach(File.ReadLines(fileToScan.FullName), line =>
                    {
                        if (line.Length > Constants.MaxCharCountPerLine || !flagKeys.Any(line.Contains))
                            return;

                        var match = Regex.Match(line, @"[`'""]?([a-zA-Z_$0-9]*)[[`'\""]?\s*(?>\:?\s*(?>[sS]tring)?\s*=?>?\s*(?>new|await)?)\s*\S*[@$]?[`'""](" + keys + ")[`'\"]",
                            RegexOptions.Compiled);

                        while (match.Success && !cancellation.IsCancellationRequested)
                        {
                            var key = match.Groups[2].Value;
                            var found = match.Groups[1].Value;
                            var flag = flags.FirstOrDefault(f => f.Key == key);

                            if (flag != null)
                                result.FoundFlags.Add(flag);

                            if (flag != null && !found.IsEmpty() && Similarity(flag.Key, found) > 0.3)
                                result.FlagAliases.AddOrUpdate(flag, new ConcurrentBag<string> { found }, (k, v) => { v.Add(found); return v; });

                            match = match.NextMatch();
                        }
                    });

                    this.output.Verbose($"{fileToScan.FullName} search completed.", ConsoleColor.Green);
                    return result;
                }, token);
            }
            catch (OperationTimeoutException)
            {
                this.output.Verbose($"{fileToScan.FullName} search timed out.", ConsoleColor.Red);
                return null;
            }
        }

        private static double Similarity(string a, string b)
        {
            a = a.ToLowerInvariant().RemoveDashes();
            b = b.ToLowerInvariant().RemoveDashes();
            var distance = DamerauLevenshteinDistance(a, b);
            return 1.0 - (distance / (double)Math.Max(a.Length, b.Length));
        }

        private static int DamerauLevenshteinDistance(string source, string target, int threshold = int.MaxValue)
        {
            var length1 = source.Length;
            var length2 = target.Length;

            if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

            if (length1 > length2)
            {
                (target, source) = (source, target);
                (length1, length2) = (length2, length1);
            }

            var maxi = length1;
            var maxj = length2;

            var dCurrent = new int[maxi + 1];
            var dMinus1 = new int[maxi + 1];
            var dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (var i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            var jm1 = 0;

            for (var j = 1; j <= maxj; j++)
            {
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;
                var minDistance = int.MaxValue;
                dCurrent[0] = j;
                var im1 = 0;
                var im2 = -1;
                for (var i = 1; i <= maxi; i++)
                {
                    var cost = source[im1] == target[jm1] ? 0 : 1;

                    var del = dCurrent[im1] + 1;
                    var ins = dMinus1[i] + 1;
                    var sub = dMinus1[im1] + cost;
                    var min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                        min = Math.Min(min, dMinus2[im2] + cost);

                    dCurrent[i] = min;
                    if (min < minDistance) { minDistance = min; }
                    im1++;
                    im2++;
                }
                jm1++;
                if (minDistance > threshold) { return int.MaxValue; }
            }

            var result = dCurrent[maxi];
            return (result > threshold) ? int.MaxValue : result;
        }
    }
}