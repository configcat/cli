using System;
using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Api;

public class PermissionGroupModel
{
        public long PermissionGroupId { get; set; }
        public string Name { get; set; }
        public bool CanManageMembers { get; set; }
        public bool CanCreateOrUpdateConfig { get; set; }
        public bool CanDeleteConfig { get; set; }
        public bool CanCreateOrUpdateEnvironment { get; set; }
        public bool CanDeleteEnvironment { get; set; }
        public bool CanCreateOrUpdateSetting { get; set; }
        public bool CanTagSetting { get; set; }
        public bool CanDeleteSetting { get; set; }
        public bool CanCreateOrUpdateTag { get; set; }
        public bool CanDeleteTag { get; set; }
        public bool CanManageWebhook { get; set; }
        public bool CanUseExportImport { get; set; }
        public bool CanManageProductPreferences { get; set; }
        public bool CanManageIntegrations { get; set; }
        public bool CanViewSdkKey { get; set; }
        public bool CanRotateSdkKey { get; set; }
        public bool CanViewProductStatistics { get; set; }
        public bool CanViewProductAuditLog { get; set; }
        public bool CanCreateOrUpdateSegments { get; set; }
        public bool CanDeleteSegments { get; set; }
        public AccessType AccessType { get; set; }
        public List<EnvironmentAccessModel> EnvironmentAccesses { get; set; }
}

public class EnvironmentAccessModel
{
        public Guid EnvironmentId { get; set; }
        public string Name { get; set; }
        public EnvironmentAccessType EnvironmentAccessType { get; set; }
}

public enum EnvironmentAccessType
{
        Full = 0,
        ReadOnly = 1,
        None = 2
}

public enum AccessType
{
        ReadOnly = 0,
        Full = 1,
        Custom = 2,
}