using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>
/// Usuario del sistema. Inicia sesión con username (o email) y clave; solo usuarios activos pueden entrar.
/// </summary>
public sealed class User : BaseEntity
{
    public const int UsernameMaxLength = 50;
    public const int EmailMaxLength = 150;
    public const int FullNameMaxLength = 100;
    public const int PasswordHashMaxLength = 256;
    public const int AttendantNameMaxLength = 100;

    // Constructor para EF Core.
    private User()
    {
    }

    public User(
        string username,
        string email,
        string fullName,
        string passwordHash,
        UserRole role,
        string? attendantName = null,
        bool canChangeAttendant = false)
    {
        Username = NormalizeUsername(username);
        PasswordHash = Guard.RequiredText(passwordHash, "hash de la clave", PasswordHashMaxLength);
        IsActive = true;
        ApplyProfile(email, fullName, role, attendantName, canChangeAttendant);
    }

    public string Username { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public string FullName { get; private set; } = default!;

    public string PasswordHash { get; private set; } = default!;

    public UserRole Role { get; private set; }

    public bool IsActive { get; private set; }

    /// <summary>Vendedor fijo que figura como "quien atiende" en las facturas de este usuario.</summary>
    public string? AttendantName { get; private set; }

    /// <summary>Si es verdadero, el usuario elige el vendedor en cada factura.</summary>
    public bool CanChangeAttendant { get; private set; }

    /// <summary>Nombre que se usa como vendedor cuando el usuario no puede elegirlo.</summary>
    public string DefaultAttendant => AttendantName ?? FullName;

    /// <summary>Normaliza el username: sin espacios y en minúsculas.</summary>
    public static string NormalizeUsername(string username)
    {
        string normalized = string.Concat((username ?? string.Empty).Where(c => !char.IsWhiteSpace(c)))
            .ToLowerInvariant();

        return Guard.RequiredText(normalized, "username", UsernameMaxLength);
    }

    public void UpdateProfile(string email, string fullName, UserRole role, string? attendantName, bool canChangeAttendant)
    {
        ApplyProfile(email, fullName, role, attendantName, canChangeAttendant);
        Touch();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        PasswordHash = Guard.RequiredText(passwordHash, "hash de la clave", PasswordHashMaxLength);
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    private void ApplyProfile(string email, string fullName, UserRole role, string? attendantName, bool canChangeAttendant)
    {
        Email = Guard.RequiredText(email, "email", EmailMaxLength).ToLowerInvariant();
        FullName = Guard.RequiredText(fullName, "nombre", FullNameMaxLength);
        Role = Enum.IsDefined(role) ? role : throw new DomainException("Rol de usuario inválido.");
        AttendantName = Guard.OptionalText(attendantName, "vendedor asignado", AttendantNameMaxLength);
        CanChangeAttendant = canChangeAttendant;
    }
}
