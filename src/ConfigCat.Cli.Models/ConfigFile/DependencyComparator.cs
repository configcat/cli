using System.ComponentModel.DataAnnotations;

namespace ConfigCat.Cli.Models.ConfigFile
{
    public enum DependencyComparator : byte
    {
        [Display(Name = "EQUALS")]
        Equals = 0,

        [Display(Name = "NOT EQUALS")]
        DoesNotEqual = 1
    }
}