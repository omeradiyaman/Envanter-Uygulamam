namespace InventorySystem.Application.Common.Security;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Editor = "Editor";
    public const string Viewer = "Görüntüleyici";
    public static readonly string[] All = [Admin, Editor, Viewer];
}

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string EditorOrAdmin = "EditorOrAdmin";
}
