namespace ConfigCat.Cli.Models.Api;

public class TagModel
{
    public ProductModel Product { get; set; }

    public int TagId { get; set; }

    public string Name { get; set; }

    public string Color { get; set; }

    public override bool Equals(object obj)
    {
        if (obj is not TagModel model)
            return false;

        return this.TagId.Equals(model.TagId);
    }

    public override int GetHashCode() => this.TagId.GetHashCode();
}