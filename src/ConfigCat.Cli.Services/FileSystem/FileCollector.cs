using ConfigCat.Cli.Services.FileSystem.Ignore;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.FileSystem
{
    public interface IFileCollector
    {
        Task<IEnumerable<FileInfo>> CollectAsync(DirectoryInfo rootDirectory, CancellationToken token);
    }

    public class FileCollector : IFileCollector
    {
        private readonly IOutput output;

        public FileCollector(IOutput output)
        {
            this.output = output;
        }

        public async Task<IEnumerable<FileInfo>> CollectAsync(DirectoryInfo rootDirectory, CancellationToken token)
        {
            using var spinner = this.output.CreateSpinner(token);

            var files = rootDirectory.GetFiles("*", new EnumerationOptions
            {
                RecurseSubdirectories = true,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.System
            });
            var ignoreFiles = files.Where(f => f.IsIgnoreFile()).ToArray();
            var filesToReturn = files.Except(ignoreFiles);
            var ignores = ignoreFiles
                .Select(ignoreFile => new IgnoreFile(ignoreFile, rootDirectory))
                .Cast<IgnorePolicy>()
                .ToList();

            ignores.Add(new GlobalIgnorePolicy(rootDirectory, "**/.git/**"));

            foreach (var ignore in ignores)
            {
                if (ignore is not IgnoreFile ignoreFile) continue;
                output.Verbose($"Using ignore file {ignoreFile.File.FullName}");
                await ignoreFile.LoadIgnoreFileAsync(token);
            }

            return filesToReturn.Where(f =>
            {
                foreach (var ignore in ignores.Where(i => i.Handles(f)).OrderByDescending(i => i.Rank))
                {
                    if (ignore.IsAccepting(f))
                        return true;

                    if (ignore.IsIgnoring(f))
                        return false;
                }

                return true;
            });
        }
    }
}
