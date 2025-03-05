using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ignore;

namespace ConfigCat.Cli.Services.FileSystem.Ignore;

internal class IgnoreFile : IgnorePolicy
{
    public static readonly string[] IgnoreFileNames = [".gitignore", ".ccignore", ".ignore"];

    public FileInfo File { get; }

    public IgnoreFile(FileInfo ignoreFileInfo, DirectoryInfo rootDirectory)
    {
        this.File = ignoreFileInfo;
        this.Rank = this.File.FullName.Replace(rootDirectory.FullName, string.Empty).Count(c => c.Equals(Path.DirectorySeparatorChar));
    }

    public async Task LoadIgnoreFileAsync(CancellationToken token)
    {
        var lines = await System.IO.File.ReadAllLinesAsync(this.File.FullName, token);
        this.ProcessPatterns(lines);
    }

    public override bool Handles(FileInfo file) =>
        file.DirectoryName.WithSlashes().Contains(this.File.DirectoryName.WithSlashes().TrimEnd('/'));

    protected override string PreProcessFilePath(FileInfo file) =>
        Path.GetRelativePath(this.File.DirectoryName, file.FullName).WithSlashes();

    private void ProcessPatterns(string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            var current = pattern;
            if (current.Trim().IsEmpty() || current.StartsWith('#'))
                continue;

            if (current.StartsWith('!'))
            {
                current = current[1..];
                base.AcceptRules.Add(new IgnoreRule(current));
                continue;
            }

            if (pattern.StartsWith(@"\#") || pattern.StartsWith(@"\!"))
                current = current[1..];

            base.DenyRules.Add(new IgnoreRule(current));
        }
    }
}