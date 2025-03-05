using ConfigCat.Cli.Services.FileSystem.Ignore;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.FileSystem;

public interface IFileCollector
{
    Task<IEnumerable<FileInfo>> CollectAsync(DirectoryInfo searchDirectory, DirectoryInfo gitRepoDir,
        CancellationToken token);
}

public class FileCollector(IOutput output) : IFileCollector
{
    public async Task<IEnumerable<FileInfo>> CollectAsync(DirectoryInfo searchDirectory, DirectoryInfo gitRepoDir,
        CancellationToken token)
    {
        using var spinner = output.CreateSpinner(token);

        var (ignoreFiles, filesToReturn) = GetFilesFromWorkSpace(searchDirectory, gitRepoDir);
        List<IgnorePolicy> policies =
        [
            new GlobalIgnorePolicy(gitRepoDir ?? searchDirectory, ".git", "*.lock", "*lock.json", "*.graphql", "*.md",
                ".dockerignore")
        ];
        policies.AddRange(ignoreFiles
            .Select(ignoreFile => new IgnoreFile(ignoreFile, gitRepoDir ?? searchDirectory)));

        foreach (var ignore in policies)
        {
            if (ignore is not IgnoreFile ignoreFile) continue;
            output.Verbose($"Using ignore file {ignoreFile.File.FullName}");
            await ignoreFile.LoadIgnoreFileAsync(token);
        }

        return filesToReturn.Where(f =>
        {
            foreach (var ignore in policies.Where(i => i.Handles(f)).OrderByDescending(i => i.Rank))
            {
                if (ignore.IsAccepting(f))
                    return true;

                if (ignore.IsIgnoring(f))
                    return false;
            }

            return true;
        });
    }

    internal static (IEnumerable<FileInfo> ignoreFiles, IEnumerable<FileInfo> files) GetFilesFromWorkSpace(
        DirectoryInfo searchDirectory, DirectoryInfo gitRepoDir)
    {
        var allFiles = EnumerateFiles(searchDirectory, true).ToArray();
        var ignoreFiles = allFiles.Where(f => f.IsIgnoreFile()).ToArray();
        var otherFiles = allFiles.Except(ignoreFiles);
        if (gitRepoDir == null || searchDirectory.SameDirectory(gitRepoDir))
            return (ignoreFiles, otherFiles);

        // We are in a git repository's subdirectory, so we walk up to
        // the root folder and collect all .ignore files along the way.
        var currentDir = searchDirectory;
        do
        {
            currentDir = currentDir.Parent;
            ignoreFiles = ignoreFiles.Concat(EnumerateFiles(currentDir, false).Where(f => f.IsIgnoreFile())).ToArray();
        } while (currentDir != null && !currentDir.SameDirectory(gitRepoDir));

        return (ignoreFiles, otherFiles);
    }

    private static IEnumerable<FileInfo> EnumerateFiles(DirectoryInfo directory, bool recurse) =>
        directory == null
            ? []
            : directory.EnumerateFiles("*", new EnumerationOptions
            {
                RecurseSubdirectories = recurse,
                IgnoreInaccessible = true,
                AttributesToSkip = FileAttributes.System
            });
}