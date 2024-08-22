using System.Collections.Generic;
using ConfigCat.Cli.Models.Api;

namespace ConfigCat.Cli.Models;

public class FlagValueJsonOutput
{
    public FlagModel Setting { get; set; }

    public IEnumerable<ValueInEnvironmentJsonOutput> Values { get; set; }
}

public class ValueInEnvironmentJsonOutput
{
    public string EnvironmentId { get; set; }

    public string EnvironmentName { get; set; }

    public List<PercentageModel> PercentageRules { get; set; }

    public List<TargetingModel> TargetingRules { get; set; }

    public object Value { get; set; }
}

public class FlagValueV2JsonOutput
{
    public FlagModel Setting { get; set; }
    public IEnumerable<ValueV2InEnvironmentJsonOutput> Values { get; set; }
}

public class ValueV2InEnvironmentJsonOutput
{
    public string EnvironmentId { get; set; }
    public string EnvironmentName { get; set; }
    public ValueModel DefaultValue { get; set; }
    public List<TargetingRuleModel> TargetingRules { get; set; }
    public string PercentageEvaluationAttribute { get; set; }
}

public class ProductJsonOutput : ProductModel
{
    public IEnumerable<EnvironmentModel> Environments { get; set; }

    public IEnumerable<ConfigModel> Configs { get; set; }
}