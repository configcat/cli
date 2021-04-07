using ConfigCat.Cli.Api.Product;

namespace ConfigCat.Cli.Api.Config
{
    class ConfigModel
    {
        public ProductModel Product { get; set; }

        public string ConfigId { get; set; }

        public string Name { get; set; }
    }
}
