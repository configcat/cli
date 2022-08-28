using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands;

internal class Environment
{
    private readonly IEnvironmentClient environmentClient;
    private readonly IWorkspaceLoader workspaceLoader;
    private readonly IProductClient productClient;
    private readonly IPrompt prompt;
    private readonly IOutput output;

    public Environment(IEnvironmentClient environmentClient,
        IWorkspaceLoader workspaceLoader,
        IProductClient productClient,
        IPrompt prompt,
        IOutput output)
    {
        this.environmentClient = environmentClient;
        this.workspaceLoader = workspaceLoader;
        this.productClient = productClient;
        this.prompt = prompt;
        this.output = output;
    }

    public async Task<int> ListAllEnvironmentsAsync(string productId, bool json, CancellationToken token)
    {
        var environments = new List<EnvironmentModel>();
        if (!productId.IsEmpty())
            environments.AddRange(await this.environmentClient.GetEnvironmentsAsync(productId, token));
        else
        {
            var products = await this.productClient.GetProductsAsync(token);
            foreach (var product in products)
                environments.AddRange(await this.environmentClient.GetEnvironmentsAsync(product.ProductId, token));
        }

        if (json)
        {
            this.output.RenderJson(environments);
            return ExitCodes.Ok;
        }

        var itemsToRender = environments.Select(e => new
        {
            Id = e.EnvironmentId,
            e.Name,
            Description = e.Description.TrimToFitColumn(),
            e.Color,
            Product = $"{e.Product.Name} [{e.Product.ProductId}]"
        });
        this.output.RenderTable(itemsToRender);
        return ExitCodes.Ok;
    }

    public async Task<int> CreateEnvironmentAsync(string productId, string name, string description, string color, CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

        if (name.IsEmpty())
            name = await this.prompt.GetStringAsync("Name", token);

        if (description.IsEmpty())
            description = await this.prompt.GetStringAsync("Description", token);

        if (color.IsEmpty())
            color = await this.prompt.GetStringAsync("Color", token);

        var result = await this.environmentClient.CreateEnvironmentAsync(productId, name, description, color, token);
        this.output.Write(result.EnvironmentId);

        return ExitCodes.Ok;
    }

    public async Task<int> DeleteEnvironmentAsync(string environmentId, CancellationToken token)
    {
        if (environmentId.IsEmpty())
            environmentId = (await this.workspaceLoader.LoadEnvironmentAsync(token)).EnvironmentId;

        await this.environmentClient.DeleteEnvironmentAsync(environmentId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdateEnvironmentAsync(string environmentId, string name, string description, string color, CancellationToken token)
    {
        var environment = environmentId.IsEmpty()
            ? await this.workspaceLoader.LoadEnvironmentAsync(token)
            : await this.environmentClient.GetEnvironmentAsync(environmentId, token);

        if (name.IsEmpty())
            name = await this.prompt.GetStringAsync("Name", token, environment.Name);

        if (description.IsEmpty())
            description = await this.prompt.GetStringAsync("Description", token, environment.Description);

        if (color.IsEmpty())
            color = await this.prompt.GetStringAsync("Color", token, environment.Color);

        if (name.IsEmptyOrEquals(environment.Name) &&
            description.IsEmptyOrEquals(environment.Description) &&
            color.IsEmptyOrEquals(environment.Color))
        {
            this.output.WriteNoChange();
            return ExitCodes.Ok;
        }

        await this.environmentClient.UpdateEnvironmentAsync(environment.EnvironmentId, name, description, color, token);
        return ExitCodes.Ok;
    }
}