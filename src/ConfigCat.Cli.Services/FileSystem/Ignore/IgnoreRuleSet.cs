using DotNet.Globbing;
using System;
using System.Collections.Generic;

namespace ConfigCat.Cli.Services.FileSystem.Ignore;

internal class IgnoreRuleSet
{
    private readonly List<Glob> rules = new List<Glob>();

    public void Add(string pattern)
    {
        if (!pattern.StartsWith('/'))
            pattern = $"**/{pattern}";

        if (pattern.EndsWith('/'))
            pattern = $"{pattern}**";

        pattern = pattern.Replace(@"\ ", " ");

        if (pattern.StartsWith('/') && !pattern.EndsWith("/**"))
            this.rules.Add(Glob.Parse($"{pattern}{(pattern.EndsWith('/') ? "**" : "/**")}".Trim()));

        this.rules.Add(Glob.Parse(pattern.Trim()));
    }

    public bool HasMatch(ReadOnlySpan<char> path)
    {
        foreach (var glob in this.rules)
        {
            if (glob.IsMatch(path))
                return true;
        }

        return false;
    }
}