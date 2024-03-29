using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ConfigCat.Cli.Services;

public static class Constants
{
    public const string DefaultApiHost = "api.configcat.com";

    public const string ApiHostEnvironmentVariableName = "CONFIGCAT_API_HOST";

    public const string ApiUserNameEnvironmentVariableName = "CONFIGCAT_API_USER";

    public const string ApiPasswordEnvironmentVariableName = "CONFIGCAT_API_PASS";

    public static readonly string ConfigFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".configcat", "cli.json");

    public static readonly Dictionary<string, string> ComparatorTypes = new()
    {
        { "sensitiveIsOneOf", "IS ONE OF (hashed)" },
        { "sensitiveIsNotOneOf", "IS NOT ONE OF (hashed)" },
        { "isOneOf", "IS ONE OF" },
        { "isNotOneOf", "IS NOT ONE OF" },
        { "contains", "CONTAINS" },
        { "doesNotContain", "DOES NOT CONTAIN" },
        { "semVerIsOneOf", "IS ONE OF (semver)" },
        { "semVerIsNotOneOf", "IS NOT ONE OF (semver)" },
        { "semVerLess", "< (semver)" },
        { "semVerLessOrEquals", "<= (semver)" },
        { "semVerGreater", "> (semver)" },
        { "semVerGreaterOrEquals", ">= (semver)" },
        { "numberEquals", "= (number)" },
        { "numberDoesNotEqual", "<> (number)" },
        { "numberLess", "< (number)" },
        { "numberLessOrEquals", "<= (number)" },
        { "numberGreater", "> (number)" },
        { "numberGreaterOrEquals", ">= (number)" },
    };
    
    public static readonly Dictionary<string, string> UserComparatorTypes = new()
    {
        { "sensitiveIsOneOf", "IS ONE OF (hashed)" },
        { "sensitiveIsNotOneOf", "IS NOT ONE OF (hashed)" },
        { "isOneOf", "IS ONE OF" },
        { "isNotOneOf", "IS NOT ONE OF" },
        { "containsAnyOf", "CONTAINS ANY OF" },
        { "doesNotContainAnyOf", "NOT CONTAINS ANY OF" },
        { "semVerIsOneOf", "IS ONE OF (semver)" },
        { "semVerIsNotOneOf", "IS NOT ONE OF (semver)" },
        { "semVerLess", "< (semver)" },
        { "semVerLessOrEquals", "<= (semver)" },
        { "semVerGreater", "> (semver)" },
        { "semVerGreaterOrEquals", ">= (semver)" },
        { "numberEquals", "= (number)" },
        { "numberDoesNotEqual", "<> (number)" },
        { "numberLess", "< (number)" },
        { "numberLessOrEquals", "<= (number)" },
        { "numberGreater", "> (number)" },
        { "dateTimeBefore", ">= (number)" },
        { "dateTimeAfter", ">= (number)" },
        { "sensitiveTextEquals", "EQUALS (hashed)" },
        { "sensitiveTextDoesNotEqual", "NOT EQUALS (hashed)" },
        { "sensitiveTextStartsWithAnyOf", "STARTS WITH ANY OF (hashed)" },
        { "sensitiveTextNotStartsWithAnyOf", "NOT STARTS WITH ANY OF (hashed)" },
        { "sensitiveTextEndsWithAnyOf", "ENDS WITH ANY OF (hashed)" },
        { "sensitiveTextNotEndsWithAnyOf", "NOT ENDS WITH ANY OF (hashed)" },
        { "sensitiveArrayContainsAnyOf", "ARRAY CONTAINS ANY OF (hashed)" },
        { "sensitiveArrayDoesNotContainAnyOf", "ARRAY NOT CONTAINS ANY OF (hashed)" },
        { "textEquals", "EQUALS" },
        { "textDoesNotEqual", "NOT EQUALS" },
        { "textStartsWithAnyOf", "STARTS WITH ANY OF" },
        { "textNotStartsWithAnyOf", "NOT STARTS WITH ANY OF" },
        { "textEndsWithAnyOf", "ENDS WITH ANY OF" },
        { "textNotEndsWithAnyOf", "NOT ENDS WITH ANY OF" },
        { "arrayContainsAnyOf", "ARRAY CONTAINS ANY OF" },
        { "arrayDoesNotContainAnyOf", "ARRAY NOT CONTAINS ANY OF" },
    };

    public static readonly Dictionary<string, string> SegmentComparatorTypes = new()
    {
        { "isIn", "IS IN SEGMENT" },
        { "isNotIn", "IS NOT IN SEGMENT" },
    };
    
    public static readonly Dictionary<string, string> PrerequisiteComparatorTypes = new()
    {
        { "equals", "EQUALS" },
        { "doesNotEqual", "NOT EQUALS" },
    };

    public static readonly Dictionary<string, string> AccessTypes = new()
    {
        { "readOnly", "Read-only" },
        { "full", "Read/Write" },
        { "custom", "Environment specific" },
    };

    public static readonly Dictionary<string, string> EnvironmentAccessTypes = new()
    {
        { "full", "Read/Write" },
        { "readOnly", "Read-only" },
        { "none", "No" },
    };

    public static readonly List<string> Permissions =
    [
        "Manage Members and Permission Groups",
        "Create, edit, and reorder Configs",
        "Delete Configs",
        "Create, edit and reorder Environments",
        "Delete Environments",
        "Create, rename, reorder Feature Flags and change their description",
        "Add, and remove Tags from Feature Flags",
        "Delete Feature Flags",
        "Create, rename Tags and change their color",
        "Delete Tags",
        "Create, update, and delete Webhooks",
        "Export (download), and import (upload) Configs, Environments, and Feature Flags",
        "Access, and change Product preferences",
        "Connect, and disconnect 3rd party integrations",
        "View the SDK key, and the code examples",
        "Add, and remove SDK keys",
        "View the config.json download statistics",
        "View the Product level Audit Log about who changed what in the Product",
        "Create, and edit Segments",
        "Delete Segments"
    ];

    public static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static readonly JsonSerializerOptions PrettyFormattedCamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public const int MaxCharCountPerLine = 1000;
}

public static class ExitCodes
{
    public const int Ok = 0;
    public const int Error = 1;
}

public static class EvalVersion
{
    public static readonly string[] Collection =
    [
        V1,
        V2
    ];

    public const string V1 = "v1";
    public const string V2 = "v2";
}

public static class SettingTypes
{
    public static readonly string[] Collection =
    [
        Boolean,
        String,
        Int,
        Double
    ];

    public const string Boolean = "boolean";
    public const string String = "string";
    public const string Int = "int";
    public const string Double = "double";
}