using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Api;

public class ProductModel
{
    public OrganizationModel Organization { get; set; }

    public string ProductId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Order { get; set; }
}

public class ProductPreferencesModel
{
    public bool ReasonRequired { get; set; }

    public string KeyGenerationMode { get; set; }

    public bool ShowVariationId { get; set; }

    public bool MandatorySettingHint { get; set; }

    public IEnumerable<ReasonRequiredEnvironmentModel> ReasonRequiredEnvironments { get; set; }
}

public class ReasonRequiredEnvironmentModel
{
    public string EnvironmentId { get; set; }

    public bool ReasonRequired { get; set; }

    public string EnvironmentName { get; set; }
}
