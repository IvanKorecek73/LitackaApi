namespace LitackaApi.Auth;

public static class Roles
{
    public const string SuperAdmin = "super-admin";
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string User = "user";
    public const string NewUser = "new-user";

    public static readonly string[] All = [SuperAdmin, Admin, Manager, User, NewUser];
}
