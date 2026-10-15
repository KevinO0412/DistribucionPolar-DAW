using System.Diagnostics;

namespace Core.Domain.Services;

/// <summary>Estado de salud del inventario de un producto.</summary>
public enum StockHealthStatus
{
    OutOfStock,     // Agotado total (Stock == 0)
    CriticalRisk,   // Riesgo crítico (Stock <= MinStock)
    Understocked,   // Por debajo del punto óptimo
    Optimal,        // Nivel óptimo y saludable
    Overstocked,    // Bodega al 100% de capacidad (Stock == MaxStock)
    Excessive       // Exceso que supera MaxStock
}

/// <summary>Diagnóstico de salud del inventario. Cantidades en unidades.</summary>
/// <param name="SKU">SKU evaluado en mayúsculas ("N/A" si viene vacío).</param>
/// <param name="CurrentStock">Stock actual.</param>
/// <param name="MinStock">Stock mínimo de seguridad.</param>
/// <param name="MaxStock">Capacidad máxima.</param>
/// <param name="Status">Estado: OutOfStock, CriticalRisk, Understocked, Optimal, Overstocked o Excessive.</param>
/// <param name="DiagnosticMessage">Mensaje explicativo del diagnóstico.</param>
/// <param name="RecommendedPurchaseUnits">Unidades sugeridas para reponer hasta el máximo.</param>
public sealed record StockHealthReport(
    string SKU,
    int CurrentStock,
    int MinStock,
    int MaxStock,
    StockHealthStatus Status,
    string DiagnosticMessage,
    int RecommendedPurchaseUnits);

/// <summary>
/// Evaluador declarativo de la salud del inventario mediante pattern matching sobre tuplas.
/// Todas las cantidades se expresan en unidades.
/// </summary>
public static class ProductHealthEvaluator
{
    public static StockHealthReport Evaluate(string sku, int stock, int minStock, int maxStock)
    {
        if (maxStock <= minStock)
        {
            throw new InvalidOperationException("El stock máximo debe superar al mínimo.");
        }

        var (status, message, purchase) = (stock, minStock, maxStock) switch
        {
            ( <= 0, _, _) => (
                StockHealthStatus.OutOfStock,
                "🚨 ALERTA ROJA: Producto completamente agotado. Detención de ventas.",
                maxStock),
            (var s, var min, _) when s <= min => (
                StockHealthStatus.CriticalRisk,
                $"⚠️ RIESGO CRÍTICO: Stock ({s}) por debajo del mínimo de seguridad ({min}). Reposición urgente requerida.",
                maxStock - s),
            (var s, var min, var max) when s <= min + (max - min) / 2 => (
                StockHealthStatus.Understocked,
                "🟡 Nivel de existencias bajo pero aceptable. Planificar orden de compra.",
                maxStock - s),
            (var s, _, var max) when s < max => (
                StockHealthStatus.Optimal,
                "🟢 Estado de existencias óptimo y equilibrado. Flujo comercial saludable.",
                0),
            (var s, _, var max) when s == max => (
                StockHealthStatus.Overstocked,
                "🔵 Bodega al 100% de capacidad. No ordenar nuevas compras.",
                0),
            (var s, _, var max) when s > max => (
                StockHealthStatus.Excessive,
                $"⛔ EXCESO DE ALMACÉN: Stock ({s}) supera la capacidad física permitida ({max}).",
                0),
            _ => throw new UnreachableException()
        };

        return new StockHealthReport(
            SKU: string.IsNullOrWhiteSpace(sku) ? "N/A" : sku.Trim().ToUpperInvariant(),
            CurrentStock: stock,
            MinStock: minStock,
            MaxStock: maxStock,
            Status: status,
            DiagnosticMessage: message,
            RecommendedPurchaseUnits: purchase);
    }
}
