using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    public enum RolloutRuleComparator
    {
        [Display(Name = "IS ONE OF")]
        IsOneOf = 0,

        [Display(Name = "IS NOT ONE OF")]
        IsNotOneOf = 1,

        [Display(Name = "CONTAINS")]
        Contains = 2,

        [Display(Name = "DOES NOT CONTAIN")]
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

        [Display(Name = "NUMBER DOES NOT EQUAL TO")]
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
    }
}