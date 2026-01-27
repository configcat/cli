using System;
using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Api;

public class FlagValueV2Model
{
    public ValueWithPredefinedVariationModel DefaultValue { get; set; }
    public List<TargetingRuleModel> TargetingRules { get; set; }
    public string PercentageEvaluationAttribute { get; set; }
    public FlagModel Setting { get; set; }
}

public class ValueModel
{
    public bool? BoolValue { get; init; }
    public string StringValue { get; init; }
    public int? IntValue { get; init; }
    public double? DoubleValue { get; init; }


    public override string ToString() => this.StringValue ?? this.BoolValue?.ToString() ?? this.IntValue?.ToString() ?? this.DoubleValue?.ToString() ?? string.Empty;
    
    public override bool Equals(object obj) =>
        obj is ValueModel other && BoolValue == other.BoolValue && StringValue == other.StringValue && IntValue == other.IntValue && Nullable.Equals(DoubleValue, other.DoubleValue);

    public override int GetHashCode() => HashCode.Combine(BoolValue, StringValue, IntValue, DoubleValue);
}

public class ValueWithPredefinedVariationModel : ValueModel
{
    public string PredefinedVariationId { get; set; }
}

public class TargetingRuleModel
{
    public List<ConditionModel> Conditions { get; set; }
    public List<PercentageOptionModel> PercentageOptions { get; set; }
    public ValueWithPredefinedVariationModel Value { get; set; }
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
    public ValueWithPredefinedVariationModel PrerequisiteComparisonValue { get; set; }
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
    public ValueWithPredefinedVariationModel Value { get; set; }
}