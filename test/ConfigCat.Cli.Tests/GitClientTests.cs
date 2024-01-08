using System.IO;
using System.Reflection;
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
        var response = await client.GatherGitInfo(Directory.GetCurrentDirectory());
        
        Assert.NotEmpty(response.WorkingDirectory);
        Assert.NotEmpty(response.Branch);
        Assert.NotEmpty(response.ActiveBranches);
        Assert.NotEmpty(response.CurrentCommitHash);
    }
}