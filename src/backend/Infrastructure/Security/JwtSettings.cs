namespace Infrastructure.Security;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>Clave HMAC-SHA256 de al menos 32 bytes. Se configura por user-secrets o variable de entorno.</summary>
    public string Key { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int ExpirationMinutes { get; init; } = 60;
}
