using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Json;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands;

public class Webhook(
    IProductClient productClient,
    IWebhookClient webhookClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
     public async Task<int> ListAllWebhooksAsync(string productId, bool json, CancellationToken token)
    {
        var webhooks = new List<WebhookModel>();
        if (!productId.IsEmpty())
            webhooks.AddRange(await webhookClient.GetWebhooksAsync(productId, token));
        else
        {
            var products = await productClient.GetProductsAsync(token);
            foreach (var product in products)
                webhooks.AddRange(await webhookClient.GetWebhooksAsync(product.ProductId, token));
        }

        if (json)
        {
            output.RenderJson(webhooks);
            return ExitCodes.Ok;
        }

        var itemsToRender = webhooks.Select(w => new
        {
            Id = w.WebhookId,
            Method = w.HttpMethod,
            Url = w.Url.TrimToLength(30),
            Headers = string.Join(", ", w.WebhookHeaders?.Select(wh => wh.Key) ?? []),
            Environment = $"{w.Environment.Name} [{w.Environment.EnvironmentId}]",
            Config = $"{w.Config.Name} [{w.Config.ConfigId}]"
        });
        output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }
     
    public async Task<int> ShowWebhookAsync(int? webhookId, bool json, CancellationToken token)
    {
        var webhook = webhookId is null
            ? await workspaceLoader.LoadWebhookAsync(token)
            : await webhookClient.GetWebhookAsync(webhookId.Value, token);

        if (json)
        {
            output.RenderJson(webhook);
            return ExitCodes.Ok;
        }

        output.WriteDarkGray("URL: ").Write(webhook.Url).WriteLine();
        output.WriteDarkGray("HTTP method: ").Write(webhook.HttpMethod).WriteLine();
        output.WriteDarkGray("HTTP body: ").Write(webhook.Content).WriteLine();
        if (!webhook.WebhookHeaders.IsEmpty())
        {
            output.WriteDarkGray("HTTP headers: ").WriteLine();
            output.RenderTable(webhook.WebhookHeaders.Select(wh => new
            {
                wh.Key,
                Value = wh.IsSecure ? "<secure>" : wh.Value
            }));
        }

        return ExitCodes.Ok;
    }

    public async Task<int> CreateWebhookAsync(string configId, 
        string environmentId,
        string url,
        string httpMethod,
        string content,
        CancellationToken token)
    {
        if (configId.IsEmpty())
            configId = (await workspaceLoader.LoadConfigAsync(token)).ConfigId;
        
        if (environmentId.IsEmpty())
            environmentId = (await workspaceLoader.LoadEnvironmentAsync(token)).EnvironmentId;
        
        if (url.IsEmpty())
            url = await prompt.GetStringAsync("URL", token);

        if (content.IsEmpty())
            content = await prompt.GetStringAsync("HTTP body", token, "");
        
        if (httpMethod.IsEmpty())
            httpMethod = await prompt.ChooseFromListAsync("HTTP method", HttpMethods.Collection.ToList(), a => a, token);

        if (!HttpMethods.Collection.ToList()
                .Contains(httpMethod, StringComparer.OrdinalIgnoreCase))
            throw new ShowHelpException($"Http method must be one of the following: {string.Join('|', HttpMethods.Collection)}");
        
        var result = await webhookClient.CreateWebhookAsync(configId, environmentId, url, httpMethod, content, token);
        
        output.Write(result.WebhookId.ToString());
        return ExitCodes.Ok;
    }

    public async Task<int> DeleteWebhookAsync(int? webhookId, CancellationToken token)
    {
        webhookId ??= (await workspaceLoader.LoadWebhookAsync(token)).WebhookId;

        await webhookClient.DeleteWebhookAsync(webhookId.Value, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateWebhookAsync(int? webhookId, 
        string url,
        string httpMethod,
        string content, 
        CancellationToken token)
    {
        var webhook = webhookId is null
            ? await workspaceLoader.LoadWebhookAsync(token)
            : await webhookClient.GetWebhookAsync(webhookId.Value, token);

        if (webhookId is null)
        {
            if (url.IsEmpty())
                url = await prompt.GetStringAsync("URL", token, webhook.Url);
            
            if (httpMethod.IsEmpty())
                httpMethod = await prompt.GetStringAsync("HTTP method", token, webhook.HttpMethod);
            
            if (content.IsEmpty())
                content = await prompt.GetStringAsync("HTTP body", token, webhook.Content);
            
            if (httpMethod.IsEmpty())
                httpMethod = await prompt.ChooseFromListAsync("HTTP method", HttpMethods.Collection.ToList(), a => a, token, webhook.HttpMethod);
        }

        if (url.IsEmptyOrEquals(webhook.Url) &&
            httpMethod.IsEmptyOrEquals(webhook.HttpMethod) &&
            content.IsEmptyOrEquals(webhook.Content))
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }
        
        var patchDocument = JsonPatch.GenerateDocument(webhook.ToUpdateModel(), new UpdateWebhookModel
        {
            Url = url,
            HttpMethod = httpMethod,
            Content = content
        });

        await webhookClient.UpdateWebhookAsync(webhook.WebhookId, patchDocument.Operations, token);
        return ExitCodes.Ok;
    }
    
    public async Task<int> AddWebhookHeaderAsync(int? webhookId, 
        string key,
        string value,
        bool secure, 
        CancellationToken token)
    {
        var webhook = webhookId is null
            ? await workspaceLoader.LoadWebhookAsync(token)
            : await webhookClient.GetWebhookAsync(webhookId.Value, token);

        if (key.IsEmpty())
            key = await prompt.GetStringAsync("Key", token);
            
        if (value.IsEmpty())
            value = await prompt.GetStringAsync("Value", token);

        var patchDocument = new JsonPatchDocument();
        patchDocument.Operations.Add(new JsonPatchOperation
        {
            Op = "add",
            Path = "/webHookHeaders/-",
            Value = new
            {
                Key = key,
                Value = value,
                IsSecure = secure
            }
        });

        await webhookClient.UpdateWebhookAsync(webhook.WebhookId, patchDocument.Operations, token);
        return ExitCodes.Ok;
    }
    
    public async Task<int> RemoveHeaderAsync(int? webhookId, 
        string key, 
        CancellationToken token)
    {
        var webhook = webhookId is null
            ? await workspaceLoader.LoadWebhookAsync(token)
            : await webhookClient.GetWebhookAsync(webhookId.Value, token);

        if (key.IsEmpty())
            key = await prompt.GetStringAsync("Key", token);

        var item = webhook.WebhookHeaders?.FirstOrDefault(wh => wh.Key == key);

        if (item is null)
        {
            output.WriteNoChange();
            return ExitCodes.Ok;
        }

        var index = webhook.WebhookHeaders.ToList().IndexOf(item);
        
        var patchDocument = new JsonPatchDocument();
        patchDocument.Operations.Add(new JsonPatchOperation
        {
            Op = "remove",
            Path = $"/webHookHeaders/{index}"
        });

        await webhookClient.UpdateWebhookAsync(webhook.WebhookId, patchDocument.Operations, token);
        return ExitCodes.Ok;
    }
}