using Core.Application.DTOs;

namespace Core.Application.Abstractions;

/// <summary>
/// Consultas de solo lectura para reportes: agregaciones calculadas en PostgreSQL y proyectadas
/// directamente a DTO, sin materializar entidades.
/// </summary>
public interface IReportQueries
{
    Task<DailyReportResponse> GetDailyReportAsync(DateOnly date, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);

    Task<InventorySummaryResponse> GetInventorySummaryAsync(CancellationToken cancellationToken = default);

    Task<(decimal MoneyOnStreet, int EmptyBoxesOnStreet, int EmptyUnitsOnStreet, int ReceivableClients)> GetStreetBalancesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Datos crudos del dashboard: productos activos con su stock en camiones, renglones vendidos en el rango [startUtc, endUtc)
    /// y camiones con su carga. Las agregaciones por día local y los KPI se calculan en la capa de aplicación.
    /// </summary>
    Task<DashboardData> GetDashboardDataAsync(DateTime startUtc, DateTime endUtc, Guid? categoryId, CancellationToken cancellationToken = default);

    Task<ProductLocationResponse?> GetProductLocationAsync(Guid productId, CancellationToken cancellationToken = default);
}

/// <summary>Producto activo con existencias en galpón y en camiones.</summary>
public sealed record DashboardProductRow(
    Guid Id, string SKU, string Name, Guid CategoryId, string? CategoryName, decimal PriceUnit, decimal CostPrice,
    int UnitsPerBox, int WarehouseUnits, int TruckUnits, int MinStock, int MaxStock);

/// <summary>Renglón de una factura o regalía no anulada.</summary>
public sealed record DashboardSaleLine(
    Guid InvoiceId, DateTime IssuedAtUtc, bool IsGift, Guid? TruckId, Guid ProductId, string SKU, string ProductName,
    Guid CategoryId, string CategoryName, int Units, decimal Subtotal);

/// <summary>Totales de cobro de las facturas de venta del rango (sin filtro de categoría).</summary>
public sealed record DashboardCollections(decimal Collected, decimal Credit);

/// <summary>Camión con su capacidad y su carga en cajas equivalentes.</summary>
public sealed record DashboardTruckRow(Guid Id, string Name, string Plate, int CapacityBoxes, decimal LoadedBoxes, int StockUnits, decimal InventoryValue);

public sealed record DashboardData(
    IReadOnlyList<DashboardProductRow> Products,
    IReadOnlyList<DashboardSaleLine> SaleLines,
    DashboardCollections Collections,
    IReadOnlyList<DashboardTruckRow> Trucks);
