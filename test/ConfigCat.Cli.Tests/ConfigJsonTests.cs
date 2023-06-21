#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json;
using ConfigCat.Cli.Models.ConfigFile.V5;
using ConfigCat.Cli.Services.ConfigFile;
using ConfigCat.Cli.Services.Utilities;
using ConfigCat.Cli.Tests.Fakes;
using Xunit;

namespace ConfigCat.Cli.Tests;

public class ConfigJsonTests
{
    [InlineData(
        "{ \"f\": { \"fakeKey\": { \"v\": \"fakeValue\", \"p\": [] ,\"r\": [] } } }",
        false, null,
        "{\"p\":{\"s\":\"GgxGb1115NitZ2XV9RnbyCt8NDs3\\u002BIUA7F5kAFOTsw0=\"},\"f\":{\"fakeKey\":{\"t\":1,\"v\":{\"s\":\"fakeValue\"}}}}")]
    [InlineData(
        "{ \"f\": { \"fakeKey\": { \"v\": \"fakeValue\", \"p\": [] ,\"r\": [] } } }",
        true, null,
        "{\"f\":{\"fakeKey\":{\"t\":1,\"v\":{\"s\":\"fakeValue\"}}}}")]
    [InlineData(
        "{\"p\":{\"u\":\"https://test-cdn-global.configcat.com\",\"r\":0},\"f\":{\"x\":{\"v\":false,\"i\":\"f90da021\",\"t\":0,\"p\":[{\"o\":0,\"v\":true,\"p\":25,\"i\":\"439d3fcb\"},{\"o\":1,\"v\":false,\"p\":75,\"i\":\"f90da021\"}],\"r\":[{\"o\":1,\"a\":\"Country\",\"t\":1,\"c\":\"US,GB\",\"v\":false,\"i\":\"f90da021\"},{\"o\":0,\"a\":\"Country\",\"t\":16,\"c\":\"aa3093554472fd113135bed5b63e12f84c2e9fe8,505a44facaa3e758845f6e101f4e21f9d99acf63\",\"v\":false,\"i\":\"f90da021\"},{\"o\":2,\"a\":\"Identifier\",\"t\":2,\"c\":\"x\",\"v\":false,\"i\":\"f90da021\"},{\"o\":3,\"a\":\"Identifier\",\"t\":4,\"c\":\"1.0.0\",\"v\":false,\"i\":\"f90da021\"},{\"o\":4,\"a\":\"Version\",\"t\":4,\"c\":\"1.0.0,2.0.0\",\"v\":false,\"i\":\"f90da021\"},{\"o\":5,\"a\":\"Version\",\"t\":6,\"c\":\"2.0.0\",\"v\":false,\"i\":\"f90da021\"},{\"o\":6,\"a\":\"Identifier\",\"t\":10,\"c\":\"42\",\"v\":false,\"i\":\"f90da021\"}]}}}",
        true, null,
        "{\"p\":{\"u\":\"https://test-cdn-global.configcat.com\",\"r\":0,\"s\":\"GgxGb1115NitZ2XV9RnbyCt8NDs3\\u002BIUA7F5kAFOTsw0=\"},\"f\":{\"x\":{\"t\":0,\"r\":[{\"c\":[{\"t\":{\"a\":\"Country\",\"c\":16,\"s\":\"(!not converted!) aa3093554472fd113135bed5b63e12f84c2e9fe8,505a44facaa3e758845f6e101f4e21f9d99acf63\"}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Country\",\"c\":17,\"l\":[\"2a8780f9b04c70a13aec9cc32f23ac9168f9b20a3f3e1779b9c67b44f287eef7\",\"ded3af0dff05e83d517fc3129159781cb292f44dc2def941e73d0eacaf30ebe3\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Identifier\",\"c\":2,\"l\":[\"x\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Identifier\",\"c\":4,\"l\":[\"1.0.0\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Version\",\"c\":4,\"l\":[\"1.0.0\",\"2.0.0\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Version\",\"c\":6,\"s\":\"2.0.0\"}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Identifier\",\"c\":10,\"d\":42}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"p\":[{\"p\":25,\"v\":{\"b\":true},\"i\":\"439d3fcb\"},{\"p\":75,\"v\":{\"b\":false},\"i\":\"f90da021\"}]}],\"v\":{\"b\":false},\"i\":\"f90da021\"}}}")]
    [InlineData(
        "{\"p\":{\"u\":\"https://test-cdn-global.configcat.com\",\"r\":0},\"f\":{\"x\":{\"v\":false,\"i\":\"f90da021\",\"t\":0,\"p\":[{\"o\":0,\"v\":true,\"p\":25,\"i\":\"439d3fcb\"},{\"o\":1,\"v\":false,\"p\":75,\"i\":\"f90da021\"}],\"r\":[{\"o\":1,\"a\":\"Country\",\"t\":1,\"c\":\"US,GB\",\"v\":false,\"i\":\"f90da021\"},{\"o\":0,\"a\":\"Country\",\"t\":16,\"c\":\"aa3093554472fd113135bed5b63e12f84c2e9fe8,505a44facaa3e758845f6e101f4e21f9d99acf63\",\"v\":false,\"i\":\"f90da021\"},{\"o\":2,\"a\":\"Identifier\",\"t\":2,\"c\":\"x\",\"v\":false,\"i\":\"f90da021\"},{\"o\":3,\"a\":\"Identifier\",\"t\":4,\"c\":\"1.0.0\",\"v\":false,\"i\":\"f90da021\"},{\"o\":4,\"a\":\"Version\",\"t\":4,\"c\":\"1.0.0,2.0.0\",\"v\":false,\"i\":\"f90da021\"},{\"o\":5,\"a\":\"Version\",\"t\":6,\"c\":\"2.0.0\",\"v\":false,\"i\":\"f90da021\"},{\"o\":6,\"a\":\"Identifier\",\"t\":10,\"c\":\"42\",\"v\":false,\"i\":\"f90da021\"}]}}}",
        true, "{\"aa3093554472fd113135bed5b63e12f84c2e9fe8\":\"US\",\"505a44facaa3e758845f6e101f4e21f9d99acf63\":\"GB\"}",
        "{\"p\":{\"u\":\"https://test-cdn-global.configcat.com\",\"r\":0,\"s\":\"GgxGb1115NitZ2XV9RnbyCt8NDs3\\u002BIUA7F5kAFOTsw0=\"},\"f\":{\"x\":{\"t\":0,\"r\":[{\"c\":[{\"t\":{\"a\":\"Country\",\"c\":16,\"l\":[\"2a8780f9b04c70a13aec9cc32f23ac9168f9b20a3f3e1779b9c67b44f287eef7\",\"ded3af0dff05e83d517fc3129159781cb292f44dc2def941e73d0eacaf30ebe3\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Country\",\"c\":17,\"l\":[\"2a8780f9b04c70a13aec9cc32f23ac9168f9b20a3f3e1779b9c67b44f287eef7\",\"ded3af0dff05e83d517fc3129159781cb292f44dc2def941e73d0eacaf30ebe3\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Identifier\",\"c\":2,\"l\":[\"x\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Identifier\",\"c\":4,\"l\":[\"1.0.0\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Version\",\"c\":4,\"l\":[\"1.0.0\",\"2.0.0\"]}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Version\",\"c\":6,\"s\":\"2.0.0\"}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"c\":[{\"t\":{\"a\":\"Identifier\",\"c\":10,\"d\":42}}],\"s\":{\"v\":{\"b\":false},\"i\":\"f90da021\"}},{\"p\":[{\"p\":25,\"v\":{\"b\":true},\"i\":\"439d3fcb\"},{\"p\":75,\"v\":{\"b\":false},\"i\":\"f90da021\"}]}],\"v\":{\"b\":false},\"i\":\"f90da021\"}}}")]
    [Theory]
    public void ConfigJsonV5ToV6_Conversion_Works(string inputJson, bool skipSaltIfUnused, string? hashMapJson, string expectedOutputJson)
    {
        var rng = new FakeRandomNumberGenerator();
        var tokenGenerator = new TokenGenerator(rng);
        var converter = new ConfigJsonConverter(tokenGenerator);

        Func<string, string?>? reverseComparisonValueHash = null;
        if (!string.IsNullOrEmpty(hashMapJson))
        {
            var comparisonValueHashMap = JsonSerializer.Deserialize<Dictionary<string, string>>(hashMapJson)!;
            reverseComparisonValueHash = hash => comparisonValueHashMap.GetValueOrDefault(hash);
        }

        var configV5 = JsonSerializer.Deserialize<ConfigV5>(inputJson)!;
        var configV6 = converter.ConvertV5ToV6(configV5, skipSaltIfUnused, reverseComparisonValueHash);
        var actualOutput = JsonSerializer.Serialize(configV6, converter.CreateSerializerOptionsV6());

        Assert.Equal(expectedOutputJson, actualOutput);
    }
}