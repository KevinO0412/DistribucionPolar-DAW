using Core.Domain.Enums;

namespace Presentation.API.Security;

/// <summary>Nombres de rol para [Authorize(Roles = ...)].</summary>
public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string Employee = nameof(UserRole.Employee);
    public const string AdminOrEmployee = Admin + "," + Employee;
}
