using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Git;

public class GitRepositoryInfo
{
    public string Branch { get; set; }

    public string CurrentCommitHash { get; set; }

    public List<string> ActiveBranches { get; set; }
}