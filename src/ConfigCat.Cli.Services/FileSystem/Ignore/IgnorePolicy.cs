using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ignore;

namespace ConfigCat.Cli.Services.FileSystem.Ignore;

internal abstract class IgnorePolicy
{
    protected readonly List<IgnoreRule> AcceptRules = [];
    protected readonly List<IgnoreRule> DenyRules = [];

    public bool IsAccepting(FileInfo file) => AcceptRules.Any(rule => rule.IsMatch(PreProcessFilePath(file)));

    public bool IsIgnoring(FileInfo file) => DenyRules.Any(rule => rule.IsMatch(PreProcessFilePath(file)));

    public abstract bool Handles(FileInfo file);
    
    protected abstract string PreProcessFilePath(FileInfo file);

    public int Rank { get; protected init; }
}