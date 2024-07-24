using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    public override bool IsAccepting(FileInfo file) => base.IsAcceptingInternal(file.FullName.Replace(this.File.DirectoryName, string.Empty));

    public override bool IsIgnoring(FileInfo file) => base.IsIgnoringInternal(file.FullName.Replace(this.File.DirectoryName, string.Empty));

    public override bool Handles(FileInfo file) =>
        file.DirectoryName.AsSlash().Contains(this.File.DirectoryName.AsSlash().TrimEnd('/'));

    private void ProcessPatterns(string[] patterns)
    {
        foreach (var pattern in patterns)
        {
            var current = pattern;
            if (current.Trim().IsEmpty() || current.StartsWith('#'))
                continue;

            if (current.StartsWith('!'))
            {
                current = current.Substring(1);
                base.AcceptMatcher.Add(current);
                continue;
            }

            if (pattern.StartsWith(@"\#") || pattern.StartsWith(@"\!"))
                current = current.Substring(1);

            base.IgnoreMatcher.Add(current);
        }
    }
}