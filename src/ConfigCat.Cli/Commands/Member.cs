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

namespace ConfigCat.Cli.Commands
{
    internal class Member
    {
        private readonly IMemberClient memberClient;
        private readonly IProductClient productClient;
        private readonly IPermissionGroupClient permissionGroupClient;
        private readonly IWorkspaceLoader workspaceLoader;
        private readonly IPrompt prompt;
        private readonly IOutput output;

        public Member(IMemberClient memberClient,
            IProductClient productClient,
            IPermissionGroupClient permissionGroupClient,
            IWorkspaceLoader workspaceLoader,
            IPrompt prompt,
            IOutput output)
        {
            this.memberClient = memberClient;
            this.productClient = productClient;
            this.permissionGroupClient = permissionGroupClient;
            this.workspaceLoader = workspaceLoader;
            this.prompt = prompt;
            this.output = output;
        }

        public async Task<int> ListOrganizationMembersAsync(string organizationId, bool json, CancellationToken token)
        {
            var members = new List<MemberModel>();
            if (!organizationId.IsEmpty())
                members.AddRange(await this.memberClient.GetOrganizationMembersAsync(organizationId, token));
            else
            {
                var organization = await this.workspaceLoader.LoadOrganizationAsync(token);
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
            var members = new List<ProductMemberModel>();
            if (!productId.IsEmpty())
                members.AddRange(await this.memberClient.GetProductMembersAsync(productId, token));
            else
            {
                var product = await this.workspaceLoader.LoadProductAsync(token);
                productId = product.ProductId;
                members.AddRange(await this.memberClient.GetProductMembersAsync(productId, token));
            }

            var permissionGroups = await this.permissionGroupClient.GetPermissionGroupsAsync(productId, token);
            var result = members.Select(m => new
            {
                m.Email,
                m.FullName,
                m.UserId,
                PermissionGroup = permissionGroups.FirstOrDefault(pg => pg.PermissionGroupId == m.PermissionGroupId)
            });

            if (json)
            {
                this.output.RenderJson(result);
                return ExitCodes.Ok;
            }

            this.output.RenderTable(result.Select(m => new
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
                organizationId = (await this.workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

            if (userId.IsEmpty())
            {
                var members = await this.memberClient.GetOrganizationMembersAsync(organizationId, token);
                userId = (await this.prompt.ChooseFromListAsync("Choose member", members.ToList(), m => $"{m.FullName} ({m.Email})", token)).UserId;
            }

            await this.memberClient.RemoveFromOrganizationAsync(organizationId, userId, token);
            return ExitCodes.Ok;
        }

        public async Task<int> InviteMembersAsync(string productId, InviteMemberModel model, CancellationToken token)
        {
            if (model.Emails.Length == 0)
                throw new ShowHelpException("Required argument <emails> is missing.");

            if (productId.IsEmpty())
                productId = (await this.workspaceLoader.LoadProductAsync(token)).ProductId;

            if (model.PermissionGroupId == null)
            {
                var permissionGroups = await this.permissionGroupClient.GetPermissionGroupsAsync(productId, token);
                model.PermissionGroupId = (await this.prompt.ChooseFromListAsync("Choose permission group", permissionGroups.ToList(), pg => pg.Name, token)).PermissionGroupId;
            }

            await this.memberClient.InviteMemberAsync(productId, model, token);
            return ExitCodes.Ok;
        }

        public async Task<int> AddPermissionsAsync(string organizationId, string userId, UpdateMembersModel model, CancellationToken token)
        {
            if (organizationId.IsEmpty())
                organizationId = (await this.workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

            if (userId.IsEmpty())
            {
                var members = await this.memberClient.GetOrganizationMembersAsync(organizationId, token);
                userId = (await this.prompt.ChooseFromListAsync("Choose member", members.ToList(), m => $"{m.FullName} ({m.Email})", token)).UserId;
            }

            var products = (await this.productClient.GetProductsAsync(token)).Where(p => p.Organization.OrganizationId == organizationId).ToList();
            var userPermissions = new List<long>();
            foreach (var product in products)
            {
                var productPermissions = await this.memberClient.GetProductMembersAsync(product.ProductId, token);
                var userPermission = productPermissions.FirstOrDefault(p => p.UserId == userId);
                if (userPermission != null)
                    userPermissions.Add(userPermission.PermissionGroupId);
            }


            if (model.PermissionGroupIds == null || !model.PermissionGroupIds.Any())
            {
                var permissionGroups = new List<PermissionGroupModel>();
                foreach (var product in products)
                    permissionGroups.AddRange(await this.permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));

                var existingPermissions = permissionGroups.Where(pg => userPermissions.Contains(pg.PermissionGroupId)).ToList();
                var selected = await this.prompt.ChooseMultipleFromListAsync("Choose permission groups (only 1 group is allowed per Product)",
                    permissionGroups.ToList(), pg => $"{pg.Name} ({pg.Product.Name})", token, existingPermissions);

                if (selected == null)
                    throw new ShowHelpException("Required option `--permission-group-ids` is missing.");

                if (selected.Count == 0)
                {
                    this.output.WriteNoChange();
                    return ExitCodes.Ok;
                }

                model.PermissionGroupIds = selected.Select(pg => pg.PermissionGroupId).ToArray();
            }

            await this.memberClient.UpdateMemberAsync(organizationId, userId, model, token);
            return ExitCodes.Ok;
        }

        public async Task<int> RemovePermissionsAsync(string organizationId, string userId, UpdateMembersModel model, CancellationToken token)
        {
            if (organizationId.IsEmpty())
                organizationId = (await this.workspaceLoader.LoadOrganizationAsync(token)).OrganizationId;

            if (userId.IsEmpty())
            {
                var members = await this.memberClient.GetOrganizationMembersAsync(organizationId, token);
                userId = (await this.prompt.ChooseFromListAsync("Choose member", members.ToList(), m => $"{m.FullName} ({m.Email})", token)).UserId;
            }

            var products = (await this.productClient.GetProductsAsync(token)).Where(p => p.Organization.OrganizationId == organizationId).ToList();
            var userPermissions = new List<long>();
            foreach (var product in products)
            {
                var productPermissions = await this.memberClient.GetProductMembersAsync(product.ProductId, token);
                var userPermission = productPermissions.FirstOrDefault(p => p.UserId == userId);
                if (userPermission != null)
                    userPermissions.Add(userPermission.PermissionGroupId);
            }

            var permissionGroups = new List<PermissionGroupModel>();
            foreach (var product in products)
                permissionGroups.AddRange(await this.permissionGroupClient.GetPermissionGroupsAsync(product.ProductId, token));
            var existingPermissions = permissionGroups.Where(pg => userPermissions.Contains(pg.PermissionGroupId)).ToList();

            if (model.PermissionGroupIds == null || !model.PermissionGroupIds.Any())
            {
                var selected = await this.prompt.ChooseMultipleFromListAsync("Choose permission groups to remove",
                   existingPermissions.ToList(), pg => $"{pg.Name} ({pg.Product.Name})", token);

                if (selected == null)
                    throw new ShowHelpException("Required option `--permission-group-ids` is missing.");

                if (selected.Count == 0)
                {
                    this.output.WriteNoChange();
                    return ExitCodes.Ok;
                }

                model.PermissionGroupIds = selected.Select(pg => pg.PermissionGroupId).ToArray();
            }

            var permissionGroupsToDelete = permissionGroups.Where(pg => model.PermissionGroupIds.Contains(pg.PermissionGroupId));
            foreach (var pg in permissionGroupsToDelete)
                await this.memberClient.RemoveFromProductAsync(pg.Product, userId, token);

            return ExitCodes.Ok;
        }
    }
}