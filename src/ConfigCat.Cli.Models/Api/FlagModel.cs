using System.Collections.Generic;
using System.Linq;

namespace ConfigCat.Cli.Models.Api;

public class FlagModel
{
    public string ConfigId { get; set; }

    public string ConfigName { get; set; }

    public int SettingId { get; set; }

    public string Key { get; set; }

    public string Name { get; set; }

    public string Hint { get; set; }

    public string SettingType { get; set; }

    public string OwnerUserFullName { get; set; }

    public string OwnerUserEmail { get; set; }

    public List<TagModel> Tags { get; set; }

    public List<string> Aliases { get; set; } = new List<string>();

    public UpdateFlagModel ToUpdateModel() =>
        new()
        {
            Name = this.Name,
            Hint = this.Hint,
            TagIds = this.Tags?.Select(t => t.TagId)
        };
}

public class DeletedFlagModel : FlagModel { }