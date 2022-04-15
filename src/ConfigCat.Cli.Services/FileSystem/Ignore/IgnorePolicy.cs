using System;
using System.IO;

namespace ConfigCat.Cli.Services.FileSystem.Ignore;

internal abstract class IgnorePolicy
{
    protected readonly IgnoreRuleSet AcceptMatcher = new IgnoreRuleSet();
    protected readonly IgnoreRuleSet IgnoreMatcher = new IgnoreRuleSet();

    public abstract bool IsAccepting(FileInfo file);

    public abstract bool IsIgnoring(FileInfo file);

    public abstract bool Handles(FileInfo file);

    public int Rank { get; protected set; }

    protected bool IsAcceptingInternal(string filePath) => this.AcceptMatcher.HasMatch(filePath.AsSlash().AsSpan());

    protected bool IsIgnoringInternal(string filePath) => this.IgnoreMatcher.HasMatch(filePath.AsSlash().AsSpan());
}