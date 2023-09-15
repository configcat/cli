using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    /// <summary>
    /// The segment comparison operator used during the evaluation process.
    /// </summary>
    public enum SegmentComparator
    {
        [Display(Name = "IS IN SEGMENT")]
        IsIn,

        [Display(Name = "IS NOT IN SEGMENT")]
        IsNotIn
    }
}