using ConfigCat.Cli.Models.Api;
using ConfigCat.Cli.Options;
using ConfigCat.Cli.Services;
using ConfigCat.Cli.Services.Api;
using ConfigCat.Cli.Services.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands.PermissionGroups;

internal class PermissionGroupEnvironmentAccess
{
    private readonly IPermissionGroupClient permissionGroupClient;
    private readonly IWorkspaceLoader workspaceLoader;
    private readonly IEnvironmentClient environmentClient;
    private readonly IPrompt prompt;

    public PermissionGroupEnvironmentAccess(IPermissionGroupClient permissionGroupClient,
        IWorkspaceLoader workspaceLoader,
        IEnvironmentClient environmentClient,
        IPrompt prompt)
    {
        this.permissionGroupClient = permissionGroupClient;
        this.workspaceLoader = workspaceLoader;
        this.environmentClient = environmentClient;
        this.prompt = prompt;
    }

    public async Task<int> UpdatePermissionGroupEnvironmentAccessesAsync(long? permissionGroupId, string accessType, 
        string newEnvironmentAccessType, EnvironmentSpecificAccess[] environmentSpecificAccessTypes, string defaultAccessTypeWhenNotSet, CancellationToken token)
    {
        var permissionGroup = permissionGroupId == null
            ? await this.workspaceLoader.LoadPermissionGroupAsync(token)
            : await this.permissionGroupClient.GetPermissionGroupAsync(permissionGroupId.Value, token);

        if (accessType.IsEmpty() || !Constants.AccessTypes.Keys.Contains(accessType, StringComparer.OrdinalIgnoreCase))
            accessType = (await this.prompt.ChooseFromListAsync("Choose access type", Constants.AccessTypes.ToList(), t => t.Value, token,
                new KeyValuePair<string, string>(permissionGroup.AccessType, Constants.AccessTypes[permissionGroup.AccessType]))).Key;

        if (!accessType.IsEmptyOrEquals(permissionGroup.AccessType))
            permissionGroup.AccessType = accessType;

        if (permissionGroup.AccessType.Equals("custom", StringComparison.OrdinalIgnoreCase))
        {
            if (newEnvironmentAccessType.IsEmpty() && !defaultAccessTypeWhenNotSet.IsEmpty())
            {
                newEnvironmentAccessType = defaultAccessTypeWhenNotSet;
            }
            
            if (newEnvironmentAccessType.IsEmpty() || !Constants.EnvironmentAccessTypes.Keys.Contains(newEnvironmentAccessType, StringComparer.OrdinalIgnoreCase))
                newEnvironmentAccessType = (await this.prompt.ChooseFromListAsync(
                    "Choose access type for newly created environments",
                    Constants.EnvironmentAccessTypes.ToList(),
                    t => t.Value,
                    token,
                    new KeyValuePair<string, string>(permissionGroup.NewEnvironmentAccessType, Constants.EnvironmentAccessTypes[permissionGroup.NewEnvironmentAccessType]))).Key;

            if (!newEnvironmentAccessType.IsEmptyOrEquals(permissionGroup.NewEnvironmentAccessType))
                permissionGroup.NewEnvironmentAccessType = newEnvironmentAccessType;

            if ((environmentSpecificAccessTypes == null || environmentSpecificAccessTypes.Length == 0) && defaultAccessTypeWhenNotSet.IsEmpty())
            {
                var environments = await this.environmentClient.GetEnvironmentsAsync(permissionGroup.Product.ProductId, token);
                foreach (var environment in environments)
                {
                    var existing = permissionGroup.EnvironmentAccesses?.FirstOrDefault(e => e.EnvironmentId == environment.EnvironmentId);
                    var result = await this.prompt.ChooseFromListAsync($"Choose access type for {environment.Name} environment", Constants.EnvironmentAccessTypes.ToList(), e => e.Value, token,
                        existing == null ? default : new KeyValuePair<string, string>(existing.EnvironmentAccessType, Constants.EnvironmentAccessTypes[existing.EnvironmentAccessType]));

                    if (existing != null)
                        existing.EnvironmentAccessType = result.Key ?? existing.EnvironmentAccessType;
                    else
                        permissionGroup.EnvironmentAccesses.Add(new EnvironmentAccessModel
                        {
                            EnvironmentId = environment.EnvironmentId,
                            EnvironmentAccessType = result.Key
                        });
                }
            }
            else
            {
                if (!defaultAccessTypeWhenNotSet.IsEmpty())
                {
                    var environments = await this.environmentClient.GetEnvironmentsAsync(permissionGroup.Product.ProductId, token);
                    foreach (var environment in environments)
                    {
                        var existing =
                            permissionGroup.EnvironmentAccesses.FirstOrDefault(e =>
                                e.EnvironmentId == environment.EnvironmentId);
                        if (existing != null)
                            existing.EnvironmentAccessType = defaultAccessTypeWhenNotSet;
                        else
                            permissionGroup.EnvironmentAccesses.Add(new EnvironmentAccessModel
                            {
                                EnvironmentId = environment.EnvironmentId,
                                EnvironmentAccessType = defaultAccessTypeWhenNotSet
                            });
                    }
                }
                
                if (environmentSpecificAccessTypes != null)
                {
                    foreach (var envAccess in environmentSpecificAccessTypes)
                    {
                        var existing =
                            permissionGroup.EnvironmentAccesses.FirstOrDefault(e =>
                                e.EnvironmentId == envAccess.EnvironmentId);
                        if (existing != null)
                            existing.EnvironmentAccessType = envAccess.AccessType;
                        else
                            permissionGroup.EnvironmentAccesses.Add(new EnvironmentAccessModel
                            {
                                EnvironmentId = envAccess.EnvironmentId,
                                EnvironmentAccessType = envAccess.AccessType
                            });
                    }
                }
            }
        }

        await this.permissionGroupClient.UpdatePermissionGroupAsync(permissionGroup.PermissionGroupId, permissionGroup, token);
        return ExitCodes.Ok;
    }
}