namespace ConfigCat.Cli.Services.Json;

public class JsonPatchOperation
{
    public object Value { get; set; }

    public string Path { get; set; }

    public string Op { get; set; }

    public string From { get; set; }
}