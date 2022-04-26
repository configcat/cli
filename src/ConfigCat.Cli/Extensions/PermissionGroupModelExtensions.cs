using System;
using System.Collections.Generic;
using ConfigCat.Cli.Services;

namespace ConfigCat.Cli.Models.Api;

public static class PermissionGroupModelExtensions
{
    public static IEnumerable<string> ToSelectedPermissions(this PermissionGroupModel model)
    {
        if (model.CanManageMembers) yield return Constants.Permissions[0];
        if (model.CanCreateOrUpdateConfig) yield return Constants.Permissions[1];
        if (model.CanDeleteConfig) yield return Constants.Permissions[2];
        if (model.CanCreateOrUpdateEnvironment) yield return Constants.Permissions[3];
        if (model.CanDeleteEnvironment) yield return Constants.Permissions[4];
        if (model.CanCreateOrUpdateSetting) yield return Constants.Permissions[5];
        if (model.CanTagSetting) yield return Constants.Permissions[6];
        if (model.CanDeleteSetting) yield return Constants.Permissions[7];
        if (model.CanCreateOrUpdateTag) yield return Constants.Permissions[8];
        if (model.CanDeleteTag) yield return Constants.Permissions[9];
        if (model.CanManageWebhook) yield return Constants.Permissions[10];
        if (model.CanUseExportImport) yield return Constants.Permissions[11];
        if (model.CanManageProductPreferences) yield return Constants.Permissions[12];
        if (model.CanManageIntegrations) yield return Constants.Permissions[13];
        if (model.CanViewSdkKey) yield return Constants.Permissions[14];
        if (model.CanRotateSdkKey) yield return Constants.Permissions[15];
        if (model.CanViewProductStatistics) yield return Constants.Permissions[16];
        if (model.CanViewProductAuditLog) yield return Constants.Permissions[17];
        if (model.CanCreateOrUpdateSegments) yield return Constants.Permissions[18];
        if (model.CanDeleteSegments) yield return Constants.Permissions[19];
    }
    
    public static void UpdateFromSelectedPermissions(this PermissionGroupModel model,  List<string> permissions)
    {
        model.CanManageMembers = permissions.Contains(Constants.Permissions[0]);
        model.CanCreateOrUpdateConfig = permissions.Contains(Constants.Permissions[1]);
        model.CanDeleteConfig = permissions.Contains(Constants.Permissions[2]);
        model.CanCreateOrUpdateEnvironment = permissions.Contains(Constants.Permissions[3]);
        model.CanDeleteEnvironment = permissions.Contains(Constants.Permissions[4]);
        model.CanCreateOrUpdateSetting = permissions.Contains(Constants.Permissions[5]);
        model.CanTagSetting = permissions.Contains(Constants.Permissions[6]);
        model.CanDeleteSetting = permissions.Contains(Constants.Permissions[7]);
        model.CanCreateOrUpdateTag = permissions.Contains(Constants.Permissions[8]);
        model.CanDeleteTag = permissions.Contains(Constants.Permissions[9]);
        model.CanManageWebhook = permissions.Contains(Constants.Permissions[10]);
        model.CanUseExportImport = permissions.Contains(Constants.Permissions[11]);
        model.CanManageProductPreferences = permissions.Contains(Constants.Permissions[12]);
        model.CanManageIntegrations = permissions.Contains(Constants.Permissions[13]);
        model.CanViewSdkKey = permissions.Contains(Constants.Permissions[14]);
        model.CanRotateSdkKey = permissions.Contains(Constants.Permissions[15]);
        model.CanViewProductStatistics = permissions.Contains(Constants.Permissions[16]);
        model.CanViewProductAuditLog = permissions.Contains(Constants.Permissions[17]);
        model.CanCreateOrUpdateSegments = permissions.Contains(Constants.Permissions[18]);
        model.CanDeleteSegments = permissions.Contains(Constants.Permissions[19]);
    }
    
