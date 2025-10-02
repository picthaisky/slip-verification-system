namespace SlipVerification.Application.Authorization;

/// <summary>
/// Defines roles in the system
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Guest = "Guest";
}

/// <summary>
/// Defines permissions in the system
/// </summary>
public static class Permissions
{
    // Slip permissions
    public const string ViewSlips = "slips.view";
    public const string UploadSlips = "slips.upload";
    public const string DeleteSlips = "slips.delete";
    public const string VerifySlips = "slips.verify";

    // Order permissions
    public const string ViewOrders = "orders.view";
    public const string CreateOrders = "orders.create";
    public const string UpdateOrders = "orders.update";
    public const string DeleteOrders = "orders.delete";

    // User permissions
    public const string ViewUsers = "users.view";
    public const string ManageUsers = "users.manage";

    // Report permissions
    public const string ViewReports = "reports.view";
    public const string ExportReports = "reports.export";
}
