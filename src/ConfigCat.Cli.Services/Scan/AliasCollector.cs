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
        private readonly static string[] Prefixes = new[] { ".", "->" };
        private readonly IBotPolicy<AliasScanResult> botPolicy;
        private readonly IOutput output;

        public AliasCollector(IBotPolicy<AliasScanResult> botPolicy,
            IOutput output)
        {
            this.botPolicy = botPolicy;
            this.output = output;
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

                    this.output.Verbose($"{fileToScan.FullName} scanning...");

                    using var reader = new StreamReader(stream);
                    var text = await reader.ReadToEndAsync();

                    var keys = string.Join('|', flags.Select(f => f.Key));

                    var match = Regex.Match(text, @"([a-zA-Z_$0-9]*)\s*(?>\:?\s*(?>[sS]tring)?\s*=?){1}\s*[@$]?[`'""](" + keys + ")[`'\"]",
                                       RegexOptions.Compiled);

                    var result = new AliasScanResult { ScannedFile = fileToScan };
                    while (match.Success)
                    {
                        var flag = flags.FirstOrDefault(f => f.Key == match.Groups[2].Value);
                        if(!result.FlagAliases.ContainsKey(flag))
                            result.FlagAliases[flag] = new List<string>();

                        if (flag != null && !match.Groups[1].Value.IsEmpty() && Similarity(flag.Key, match.Groups[1].Value) > 0.3)
                            result.FlagAliases[flag].Add(match.Groups[1].Value);

                        match = match.NextMatch();
                    }

                    //foreach (var flag in flags)
                    //{
                    //    var sampleVariations = string.Join('|', GetSampleVariations(flag));
                    //    var samplesMatch = Regex.Match(text, @"(" + sampleVariations.Replace(".", "\\.") + ")",
                    //                       RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    //    while (samplesMatch.Success)
                    //    {
                    //        flag.Aliases ??= new List<string>();
                    //        var alias = samplesMatch.Groups[1].Value;
                    //        alias = alias?.Remove(Prefixes);
                    //        if (!alias.IsEmpty())
                    //            flag.Aliases.Add(alias);

                    //        samplesMatch = samplesMatch.NextMatch();
                    //    }
                    //}

                    this.output.Verbose($"{fileToScan.FullName} scan completed.", ConsoleColor.Green);
                    return result;
                }, token);
            }
            catch (OperationTimeoutException)
            {
                this.output.Verbose($"{fileToScan.FullName} scan timed out.", ConsoleColor.Red);
                return null;
            }
        }

        private static IEnumerable<string> GetSampleVariations(FlagModel flag)
        {
            var result = new List<string>();
            var originals = new[] { flag.Key }.Concat(flag.Aliases);
            foreach (var original in originals)
            {
                var samples = ProduceVariationSamples(original, flag.SettingType == SettingTypes.Boolean).Distinct();
                var prefixed = Prefixes.SelectMany(p => samples.Select(s => $"{p}{s}"));

                result.AddRange(prefixed);
            }

            return result;
        }

        private static IEnumerable<string> ProduceVariationSamples(string original, bool isBoolFlag)
        {
            var trimmed = original.RemoveDashes();
            var includeUnderscore = trimmed != original;

            yield return original;
            yield return trimmed;
            yield return $"get{trimmed}";

            if (includeUnderscore)
                yield return $"get_{original}";

            if (isBoolFlag)
            {
                yield return $"is{trimmed}";
                yield return $"is{trimmed}enabled";
                yield return $"has{trimmed}";

                if (includeUnderscore)
                {
                    yield return $"is_{original}";
                    yield return $"is_{original}_enabled";
                    yield return $"has_{original}";
                }
            }
        }

        private static double Similarity(string a, string b)
        {
            a = a.ToLowerInvariant().RemoveDashes();
            b = b.ToLowerInvariant().RemoveDashes();
            var distance = DamerauLevenshteinDistance(a, b);
            return 1.0 - (distance / (double)Math.Max(a.Length, b.Length));
        }

        // Damerau-Levenshtein Distance implementation from https://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        private static int DamerauLevenshteinDistance(string source, string target, int threshold = int.MaxValue)
        {
            int length1 = source.Length;
            int length2 = target.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            if (Math.Abs(length1 - length2) > threshold) { return int.MaxValue; }

            // Ensure arrays [i] / length1 use shorter length 
            if (length1 > length2)
            {
                Swap(ref target, ref source);
                Swap(ref length1, ref length2);
            }

            int maxi = length1;
            int maxj = length2;

            int[] dCurrent = new int[maxi + 1];
            int[] dMinus1 = new int[maxi + 1];
            int[] dMinus2 = new int[maxi + 1];
            int[] dSwap;

            for (int i = 0; i <= maxi; i++) { dCurrent[i] = i; }

            int jm1 = 0;

            for (int j = 1; j <= maxj; j++)
            {

                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int minDistance = int.MaxValue;
                dCurrent[0] = j;
                int im1 = 0;
                int im2 = -1;
                for (int i = 1; i <= maxi; i++)
                {
                    int cost = source[im1] == target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + cost;

                    //Fastest execution for min value of 3 integers
                    int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

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

            int result = dCurrent[maxi];
            return (result > threshold) ? int.MaxValue : result;
        }

        private static void Swap<T>(ref T arg1, ref T arg2)
        {
            T temp = arg1;
            arg1 = arg2;
            arg2 = temp;
        }
    }
}
