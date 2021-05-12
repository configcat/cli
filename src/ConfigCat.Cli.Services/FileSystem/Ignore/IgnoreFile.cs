using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.FileSystem.Ignore
{
    class IgnoreFile
    {
        public static readonly string[] IgnoreFileNames = new[] { ".gitignore", ".ccignore", ".ignore" };

        public int Rank { get; }

        public FileInfo File { get; }

        private readonly IgnoreRuleSet acceptMatcher = new IgnoreRuleSet();
        private readonly IgnoreRuleSet ignoreMatcher = new IgnoreRuleSet();

        public IgnoreFile(FileInfo ignoreFileInfo, DirectoryInfo rootDirectory)
        {
            this.File = ignoreFileInfo;
            this.Rank = this.File.FullName.Replace(rootDirectory.FullName, string.Empty).Count(c => c.Equals(Path.DirectorySeparatorChar));
            this.ignoreMatcher.Add("**/.git/**");
        }

        public async Task LoadIgnoreFileAsync(CancellationToken token)
        {
            var lines = await System.IO.File.ReadAllLinesAsync(this.File.FullName, token);
            this.ProcessPatterns(lines);
        }

        public bool IsAccepting(FileInfo file) => this.acceptMatcher.HasMatch(file.FullName.Replace(this.File.DirectoryName, string.Empty).AsSlash().AsSpan());

        public bool IsIgnoring(FileInfo file) => this.ignoreMatcher.HasMatch(file.FullName.Replace(this.File.DirectoryName, string.Empty).AsSlash().AsSpan());

        public bool Handles(FileInfo file) =>
            file.DirectoryName.IndexOf(this.File.DirectoryName) != -1;

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
                    this.acceptMatcher.Add(current);
                    continue;
                }

                if (pattern.StartsWith(@"\#") || pattern.StartsWith(@"\!"))
                    current = current.Substring(1);

                this.ignoreMatcher.Add(current);
            }
        }
    }
}
