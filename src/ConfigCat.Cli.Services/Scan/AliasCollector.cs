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
using System.Collections.ObjectModel;

namespace ConfigCat.Cli.Services.Scan;

public interface IAliasCollector
{
    Task<AliasScanResult> CollectAsync(FlagModel[] flags,
        FileInfo fileToScan,
        string[] matchPatterns,
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

    public async Task<AliasScanResult> CollectAsync(FlagModel[] flags, FileInfo fileToScan,
        string[] matchPatterns, CancellationToken token)
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

                    var match = Regex.Match(line, @"[`{'""]?([a-zA-Z_$0-9]*)[[`}'\""]?\s*(?>\:?\s*(?>[sS]tring)?\s*=?>?\s*(?>new|await)?)\s*\S*[@$]?[`'""](" + keys + ")[`'\"]",
                        RegexOptions.Compiled);
                    
                    while (match.Success && !cancellation.IsCancellationRequested)
                    {
                        var key = match.Groups[2].Value;
                        var found = match.Groups[1].Value;
                        var flag = flags.FirstOrDefault(f => f.Key == key);

                        if (flag != null)
                            result.FoundFlags.Add(flag);

                        if (flag != null && !found.IsEmpty() && Similarity(flag.Key, found) > 0.3)
                            result.FlagAliases.AddOrUpdate(flag, [found], (k, v) => { v.Add(found); return v; });

                        match = match.NextMatch();
                    }

                    if (matchPatterns.Length != 0)
                    {
                        foreach (var matchPattern in matchPatterns)
                        {
                            if (!matchPattern.Contains(Constants.KeyPatternPlaceHolder))
                                continue;
                            
                            var regMatch = Regex.Match(line, matchPattern.Replace(Constants.KeyPatternPlaceHolder, $"[`'\"]?(?<keys>{keys})[`'\"]?"), RegexOptions.Compiled);
                            while (regMatch.Success && !cancellation.IsCancellationRequested)
                            {
                                var keyGroup = regMatch.Groups["keys"];
                                var found = regMatch.Groups.Values.Skip(1).Except([keyGroup]).FirstOrDefault();
                                if (found is null)
                                {
                                    regMatch = regMatch.NextMatch();
                                    continue;
                                }

                                var flag = flags.FirstOrDefault(f => f.Key == keyGroup.Value);

                                if (flag != null)
                                    result.FoundFlags.Add(flag);

                                if (flag != null && !found.Value.IsEmpty())
                                    result.FlagAliases.AddOrUpdate(flag, [found.Value], (k, v) => { v.Add(found.Value); return v; });

                                regMatch = regMatch.NextMatch();
                            }
                        }
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
        return QGramSimilarity(a, b);
    }

    private static readonly Regex MultipleSpaces = new("\\s+");

    private static double QGramSimilarity(string s1, string s2)
    {
        if (s1 == s2) return 1;

        var qGrams1 = GetQGrams(s1);
        var qGrams2 = GetQGrams(s2);

        var sum = qGrams1.Values.Sum() + qGrams2.Values.Sum();
        return (sum - Distance()) / sum;

        IDictionary<string, int> GetQGrams(string s)
        {
            const int tokenLength = 3;

            var shingles = new Dictionary<string, int>();
            var trimmed = MultipleSpaces.Replace(s, " ");
            for (var i = 0; i < (trimmed.Length - tokenLength + 1); i++)
            {
                var shingle = trimmed.Substring(i, tokenLength);
                if (shingles.TryGetValue(shingle, out var old))
                    shingles[shingle] = old + 1;
                else
                    shingles[shingle] = 1;
            }
            return new ReadOnlyDictionary<string, int>(shingles);
        }

        double Distance()
        {
            var union = new HashSet<string>();
            union.UnionWith(qGrams1.Keys);
            union.UnionWith(qGrams2.Keys);

            var distance = 0;
            foreach (var key in union)
            {
                var v1 = 0;
                var v2 = 0;

                if (qGrams1.TryGetValue(key, out var iv1))
                    v1 = iv1;

                if (qGrams2.TryGetValue(key, out var iv2))
                    v2 = iv2;

                distance += Math.Abs(v1 - v2);
            }
            return distance;
        }
    }
}