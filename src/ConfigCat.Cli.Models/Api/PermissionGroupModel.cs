using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Api;

public class PermissionGroupModel
{
        public long PermissionGroupId { get; set; }
        public string Name { get; set; }
        public bool CanManageMembers { get; set; } = true;
        public bool CanCreateOrUpdateConfig { get; set; } = true; 
        public bool CanDeleteConfig { get; set; } = true;
        public bool CanCreateOrUpdateEnvironment { get; set; } = true;
        public bool CanDeleteEnvironment { get; set; } = true;
        public bool CanCreateOrUpdateSetting { get; set; } = true;
        public bool CanTagSetting { get; set; } = true;
        public bool CanDeleteSetting { get; set; } = true;
        public bool CanCreateOrUpdateTag { get; set; } = true;
        public bool CanDeleteTag { get; set; } = true;
        public bool CanManageWebhook { get; set; } = true;
        public bool CanUseExportImport { get; set; } = true;
        public bool CanManageProductPreferences { get; set; } = true;
        public bool CanManageIntegrations { get; set; } = true;
        public bool CanViewSdkKey { get; set; } = true;
        public bool CanRotateSdkKey { get; set; } = true;
        public bool CanViewProductStatistics { get; set; } = true;
        public bool CanViewProductAuditLog { get; set; } = true;
        public bool CanCreateOrUpdateSegments { get; set; } = true;
        public bool CanDeleteSegments { get; set; } = true;
        public string AccessType { get; set; } = "full";
        public string NewEnvironmentAccessType { get; set; } = "full";
        public List<EnvironmentAccessModel> EnvironmentAccesses { get; set; } = new();

        public ProductModel Product { get; set; }
}

public class EnvironmentAccessModel
{
        public string EnvironmentId { get; set; }
        public string Name { get; set; }
        public string EnvironmentAccessType { get; set; }
}

public class UpdatePermissionGroupModel
{
        public long PermissionGroupId { get; set; }
        public string Name { get; set; }
        public bool? CanManageMembers { get; set; }
        public bool? CanCreateOrUpdateConfig { get; set; }
        public bool? CanDeleteConfig { get; set; }
        public bool? CanCreateOrUpdateEnvironment { get; set; }
        public bool? CanDeleteEnvironment { get; set; }
        public bool? CanCreateOrUpdateSetting { get; set; }
        public bool? CanTagSetting { get; set; }
        public bool? CanDeleteSetting { get; set; }
        public bool? CanCreateOrUpdateTag { get; set; }
        public bool? CanDeleteTag { get; set; }
        public bool? CanManageWebhook { get; set; }
        public bool? CanUseExportImport { get; set; }
        public bool? CanManageProductPreferences { get; set; }
        public bool? CanManageIntegrations { get; set; }
        public bool? CanViewSdkKey { get; set; }
        public bool? CanRotateSdkKey { get; set; }
        public bool? CanViewProductStatistics { get; set; }
        public bool? CanViewProductAuditLog { get; set; }
        public bool? CanCreateOrUpdateSegments { get; set; }
        public bool? CanDeleteSegments { get; set; }

        public bool IsAnyPermissionSet() => this.CanManageMembers != null || this.CanCreateOrUpdateConfig != null ||
            this.CanDeleteConfig != null || this.CanCreateOrUpdateEnvironment != null || this.CanDeleteEnvironment != null ||
            this.CanCreateOrUpdateSetting != null || this.CanTagSetting != null || this.CanDeleteSetting != null ||
            this.CanCreateOrUpdateTag != null || this.CanDeleteTag != null || this.CanManageWebhook != null ||
            this.CanUseExportImport != null || this.CanManageProductPreferences != null || this.CanManageIntegrations != null ||
            this.CanViewSdkKey != null || this.CanRotateSdkKey != null || this.CanViewProductStatistics != null ||
            this.CanViewProductAuditLog != null || this.CanCreateOrUpdateSegments != null || this.CanDeleteSegments != null;
}