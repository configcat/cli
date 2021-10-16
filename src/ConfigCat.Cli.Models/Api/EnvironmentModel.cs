namespace ConfigCat.Cli.Models.Api
{
    public class EnvironmentModel
    {
        public ProductModel Product { get; set; }

        public string EnvironmentId { get; set; }

        public string Name { get; set; }

        public string Color { get; set; }

        public string Description { get; set; }
    }
}
