using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>Agregaciones de solo lectura calculadas en PostgreSQL (SUM/COUNT) y proyectadas a DTO.</summary>
public sealed class ReportQueries(ApplicationDbContext context) : IReportQueries
{
    public async Task<DailyReportResponse> GetDailyReportAsync(DateOnly date, DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        var documents = context.Invoices
            .AsNoTracking()
            .Where(i => !i.IsCancelled && i.IssuedAt >= startUtc && i.IssuedAt < endUtc);

        var sales = await documents
            .Where(i => i.Type == InvoiceType.Sale)
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Sum(i => i.Total), Count = g.Count(), Paid = g.Sum(i => i.Payment), Pending = g.Sum(i => i.Pending) })
            .FirstOrDefaultAsync(cancellationToken);

        int gifts = await documents.CountAsync(i => i.Type == InvoiceType.Gift, cancellationToken);

        var empties = await documents
            .GroupBy(_ => 1)
            .Select(g => new
            {
                GeneratedBoxes = g.Sum(i => i.GeneratedEmptyBoxes),
                GeneratedUnits = g.Sum(i => i.GeneratedEmptyUnits),
                ReturnedBoxes = g.Sum(i => i.ReturnedEmptyBoxes),
                ReturnedUnits = g.Sum(i => i.ReturnedEmptyUnits)
            })
            .FirstOrDefaultAsync(cancellationToken);

        decimal payments = await context.ClientMovements
            .AsNoTracking()
            .Where(m => m.Type == ClientMovementType.Payment && m.CreatedAt >= startUtc && m.CreatedAt < endUtc)
            .SumAsync(m => m.Amount, cancellationToken);

        decimal consumption = await context.InternalConsumptions
            .AsNoTracking()
            .Where(c => c.OccurredAt >= startUtc && c.OccurredAt < endUtc)
            .SumAsync(c => c.EstimatedValue, cancellationToken);

        decimal damages = await context.DamagedProducts
            .AsNoTracking()
            .Where(d => d.OccurredAt >= startUtc && d.OccurredAt < endUtc)
            .SumAsync(d => d.EstimatedValue, cancellationToken);

        return new DailyReportResponse(
            date,
            sales?.Total ?? 0, sales?.Count ?? 0, gifts, sales?.Paid ?? 0, sales?.Pending ?? 0, payments,
            empties?.GeneratedBoxes ?? 0, empties?.GeneratedUnits ?? 0, empties?.ReturnedBoxes ?? 0, empties?.ReturnedUnits ?? 0,
            consumption, damages);
    }

    public async Task<InventorySummaryResponse> GetInventorySummaryAsync(CancellationToken cancellationToken = default)
    {
        var warehouse = await context.Products
            .AsNoTracking()
            .GroupBy(p => p.Category!.Name)
            .Select(g => new { Category = g.Key, Units = g.Sum(p => p.StockUnits), Value = g.Sum(p => p.StockUnits * p.PriceUnit) })
            .ToListAsync(cancellationToken);

        var truckLines = await (
                from s in context.TruckStocks.AsNoTracking()
                join t in context.Trucks on s.TruckId equals t.Id
                select new
                {
                    TruckId = t.Id,
                    TruckName = t.Name,
                    Category = s.Product!.Category!.Name,
                    s.StockUnits,
                    Value = s.StockUnits * s.Product!.PriceUnit
                })
            .ToListAsync(cancellationToken);

        var categories = warehouse.Select(w => w.Category).Union(truckLines.Select(l => l.Category)).Distinct().OrderBy(c => c);
        var byCategory = categories.Select(c => new CategoryInventoryResponse(
                c,
                warehouse.Where(w => w.Category == c).Sum(w => w.Units),
                warehouse.Where(w => w.Category == c).Sum(w => w.Value),
                truckLines.Where(l => l.Category == c).Sum(l => l.StockUnits),
                truckLines.Where(l => l.Category == c).Sum(l => l.Value)))
            .ToList();

        var trucks = await context.Trucks.AsNoTracking().OrderBy(t => t.Name).Select(t => new { t.Id, t.Name }).ToListAsync(cancellationToken);
        var byTruck = trucks.Select(t => new TruckInventoryResponse(
                t.Id, t.Name,
                truckLines.Where(l => l.TruckId == t.Id).Sum(l => l.StockUnits),
                truckLines.Where(l => l.TruckId == t.Id).Sum(l => l.Value)))
            .ToList();

        int warehouseUnits = warehouse.Sum(w => w.Units);
        decimal warehouseValue = warehouse.Sum(w => w.Value);
        int truckUnits = truckLines.Sum(l => l.StockUnits);
        decimal truckValue = truckLines.Sum(l => l.Value);

        return new InventorySummaryResponse(
            warehouseUnits, warehouseValue, truckUnits, truckValue,
            warehouseUnits + truckUnits, warehouseValue + truckValue, byCategory, byTruck);
    }

    public async Task<(decimal MoneyOnStreet, int EmptyBoxesOnStreet, int EmptyUnitsOnStreet, int ReceivableClients)> GetStreetBalancesAsync(
        CancellationToken cancellationToken = default)
    {
        var clients = context.Clients.AsNoTracking();
        decimal money = await clients.Where(c => c.MoneyBalance < 0).SumAsync(c => -c.MoneyBalance, cancellationToken);
        int boxes = await clients.Where(c => c.EmptyBoxesBalance < 0).SumAsync(c => -c.EmptyBoxesBalance, cancellationToken);
        int units = await clients.Where(c => c.EmptyUnitsBalance < 0).SumAsync(c => -c.EmptyUnitsBalance, cancellationToken);
        int receivable = await clients.CountAsync(c => c.MoneyBalance < 0 || c.EmptyBoxesBalance < 0 || c.EmptyUnitsBalance < 0, cancellationToken);
        return (money, boxes, units, receivable);
    }

    public async Task<DashboardData> GetDashboardDataAsync(
        DateTime startUtc, DateTime endUtc, Guid? categoryId, CancellationToken cancellationToken = default)
    {
        var products = await context.Products
            .AsNoTracking()
            .Where(p => p.IsActive && (categoryId == null || p.CategoryId == categoryId))
            .Select(p => new DashboardProductRow(
                p.Id, p.SKU, p.Name, p.CategoryId, p.Category!.Name, p.PriceUnit, p.CostPrice, p.UnitsPerBox, p.StockUnits,
                context.TruckStocks.Where(s => s.ProductId == p.Id).Sum(s => (int?)s.StockUnits) ?? 0,
                p.MinStock, p.MaxStock))
            .ToListAsync(cancellationToken);

        // Los renglones guardan la copia del producto; la categoría se toma del producto aunque esté eliminado.
        var saleLines = await (
                from i in context.Invoices.AsNoTracking()
                where !i.IsCancelled && i.IssuedAt >= startUtc && i.IssuedAt < endUtc
                from it in i.Items
                join p in context.Products.IgnoreQueryFilters() on it.ProductId equals p.Id
                where categoryId == null || p.CategoryId == categoryId
                select new DashboardSaleLine(
                    i.Id, i.IssuedAt, i.Type == InvoiceType.Gift, i.TruckId, it.ProductId, it.SKU, it.ProductName,
                    p.CategoryId, p.Category!.Name, it.TotalUnits, it.Subtotal))
            .ToListAsync(cancellationToken);

        var collections = await context.Invoices
            .AsNoTracking()
            .Where(i => !i.IsCancelled && i.Type == InvoiceType.Sale && i.IssuedAt >= startUtc && i.IssuedAt < endUtc)
            .GroupBy(_ => 1)
            .Select(g => new DashboardCollections(g.Sum(i => i.Payment), g.Sum(i => i.Pending)))
            .FirstOrDefaultAsync(cancellationToken);

        var trucks = await context.Trucks
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new DashboardTruckRow(
                t.Id, t.Name, t.Plate, t.CapacityBoxes,
                context.TruckStocks.Where(s => s.TruckId == t.Id).Sum(s => (decimal?)s.StockUnits / s.Product!.UnitsPerBox) ?? 0,
                context.TruckStocks.Where(s => s.TruckId == t.Id).Sum(s => (int?)s.StockUnits) ?? 0,
                context.TruckStocks.Where(s => s.TruckId == t.Id).Sum(s => (decimal?)(s.StockUnits * s.Product!.PriceUnit)) ?? 0))
            .ToListAsync(cancellationToken);

        return new DashboardData(products, saleLines, collections ?? new DashboardCollections(0, 0), trucks);
    }

    public async Task<ProductLocationResponse?> GetProductLocationAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await context.Products
            .AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.Id, p.Name, p.StockUnits, p.PriceUnit })
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            return null;
        }

        var trucks = await (
                from s in context.TruckStocks.AsNoTracking()
                join t in context.Trucks on s.TruckId equals t.Id
                where s.ProductId == productId
                orderby t.Name
                select new TruckInventoryResponse(t.Id, t.Name, s.StockUnits, s.StockUnits * product.PriceUnit))
            .ToListAsync(cancellationToken);

        return new ProductLocationResponse(product.Id, product.Name, product.StockUnits, trucks, product.StockUnits + trucks.Sum(t => t.Units));
    }
}
