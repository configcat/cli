using System;
using System.Collections.Generic;
using System.CommandLine.Rendering;
using System.IO;
using System.Text.Json;

namespace ConfigCat.Cli.Services
{
    public static class Constants
    {
        public const string DefaultApiHost = "api.configcat.com";

        public const string ApiHostEnvironmentVariableName = "CONFIGCAT_API_HOST";

        public const string ApiUserNameEnvironmentVariableName = "CONFIGCAT_API_USER";

        public const string ApiPasswordEnvironmentVariableName = "CONFIGCAT_API_PASS";

        public static string ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".configcat", "cli.json");

        public static Size MaxSize = new Size(int.MaxValue, int.MaxValue);

        public static Dictionary<string, string> ComparatorTypes = new Dictionary<string, string>
        {
           { "isOneOf",                 "IS ONE OF" },
           { "isNotOneOf",              "IS NOT ONE OF" },
           { "contains",                "CONTAINS" },
           { "doesNotContain",          "DOES NOT CONTAIN" },
           { "semVerIsOneOf",           "IS ONE OF (SemVer)" },
           { "semVerIsNotOneOf",        "IS NOT ONE OF (SemVer)" },
           { "semVerLess",              "< (SemVer)" },
           { "semVerLessOrEquals",      "<= (SemVer)" },
           { "semVerGreater",           "> (SemVer)" },
           { "semVerGreaterOrEquals",   ">= (SemVer)" },
           { "numberEquals",            "= (Number)" },
           { "numberDoesNotEqual",      "<> (Number)" },
           { "numberLess",              "< (Number)" },
           { "numberLessOrEquals",      "<= (Number)" },
           { "numberGreater",           "> (Number)" },
           { "numberGreaterOrEquals",   ">= (Number)" },
           { "sensitiveIsOneOf",        "IS ONE OF (Sensitive)" },
           { "sensitiveIsNotOneOf",     "IS NOT ONE OF (Sensitive)" },
        };

        public static JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public static class ExitCodes
    {
        public static int Ok = 0;
        public static int Error = 1;
    }

    public static class SettingTypes
    {
        public static string[] Collection = new[]
        {
                Boolean,
                String,
                Int,
                Double
            };

        public const string Boolean = "boolean";
        public const string String = "string";
        public const string Int = "int";
        public const string Double = "double";
    }
}
