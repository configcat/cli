using ConfigCat.Cli.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System
{
    public static class SystemExtensions
    {
        public static string AsSlash(this string text) => text.Replace(Path.DirectorySeparatorChar, '/');

        public static string RemoveDashes(this string text) => text.Replace("-", string.Empty).Replace("_", string.Empty);

        public static int GetDigitCount(this int number) => (int)Math.Floor(Math.Log10(Math.Abs(number)) + 1);

        public static bool IsEmpty(this string value) =>
            string.IsNullOrWhiteSpace(value);

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
    }
}