using ConfigCat.Cli.Api.Product;

namespace ConfigCat.Cli.Api.Tag
{
    class TagModel
    {
        public ProductModel Product { get; set; }

        public int TagId { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }
    }
}
