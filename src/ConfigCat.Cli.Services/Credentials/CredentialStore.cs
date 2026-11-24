#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ConfigCat.Cli.Models.Credentials;
using Microsoft.Identity.Client.Extensions.Msal;

namespace ConfigCat.Cli.Services.Credentials;

public interface ICredentialStore
{
    CredentialData? Get();
    void Set(CredentialData value);
    void Clear();
}

public sealed class CredentialStore : ICredentialStore
{
    private const string AppName = "configcat";
    private const string ServiceIdentifier = "com.configcat.cli";
    
    private readonly string lockFilePath;
    private readonly Storage storage;
    
    public CredentialStore()
    {
        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppName);

        Directory.CreateDirectory(cacheDir);
        lockFilePath = Path.Combine(cacheDir, "credential.cache.lockfile");

        var builder = new StorageCreationPropertiesBuilder(
                cacheFileName: "credential.cache",
                cacheDirectory: cacheDir)
            .WithMacKeyChain(
                serviceName: $"{ServiceIdentifier}.service",
                accountName:  $"{ServiceIdentifier}.account")
            .WithLinuxKeyring(
                schemaName: $"{ServiceIdentifier}.credentialcache",
                collection: MsalCacheHelper.LinuxKeyRingDefaultCollection,
                secretLabel: $"{AppName} Stored Credentials",
                attribute1: new KeyValuePair<string, string>("Version", "1"),
                attribute2: new KeyValuePair<string, string>("Product", AppName));

        storage = Storage.Create(builder.Build());
    }
    
    public CredentialData? Get()
    {
        using var fileLock = new CrossPlatLock(lockFilePath);
        var bytes = storage.ReadData();

        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CredentialData>(bytes);
    }

    public void Set(CredentialData value)
    {
        using var fileLock = new CrossPlatLock(lockFilePath);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
        storage.WriteData(bytes);
    }

    public void Clear()
    {
        using var fileLock = new CrossPlatLock(lockFilePath);
        storage.Clear();
    }
}