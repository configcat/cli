using System.IO;
using System.Threading.Tasks;
using ConfigCat.Cli.Models;
using ConfigCat.Cli.Services.Git;
using ConfigCat.Cli.Services.Rendering;
using Xunit;

namespace ConfigCat.Cli.Tests;

public class GitClientTests
{
    [Fact]
    public async Task Test_GitClient()
    {
        var client = new GitClient(new Output(new CliOptions()));
        var executingDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        var info = await client.GetRepoDetailsOrNull(executingDir);
        var repoDir = await client.GetRepoRootDirectoryOrNull(executingDir);
        
        Assert.NotEmpty(repoDir.FullName);
        Assert.NotEmpty(info.Branch);
        Assert.NotEmpty(info.ActiveBranches);
        Assert.NotEmpty(info.CurrentCommitHash);
    }
}