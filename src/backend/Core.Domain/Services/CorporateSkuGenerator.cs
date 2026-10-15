using System.Text.RegularExpressions;

namespace Core.Domain.Services;

/// <summary>Resultado de la generación de un SKU corporativo.</summary>
/// <param name="RawInput">Nombre recibido.</param>
/// <param name="Category">Categoría recibida.</param>
/// <param name="SanitizedName">Nombre sin espacios, acentos ni símbolos.</param>
/// <param name="GeneratedSKU">SKU con formato [CAT]-[PRO]-[SEQ].</param>
/// <param name="SkuFormula">Fórmula aplicada.</param>
/// <param name="IsValidForEnterprise">Indica si el SKU cumple el formato ^[A-Z0-9]{3}-[A-Z0-9]{3}-[0-9]{4}$.</param>
public sealed record SkuGenerationResult(
    string RawInput,
    string Category,
    string SanitizedName,
    string GeneratedSKU,
    string SkuFormula,
    bool IsValidForEnterprise);

/// <summary>
/// Genera SKUs corporativos con el formato [CAT]-[PRO]-[SEQ], por ejemplo BEB-MAL-0006.
/// </summary>
public static partial class CorporateSkuGenerator
{
    public const string SkuFormula = "[3_LETRAS_CAT]-[3_LETRAS_PRO]-[SECUENCIA_4D]";

    public static SkuGenerationResult Generate(string rawProductName, string categoryName, int sequenceNumber)
    {
        if (string.IsNullOrWhiteSpace(rawProductName))
        {
            throw new InvalidOperationException("El nombre del producto no puede estar vacío.");
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new InvalidOperationException("La categoría es obligatoria para formar el SKU.");
        }

        // 1. Sanitizar: solo letras mayúsculas A-Z y dígitos (se eliminan espacios, acentos y símbolos).
        string cleanCategory = CleanRegex().Replace(categoryName.Trim().ToUpperInvariant(), "");
        string cleanProduct = CleanRegex().Replace(rawProductName.Trim().ToUpperInvariant(), "");

        // 2. Prefijos de 3 caracteres mediante rangos de C#.
        string catPrefix = cleanCategory.Length >= 3 ? cleanCategory[..3] : cleanCategory.PadRight(3, 'X');
        string prodPrefix = cleanProduct.Length >= 3 ? cleanProduct[..3] : cleanProduct.PadRight(3, 'P');

        // 3. Secuencia numérica a 4 dígitos.
        string sequenceFormatted = Math.Clamp(sequenceNumber, 1, 9999).ToString("D4");

        // 4. SKU final: [CAT]-[PRO]-[SEQ]
        string corporateSku = $"{catPrefix}-{prodPrefix}-{sequenceFormatted}";

        return new SkuGenerationResult(
            RawInput: rawProductName,
            Category: categoryName,
            SanitizedName: cleanProduct,
            GeneratedSKU: corporateSku,
            SkuFormula: SkuFormula,
            IsValidForEnterprise: IsValid(corporateSku));
    }

    public static bool IsValid(string sku) => SkuFormatRegex().IsMatch(sku);

    [GeneratedRegex("[^A-Z0-9]")]
    private static partial Regex CleanRegex();

    [GeneratedRegex("^[A-Z0-9]{3}-[A-Z0-9]{3}-[0-9]{4}$")]
    private static partial Regex SkuFormatRegex();
}
