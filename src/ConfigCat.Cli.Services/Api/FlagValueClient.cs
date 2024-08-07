﻿using System;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Models.Configuration;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Trybot;

namespace ConfigCat.Cli.Services.Api;

public interface IFlagValueClient
{
    Task<FlagValueModel> GetValueAsync(int settingId, string environmentId, CancellationToken token);

    Task ReplaceValueAsync(int settingId, string environmentId, string reason, FlagValueModel model, CancellationToken token);

    Task UpdateValueAsync(int settingId, string environmentId, string reason, List<JsonPatchOperation> operations, CancellationToken token);
}

public class FlagValueClient(
    IOutput output,
    CliConfig config,
    IBotPolicy<HttpResponseMessage> botPolicy,
    HttpClient httpClient)
    : ApiClient(output, config, botPolicy, httpClient), IFlagValueClient
{
    public Task<FlagValueModel> GetValueAsync(int settingId, string environmentId, CancellationToken token) =>
        this.GetAsync<FlagValueModel>(HttpMethod.Get, $"v1/environments/{environmentId}/settings/{settingId}/value", token);

    public async Task ReplaceValueAsync(int settingId, string environmentId, string reason, FlagValueModel model, CancellationToken token)
    {
        this.Output.Write($"Updating Flag Value... ");
        await this.SendAsync(HttpMethod.Put, $"v1/environments/{environmentId}/settings/{settingId}/value{(string.IsNullOrWhiteSpace(reason)? string.Empty : $"?reason={Uri.EscapeDataString(reason)}")}", model, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }

    public async Task UpdateValueAsync(int settingId, string environmentId, string reason, List<JsonPatchOperation> operations, CancellationToken token)
    {
        this.Output.Write($"Updating Flag Value... ");
        await this.SendAsync(HttpMethod.Patch, $"v1/environments/{environmentId}/settings/{settingId}/value{(string.IsNullOrWhiteSpace(reason)? string.Empty : $"?reason={Uri.EscapeDataString(reason)}")}", operations, token);
        this.Output.WriteSuccess();
        this.Output.WriteLine();
    }
}