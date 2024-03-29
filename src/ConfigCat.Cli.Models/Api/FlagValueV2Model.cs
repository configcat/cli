using System;
using System.Collections.Generic;
using ConfigCat.Cli.Models.ConfigFile;

namespace ConfigCat.Cli.Models.Api;

public class FlagValueV2Model
{
    public ValueModel DefaultValue { get; set; }
    public List<TargetingRuleModel> TargetingRules { get; set; }
    public string PercentageEvaluationAttribute { get; set; }
    public FlagModel Setting { get; set; }
}

public class ValueModel
{
    public bool? BoolValue { get; set; }
    public string StringValue { get; set; }
    public int? IntValue { get; set; }
    public double? DoubleValue { get; set; }
}

public class TargetingRuleModel
{
    public List<ConditionModel> Conditions { get; set; }
    public List<PercentageOptionModel> PercentageOptions { get; set; }
    public ValueModel Value { get; set; }
}

public class ConditionModel
{
    public UserConditionModel UserCondition { get; set; }
    public SegmentConditionModel SegmentCondition { get; set; }
    public PrerequisiteFlagConditionModel PrerequisiteFlagCondition { get; set; }
}

public class UserConditionModel
{
    public string ComparisonAttribute { get; set; }
    public string Comparator { get; set; }
    public ComparisonValueModel ComparisonValue { get; set; }
}

public class SegmentConditionModel
{
    public string SegmentId { get; set; }
    public string Comparator { get; set; }
}

public class PrerequisiteFlagConditionModel
{
    public int PrerequisiteSettingId { get; set; }
    public string Comparator { get; set; }
    public ValueModel PrerequisiteComparisonValue { get; set; }
}

public class ComparisonValueModel
{
    public string StringValue { get; set; }
    public double? DoubleValue { get; set; }
    public ICollection<ComparisonValueListModel> ListValue { get; set; }
}

public class ComparisonValueListModel
{
    public string Value { get; set; }
    public string Hint { get; set; }
}

public class PercentageOptionModel
{
    public int Percentage { get; set; }
    public ValueModel Value { get; set; }
}