#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Models.Credentials;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Configuration;
using ConfigCat.Cli.Services.Credentials;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;
using Moq;
using Xunit;

namespace ConfigCat.Cli.Tests;

[Collection("Environment variables")]
public class ContextProviderTests
{
    [Fact]
    public async Task MigratesLegacyCredentials_WhenEnvironmentCredentialsAreNotSet()
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, null,
            Constants.ApiPasswordEnvironmentVariableName, null);

        var config = CreateConfig("legacy-user", "legacy-pass");
        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        var credentialStore = new Mock<ICredentialStore>();
        var provider = CreateProvider(storage, credentialStore);

        var context = await provider.GetConfigAsync(CancellationToken.None);
        var expectedContext = new CliContext();
        expectedContext.SetBasicAuth("legacy-user", "legacy-pass");
        
        Assert.Equal(expectedContext.AuthenticationHeader.Scheme, context.AuthenticationHeader.Scheme);
        Assert.Equal(expectedContext.AuthenticationHeader.Parameter, context.AuthenticationHeader.Parameter);
#pragma warning disable CS0612
        Assert.Null(config.Auth.UserName);
        Assert.Null(config.Auth.Password);
#pragma warning restore CS0612
        credentialStore.Verify(x => x.Set(It.Is<CredentialData>(c =>
            c.UserName == "legacy-user" && c.Password == "legacy-pass")), Times.Once);
        storage.Verify(x => x.WriteConfigAsync(config, It.IsAny<CancellationToken>()), Times.Once);
        credentialStore.Verify(x => x.Get(), Times.Never);
    }

    [Fact]
    public async Task ClearsLegacyCredentials_WhenEnvironmentCredentialsAreSet()
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, "env-user",
            Constants.ApiPasswordEnvironmentVariableName, "env-pass");

        var config = CreateConfig("legacy-user", "legacy-pass");
        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        var credentialStore = new Mock<ICredentialStore>();
        var provider = CreateProvider(storage, credentialStore);

        var context = await provider.GetConfigAsync(CancellationToken.None);
        var expectedContext = new CliContext();
        expectedContext.SetBasicAuth("env-user", "env-pass");

        Assert.Equal(expectedContext.AuthenticationHeader.Scheme, context.AuthenticationHeader.Scheme);
        Assert.Equal(expectedContext.AuthenticationHeader.Parameter, context.AuthenticationHeader.Parameter);
#pragma warning disable CS0612
        Assert.Null(config.Auth.UserName);
        Assert.Null(config.Auth.Password);
