using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Services;
using Core.Domain.ValueObjects;

namespace Core.Application.Services;

public sealed class ReportService(
    IReportQueries reports,
    IConsignmentService consignments,
    IAuditLogRepository auditLogs,
    IClock clock) : IReportService
{
    /// <summary>Reporte diario del día local indicado (hoy por defecto). Excluye documentos anulados.</summary>
    public Task<DailyReportResponse> GetDailyReportAsync(DateOnly? date, CancellationToken cancellationToken = default)
    {
        var day = date ?? clock.Today;
        var (startUtc, endUtc) = clock.GetDayRangeUtc(day);
        return reports.GetDailyReportAsync(day, startUtc, endUtc, cancellationToken);
    }

    /// <summary>Balance general: dinero y vacíos en la calle, inventario total y consignación por cobrar.</summary>
    public async Task<BalanceResponse> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        var street = await reports.GetStreetBalancesAsync(cancellationToken);
        var inventory = await reports.GetInventorySummaryAsync(cancellationToken);
        var consignment = await consignments.GetIndicatorsAsync(cancellationToken);
        return new BalanceResponse(
            street.MoneyOnStreet, street.EmptyBoxesOnStreet, street.EmptyUnitsOnStreet, street.ReceivableClients,
            inventory, consignment.ReceivableTotal);
    }

    public const int MaxDashboardDays = 366;
    public const int DefaultDashboardDays = 30;

    /// <summary>
    /// Dashboard KPI: valorización (costo frente a venta), alertas de stock según MinStock/MaxStock, ventas por día y
    /// por categoría, rotación por producto y ocupación de la flota. Por defecto, los últimos 30 días.
    /// </summary>
    public async Task<DashboardResponse> GetDashboardAsync(
        DateOnly? from, DateOnly? to, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var end = to ?? clock.Today;
        var start = from ?? end.AddDays(-(DefaultDashboardDays - 1));
        if (start > end)
        {
            throw new InvalidOperationException("La fecha inicial no puede ser posterior a la final.");
        }

        if (end.DayNumber - start.DayNumber + 1 > MaxDashboardDays)
        {
            throw new InvalidOperationException($"El rango del dashboard no puede superar {MaxDashboardDays} días.");
        }

        var data = await reports.GetDashboardDataAsync(
            clock.GetDayRangeUtc(start).StartUtc, clock.GetDayRangeUtc(end).EndUtc, categoryId, cancellationToken);

        return new DashboardResponse(
            start, end, categoryId,
            BuildInventory(data.Products),
            BuildSales(data),
            BuildStockAlerts(data.Products, out var critical),
            critical,
            BuildSalesByDay(data.SaleLines, start, end),
            data.SaleLines
                .GroupBy(l => new { l.CategoryId, l.CategoryName })
                .Select(g => new CategorySales(g.Key.CategoryId, g.Key.CategoryName, g.Where(l => !l.IsGift).Sum(l => l.Subtotal), g.Sum(l => l.Units)))
                .OrderByDescending(c => c.Total)
                .ToList(),
            BuildTopProducts(data),
            BuildFleet(data));
    }

    private static DashboardInventoryKpi BuildInventory(IReadOnlyList<DashboardProductRow> products)
    {
        int units = products.Sum(p => p.WarehouseUnits + p.TruckUnits);
        decimal cost = Math.Round(products.Sum(p => (p.WarehouseUnits + p.TruckUnits) * (p.UnitsPerBox > 0 ? p.CostPrice / p.UnitsPerBox : 0)), 2);
        decimal sale = Math.Round(products.Sum(p => (p.WarehouseUnits + p.TruckUnits) * p.PriceUnit), 2);
        decimal margin = sale - cost;
        return new DashboardInventoryKpi(products.Count, units, cost, sale, margin, sale > 0 ? Math.Round(margin * 100 / sale, 1) : 0);
    }

    private static DashboardSalesKpi BuildSales(DashboardData data)
    {
        var sales = data.SaleLines.Where(l => !l.IsGift).ToList();
        decimal total = sales.Sum(l => l.Subtotal);
        int invoices = sales.Select(l => l.InvoiceId).Distinct().Count();
        int gifts = data.SaleLines.Where(l => l.IsGift).Select(l => l.InvoiceId).Distinct().Count();
        int unitsSold = data.SaleLines.Sum(l => l.Units);
        int stock = data.Products.Sum(p => p.WarehouseUnits + p.TruckUnits);
        return new DashboardSalesKpi(
            total, invoices, gifts, invoices > 0 ? Math.Round(total / invoices, 2) : 0, unitsSold,
            data.Collections.Collected, data.Collections.Credit, stock > 0 ? Math.Round((decimal)unitsSold / stock, 2) : 0);
    }

    /// <summary>Estado de salud del stock de galpón de cada producto (ProductHealthEvaluator del dominio).</summary>
    private static IReadOnlyList<StockAlertCount> BuildStockAlerts(
        IReadOnlyList<DashboardProductRow> products, out IReadOnlyList<CriticalProductResponse> critical)
    {
        var evaluated = products
            .Where(p => p.MaxStock > p.MinStock)
            .Select(p => (Product: p, Report: ProductHealthEvaluator.Evaluate(p.SKU, p.WarehouseUnits, p.MinStock, p.MaxStock)))
            .ToList();

        critical = evaluated
            .Where(e => e.Report.Status is StockHealthStatus.OutOfStock or StockHealthStatus.CriticalRisk or StockHealthStatus.Excessive)
            .OrderBy(e => e.Report.Status switch
            {
                StockHealthStatus.OutOfStock => 0,
                StockHealthStatus.CriticalRisk => 1,
                _ => 2
            })
            .ThenBy(e => e.Product.MinStock > 0 ? (decimal)e.Product.WarehouseUnits / e.Product.MinStock : 0)
            .Take(25)
            .Select(e => new CriticalProductResponse(
                e.Product.Id, e.Product.SKU, e.Product.Name, e.Product.CategoryName, e.Product.WarehouseUnits,
                BoxQuantity.FromUnits(e.Product.WarehouseUnits, e.Product.UnitsPerBox).ToString(),
                e.Product.MinStock, e.Product.MaxStock, e.Report.Status.ToString(), e.Report.RecommendedPurchaseUnits))
            .ToList();

        return Enum.GetValues<StockHealthStatus>()
            .Select(status => new StockAlertCount(status.ToString(), evaluated.Count(e => e.Report.Status == status)))
            .ToList();
    }

    private IReadOnlyList<SalesPoint> BuildSalesByDay(IReadOnlyList<DashboardSaleLine> lines, DateOnly start, DateOnly end)
    {
        var byDay = lines
            .GroupBy(l => DateOnly.FromDateTime(clock.ToLocal(l.IssuedAtUtc)))
            .ToDictionary(
                g => g.Key,
                g => new SalesPoint(
                    g.Key,
                    g.Where(l => !l.IsGift).Sum(l => l.Subtotal),
                    g.Where(l => !l.IsGift).Select(l => l.InvoiceId).Distinct().Count(),
                    g.Sum(l => l.Units)));

        return Enumerable.Range(0, end.DayNumber - start.DayNumber + 1)
            .Select(offset => start.AddDays(offset))
            .Select(day => byDay.GetValueOrDefault(day) ?? new SalesPoint(day, 0, 0, 0))
            .ToList();
    }

    /// <summary>Rotación = unidades vendidas en el rango / existencia actual (galpón + camiones); 0 si no hay existencia.</summary>
    private static IReadOnlyList<ProductRotation> BuildTopProducts(DashboardData data)
    {
        var stock = data.Products.ToDictionary(p => p.Id, p => p.WarehouseUnits + p.TruckUnits);
        return data.SaleLines
            .GroupBy(l => new { l.ProductId, l.SKU, l.ProductName, l.CategoryName })
            .Select(g =>
            {
                int sold = g.Sum(l => l.Units);
                int current = stock.GetValueOrDefault(g.Key.ProductId);
                return new ProductRotation(
                    g.Key.ProductId, g.Key.SKU, g.Key.ProductName, g.Key.CategoryName, sold,
                    g.Where(l => !l.IsGift).Sum(l => l.Subtotal), current, current > 0 ? Math.Round((decimal)sold / current, 2) : 0);
            })
            .OrderByDescending(p => p.UnitsSold)
            .ThenByDescending(p => p.Revenue)
            .Take(10)
            .ToList();
    }

    private static DashboardFleetKpi BuildFleet(DashboardData data)
    {
        var sales = data.SaleLines.Where(l => !l.IsGift && l.TruckId != null).GroupBy(l => l.TruckId!.Value)
            .ToDictionary(g => g.Key, g => (Total: g.Sum(l => l.Subtotal), Invoices: g.Select(l => l.InvoiceId).Distinct().Count()));

        var trucks = data.Trucks
            .Select(t =>
            {
                var truckSales = sales.GetValueOrDefault(t.Id);
                return new TruckOccupancy(
                    t.Id, t.Name, t.Plate, t.CapacityBoxes, Math.Round(t.LoadedBoxes, 1),
                    t.CapacityBoxes > 0 ? Math.Round(t.LoadedBoxes * 100 / t.CapacityBoxes, 1) : 0,
                    t.StockUnits, Math.Round(t.InventoryValue, 2), truckSales.Total, truckSales.Invoices);
            })
            .ToList();

        int capacity = trucks.Sum(t => t.CapacityBoxes);
        decimal loaded = Math.Round(data.Trucks.Sum(t => t.LoadedBoxes), 1);
        return new DashboardFleetKpi(capacity, loaded, capacity > 0 ? Math.Round(loaded * 100 / capacity, 1) : 0, trucks);
    }

    public Task<PagedResponse<AuditLogResponse>> GetAuditLogAsync(
        DateOnly? from, DateOnly? to, string? module, string? username, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        DateTime? fromUtc = from is null ? null : clock.GetDayRangeUtc(from.Value).StartUtc;
        DateTime? toUtc = to is null ? null : clock.GetDayRangeUtc(to.Value).EndUtc;
        return auditLogs.ListAsync(fromUtc, toUtc, module, username, Math.Max(1, page), Math.Clamp(pageSize, 1, 200), cancellationToken);
    }
}
