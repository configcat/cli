using ConfigCat.Cli.Api.Product;

namespace ConfigCat.Cli.Api.Environment
{
    class EnvironmentModel
    {
        public ProductModel Product { get; set; }

        public string EnvironmentId { get; set; }

        public string Name { get; set; }
    }
}
