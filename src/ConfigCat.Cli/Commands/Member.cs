using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands;

internal class Member
{
    private readonly IMemberClient memberClient;
    private readonly IOrganizationClient organizationClient;
    private readonly IWorkspaceLoader workspaceLoader;
    private readonly IPrompt prompt;
    private readonly IOutput output;

    public Member(IMemberClient memberClient,
        IOrganizationClient organizationClient,
        IWorkspaceLoader workspaceLoader,
        IPrompt prompt,
        IOutput output)
    {
        this.memberClient = memberClient;
        this.organizationClient = organizationClient;
        this.workspaceLoader = workspaceLoader;
        this.prompt = prompt;
        this.output = output;
    }

    public async Task<int> ListOrganizationMembersAsync(string organizationId, bool json, CancellationToken token)
    {
        var members = new List<Models.Api.Member>();
        if (!organizationId.IsEmpty())
            members.AddRange(await this.memberClient.GetOrganizationMembersAsync(organizationId, token));
        else
        {
            var organizations = await this.organizationClient.GetOrganizationsAsync(token);
            foreach (var organization in organizations)
                members.AddRange(await this.memberClient.GetOrganizationMembersAsync(organization.OrganizationId, token));
        }

        if (json)
        {
            this.output.RenderJson(members);
            return ExitCodes.Ok;
        }

        this.output.RenderTable(members);
        return ExitCodes.Ok;
    }
    
    public async Task<int> ListProductMembersAsync(string productId, bool json, CancellationToken token)
    {
        var members = new List<Models.Api.Member>();
        if (productId.IsEmpty())
        {
            var product = await this.workspaceLoader.LoadProductAsync(token);
            members.AddRange(await this.memberClient.GetProductMembersAsync(product.ProductId, token));
        }
        else
        {
            members.AddRange(await this.memberClient.GetProductMembersAsync(productId, token));
        }

        if (json)
        {
            this.output.RenderJson(members);
            return ExitCodes.Ok;
        }

        this.output.RenderTable(members);
        return ExitCodes.Ok;
    }
}