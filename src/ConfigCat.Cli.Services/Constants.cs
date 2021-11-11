using System;
using System.Collections.Generic;
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

        public static readonly string ConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".configcat", "cli.json");

        public static readonly Dictionary<string, string> ComparatorTypes = new()
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

        public static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public const int MaxCharCountPerLine = 1000;
    }

    public static class ExitCodes
    {
        public const int Ok = 0;
        public const int Error = 1;
    }

    public static class SettingTypes
    {
        public static readonly string[] Collection = {
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