    public static bool GetPermissionValue(this PermissionGroupModel model,  string permission)
    {
        if (permission == Constants.Permissions[0]) return model.CanManageMembers;
        if (permission == Constants.Permissions[1]) return model.CanCreateOrUpdateConfig;
        if (permission == Constants.Permissions[2]) return model.CanDeleteConfig;
        if (permission == Constants.Permissions[3]) return model.CanCreateOrUpdateEnvironment;
        if (permission == Constants.Permissions[4]) return model.CanDeleteEnvironment;
        if (permission == Constants.Permissions[5]) return model.CanCreateOrUpdateSetting;
        if (permission == Constants.Permissions[6]) return model.CanTagSetting;
        if (permission == Constants.Permissions[7]) return model.CanDeleteSetting;
        if (permission == Constants.Permissions[8]) return model.CanCreateOrUpdateTag;
        if (permission == Constants.Permissions[9]) return model.CanDeleteTag;
        if (permission == Constants.Permissions[10]) return model.CanManageWebhook;
        if (permission == Constants.Permissions[11]) return model.CanUseExportImport;
        if (permission == Constants.Permissions[12]) return model.CanManageProductPreferences;
        if (permission == Constants.Permissions[13]) return model.CanManageIntegrations;
        if (permission == Constants.Permissions[14]) return model.CanViewSdkKey;
        if (permission == Constants.Permissions[15]) return model.CanRotateSdkKey;
        if (permission == Constants.Permissions[16]) return model.CanViewProductStatistics;
        if (permission == Constants.Permissions[17]) return model.CanViewProductAuditLog;
        if (permission == Constants.Permissions[18]) return model.CanCreateOrUpdateSegments;
        if (permission == Constants.Permissions[19]) return model.CanDeleteSegments;

        throw new ArgumentOutOfRangeException(permission);
    }
    
    public static void UpdateFromUpdateModel(this PermissionGroupModel model,  UpdatePermissionGroupModel updateModel)
    {
        if (!updateModel.Name.IsEmpty()) model.Name = updateModel.Name;
        if (updateModel.CanManageMembers != null) model.CanManageMembers = updateModel.CanManageMembers.Value;
        if (updateModel.CanCreateOrUpdateConfig != null) model.CanCreateOrUpdateConfig = updateModel.CanCreateOrUpdateConfig.Value;
        if (updateModel.CanDeleteConfig != null) model.CanDeleteConfig = updateModel.CanDeleteConfig.Value;
        if (updateModel.CanCreateOrUpdateEnvironment != null) model.CanCreateOrUpdateEnvironment = updateModel.CanCreateOrUpdateEnvironment.Value;
        if (updateModel.CanDeleteEnvironment != null) model.CanDeleteEnvironment = updateModel.CanDeleteEnvironment.Value;
        if (updateModel.CanCreateOrUpdateSetting != null) model.CanCreateOrUpdateSetting = updateModel.CanCreateOrUpdateSetting.Value;
        if (updateModel.CanTagSetting != null) model.CanTagSetting = updateModel.CanTagSetting.Value;
        if (updateModel.CanDeleteSetting != null) model.CanDeleteSetting = updateModel.CanDeleteSetting.Value;
        if (updateModel.CanCreateOrUpdateTag != null) model.CanCreateOrUpdateTag = updateModel.CanCreateOrUpdateTag.Value;
        if (updateModel.CanDeleteTag != null) model.CanDeleteTag = updateModel.CanDeleteTag.Value;
        if (updateModel.CanManageWebhook != null) model.CanManageWebhook = updateModel.CanManageWebhook.Value;
        if (updateModel.CanUseExportImport != null) model.CanUseExportImport = updateModel.CanUseExportImport.Value;
        if (updateModel.CanManageProductPreferences != null) model.CanManageProductPreferences = updateModel.CanManageProductPreferences.Value;
        if (updateModel.CanManageIntegrations != null) model.CanManageIntegrations = updateModel.CanManageIntegrations.Value;
        if (updateModel.CanViewSdkKey != null) model.CanViewSdkKey = updateModel.CanViewSdkKey.Value;
        if (updateModel.CanRotateSdkKey != null) model.CanRotateSdkKey = updateModel.CanRotateSdkKey.Value;
        if (updateModel.CanViewProductStatistics != null) model.CanViewProductStatistics = updateModel.CanViewProductStatistics.Value;
        if (updateModel.CanViewProductAuditLog != null) model.CanViewProductAuditLog = updateModel.CanViewProductAuditLog.Value;
        if (updateModel.CanCreateOrUpdateSegments != null) model.CanCreateOrUpdateSegments = updateModel.CanCreateOrUpdateSegments.Value;
        if (updateModel.CanDeleteSegments != null) model.CanDeleteSegments = updateModel.CanDeleteSegments.Value;
    }
}