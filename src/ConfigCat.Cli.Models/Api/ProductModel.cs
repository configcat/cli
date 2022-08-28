namespace ConfigCat.Cli.Models.Api;

public class ProductModel
{
    public OrganizationModel Organization { get; set; }

    public string ProductId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int Order { get; set; }
}