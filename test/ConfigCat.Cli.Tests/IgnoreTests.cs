using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models;
using ConfigCat.Cli.Services.FileSystem;
using ConfigCat.Cli.Services.Git;
using ConfigCat.Cli.Services.Rendering;
using Xunit;

namespace ConfigCat.Cli.Tests;

public class IgnoreTests
{
    [Theory]
    [InlineData("a", new[] { "a/.ignore", "a/b/.ignore", "a/c/f/.ignore" }, 8)]
    [InlineData("a/b", new[] { "a/.ignore", "a/b/.ignore" }, 4)]
    [InlineData("a/c", new[] { "a/.ignore", "a/c/f/.ignore" }, 3)]
    [InlineData("a/c/f", new[] { "a/.ignore", "a/c/f/.ignore" }, 1)]
    [InlineData("a/c/g", new[] { "a/.ignore" }, 1)]
    public async Task Test_IgnoreFile_Collect(string searchDir, string[] expectedIgnoreFiles, int fileCount)
    {
        var executingDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), searchDir));
        var client = new GitClient(new Output(new CliOptions()));
        var repoDir = await client.GetRepoRootDirectoryOrNull(executingDir);
        var (ignoreFiles, files) = FileCollector.GetFilesFromWorkSpace(executingDir, repoDir);

        var expectations = expectedIgnoreFiles.Select(f => Path.Combine(Directory.GetCurrentDirectory(), f).WithSlashes())
            .Concat([Path.Combine(repoDir.FullName, ".gitignore").WithSlashes()]).Order().ToArray();

        var ignoreFileNames = ignoreFiles.Select(f => f.FullName.WithSlashes()).Order();

        Assert.Equal(expectations, ignoreFileNames);
        Assert.Equal(fileCount, files.Count());
    }

    [Theory]
    [InlineData("a", new[] { "a/b/d/d.txt", "a/b/d/h.txt", "a/b/e/e.txt" })]
    [InlineData("a/b", new[] { "a/b/d/d.txt", "a/b/d/h.txt", "a/b/e/e.txt" })]
    [InlineData("a/b/d", new[] { "a/b/d/d.txt", "a/b/d/h.txt" })]
    [InlineData("a/b/e", new[] { "a/b/e/e.txt" })]
    [InlineData("a/c", new string[0])]
    public async Task Test_IgnoreFile_Process_With_GitIgnore(string searchDir, string[] expectedFiles)
    {
        var executingDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), searchDir));
        var client = new GitClient(new Output(new CliOptions()));
        var repoDir = await client.GetRepoRootDirectoryOrNull(executingDir);
        var fileCollector = new FileCollector(new Output(new CliOptions()));
        var files = (await fileCollector.CollectAsync(executingDir, repoDir, CancellationToken.None)).ToArray();

        var expectations = expectedFiles.Select(f => Path.Combine(Directory.GetCurrentDirectory(), f).WithSlashes()).Order()
            .ToArray();
        var fileNames = files.Select(f => f.FullName.WithSlashes()).Order();

        Assert.Equal(expectations, fileNames);
    }

    [Theory]
    [InlineData("a",
        new[] { "a/a.txt", "a/b/d/d.txt", "a/b/d/h.txt", "a/b/e/e.txt", "a/c/c.txt", "a/c/g/g.txt" })]
    [InlineData("a/b", new[] { "a/b/d/d.txt", "a/b/d/h.txt", "a/b/e/e.txt" })]
    [InlineData("a/b/d", new[] { "a/b/d/d.txt", "a/b/d/h.txt" })]
    [InlineData("a/b/e", new[] { "a/b/e/e.txt" })]
    [InlineData("a/c", new[] { "a/c/c.txt", "a/c/g/g.txt" })]
    [InlineData("a/c/g", new[] { "a/c/g/g.txt" })]
    public async Task Test_IgnoreFile_Process_Without_GitIgnore(string searchDir, string[] expectedFiles)
    {
        var executingDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), searchDir));
        var repoDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var fileCollector = new FileCollector(new Output(new CliOptions()));
        var files = (await fileCollector.CollectAsync(executingDir, repoDir, CancellationToken.None)).ToArray();

        var expectations = expectedFiles.Select(f => Path.Combine(Directory.GetCurrentDirectory(), f).WithSlashes()).Order()
            .ToArray();
        var fileNames = files.Select(f => f.FullName.WithSlashes()).Order();

        Assert.Equal(expectations, fileNames);
    }
    
    [Theory]
    [InlineData("a",
        new[] { "a/a.txt", "a/b/d/d.txt", "a/b/d/h.txt", "a/b/e/e.txt", "a/c/c.txt", "a/c/g/g.txt" })]
    [InlineData("a/b", new[] { "a/b/b.txt", "a/b/d/d.txt", "a/b/d/h.txt", "a/b/e/e.txt" })]
    [InlineData("a/b/d", new[] { "a/b/d/d.txt", "a/b/d/h.txt" })]
    [InlineData("a/b/e", new[] { "a/b/e/e.txt" })]
    [InlineData("a/c", new[] { "a/c/c.txt", "a/c/g/g.txt" })]
    [InlineData("a/c/g", new[] { "a/c/g/g.txt" })]
    public async Task Test_IgnoreFile_Process_Without_Git(string searchDir, string[] expectedFiles)
    {
        var executingDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), searchDir));
        var fileCollector = new FileCollector(new Output(new CliOptions()));
        var files = (await fileCollector.CollectAsync(executingDir, null, CancellationToken.None)).ToArray();

        var expectations = expectedFiles.Select(f => Path.Combine(Directory.GetCurrentDirectory(), f).WithSlashes()).Order()
            .ToArray();
        var fileNames = files.Select(f => f.FullName.WithSlashes()).Order();

        Assert.Equal(expectations, fileNames);
    }
}