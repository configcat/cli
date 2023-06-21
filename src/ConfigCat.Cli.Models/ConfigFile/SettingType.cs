using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    public enum SettingType
    {
        [Display(Name = "On/Off Toggle (Boolean)")]
        Boolean = 0,

        [Display(Name = "Text (String)")]
        String = 1,

        [Display(Name = "Whole Number (Integer) - eg. 42")]
        Int = 2,

        [Display(Name = "Decimal Number (Double) - eg. 3.14")]
        Double = 3,
    }
}