using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Scan;
using ConfigCat.Cli.Services.Rendering;
using ConfigCat.Cli.Services.Scan;
using Moq;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trybot;
using Xunit;

namespace ConfigCat.Cli.Tests;

public class ScanTests
{
    [Fact]
    public async Task Alias()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "test_flag" };
        var flag2 = new FlagModel { Key = "leadershipSurvey" };

        var result = await aliasCollector.CollectAsync(new[] { flag, flag2 }, new FileInfo("alias.txt"), [], CancellationToken.None);

        var aliases = result.FlagAliases.Values.SelectMany(v => v);

        Assert.DoesNotContain("feature", aliases);
        Assert.DoesNotContain("notRelevant", aliases);

        Assert.Contains("cTestFlag", aliases);
        Assert.Contains("CTESTFlAG", aliases);
        Assert.Contains("CTestFlagD", aliases);
        Assert.Contains("CTestFlagF", aliases);
        Assert.Contains("cvTestFlag", aliases);
        Assert.Contains("CMTestFlag", aliases);

        Assert.Contains("jTestFlag", aliases);
        Assert.Contains("JTESTFlAG", aliases);
        Assert.Contains("JTestFlagD", aliases);
        Assert.Contains("JTestFlagF", aliases);

        Assert.Contains("KTESTFlAG", aliases);
        Assert.Contains("KSTESTFlAG", aliases);

        Assert.Contains("OCTESTFlAG", aliases);
        Assert.Contains("OCDTESTFLAG", aliases);
        Assert.Contains("STestFlag", aliases);
        Assert.Contains("SETestFlag", aliases);
        Assert.Contains("SEMTestFlag", aliases);

        Assert.Contains("TSETestFlag", aliases);
        Assert.Contains("TSCTestFlag", aliases);
        Assert.Contains("TSVTestFlag", aliases);
        Assert.Contains("TSCETestFlag", aliases);

        Assert.Contains("GTestFlag", aliases);
        Assert.Contains("GDTestFlag", aliases);
        Assert.Contains("gVTestFlag", aliases);

        Assert.Contains("PTestFlag", aliases);

        Assert.Contains("RTEST_FLAG", aliases);

        Assert.Contains("PHETestFlag", aliases);
        Assert.Contains("PHCTestFlag", aliases);
        Assert.Contains("PHATestFlag", aliases);
    }

    [Fact]
    public async Task Scan()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());
        var scanner = new ReferenceCollector(new BotPolicy<FlagReferenceResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "test_flag", SettingType = "boolean" };
        var file = new FileInfo("refs.txt");

        var result = await aliasCollector.CollectAsync(new[] { flag }, file, [], CancellationToken.None);
        flag.Aliases = result.FlagAliases[flag.Key].ToList();

        var references = await scanner.CollectAsync(new[] { flag }, file, 0, [], CancellationToken.None);

        var referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("wrapper.test_flag", referenceLines);
        Assert.Contains("wrapper.testFlag", referenceLines);
        Assert.Contains("wrapper.TestFlag", referenceLines);
        Assert.Contains("wrapper.TESTFLAG", referenceLines);
                             
        Assert.Contains("wrapper.test_flag_alias", referenceLines);
        Assert.Contains("wrapper.testFlagAlias", referenceLines);
        Assert.Contains("wrapper.TestFlagAlias", referenceLines);
        Assert.Contains("wrapper.TESTFLAGALIAS", referenceLines);
        Assert.Contains("wrapper.TEST_FLAG_ALIAS", referenceLines);
                             
        Assert.Contains("wrapper.get_test_flag()", referenceLines);
        Assert.Contains("wrapper.getTestFlag()", referenceLines);
        Assert.Contains("wrapper.GetTestFlag()", referenceLines);
        Assert.Contains("wrapper.GETTESTFLAG()", referenceLines);
                             
        Assert.Contains("wrapper.get_test_flag_alias()", referenceLines);
        Assert.Contains("wrapper.getTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper.GetTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper.GETTESTFLAGALIAS()", referenceLines);
        Assert.Contains("wrapper.GET_TEST_FLAG_ALIAS()", referenceLines);
                             
        Assert.Contains("wrapper.is_test_flag()", referenceLines);
        Assert.Contains("wrapper.isTestFlag()", referenceLines);
        Assert.Contains("wrapper.IsTestFlag()", referenceLines);
        Assert.Contains("wrapper.ISTESTFLAG()", referenceLines);
                             
        Assert.Contains("wrapper.is_test_flag_alias()", referenceLines);
        Assert.Contains("wrapper.isTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper.IsTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper.ISTESTFLAGALIAS()", referenceLines);
        Assert.Contains("wrapper.IS_TEST_FLAG_ALIAS()", referenceLines);
                             
        Assert.Contains("wrapper.is_test_flag_enabled()", referenceLines);
        Assert.Contains("wrapper.isTestFlagEnable()", referenceLines);
        Assert.Contains("wrapper.IsTestFlagEnabled()", referenceLines);
        Assert.Contains("wrapper.ISTESTFLAGENABLED()", referenceLines);
                             
        Assert.Contains("wrapper.is_test_flag_alias_enabled()", referenceLines);
        Assert.Contains("wrapper.isTestFlagAliasEnabled()", referenceLines);
        Assert.Contains("wrapper.IsTestFlagAliasEnabled()", referenceLines);
        Assert.Contains("wrapper.ISTESTFLAGALIASENABLED()", referenceLines);
        Assert.Contains("wrapper.IS_TEST_FLAG_ALIAS_ENABLED()", referenceLines);

        Assert.Contains("wrapper->test_flag", referenceLines);
        Assert.Contains("wrapper->testFlag", referenceLines);
        Assert.Contains("wrapper->TestFlag", referenceLines);
        Assert.Contains("wrapper->TESTFLAG", referenceLines);
                                    
        Assert.Contains("wrapper->test_flag_alias", referenceLines);
        Assert.Contains("wrapper->testFlagAlias", referenceLines);
        Assert.Contains("wrapper->TestFlagAlias", referenceLines);
        Assert.Contains("wrapper->TESTFLAGALIAS", referenceLines);
        Assert.Contains("wrapper->TEST_FLAG_ALIAS", referenceLines);
                                    
        Assert.Contains("wrapper->get_test_flag()", referenceLines);
        Assert.Contains("wrapper->getTestFlag()", referenceLines);
        Assert.Contains("wrapper->GetTestFlag()", referenceLines);
        Assert.Contains("wrapper->GETTESTFLAG()", referenceLines);
                                    
        Assert.Contains("wrapper->get_test_flag_alias()", referenceLines);
        Assert.Contains("wrapper->getTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper->GetTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper->GETTESTFLAGALIAS()", referenceLines);
        Assert.Contains("wrapper->GET_TEST_FLAG_ALIAS()", referenceLines);
                                   
        Assert.Contains("wrapper->is_test_flag()", referenceLines);
        Assert.Contains("wrapper->isTestFlag()", referenceLines);
        Assert.Contains("wrapper->IsTestFlag()", referenceLines);
        Assert.Contains("wrapper->ISTESTFLAG()", referenceLines);
                                    
        Assert.Contains("wrapper->is_test_flag_alias()", referenceLines);
        Assert.Contains("wrapper->isTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper->IsTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper->ISTESTFLAGALIAS()", referenceLines);
        Assert.Contains("wrapper->IS_TEST_FLAG_ALIAS()", referenceLines);
                                   
        Assert.Contains("wrapper->is_test_flag_enabled()", referenceLines);
        Assert.Contains("wrapper->isTestFlagEnable()", referenceLines);
        Assert.Contains("wrapper->IsTestFlagEnabled()", referenceLines);
        Assert.Contains("wrapper->ISTESTFLAGENABLED()", referenceLines);
                                    
        Assert.Contains("wrapper->is_test_flag_alias_enabled()", referenceLines);
        Assert.Contains("wrapper->isTestFlagAliasEnabled()", referenceLines);
        Assert.Contains("wrapper->IsTestFlagAliasEnabled()", referenceLines);
        Assert.Contains("wrapper->ISTESTFLAGALIASENABLED()", referenceLines);
        Assert.Contains("wrapper->IS_TEST_FLAG_ALIAS_ENABLED()", referenceLines);

        Assert.Contains("wrapper::test_flag", referenceLines);
        Assert.Contains("wrapper::testFlag", referenceLines);
        Assert.Contains("wrapper::TestFlag", referenceLines);
        Assert.Contains("wrapper::TESTFLAG", referenceLines);
                                    
        Assert.Contains("wrapper::test_flag_alias", referenceLines);
        Assert.Contains("wrapper::testFlagAlias", referenceLines);
        Assert.Contains("wrapper::TestFlagAlias", referenceLines);
        Assert.Contains("wrapper::TESTFLAGALIAS", referenceLines);
        Assert.Contains("wrapper::TEST_FLAG_ALIAS", referenceLines);
                                    
        Assert.Contains("wrapper::get_test_flag()", referenceLines);
        Assert.Contains("wrapper::getTestFlag()", referenceLines);
        Assert.Contains("wrapper::GetTestFlag()", referenceLines);
        Assert.Contains("wrapper::GETTESTFLAG()", referenceLines);
                                    
        Assert.Contains("wrapper::get_test_flag_alias()", referenceLines);
        Assert.Contains("wrapper::getTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper::GetTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper::GETTESTFLAGALIAS()", referenceLines);
        Assert.Contains("wrapper::GET_TEST_FLAG_ALIAS()", referenceLines);
                                    
        Assert.Contains("wrapper::is_test_flag()", referenceLines);
        Assert.Contains("wrapper::isTestFlag()", referenceLines);
        Assert.Contains("wrapper::IsTestFlag()", referenceLines);
        Assert.Contains("wrapper::ISTESTFLAG()", referenceLines);
                                    
        Assert.Contains("wrapper::is_test_flag_alias()", referenceLines);
        Assert.Contains("wrapper::isTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper::IsTestFlagAlias()", referenceLines);
        Assert.Contains("wrapper::ISTESTFLAGALIAS()", referenceLines);
        Assert.Contains("wrapper::IS_TEST_FLAG_ALIAS()", referenceLines);
                                    
        Assert.Contains("wrapper::is_test_flag_enabled()", referenceLines);
        Assert.Contains("wrapper::isTestFlagEnable()", referenceLines);
        Assert.Contains("wrapper::IsTestFlagEnabled()", referenceLines);
        Assert.Contains("wrapper::ISTESTFLAGENABLED()", referenceLines);
                                    
        Assert.Contains("wrapper::is_test_flag_alias_enabled()", referenceLines);
        Assert.Contains("wrapper::isTestFlagAliasEnabled()", referenceLines);
        Assert.Contains("wrapper::IsTestFlagAliasEnabled()", referenceLines);
        Assert.Contains("wrapper::ISTESTFLAGALIASENABLED()", referenceLines);
        Assert.Contains("wrapper::IS_TEST_FLAG_ALIAS_ENABLED()", referenceLines);
    }
    
    [Fact]
    public async Task Alias_Custom()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());
        var scanner = new ReferenceCollector(new BotPolicy<FlagReferenceResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "test_flag", SettingType = "boolean" };
        var file = new FileInfo("custom.txt");

        var result = await aliasCollector.CollectAsync(new[] { flag }, file, [@"(\w+) = :CC_KEY"], CancellationToken.None);
        flag.Aliases = result.FlagAliases[flag.Key].ToList();
        
        Assert.Contains("CUS_TEST_FLAG",  flag.Aliases);

        var references = await scanner.CollectAsync(new[] { flag }, file, 0, [], CancellationToken.None);
        var referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("CUS_TEST_FLAG = :test_flag", referenceLines);
        Assert.Contains("Somewhere else refer to CUS_TEST_FLAG", referenceLines);
    }
    
    [Fact]
    public async Task Alias_Custom_Other()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());
        var scanner = new ReferenceCollector(new BotPolicy<FlagReferenceResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "test_flag", SettingType = "boolean" };
        var file = new FileInfo("custom.txt");

        var result = await aliasCollector.CollectAsync(new[] { flag }, file, [@"(\w+) := FLAGS(CC_KEY)"], CancellationToken.None);
        flag.Aliases = result.FlagAliases[flag.Key].ToList();
        
        Assert.Contains("is_test_flag_on",  flag.Aliases);

        var references = await scanner.CollectAsync(new[] { flag }, file, 0, [], CancellationToken.None);
        var referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("let is_test_flag_on := FLAGS('test_flag')", referenceLines);
        Assert.Contains("Reference to is_test_flag_on", referenceLines);
        
        result = await aliasCollector.CollectAsync(new[] { flag }, file, [@"(\w+) = client_wrapper\.get_flag\(:CC_KEY\)"], CancellationToken.None);
        flag.Aliases = result.FlagAliases[flag.Key].ToList();
        
        Assert.Contains("CUS2_TEST_FLAG",  flag.Aliases);

        references = await scanner.CollectAsync(new[] { flag }, file, 0, [], CancellationToken.None);
        referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("CUS2_TEST_FLAG = client_wrapper.get_flag(:test_flag)", referenceLines);
        Assert.Contains("Reference to CUS2_TEST_FLAG", referenceLines);
        
        result = await aliasCollector.CollectAsync(new[] { flag }, file, [@"client_wrapper\.get_flag\(:CC_KEY, (\w+) =>"], CancellationToken.None);
        flag.Aliases = result.FlagAliases[flag.Key].ToList();
        
        Assert.Contains("cust_flag_val",  flag.Aliases);

        references = await scanner.CollectAsync(new[] { flag }, file, 0, [], CancellationToken.None);
        referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("client_wrapper.get_flag(:test_flag, cust_flag_val => {", referenceLines);
        Assert.Contains("Reference to cust_flag_val", referenceLines);
    }
    
    [Fact]
    public async Task Alias_Patterns_Bad()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "another_flag", SettingType = "boolean" };
        var file = new FileInfo("custom.txt");

        var result = await aliasCollector.CollectAsync(new[] { flag }, file, [":CC_KEY"], CancellationToken.None);
        Assert.Empty(result.FlagAliases);
    }
    
    [Fact]
    public async Task Usage_Custom()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());
        var scanner = new ReferenceCollector(new BotPolicy<FlagReferenceResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "test_flag", SettingType = "boolean" };
        var file = new FileInfo("custom.txt");

        var result = await aliasCollector.CollectAsync([flag], file, [@"(\w+) = client_wrapper\.get_flag\(:CC_KEY\)"], CancellationToken.None);
        flag.Aliases = result.FlagAliases[flag.Key].ToList();
        
        Assert.Contains("is_test_flag_on",  flag.Aliases);

        var references = await scanner.CollectAsync(new[] { flag }, file, 0, [@"get_flag\(:CC_KEY"], CancellationToken.None);
        var referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("let is_test_flag_on := FLAGS('test_flag')", referenceLines);
        Assert.Contains("Reference to is_test_flag_on", referenceLines);
        Assert.Contains("CUS2_TEST_FLAG = client_wrapper.get_flag(:test_flag)", referenceLines);
        Assert.Contains("client_wrapper.get_flag(:test_flag, cust_flag_val => {", referenceLines);
        Assert.Contains("Reference to CUS2_TEST_FLAG", referenceLines);
    }
    
    [Fact]
    public async Task Usage_Custom_Direct()
    {
        var aliasCollector = new AliasCollector(new BotPolicy<AliasScanResult>(), Mock.Of<IOutput>());
        var scanner = new ReferenceCollector(new BotPolicy<FlagReferenceResult>(), Mock.Of<IOutput>());

        var flag = new FlagModel { Key = "test_direct", SettingType = "boolean" };
        var file = new FileInfo("custom.txt");

        var result = await aliasCollector.CollectAsync([flag], file, [], CancellationToken.None);
        Assert.Empty(result.FlagAliases);

        var references = await scanner.CollectAsync(new[] { flag }, file, 0, [":CC_KEY"], CancellationToken.None);
        var referenceLines = references.References.Select(r => r.ReferenceLine.LineText);

        Assert.Contains("if FLAGS.enabled(:test_direct) {", referenceLines);
    }
}