using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;

namespace ConfigCat.Cli.Commands.PermissionGroups;

internal class PermissionGroup
{
    private readonly IPermissionGroupClient permissionGroupClient;
    private readonly IWorkspaceLoader workspaceLoader;
    private readonly IProductClient productClient;
    private readonly IPrompt prompt;
    private readonly IOutput output;

    public PermissionGroup(IPermissionGroupClient permissionGroupClient,
        IWorkspaceLoader workspaceLoader,
        IProductClient productClient,
        IPrompt prompt,
        IOutput output)
    {
        this.permissionGroupClient = permissionGroupClient;
        this.workspaceLoader = workspaceLoader;
        this.productClient = productClient;
        this.prompt = prompt;
        this.output = output;
    }

    public async Task<int> ListAllPermissionGroupsAsync(string productId, bool json, CancellationToken token)
    {
        var permissionGroups = new List<PermissionGroupModel>();
        if (!productId.IsEmpty())
        {
            var product = await this.productClient.GetProductAsync(productId, token);
            permissionGroups.AddRange(
                await this.permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));
        }
        else
        {
            var products = await this.productClient.GetProductsAsync(token);
            foreach (var product in products)
                permissionGroups.AddRange(
                    await this.permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));
        }

        if (json)
        {
            this.output.RenderJson(permissionGroups);
            return ExitCodes.Ok;
        }

        var itemsToRender = permissionGroups.Select(pg => new
        {
            Id = pg.PermissionGroupId,
            pg.Name,
            Product = $"{pg.Product.Name} [{pg.Product.ProductId}]"
        });
        this.output.RenderTable(itemsToRender);

        return ExitCodes.Ok;
    }

    public async Task<int> CreatePermissionGroupAsync(string productId,
        string name,
        bool? canManageMembers,
        bool? canCreateOrUpdateConfig,
        bool? canDeleteConfig,
        bool? canCreateOrUpdateEnvironment,
        bool? canDeleteEnvironment,
        bool? canCreateOrUpdateSetting,
        bool? canTagSetting,
        bool? canDeleteSetting,
        bool? canCreateOrUpdateTag,
        bool? canDeleteTag,
        bool? canManageWebhook,
        bool? canUseExportImport,
        bool? canManageProductPreferences,
        bool? canManageIntegrations,
        bool? canViewSdkKey,
        bool? canRotateSdkKey,
        bool? canViewProductStatistics,
        bool? canViewProductAuditLog,
        bool? canCreateOrUpdateSegments,
        bool? canDeleteSegments,
        bool? defaultWhenNotSet,
        CancellationToken token)
    {
        if (productId.IsEmpty())
            productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

        if (name.IsEmpty())
            name = await this.prompt.GetStringAsync("Name", token);

        var model = new UpdatePermissionGroupModel
        {
            Name = name,
            CanManageMembers = canManageMembers,
            CanCreateOrUpdateConfig = canCreateOrUpdateConfig,
            CanDeleteConfig = canDeleteConfig,
            CanCreateOrUpdateEnvironment = canCreateOrUpdateEnvironment,
            CanDeleteEnvironment = canDeleteEnvironment,
            CanCreateOrUpdateSetting = canCreateOrUpdateSetting,
            CanTagSetting = canTagSetting,
            CanDeleteSetting = canDeleteSetting,
            CanCreateOrUpdateTag = canCreateOrUpdateTag,
            CanDeleteTag = canDeleteTag,
            CanManageWebhook = canManageWebhook,
            CanUseExportImport = canUseExportImport,
            CanManageProductPreferences = canManageProductPreferences,
            CanManageIntegrations = canManageIntegrations,
            CanViewSdkKey = canViewSdkKey,
            CanRotateSdkKey = canRotateSdkKey,
            CanViewProductStatistics = canViewProductStatistics,
            CanViewProductAuditLog = canViewProductAuditLog,
            CanCreateOrUpdateSegments = canCreateOrUpdateSegments,
            CanDeleteSegments = canDeleteSegments,
        };

        var initial = new PermissionGroupModel(defaultWhenNotSet ?? true);
        initial.UpdateFromUpdateModel(model);

        if (!model.IsAnyPermissionSet())
        {
            var permissions = await this.prompt.ChooseMultipleFromListAsync("Select permissions", Constants.Permissions,
                s => s, token,
                initial.ToSelectedPermissions().ToList());

            if (permissions != null)
                initial.UpdateFromSelectedPermissions(permissions);
        }

        var result = await this.permissionGroupClient.CreatePermissionGroupAsync(productId, initial, token);
        this.output.Write(result.PermissionGroupId.ToString());
        return ExitCodes.Ok;
    }

    public async Task<int> DeletePermissionGroupAsync(long? permissionGroupId, CancellationToken token)
    {
        permissionGroupId ??= (await this.workspaceLoader.LoadPermissionGroupAsync(token)).PermissionGroupId;

        await this.permissionGroupClient.DeletePermissionGroupAsync(permissionGroupId.Value, token);
        return ExitCodes.Ok;
    }

    public async Task<int> UpdatePermissionGroupAsync(long? permissionGroupId, 
        string name,
        bool? canManageMembers,
        bool? canCreateOrUpdateConfig,
        bool? canDeleteConfig,
        bool? canCreateOrUpdateEnvironment,
        bool? canDeleteEnvironment,
        bool? canCreateOrUpdateSetting,
        bool? canTagSetting,
        bool? canDeleteSetting,
        bool? canCreateOrUpdateTag,
        bool? canDeleteTag,
        bool? canManageWebhook,
        bool? canUseExportImport,
        bool? canManageProductPreferences,
        bool? canManageIntegrations,
        bool? canViewSdkKey,
        bool? canRotateSdkKey,
        bool? canViewProductStatistics,
        bool? canViewProductAuditLog,
        bool? canCreateOrUpdateSegments,
        bool? canDeleteSegments,
        CancellationToken token)
    {
        var permissionGroup = permissionGroupId == null
            ? await this.workspaceLoader.LoadPermissionGroupAsync(token)
            : await this.permissionGroupClient.GetPermissionGroupAsync(permissionGroupId.Value, token);

        if (name.IsEmpty())
            name = await this.prompt.GetStringAsync("Name", token, permissionGroup.Name);

        var model = new UpdatePermissionGroupModel
        {
            Name = name,
            CanManageMembers = canManageMembers,
            CanCreateOrUpdateConfig = canCreateOrUpdateConfig,
            CanDeleteConfig = canDeleteConfig,
            CanCreateOrUpdateEnvironment = canCreateOrUpdateEnvironment,
            CanDeleteEnvironment = canDeleteEnvironment,
            CanCreateOrUpdateSetting = canCreateOrUpdateSetting,
            CanTagSetting = canTagSetting,
            CanDeleteSetting = canDeleteSetting,
            CanCreateOrUpdateTag = canCreateOrUpdateTag,
            CanDeleteTag = canDeleteTag,
            CanManageWebhook = canManageWebhook,
            CanUseExportImport = canUseExportImport,
            CanManageProductPreferences = canManageProductPreferences,
            CanManageIntegrations = canManageIntegrations,
            CanViewSdkKey = canViewSdkKey,
            CanRotateSdkKey = canRotateSdkKey,
            CanViewProductStatistics = canViewProductStatistics,
            CanViewProductAuditLog = canViewProductAuditLog,
            CanCreateOrUpdateSegments = canCreateOrUpdateSegments,
            CanDeleteSegments = canDeleteSegments,
        };
        permissionGroup.UpdateFromUpdateModel(model);

        if (!model.IsAnyPermissionSet())
        {
            var permissions = await this.prompt.ChooseMultipleFromListAsync("Select permissions", Constants.Permissions,
                s => s, token,
                permissionGroup.ToSelectedPermissions().ToList());

            if (permissions != null)
                permissionGroup.UpdateFromSelectedPermissions(permissions);
        }

        await this.permissionGroupClient.UpdatePermissionGroupAsync(permissionGroup.PermissionGroupId, permissionGroup,
            token);
        return ExitCodes.Ok;
    }

    public async Task<int> ShowPermissionGroupAsync(long? permissionGroupId, bool json, CancellationToken token)
    {
        var permissionGroup = permissionGroupId == null
            ? await this.workspaceLoader.LoadPermissionGroupAsync(token)
            : await this.permissionGroupClient.GetPermissionGroupAsync(permissionGroupId.Value, token);

        if (json)
        {
            this.output.RenderJson(permissionGroup);
            return ExitCodes.Ok;
        }

        var separatorLength = permissionGroup.Name.Length + permissionGroup.PermissionGroupId.ToString().Length + 9;

        this.output.WriteDarkGray(new string('-', separatorLength));
        this.output.WriteLine().Write(" ");
        this.output.WriteColor($" {permissionGroup.Name} ", ConsoleColor.White, ConsoleColor.DarkGreen);
        this.output.WriteDarkGray($" [{permissionGroup.PermissionGroupId}]").WriteLine();
        this.output.WriteDarkGray(new string('-', separatorLength));

        foreach (var permission in Constants.Permissions)
        {
            this.output.WriteLine()
                .WriteDarkGray($"| ")
                .WriteCyan(permissionGroup.GetPermissionValue(permission) ? "[*]" : "[ ]")
                .Write(" ")
                .Write(permission);
        }

        this.output.WriteLine()
            .WriteDarkGray($"| ");

        var accessTypeName = Constants.AccessTypes.GetValueOrDefault(permissionGroup.AccessType) ??
                             permissionGroup.AccessType.ToUpperInvariant();
        this.output.WriteLine()
            .WriteDarkGray($"| ")
            .WriteCyan(accessTypeName)
            .Write(" access in all environments");

        var newEnvAccessTypeName =
            Constants.EnvironmentAccessTypes.GetValueOrDefault(permissionGroup.NewEnvironmentAccessType) ??
            permissionGroup.NewEnvironmentAccessType.ToUpperInvariant();
        this.output.WriteLine()
            .WriteDarkGray($"| ")
            .WriteCyan(newEnvAccessTypeName)
            .Write(" access in new environments");

        if (permissionGroup.EnvironmentAccesses.Count > 0)
        {
            this.output.WriteLine().WriteLine()
                .WriteDarkGray($"| ")
                .WriteDarkGray("Environment specific permissions:");

            foreach (var environmentAccess in permissionGroup.EnvironmentAccesses)
            {
                var envAccessTypeName =
                    Constants.EnvironmentAccessTypes.GetValueOrDefault(environmentAccess.EnvironmentAccessType) ??
                    environmentAccess.EnvironmentAccessType.ToUpperInvariant();
                this.output.WriteLine()
                    .WriteDarkGray($"|  ")
                    .WriteCyan(envAccessTypeName)
                    .Write(" access in ")
                    .WriteMagenta(environmentAccess.Name);
            }
        }

        this.output.WriteLine()
            .WriteDarkGray(new string('-', separatorLength))
            .WriteLine();

        return ExitCodes.Ok;
    }
}