#pragma warning restore CS0612
        credentialStore.Verify(x => x.Set(It.IsAny<CredentialData>()), Times.Once);
        storage.Verify(x => x.WriteConfigAsync(config, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UsesCredentialStore_WhenConfigHasNoLegacyCredentials()
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, null,
            Constants.ApiPasswordEnvironmentVariableName, null);

        var config = CreateConfig(null, null);
        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        var credentialStore = new Mock<ICredentialStore>();
        credentialStore.Setup(x => x.Get()).Returns(new CredentialData
        {
            UserName = "stored-user",
            Password = "stored-pass"
        });
        var provider = CreateProvider(storage, credentialStore);

        var context = await provider.GetConfigAsync(CancellationToken.None);
        var expectedContext = new CliContext();
        expectedContext.SetBasicAuth("stored-user", "stored-pass");

        Assert.Equal(expectedContext.AuthenticationHeader.Scheme, context.AuthenticationHeader.Scheme);
        Assert.Equal(expectedContext.AuthenticationHeader.Parameter, context.AuthenticationHeader.Parameter);
        credentialStore.Verify(x => x.Get(), Times.Once);
        credentialStore.Verify(x => x.Set(It.IsAny<CredentialData>()), Times.Never);
        storage.Verify(x => x.WriteConfigAsync(It.IsAny<CliConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ThrowsSetupHelp_WhenNoCredentialSourceIsAvailable()
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, null,
            Constants.ApiPasswordEnvironmentVariableName, null);

        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CliConfig?)null);
        var credentialStore = new Mock<ICredentialStore>();
        credentialStore.Setup(x => x.Get()).Returns((CredentialData?)null);
        var provider = CreateProvider(storage, credentialStore);

        await Assert.ThrowsAsync<ShowHelpException>(() =>
            provider.GetConfigAsync(CancellationToken.None));
    }

    [Theory]
    [InlineData("env-user", null)]
    [InlineData(null, "env-pass")]
    public async Task ThrowsSetupHelp_WhenOnlyOneEnvironmentCredentialIsSet(
        string? userName,
        string? password)
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, userName,
            Constants.ApiPasswordEnvironmentVariableName, password);

        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CliConfig?)null);
        var credentialStore = new Mock<ICredentialStore>();
        var provider = CreateProvider(storage, credentialStore);

        await Assert.ThrowsAsync<ShowHelpException>(() =>
            provider.GetConfigAsync(CancellationToken.None));

        credentialStore.Verify(x => x.Get(), Times.Never);
    }

    [Fact]
    public async Task ThrowsSetupHelp_WhenCredentialStoreContainsIncompleteCredentials()
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, null,
            Constants.ApiPasswordEnvironmentVariableName, null);

        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync((CliConfig?)null);
        var credentialStore = new Mock<ICredentialStore>();
        credentialStore.Setup(x => x.Get()).Returns(new CredentialData
        {
            UserName = "stored-user",
            Password = null
        });
        var provider = CreateProvider(storage, credentialStore);

        await Assert.ThrowsAsync<ShowHelpException>(() =>
            provider.GetConfigAsync(CancellationToken.None));
    }

    [Fact]
    public async Task PreservesLegacyPair_WhenOnlyOneEnvironmentCredentialIsSet()
    {
        using var environment = new EnvironmentVariableScope(
            Constants.ApiHostEnvironmentVariableName, null,
            Constants.ApiUserNameEnvironmentVariableName, "env-user",
            Constants.ApiPasswordEnvironmentVariableName, null);

        var config = CreateConfig("legacy-user", "legacy-pass");
        var storage = new Mock<IConfigurationStorage>();
        storage.Setup(x => x.ReadConfigOrDefaultAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        var credentialStore = new Mock<ICredentialStore>();
        var provider = CreateProvider(storage, credentialStore);

        await Assert.ThrowsAsync<ShowHelpException>(() =>
            provider.GetConfigAsync(CancellationToken.None));

        credentialStore.Verify(x => x.Set(It.Is<CredentialData>(c =>
            c.UserName == "legacy-user" && c.Password == "legacy-pass")), Times.Once);
#pragma warning disable CS0612
        Assert.Null(config.Auth.UserName);
        Assert.Null(config.Auth.Password);
#pragma warning restore CS0612
    }

    private static CliConfig CreateConfig(string? userName, string? password)
    {
#pragma warning disable CS0612
        return new CliConfig
        {
            Auth = new Auth
            {
                ApiHost = "api.example.com",
                UserName = userName,
                Password = password
            }
        };
#pragma warning restore CS0612
    }

    private static ContextProvider CreateProvider(
        Mock<IConfigurationStorage> storage,
        Mock<ICredentialStore> credentialStore)
    {
        return new ContextProvider(
            Mock.Of<IOutput>(),
            storage.Object,
            credentialStore.Object);
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly (string Name, string? Value)[] values;

        public EnvironmentVariableScope(
            string hostName, string? host,
            string userName, string? user,
            string passwordName, string? password)
        {
            values =
            [
                (hostName, Environment.GetEnvironmentVariable(hostName)),
                (userName, Environment.GetEnvironmentVariable(userName)),
                (passwordName, Environment.GetEnvironmentVariable(passwordName))
            ];

            Environment.SetEnvironmentVariable(hostName, host);
            Environment.SetEnvironmentVariable(userName, user);
            Environment.SetEnvironmentVariable(passwordName, password);
        }

        public void Dispose()
        {
            foreach (var (name, value) in values)
                Environment.SetEnvironmentVariable(name, value);
        }
    }
}

[CollectionDefinition("Environment variables", DisableParallelization = true)]
public class EnvironmentVariableCollection;
