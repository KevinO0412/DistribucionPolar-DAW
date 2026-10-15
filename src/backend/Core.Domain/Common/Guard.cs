using Core.Domain.Exceptions;

namespace Core.Domain.Common;

/// <summary>
/// Validaciones de invariantes reutilizadas por las entidades del dominio.
/// </summary>
internal static class Guard
{
    public static string RequiredText(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"El campo {field} es obligatorio.");
        }

        return MaxLength(value.Trim(), field, maxLength);
    }

    public static string? OptionalText(string? value, string field, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value) ? null : MaxLength(value.Trim(), field, maxLength);
    }

    public static decimal Positive(decimal value, string field)
    {
        return value > 0 ? value : throw new DomainException($"El campo {field} debe ser mayor que 0.");
    }

    public static decimal NotNegative(decimal value, string field)
    {
        return value >= 0 ? value : throw new DomainException($"El campo {field} no puede ser negativo.");
    }

    public static int Positive(int value, string field)
    {
        return value > 0 ? value : throw new DomainException($"El campo {field} debe ser mayor que 0.");
    }

    public static int NotNegative(int value, string field)
    {
        return value >= 0 ? value : throw new DomainException($"El campo {field} no puede ser negativo.");
    }

    public static Guid NotEmpty(Guid value, string field)
    {
        return value != Guid.Empty ? value : throw new DomainException($"El campo {field} es obligatorio.");
    }

    private static string MaxLength(string value, string field, int maxLength)
    {
        return value.Length <= maxLength
            ? value
            : throw new DomainException($"El campo {field} no puede superar {maxLength} caracteres.");
    }
}
