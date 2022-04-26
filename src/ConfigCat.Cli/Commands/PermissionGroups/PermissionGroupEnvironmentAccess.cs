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

namespace ConfigCat.Cli.Commands.PermissionGroups
{
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

        public async Task<int> UpdatePermissionGroupEnvironmentAccessesAsync(long? permissionGroupId, UpdatePermissionGroupEnvironmentAccess arguments, CancellationToken token)
        {
            var permissionGroup = permissionGroupId == null
                ? await this.workspaceLoader.LoadPermissionGroupAsync(token)
                : await this.permissionGroupClient.GetPermissionGroupAsync(permissionGroupId.Value, token);

            if (arguments.AccessType.IsEmpty() || !Constants.AccessTypes.Keys.Contains(arguments.AccessType, StringComparer.OrdinalIgnoreCase))
                arguments.AccessType = (await this.prompt.ChooseFromListAsync("Choose access type", Constants.AccessTypes.ToList(), t => t.Value, token,
                    new KeyValuePair<string, string>(permissionGroup.AccessType, Constants.AccessTypes[permissionGroup.AccessType]))).Key;

            if (!arguments.AccessType.IsEmptyOrEquals(permissionGroup.AccessType))
                permissionGroup.AccessType = arguments.AccessType;

            if (permissionGroup.AccessType.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                if (arguments.NewEnvironmentAccessType.IsEmpty() || !Constants.EnvironmentAccessTypes.Keys.Contains(arguments.NewEnvironmentAccessType, StringComparer.OrdinalIgnoreCase))
                    arguments.NewEnvironmentAccessType = (await this.prompt.ChooseFromListAsync(
                        "Choose access type for newly created environments",
                        Constants.EnvironmentAccessTypes.ToList(),
                        t => t.Value,
                        token,
                        new KeyValuePair<string, string>(permissionGroup.NewEnvironmentAccessType, Constants.EnvironmentAccessTypes[permissionGroup.NewEnvironmentAccessType]))).Key;

                if (!arguments.NewEnvironmentAccessType.IsEmptyOrEquals(permissionGroup.NewEnvironmentAccessType))
                    permissionGroup.NewEnvironmentAccessType = arguments.NewEnvironmentAccessType;

                if (arguments.EnvironmentSpecificAccessTypes == null || arguments.EnvironmentSpecificAccessTypes.Length == 0)
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
                    foreach (var envAccess in arguments.EnvironmentSpecificAccessTypes)
                    {
                        var existing = permissionGroup.EnvironmentAccesses.FirstOrDefault(e => e.EnvironmentId == envAccess.EnvironmentId);
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

            await this.permissionGroupClient.UpdatePermissionGroupAsync(permissionGroup.PermissionGroupId, permissionGroup, token);
            return ExitCodes.Ok;
        }
    }

    internal class UpdatePermissionGroupEnvironmentAccess
    {
        public string AccessType { get; set; }

        public string NewEnvironmentAccessType { get; set; }

        public EnvironmentSpecificAccess[] EnvironmentSpecificAccessTypes { get; set; }
    }
}