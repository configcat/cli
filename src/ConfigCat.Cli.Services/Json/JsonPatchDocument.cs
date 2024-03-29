using System.Collections.Generic;

namespace ConfigCat.Cli.Services.Json;

public class JsonPatchDocument
{
    public List<JsonPatchOperation> Operations { get; } = [];

    public void Add(string path, object value) =>
        this.Operations.Add(new JsonPatchOperation
        {
            Op = "add",
            Path = path,
            Value = value
        });

    public void Replace(string path, object value) =>
        this.Operations.Add(new JsonPatchOperation
        {
            Op = "replace",
            Path = path,
            Value = value
        });

    public void Remove(string path) =>
        this.Operations.Add(new JsonPatchOperation
        {
            Op = "remove",
            Path = path
        });

    public void Move(string from, string to) =>
        this.Operations.Add(new JsonPatchOperation
        {
            Op = "move",
            Path = to,
            From = from
        });
}