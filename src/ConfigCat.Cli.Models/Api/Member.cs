namespace ConfigCat.Cli.Models.Api;

public class Member
{
    public string UserId { get; set; }

    public string Email { get; set; }

    public string FullName { get; set; }
}

public class ProductMember : Member
{
    public long PermissionGroupId { get; set; }
}