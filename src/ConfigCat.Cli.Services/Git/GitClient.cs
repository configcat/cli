using ConfigCat.Cli.Models.Git;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Services.Git;

public interface IGitClient
{
    Task<DirectoryInfo> GetRepoRootDirectoryOrNull(DirectoryInfo directory);

    Task<GitRepositoryInfo> GetRepoDetailsOrNull(DirectoryInfo directory);
}

public class GitClient(IOutput output) : IGitClient
{
    private static readonly TimeSpan GitCmdTimeout = TimeSpan.FromSeconds(30);

    public async Task<DirectoryInfo> GetRepoRootDirectoryOrNull(DirectoryInfo directory)
    {
        try
        {
            var startInfo = GetGitProcessStartInfoForPath(directory.FullName);

            var repoWorkingDir = await ExecuteAsync(startInfo, "rev-parse --show-toplevel", GitCmdTimeout);
            if (repoWorkingDir.StdOut.IsEmpty() || !Directory.Exists(repoWorkingDir.StdOut))
            {
                output.WriteYellow($"{directory.FullName} is not a Git repository. Skipping.").WriteLine();
                return null;
            }

            output.WriteGreen($"Git repository found at {repoWorkingDir.StdOut}").WriteLine();
            return new DirectoryInfo(repoWorkingDir.StdOut);
        }
        catch (Exception)
        {
            output.WriteYellow("Could not execute the Git CLI, it's probably not installed. Skipping.").WriteLine();
            return null;
        }
    }

    public async Task<GitRepositoryInfo> GetRepoDetailsOrNull(DirectoryInfo directory)
    {
        output.Write("Collecting Git repository information from ")
            .WriteCyan(directory.FullName)
            .WriteLine();

        try
        {
            return await this.CollectInfoFromCli(directory);
        }
        catch (Exception)
        {
            output.WriteYellow("Could not execute the Git CLI, it's probably not installed. Skipping.").WriteLine();
            return null;
        }
    }

    private async Task<GitRepositoryInfo> CollectInfoFromCli(DirectoryInfo directory)
    {
        var startInfo = GetGitProcessStartInfoForPath(directory.FullName);

        var commitHash = await ExecuteAsync(startInfo, "rev-parse HEAD", GitCmdTimeout);
        var branchName = await ExecuteAsync(startInfo, "rev-parse --abbrev-ref HEAD", GitCmdTimeout);
        var remoteBranches = await ExecuteAsync(startInfo, "ls-remote --heads --quiet", GitCmdTimeout);

        var activeBranches = new List<string>();
        var regex = Regex.Match(remoteBranches.StdOut, "refs/heads/(.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled,
            TimeSpan.FromSeconds(5));

        while (regex.Success)
        {
            activeBranches.Add(regex.Groups[1].Value.Trim());
            regex = regex.NextMatch();
        }

        return new GitRepositoryInfo
        {
            ActiveBranches = activeBranches,
            Branch = branchName.StdOut,
            CurrentCommitHash = commitHash.StdOut,
        };
    }

    private async Task<Result> ExecuteAsync(ProcessStartInfo startInfo, string arguments, TimeSpan timeout)
    {
        var result = new Result();

        using var process = new Process();
        startInfo.Arguments = arguments;
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;
        var processTasks = new List<Task>();

        var processExitEvent = new TaskCompletionSource<object>();
        process.Exited += (_, _) => { processExitEvent.TrySetResult(true); };
        processTasks.Add(processExitEvent.Task);

        var stdOutBuilder = new StringBuilder();
        var stdErrBuilder = new StringBuilder();
        var stdOutCloseEvent = new TaskCompletionSource<bool>();
        var stdErrCloseEvent = new TaskCompletionSource<bool>();

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                stdOutCloseEvent.TrySetResult(true);
            }
            else
            {
                stdOutBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data == null)
            {
                stdErrCloseEvent.TrySetResult(true);
            }
            else
            {
                stdErrBuilder.AppendLine(e.Data);
            }
        };

        processTasks.Add(stdOutCloseEvent.Task);
        processTasks.Add(stdErrCloseEvent.Task);

        if (!process.Start())
        {
            result.ExitCode = process.ExitCode;
            return result;
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var processCompletionTask = Task.WhenAll(processTasks);

        var awaitingTask = Task.WhenAny(Task.Delay(timeout), processCompletionTask);

        if (await awaitingTask == processCompletionTask)
        {
            result.ExitCode = process.ExitCode;
        }
        else
        {
            try
            {
                process.Kill();
            }
            catch
            {
                /*ignored*/
            }

            output.WriteWarning($"'{startInfo.FileName} {arguments}' has timed out without a result.");
        }

        result.StdOut = stdOutBuilder.ToString().Trim();
        result.StdErr = stdErrBuilder.ToString().Trim();

        if (result.ExitCode is not 0)
        {
            output.WriteWarning(
                $"'{startInfo.FileName} {arguments}' exited with code {result.ExitCode}. Error: {result.StdOut}{Environment.NewLine}{result.StdErr}");
        }

        return result;
    }

    private static ProcessStartInfo GetGitProcessStartInfoForPath(string path) => new()
    {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        FileName = "git",
        CreateNoWindow = true,
        WorkingDirectory = path
    };

    private class Result
    {
        public int? ExitCode { get; set; }
        public string StdErr { get; set; }
        public string StdOut { get; set; }
    }
}