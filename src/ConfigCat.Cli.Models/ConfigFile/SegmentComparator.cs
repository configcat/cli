using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    public enum SegmentComparator
    {
        [Display(Name = "IS IN SEGMENT")]
        IsIn,

        [Display(Name = "IS NOT IN SEGMENT")]
        IsNotIn
    }
}