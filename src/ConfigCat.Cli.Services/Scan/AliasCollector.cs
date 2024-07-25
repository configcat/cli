using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using ConfigCat.Cli.Models.Api;
using System.Threading;
using Trybot;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.ObjectModel;

namespace ConfigCat.Cli.Services.Scan;

public interface IAliasCollector
{
    Task<ConcurrentDictionary<string, ConcurrentBag<string>>> CollectAsync(FlagModel[] flags,
        FileInfo fileToScan,
        string[] matchPatterns,
        List<string> warningTracker,
        CancellationToken token);
}

public class AliasCollector(IOutput output) : IAliasCollector
{
    public async Task<ConcurrentDictionary<string, ConcurrentBag<string>>> CollectAsync(FlagModel[] flags,
        FileInfo fileToScan,
        string[] matchPatterns, List<string> warningTracker, CancellationToken token)
    {
        if (await fileToScan.IsBinaryAsync(token))
        {
            output.Verbose($"{fileToScan.FullName} is binary, skipping.", ConsoleColor.Yellow);
            return null;
        }

        output.Verbose($"{fileToScan.FullName} - searching aliases...");

        var flagKeys = flags.Select(f => f.Key).ToArray();
        var keys = string.Join('|', flagKeys);

        var aliases = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        var lineNumber = 1;
        var lines = File.ReadLinesAsync(fileToScan.FullName, token);
        await foreach (var line in lines)
        {
            if (line.Length > Constants.MaxCharCountPerLine)
            {
                warningTracker.Add(
                    $"{fileToScan.FullName} - {lineNumber}. line is longer than allowed ({Constants.MaxCharCountPerLine} chars), skipping alias search.");
                continue;
            }

            if (!flagKeys.Any(line.Contains))
                continue;

            var match = Regex.Match(line,
                @"[`{'""]?([a-zA-Z_$0-9]*)[[`}'\""]?\s*(?>\:?\s*(?>[sS]tring)?\s*=?>?\s*(?>new|await)?)\s*\S*[@$]?[`'""](" +
                keys + ")[`'\"]",
                RegexOptions.Compiled);

            while (match.Success && !token.IsCancellationRequested)
            {
                var key = match.Groups[2].Value;
                var found = match.Groups[1].Value;
                var flag = flags.FirstOrDefault(f => f.Key == key);

                if (flag != null && !found.IsEmpty() && Similarity(flag.Key, found) > 0.3)
                    aliases.AddOrUpdate(flag.Key, [found], (k, v) =>
                    {
                        v.Add(found);
                        return v;
                    });

                match = match.NextMatch();
            }

            if (matchPatterns.Length != 0)
            {
                foreach (var matchPattern in matchPatterns)
                {
                    if (!matchPattern.Contains(Constants.KeyPatternPlaceHolder))
                        continue;

                    var regMatch = Regex.Match(line,
                        matchPattern.Replace(Constants.KeyPatternPlaceHolder, $"[`'\"]?(?<keys>{keys})[`'\"]?"),
                        RegexOptions.Compiled);
                    while (regMatch.Success && !token.IsCancellationRequested)
                    {
                        var keyGroup = regMatch.Groups["keys"];
                        var found = regMatch.Groups.Values.Skip(1).Except([keyGroup]).FirstOrDefault();
                        if (found is null)
                        {
                            regMatch = regMatch.NextMatch();
                            continue;
                        }

                        var flag = flags.FirstOrDefault(f => f.Key == keyGroup.Value);

                        if (flag != null && !found.Value.IsEmpty())
                            aliases.AddOrUpdate(flag.Key, [found.Value], (k, v) =>
                            {
                                v.Add(found.Value);
                                return v;
                            });

                        regMatch = regMatch.NextMatch();
                    }
                }
            }

            lineNumber++;
        }

        output.Verbose($"{fileToScan.FullName} - alias search completed.", ConsoleColor.Green);
        return aliases;
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