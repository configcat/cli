using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;

namespace ConfigCat.Cli.Commands.Flags.V2;

internal class FlagTargeting
{
    public async Task<int> AddUserTargetingRuleAsync(int? flagId,
        string environmentId,
        string attribute,
        string comparator,
        string comparisonValue,
        string flagValue,
        UpdatePercentageModel[] percentageOptions,
        CancellationToken token)
    {
        return ExitCodes.Ok;
    }
}