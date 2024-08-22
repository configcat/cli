using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

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
    
    public DateTime? CreatedAt { get; set; }

    public string CreatorEmail { get; set; }

    public string CreatorFullName { get; set; }

    public List<TagModel> Tags { get; set; }

    [JsonIgnore]
    public List<string> Aliases { get; set; } = [];

    public UpdateFlagModel ToUpdateModel() =>
        new()
        {
            Name = this.Name,
            Hint = this.Hint,
            TagIds = this.Tags?.Select(t => t.TagId).ToArray()
        };
}

public class DeletedFlagModel : FlagModel { }