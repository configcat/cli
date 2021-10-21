using ConfigCat.Cli.Models.Git;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ConfigCat.Cli.Services.Git
{
    public interface IGitClient
    {
        GitRepositoryInfo GatherGitInfo(string path);
    }

    public class GitClient : IGitClient
    {
        private readonly IOutput output;

        public GitClient(IOutput output)
        {
            this.output = output;
        }

        public GitRepositoryInfo GatherGitInfo(string path)
        {
            this.output.Write("Collecting Git repository information from ")
                .WriteCyan(path)
                .WriteLine();

            try
            {
                return this.CollectInfoFromCLI(path);
            }
            catch
            {
                this.output.WriteYellow("Could not execute the Git CLI, it's probably not installed. Skipping.").WriteLine();
                return null;
            }
        }

        private GitRepositoryInfo CollectInfoFromCLI(string path)
        {
            using var process = this.GetGitProcess(path);            

            var repoWorkingDir = this.ExecuteCommand(process, "rev-parse --show-toplevel");
            if (repoWorkingDir.IsEmpty())
            {
                this.output.WriteYellow($"{path} is not a Git repository. Skipping.").WriteLine();
                return null;
            }

            this.output.WriteGreen($"Git repository found at {repoWorkingDir}").WriteLine();

            var commitHash = this.ExecuteCommand(process, "rev-parse HEAD");
            var branchName = this.ExecuteCommand(process, "rev-parse --abbrev-ref HEAD");
            var remoteBranches = this.ExecuteCommand(process, "ls-remote --heads --quiet");

            var activeBranches = new List<string>();
            var regex = Regex.Match(remoteBranches, @"refs\/heads\/(.*)",
                                       RegexOptions.IgnoreCase | RegexOptions.Compiled,
                                       TimeSpan.FromSeconds(1));

            while(regex.Success)
            {
                activeBranches.Add(regex.Groups[1].Value.Trim());
                regex = regex.NextMatch();
            }

            return new GitRepositoryInfo
            {
                ActiveBranches = activeBranches,
                WorkingDirectory = repoWorkingDir,
                Branch = branchName,
                CurrentCommitHash = commitHash,
            };
        }

        private Process GetGitProcess(string path)
        {
            var processInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                FileName = "git",
                CreateNoWindow = true,
                WorkingDirectory = path
            };

            var process = new Process { StartInfo = processInfo };
            return process;
        }

        private string ExecuteCommand(Process process, string arguments)
        {
            process.StartInfo.Arguments = arguments;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output.Trim();
        }
    }
}
