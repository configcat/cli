using System.IO;

namespace ConfigCat.Cli.Services.FileSystem.Ignore
{
    internal class GlobalIgnorePolicy : IgnorePolicy
    {
        private readonly DirectoryInfo rootDirectory;

        public GlobalIgnorePolicy(DirectoryInfo rootDirectory, params string[] patterns)
        {
            foreach (var pattern in patterns)
                base.IgnoreMatcher.Add(pattern);

            this.rootDirectory = rootDirectory;
        }

        public override bool IsAccepting(FileInfo file) => base.IsAcceptingInternal(file.FullName.Replace(this.rootDirectory.FullName, string.Empty));

        public override bool IsIgnoring(FileInfo file) => base.IsIgnoringInternal(file.FullName.Replace(this.rootDirectory.FullName, string.Empty));

        public override bool Handles(FileInfo file) =>
            file.DirectoryName.IndexOf(this.rootDirectory.FullName) != -1;
    }
}
