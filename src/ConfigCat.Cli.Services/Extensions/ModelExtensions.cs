using System;
using System.Collections.Generic;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;

namespace ConfigCat.Cli.Services.Extensions;

public static class ModelExtensions
{
    public static object ToSingle(this ValueModel model, string settingType)
    {
        return settingType switch
        {
            SettingTypes.Boolean => model.BoolValue,
            SettingTypes.String => model.StringValue,
            SettingTypes.Int => model.IntValue,
            SettingTypes.Double => model.DoubleValue,
            _ => ""
        };
    }

    public static string ToStringValue(this ComparisonValueModel model)
    {
        if (!string.IsNullOrWhiteSpace(model.StringValue))
            return model.StringValue.Cut(20);
        if (model.DoubleValue is not null)
            return model.DoubleValue.ToString();
        return !model.ListValue.IsEmpty() ? $"[{model.ListValue.Count} items]" : "";
    }

    public static bool IsEmpty(this Workspace workspace) => workspace is null || workspace.Product.IsEmpty();

}