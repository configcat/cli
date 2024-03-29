using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Exceptions;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands;

internal class Member(
    IMemberClient memberClient,
    IProductClient productClient,
    IPermissionGroupClient permissionGroupClient,
    IWorkspaceLoader workspaceLoader,
    IPrompt prompt,
    IOutput output)
{
    public async Task<int> ListOrganizationMembersAsync(string organizationId, bool json, CancellationToken token)
    {
        OrganizationMembersModel members;
        if (!organizationId.IsEmpty())
            members = await memberClient.GetOrganizationMembersAsync(organizationId, token);
        else
        {
            var organization = await workspaceLoader.LoadOrganizationAsync(token);
            members = await memberClient.GetOrganizationMembersAsync(organization.OrganizationId, token);
        }

        if (json)
        {
            output.RenderJson(members);
            return ExitCodes.Ok;
        }

        var result = members.Admins.Select(m => new
        {
            m.Email,
            m.FullName,
            m.UserId,
            Permission = "Organization Admin"
        }).Concat(members.Members.Select(m => new
        {
            m.Email,
            m.FullName,
            m.UserId,
            Permission = string.Join(", ", m.Permissions.Select(p => $"{p.PermissionGroup.Name} ({p.Product.Name})"))
        }));
        output.RenderTable(result);
        return ExitCodes.Ok;
    }

    public async Task<int> ListProductMembersAsync(string productId, bool json, CancellationToken token)
    {
        var members = new List<ProductMemberModel>();
        if (!productId.IsEmpty())
            members.AddRange(await memberClient.GetProductMembersAsync(productId, token));
        else
        {
            var product = await workspaceLoader.LoadProductAsync(token);
            productId = product.ProductId;
            members.AddRange(await memberClient.GetProductMembersAsync(productId, token));
        }

        var permissionGroups = await permissionGroupClient.GetPermissionGroupsAsync(productId, token);
        var result = members.Select(m => new
        {
            m.Email,
            m.FullName,
            m.UserId,
            PermissionGroup = permissionGroups.FirstOrDefault(pg => pg.PermissionGroupId == m.PermissionGroupId)
        });

        if (json)
        {
            output.RenderJson(result);
            return ExitCodes.Ok;
        }

        output.RenderTable(result.Select(m => new
        {
            m.Email,
            m.FullName,
            m.UserId,
            PermissionGroup = $"{m.PermissionGroup.Name} [{m.PermissionGroup.PermissionGroupId}]"
        }));
        return ExitCodes.Ok;
    }

    public async Task<int> RemoveMemberFromOrganizationAsync(string organizationId, string userId, CancellationToken token)
    {
        if (organizationId.IsEmpty())
            organizationId = (await workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

        if (userId.IsEmpty())
        {
            var membersResult = await memberClient.GetOrganizationMembersAsync(organizationId, token);
            var members = membersResult.Admins.Concat(membersResult.Members);
            userId = (await prompt.ChooseFromListAsync("Choose member", members.ToList(), m => $"{m.FullName} ({m.Email})", token)).UserId;
        }

        await memberClient.RemoveFromOrganizationAsync(organizationId, userId, token);
        return ExitCodes.Ok;
    }

    public async Task<int> InviteMembersAsync(string productId, string[] emails, long? permissionGroupId, CancellationToken token)
    {
        if (emails.Length == 0)
            throw new ShowHelpException("Required argument <emails> is missing.");

        if (productId.IsEmpty())
            productId = (await workspaceLoader.LoadProductAsync(token)).ProductId;

        if (permissionGroupId == null)
        {
            var permissionGroups = await permissionGroupClient.GetPermissionGroupsAsync(productId, token);
            permissionGroupId = (await prompt.ChooseFromListAsync("Choose permission group", permissionGroups.ToList(), pg => pg.Name, token)).PermissionGroupId;
        }

        await memberClient.InviteMemberAsync(productId, new InviteMemberModel { Emails = emails, PermissionGroupId = permissionGroupId }, token);
        return ExitCodes.Ok;
    }

    public async Task<int> AddPermissionsAsync(string organizationId, string userId, long[] permissionGroupIds, CancellationToken token)
    {
        if (organizationId.IsEmpty())
            organizationId = (await workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

        if (userId.IsEmpty())
        {
            var membersResult = await memberClient.GetOrganizationMembersAsync(organizationId, token);
            var members = membersResult.Admins.Concat(membersResult.Members);
            userId = (await prompt.ChooseFromListAsync("Choose member", members.ToList(), m => $"{m.FullName} ({m.Email})", token)).UserId;
        }

        var products = (await productClient.GetProductsAsync(token)).Where(p => p.Organization.OrganizationId == organizationId).ToList();
        var userPermissions = new List<long>();
        foreach (var product in products)
        {
            var productPermissions = await memberClient.GetProductMembersAsync(product.ProductId, token);
            var userPermission = productPermissions.FirstOrDefault(p => p.UserId == userId);
            if (userPermission != null)
                userPermissions.Add(userPermission.PermissionGroupId);
        }


        if (permissionGroupIds == null || !permissionGroupIds.Any())
        {
            var permissionGroups = new List<PermissionGroupModel>();
            foreach (var product in products)
                permissionGroups.AddRange(await permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));

            var existingPermissions = permissionGroups.Where(pg => userPermissions.Contains(pg.PermissionGroupId)).ToList();
            var selected = await prompt.ChooseMultipleFromListAsync("Choose permission groups (only 1 group is allowed per Product)",
                permissionGroups.ToList(), pg => $"{pg.Name} ({pg.Product.Name})", token, existingPermissions);

            if (selected == null)
                throw new ShowHelpException("Required option `--permission-group-ids` is missing.");

            if (selected.Count == 0)
            {
                output.WriteNoChange();
                return ExitCodes.Ok;
            }

            permissionGroupIds = selected.Select(pg => pg.PermissionGroupId).ToArray();
        }

        await memberClient.UpdateMemberAsync(organizationId, userId, new UpdateMembersModel { PermissionGroupIds = permissionGroupIds }, token);
        return ExitCodes.Ok;
    }

    public async Task<int> RemovePermissionsAsync(string organizationId, string userId, long[] permissionGroupIds, CancellationToken token)
    {
        if (organizationId.IsEmpty())
            organizationId = (await workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

        if (userId.IsEmpty())
        {
            var membersResult = await memberClient.GetOrganizationMembersAsync(organizationId, token);
            var members = membersResult.Admins.Concat(membersResult.Members);
            userId = (await prompt.ChooseFromListAsync("Choose member", members.ToList(), m => $"{m.FullName} ({m.Email})", token)).UserId;
        }

        var products = (await productClient.GetProductsAsync(token)).Where(p => p.Organization.OrganizationId == organizationId).ToList();
        var userPermissions = new List<long>();
        foreach (var product in products)
        {
            var productPermissions = await memberClient.GetProductMembersAsync(product.ProductId, token);
            var userPermission = productPermissions.FirstOrDefault(p => p.UserId == userId);
            if (userPermission != null)
                userPermissions.Add(userPermission.PermissionGroupId);
        }

        var permissionGroups = new List<PermissionGroupModel>();
        foreach (var product in products)
            permissionGroups.AddRange(await permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));
        var existingPermissions = permissionGroups.Where(pg => userPermissions.Contains(pg.PermissionGroupId)).ToList();

        if (permissionGroupIds == null || !permissionGroupIds.Any())
        {
            var selected = await prompt.ChooseMultipleFromListAsync("Choose permission groups to remove",
                existingPermissions.ToList(), pg => $"{pg.Name} ({pg.Product.Name})", token);

            if (selected == null)
                throw new ShowHelpException("Required option `--permission-group-ids` is missing.");

            if (selected.Count == 0)
            {
                output.WriteNoChange();
                return ExitCodes.Ok;
            }

            permissionGroupIds = selected.Select(pg => pg.PermissionGroupId).ToArray();
        }

        var permissionGroupsToDelete = permissionGroups.Where(pg => permissionGroupIds.Contains(pg.PermissionGroupId));
        foreach (var pg in permissionGroupsToDelete)
            await memberClient.RemoveFromProductAsync(pg.Product, userId, token);

        return ExitCodes.Ok;
    }
}