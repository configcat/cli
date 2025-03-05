using System;
using System.IO;
using Ignore;

namespace ConfigCat.Cli.Services.FileSystem.Ignore;

internal class GlobalIgnorePolicy : IgnorePolicy
{
    private readonly DirectoryInfo rootDirectory;

    public GlobalIgnorePolicy(DirectoryInfo rootDirectory, params string[] patterns)
    {
        this.rootDirectory = rootDirectory;
        foreach (var pattern in patterns)
            base.DenyRules.Add(new IgnoreRule(pattern));
    }

    public override bool Handles(FileInfo file) => true;

    protected override string PreProcessFilePath(FileInfo file) =>
        Path.GetRelativePath(this.rootDirectory.FullName, file.FullName).WithSlashes();
}