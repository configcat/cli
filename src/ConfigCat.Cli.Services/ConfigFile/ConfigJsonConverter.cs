#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using ConfigCat.Cli.Models.ConfigFile;
using ConfigCat.Cli.Models.ConfigFile.V5;
using ConfigCat.Cli.Models.ConfigFile.V6;
using ConfigCat.Cli.Services.Utilities;

namespace ConfigCat.Cli.Services.ConfigFile
{
    public interface IConfigJsonConverter
    {
        ConfigV6 ConvertV5ToV6(ConfigV5 configV5, bool skipSaltIfUnused = false, Func<string, string?>? reverseComparisonValueHash = null, Action<string>? reportWarning = null);
        JsonSerializerOptions CreateSerializerOptionsV5(bool pretty = false);
        JsonSerializerOptions CreateSerializerOptionsV6(bool pretty = false);
    }

    public class ConfigJsonConverter : IConfigJsonConverter
    {
        private readonly ITokenGenerator tokenGenerator;

        public ConfigJsonConverter(ITokenGenerator tokenGenerator)
        {
            this.tokenGenerator = tokenGenerator;
        }

        public ConfigV6 ConvertV5ToV6(ConfigV5 configV5, bool skipSaltIfUnused = false, Func<string, string?>? reverseComparisonValueHash = null, Action<string>? reportWarning = null)
        {
            var configJsonSalt = new Lazy<string>(() => tokenGenerator.GenerateTokenString(32), isThreadSafe: false);

            var baseUrl = configV5.Preferences?.Url is not null ? new Uri(configV5.Preferences.Url) : null;
            var redirect = configV5.Preferences?.RedirectMode;

            var featureFlags = configV5.Settings is { Count: > 0 }
                ? configV5.Settings.ToDictionary(
                    s => s.Key,
                    s => s.Value is not null ? ConvertSetting(s.Value, s.Key) : null)
                : null;

            var needsSalt = configJsonSalt.IsValueCreated || !skipSaltIfUnused;
            var preferences = needsSalt || baseUrl is not null || redirect is not null
                ? new PreferenceV6
                {
                    BaseUrl = baseUrl,
                    Redirect = redirect,
                    Salt = needsSalt ? configJsonSalt.Value : null,
                }
                : null;

            return new ConfigV6
            {
                Preferences = preferences,
                FeatureFlags = featureFlags,
            };

            EvaluationFormulaV6 ConvertSetting(SettingV5 settingV5, string settingKey)
            {
                var settingType = settingV5.SettingType;
                var settingValue = ConvertSettingValue(settingV5.Value, ref settingType);

                var targetingRules = settingV5.RolloutRules is { Length: > 0 }
                    ? settingV5.RolloutRules
                        .OrderBy(r => r.Order)
                        .Select(r => ConvertTargetingRule(r, settingKey, settingType))
                        .ToList()
                    : null;

                var percentageOptions = settingV5.RolloutPercentageItems is { Length: > 0 }
                    ?  settingV5.RolloutPercentageItems
                        .OrderBy(p => p.Order)
                        .Select(p => ConvertPercentageOption(p, settingType))
                        .ToList()
                    : null;

                return new EvaluationFormulaV6
                {
                    SettingType = settingType.Value,
                    Value = settingValue,
                    VariationId = settingV5.VariationId,
                    // PercentageRuleAttribute = Not implemented in the ancient world... 
                    TargetingRules = targetingRules,
                    PercentageOptions = percentageOptions
                };
            }

            static ValueV6 ConvertSettingValue(JsonElement jsonValue, [NotNull] ref SettingType? settingType)
            {
                ValueV6 value;
                (value, settingType) = jsonValue.ValueKind switch
                {
                    JsonValueKind.String when settingType is null or SettingType.String =>
                        (new ValueV6 { StringValue = jsonValue.GetString() }, SettingType.String),
                    JsonValueKind.False or JsonValueKind.True when settingType is null or SettingType.Boolean =>
                        (new ValueV6 { BoolValue = jsonValue.GetBoolean() }, SettingType.Boolean),
                    JsonValueKind.Number when jsonValue.TryGetInt32(out var intValue) && settingType is null or SettingType.Int =>
                        (new ValueV6 { IntValue = intValue }, SettingType.Int),
                    JsonValueKind.Number when jsonValue.TryGetDouble(out var doubleValue) && settingType is null or SettingType.Double =>
                        (new ValueV6 { DoubleValue = doubleValue }, SettingType.Double),
                    _ =>
                        throw new ArgumentException($"Setting value '{jsonValue}' does not match the specified setting type ({settingType}).", nameof(jsonValue))
                };
                return value;
            }

            TargetingRuleV6 ConvertTargetingRule(RolloutRuleV5 ruleV5, string settingKey, SettingType? settingType)
            {
                var servedValue = new ValueAndVariationIdV6
                {
                    Value = ConvertSettingValue(ruleV5.Value, ref settingType),
                    VariationId = ruleV5.VariationId
                };

                return new TargetingRuleV6
                {
                    Conditions = new List<ConditionV6>
                    {
                        new() { ComparisonRule = ConvertComparisonRule(ruleV5, settingKey) },
                    },
                    ServedValue = servedValue
                };
            }

            ComparisonRuleV6 ConvertComparisonRule(RolloutRuleV5 ruleV5, string settingKey)
            {
                // The IsOneOf and IsNotOneOf comparators are obsolete, we just simply convert them to the sensitive parts.
                var comparator = ruleV5.Comparator switch
                {
                    RolloutRuleComparator.IsOneOf => UserComparator.SensitiveIsOneOf,
                    RolloutRuleComparator.IsNotOneOf => UserComparator.SensitiveIsNotOneOf,
                    _ => (UserComparator)ruleV5.Comparator
                };

                var rule = new ComparisonRuleV6
                {
                    ComparisonAttribute = ruleV5.ComparisonAttribute,
                    Comparator = comparator,
                };

                switch (ruleV5.Comparator)
                {
                    case RolloutRuleComparator.IsOneOf:
                    case RolloutRuleComparator.IsNotOneOf:
                        // This conversion is copied from the .Net SDK (we are now basically doing here what the SDKs are doing in config_v5).
                        // I added a .Distinct() here, so we can free up space in some rare cases in the config.json
                        rule.StringListValue =
                            (ruleV5.ComparisonValue ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .Select(t => GetHashedValue(t, configJsonSalt.Value, contextSalt: settingKey))
                                .Distinct()
                                .ToList();
                        break;

                    case RolloutRuleComparator.Contains:
                    case RolloutRuleComparator.DoesNotContain:
                        rule.StringListValue = new List<string> { ruleV5.ComparisonValue };
                        break;

                    case RolloutRuleComparator.SemVerIsOneOf:
                    case RolloutRuleComparator.SemVerIsNotOneOf:
                        // This conversion is copied from the .Net SDK (we are now basically doing here what the SDKs are doing in config_v5).
                        // I added a .Distinct() here, so we can free up space in some rare cases in the config.json
                        rule.StringListValue =
                            (ruleV5.ComparisonValue ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(t => t.Trim())
                                .Distinct()
                                .ToList();
                        break;

                    case RolloutRuleComparator.SemVerLess:
                    case RolloutRuleComparator.SemVerLessOrEquals:
                    case RolloutRuleComparator.SemVerGreater:
                    case RolloutRuleComparator.SemVerGreaterOrEquals:
                        rule.StringValue = ruleV5.ComparisonValue.Trim();
                        break;

                    case RolloutRuleComparator.NumberEquals:
                    case RolloutRuleComparator.NumberDoesNotEqual:
                    case RolloutRuleComparator.NumberLess:
                    case RolloutRuleComparator.NumberLessOrEquals:
                    case RolloutRuleComparator.NumberGreater:
                    case RolloutRuleComparator.NumberGreaterOrEquals:
                        // This parse is copied from the .Net SDK (de are now basically doing here what the SDKs are doing in config_v5)
                        if (double.TryParse(ruleV5.ComparisonValue.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
                        {
                            rule.DoubleValue = doubleValue;
                        }
                        else
                        {
                            reportWarning?.Invoke($"Number comparison value '{ruleV5.ComparisonValue}' in setting '{settingKey}' could not be converted.");
                            rule.StringValue = "(!not converted!) " + ruleV5.ComparisonValue;
                        }
                        break;

                    case RolloutRuleComparator.SensitiveIsOneOf:
                    case RolloutRuleComparator.SensitiveIsNotOneOf:
                        var items =
                            (ruleV5.ComparisonValue ?? string.Empty)
                                .Split(',', StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < items.Length; i++)
                        {
                            ref var item = ref items[i];
                            item = reverseComparisonValueHash?.Invoke(item);
                            if (item is null)
                            {
                                reportWarning?.Invoke($"Hashed comparison value '{item}' in setting '{settingKey}' could not be converted." +
                                    "To convert hashed comparison values, you need to provide a reverse hash map using the `--hash-map` option.");
                                rule.StringValue = $"(!not converted!) " + ruleV5.ComparisonValue;
                                return rule;
                            }
                        }

                        rule.StringListValue = items
                            .Select(t => t.Trim())
                            .Select(t => GetHashedValue(t, configJsonSalt.Value, contextSalt: settingKey))
                            .Distinct()
                            .ToList();
                        break;
                    default:
                        throw new ArgumentException($"Comparator type '{ruleV5.Comparator}' is not supported.", nameof(ruleV5), null);
                }

                return rule;
            }

            static string GetHashedValue(string comparisonValue, string configJsonSalt, string contextSalt)
            {
                return Sha256(comparisonValue + configJsonSalt + contextSalt);
            }

            static PercentageOptionV6 ConvertPercentageOption(RolloutPercentageItem item, SettingType? settingType)
            {
                return new PercentageOptionV6
                {
                    Percentage = (byte)item.Percentage,
                    Value = ConvertSettingValue(item.Value, ref settingType),
                    VariationId = item.VariationId
                };
            }
        }

        private static string Sha256(string text)
        {
            byte[] hashedBytes;
            var textBytes = Encoding.UTF8.GetBytes(text);
            hashedBytes = SHA256.HashData(textBytes);
            return Convert.ToHexString(hashedBytes).ToLowerInvariant();
        }

        public JsonSerializerOptions CreateSerializerOptionsV5(bool pretty = false) => new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = pretty,
        };

        public JsonSerializerOptions CreateSerializerOptionsV6(bool pretty = false) => new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = pretty,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
    }
}
