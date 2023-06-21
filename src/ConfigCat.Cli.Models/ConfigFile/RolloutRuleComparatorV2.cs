using System;
using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    public enum RolloutRuleComparatorV2
    {
        /* [Display(Name = "IS ONE OF")]
        IsOneOf = 0,

        [Display(Name = "IS NOT ONE OF")]
        IsNotOneOf = 1, */

        [Display(Name = "CONTAINS ANY OF")]
        Contains = 2,

        [Display(Name = "NOT CONTAINS ANY OF")]
        DoesNotContain = 3,


        [Display(Name = "SEMVER IS ONE OF")]
        SemVerIsOneOf = 4,

        [Display(Name = "SEMVER IS NOT ONE OF")]
        SemVerIsNotOneOf = 5,

        [Display(Name = "SEMVER LESS THAN")]
        SemVerLess = 6,

        [Display(Name = "SEMVER LESS THAN OR EQUALS TO")]
        SemVerLessOrEquals = 7,

        [Display(Name = "SEMVER GREATER THAN")]
        SemVerGreater = 8,

        [Display(Name = "SEMVER GREATER THAN OR EQUALS TO")]
        SemVerGreaterOrEquals = 9,


        [Display(Name = "NUMBER EQUALS TO")]
        NumberEquals = 10,

        [Display(Name = "NUMBER NOT EQUALS TO")]
        NumberDoesNotEqual = 11,

        [Display(Name = "NUMBER LESS")]
        NumberLess = 12,

        [Display(Name = "NUMBER LESS THAN OR EQUALS TO")]
        NumberLessOrEquals = 13,

        [Display(Name = "NUMBER GREATER THAN")]
        NumberGreater = 14,

        [Display(Name = "NUMBER GREATER THAN OR EQUALS TO")]
        NumberGreaterOrEquals = 15,

        [Display(Name = "SENSITIVE IS ONE OF")]
        SensitiveIsOneOf = 16,

        [Display(Name = "SENSITIVE IS NOT ONE OF")]
        SensitiveIsNotOneOf = 17,

        [Display(Name = "DATETIME BEFORE")]
        DateTimeBefore = 18,

        [Display(Name = "DATETIME AFTER")]
        DateTimeAfter = 19,

        [Display(Name = "SENSITIVE TEXT EQUALS TO")]
        SensitiveTextEquals = 20,

        [Display(Name = "SENSITIVE TEXT NOT EQUALS TO")]
        SensitiveTextDoesNotEqual = 21,

        [Display(Name = "SENSITIVE TEXT STARTS WITH ANY OF")]
        SensitiveTextStartsWith = 22,

        [Display(Name = "SENSITIVE TEXT ENDS WITH ANY OF")]
        SensitiveTextEndsWith = 23,

        [Display(Name = "ARRAY CONTAINS")]
        ArrayContains = 24,

        [Display(Name = "ARRAY NOT CONTAINS")]
        ArrayDoesNotContain = 25,
    }
}