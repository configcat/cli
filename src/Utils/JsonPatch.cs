using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;

namespace ConfigCat.Cli.Utils
{
    static class JsonPatch
    {
        private const char PathSeparator = '/';
        private const char Dash = '-';

        public static JsonPatchDocument GenerateDocument<T>(T original, T modified)
        {
            var originalJson = JsonSerializer.Serialize(original, Constants.CamelCaseOptions);
            var modifiedJson = JsonSerializer.Serialize(modified, Constants.CamelCaseOptions);
            using var originalElement = JsonDocument.Parse(originalJson);
            using var modifiedElement = JsonDocument.Parse(modifiedJson);

            var document = new JsonPatchDocument();
            WalkOnProperties(originalElement.RootElement, modifiedElement.RootElement, "/", document);

            return document;
        }

        private static void WalkOnProperties(JsonElement original, JsonElement modified, string path, JsonPatchDocument document)
        {
            var originalProperties = original.EnumerateObject();
            var modifiedProperties = modified.EnumerateObject();

            foreach (var modifiedProperty in modifiedProperties.Intersect(originalProperties, new JsonPropertyEqualityComparer()))
            {
                var originalProperty = originalProperties.First(p => p.Name == modifiedProperty.Name);

                if (!originalProperty.Value.GetRawText().Equals(modifiedProperty.Value.GetRawText()))
                    HandlePropertyChange(originalProperty, modifiedProperty, path, document);
            }
        }

        private static void HandlePropertyChange(JsonProperty original, JsonProperty modified, string path, JsonPatchDocument document)
        {
            if(modified.Value.ValueKind == JsonValueKind.Null)
                return;

            if (original.Value.ValueKind == JsonValueKind.Object)
                WalkOnProperties(original.Value, modified.Value, path + modified.Name + PathSeparator, document);
            else if (original.Value.ValueKind == JsonValueKind.Array)
                HandleArray(original.Value, modified.Value, path + modified.Name, document);
            else
                document.Replace(path + modified.Name, JsonSerializer.Deserialize<object>(modified.Value.GetRawText(), Constants.CamelCaseOptions));
        }

        private static void HandleArray(JsonElement original, JsonElement modified, string path, JsonPatchDocument document)
        {
            var originalItems = original.EnumerateArray().ToArray();
            var modifiedItems = modified.EnumerateArray().ToArray();

            for (int i = 0; i < originalItems.Length; i++)
            {
                if (modifiedItems.Length - 1 < i)
                    document.Remove(path + PathSeparator + i);
                else
                {
                    if (originalItems[i].ValueKind == JsonValueKind.Object && modifiedItems[i].ValueKind == JsonValueKind.Object)
                        WalkOnProperties(originalItems[i], modifiedItems[i], path, document);
                    else if (!originalItems[i].GetRawText().Equals(modifiedItems[i].GetRawText()))
                        document.Replace(path + PathSeparator + i, 
                            JsonSerializer.Deserialize<object>(modifiedItems[i].GetRawText(), Constants.CamelCaseOptions));
                }
            }

            for (int i = originalItems.Length; i < modifiedItems.Length; i++)
                document.Add(path + PathSeparator + Dash, 
                    JsonSerializer.Deserialize<object>(modifiedItems[i].GetRawText(), Constants.CamelCaseOptions));
        }
    }

    class JsonPatchDocument
    {
        public List<JsonPatchOperation> Operations { get; } = new List<JsonPatchOperation>();

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

    class JsonPatchOperation
    {
        public object Value { get; set; }

        public string Path { get; set; }

        public string Op { get; set; }

        public string From { get; set; }
    }

    class JsonPropertyEqualityComparer : IEqualityComparer<JsonProperty>
    {
        public bool Equals([AllowNull] JsonProperty x, [AllowNull] JsonProperty y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] JsonProperty obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
