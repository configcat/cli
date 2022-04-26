using System.Collections.Generic;

namespace ConfigCat.Cli.Models.Api;

public class MemberModel
{
    public string UserId { get; set; }

    public string Email { get; set; }

    public string FullName { get; set; }
}

public class ProductMemberModel : MemberModel
{
    public long PermissionGroupId { get; set; }
}

public class InviteMemberModel
{
    public string[] Emails { get; set; }

    public long? PermissionGroupId { get; set; }
}

public class UpdateMembersModel
{
    public long[] PermissionGroupIds { get; set; }
}