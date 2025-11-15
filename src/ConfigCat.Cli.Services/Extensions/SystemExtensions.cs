using ConfigCat.Cli.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services.Exceptions;

namespace System;

public static class SystemExtensions
{
    public static string WithSlashes(this string text) => text.Replace(Path.DirectorySeparatorChar, '/');

    public static string RemoveDashes(this string text) => text.Replace("-", string.Empty).Replace("_", string.Empty);

    public static string Remove(this string text, IEnumerable<string> toRemove) =>
        toRemove.Aggregate(text, (current, remove) => current.Replace(remove, string.Empty));

    public static int GetDigitCount(this int number) => (int)Math.Floor(Math.Log10(Math.Abs(number)) + 1);

    public static bool IsEmpty(this string value) =>
        string.IsNullOrWhiteSpace(value);

    public static string NullIfEmpty(this string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    public static bool IsEmptyOrEquals(this string value, string other) =>
        string.IsNullOrWhiteSpace(value) || value.Equals(other);

    public static bool TryParseFlagValue(this string value, string settingType, out object parsed)
    {
        parsed = null;
        switch (settingType)
        {
            case SettingTypes.Boolean:
                if (!bool.TryParse(value, out var boolParsed)) return false;
                parsed = boolParsed;
                return true;
            case SettingTypes.Int:
                if (!int.TryParse(value, out var intParsed)) return false;
                parsed = intParsed;
                return true;
            case SettingTypes.Double:
                if (!double.TryParse(value, out var doubleParsed)) return false;
                parsed = doubleParsed;
                return true;
            case SettingTypes.String:
                parsed = value;
                return true;
            default:
                return false;
        }
    }

    public static ValueModel ToFlagValue(this string value, string settingType)
    {
        switch (settingType)
        {
            case SettingTypes.Boolean:
                if (bool.TryParse(value, out var boolParsed))
                    return new ValueModel { BoolValue = boolParsed };
                break;
            case SettingTypes.Int:
                if (int.TryParse(value, out var intParsed))
                    return new ValueModel { IntValue = intParsed };
                break;
            case SettingTypes.Double:
                if (double.TryParse(value, out var doubleParsed)) 
                    return new ValueModel { DoubleValue = doubleParsed };
                break;
            case SettingTypes.String:
                return new ValueModel { StringValue = value };
        }
        throw new ShowHelpException($"Value '{value}' doesn't conform the setting type '{settingType}'");
    }

    public static string ToValuePropertyName(this string settingType)
    {
        return settingType switch
        {
            SettingTypes.Boolean => "boolValue",
            SettingTypes.String => "stringValue",
            SettingTypes.Int => "intValue",
            SettingTypes.Double => "doubleValue",
            _ => ""
        };
    }

    public static string GetDefaultValueForType(this string type) =>
        type switch
        {
            SettingTypes.Boolean => "false",
            SettingTypes.Int => "42",
            SettingTypes.Double => "3.14",
            SettingTypes.String => "initial value",
            _ => ""
        };

    public static string Cut(this string text, int length)
        => text == null ? string.Empty : text.Length > length ? $"{text[..(length - 3)]}..." : text;

    public static bool IsListComparator(this string comparator) =>
        comparator switch
        {
            "sensitiveIsOneOf" or
                "sensitiveIsNotOneOf" or
                "semVerIsOneOf" or
                "semVerIsNotOneOf" or
                "containsAnyOf" or
                "doesNotContainAnyOf" or
                "sensitiveTextStartsWithAnyOf" or
                "sensitiveTextNotStartsWithAnyOf" or
                "sensitiveTextEndsWithAnyOf" or
                "sensitiveTextNotEndsWithAnyOf" or
                "sensitiveArrayContainsAnyOf" or
                "sensitiveArrayDoesNotContainAnyOf" or
                "isOneOf" or
                "isNotOneOf" or
                "textStartsWithAnyOf" or
                "textNotStartsWithAnyOf" or
                "textEndsWithAnyOf" or
                "textNotEndsWithAnyOf" or
                "arrayContainsAnyOf" or
                "arrayDoesNotContainAny" => true,
            _ => false
        };
    
    public static bool IsNumberComparator(this string comparator) =>
        comparator switch
        {
            "numberEquals" or
                "numberDoesNotEqual" or
                "numberLess" or
                "numberLessOrEquals" or
                "numberGreater" or
                "numberGreaterOrEquals" => true,
            _ => false
        };
    
    public static string TrimToFitColumn(this string text)
        => text == null ? "\"\"" : $"\"{text.TrimToLength(30)}\"";
    
    public static string TrimToLength(this string text, int length)
        => text.Length > length ? $"{text[..(length - 2)]}..." : text;
    
    public static object FormatIfBool(this object val) 
        => val is bool b ? b.ToString().ToLowerInvariant() : val; 
}