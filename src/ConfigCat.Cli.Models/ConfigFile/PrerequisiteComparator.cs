using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    /// <summary>
    /// The comparison operator used during the evaluation process.
    /// </summary>
    public enum PrerequisiteComparator : byte
    {
        [Display(Name = "EQUALS")]
        Equals = 0,

        [Display(Name = "NOT EQUALS")]
        DoesNotEqual = 1
    }
}