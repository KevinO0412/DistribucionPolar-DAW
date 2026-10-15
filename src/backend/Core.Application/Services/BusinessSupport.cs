using System.Globalization;
using Core.Application.Abstractions;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Application.Services;

/// <summary>Operaciones de inventario compartidas por los procesos de negocio.</summary>
internal static class StockOperations
{
    /// <summary>Unidades disponibles del producto en el origen indicado.</summary>
    public static int Available(StockOrigin origin, Truck? truck, Product product) =>
        origin == StockOrigin.Warehouse ? product.StockUnits : truck!.GetUnits(product.Id);

    /// <summary>Descuenta mercancía del galpón o del camión; falla sin cambios si no alcanza.</summary>
    public static void Remove(StockOrigin origin, Truck? truck, Product product, BoxQuantity quantity)
    {
        if (origin == StockOrigin.Warehouse)
        {
            product.RemoveStock(quantity);
        }
        else
        {
            truck!.RemoveStock(product.Id, quantity.ToUnits(product.UnitsPerBox));
        }
    }

    /// <summary>Devuelve mercancía al galpón o al camión.</summary>
    public static void Add(StockOrigin origin, Truck? truck, Product product, int units)
    {
        if (origin == StockOrigin.Warehouse)
        {
            product.AddStock(BoxQuantity.FromUnits(units, product.UnitsPerBox));
        }
        else
        {
            truck!.AddStock(product.Id, units);
        }
    }

    /// <summary>Verifica que haya stock para TODAS las líneas antes de descontar; informa todas las faltas juntas.</summary>
    public static void EnsureAvailable(StockOrigin origin, Truck? truck, IEnumerable<ProductLine> lines)
    {
        var shortages = lines
            .Select(l => (l.Product, Requested: l.Quantity.ToUnits(l.Product.UnitsPerBox), Available: Available(origin, truck, l.Product)))
            .Where(x => x.Requested > x.Available)
            .Select(x => $"{x.Product.Name}: se solicitan {x.Requested} und y hay {x.Available} und")
            .ToList();

        if (shortages.Count > 0)
        {
            string where = origin == StockOrigin.Warehouse ? "el galpón" : $"el camión {truck!.Name}";
            throw new DomainException($"Inventario insuficiente en {where}. {string.Join("; ", shortages)}.");
        }
    }

    /// <summary>Carga el camión de origen si el origen es un camión.</summary>
    public static async Task<Truck?> LoadOriginTruckAsync(
        ITruckRepository trucks, StockOrigin origin, Guid? truckId, CancellationToken cancellationToken)
    {
        if (origin != StockOrigin.Truck)
        {
            return null;
        }

        if (truckId is null)
        {
            throw new InvalidOperationException("Indique el camión cuando el origen es un camión.");
        }

        return await trucks.GetForUpdateAsync(truckId.Value, cancellationToken)
            ?? throw new InvalidOperationException($"El camión {truckId} no existe.");
    }

    /// <summary>Carga con seguimiento los productos de las líneas y verifica que existan y estén activos.</summary>
    public static async Task<List<ProductLine>> LoadLinesAsync(
        IProductRepository products, IEnumerable<DTOs.LineRequest> items, CancellationToken cancellationToken)
    {
        var requested = items.ToList();
        var found = await products.GetManyForUpdateAsync(requested.Select(i => i.ProductId).Distinct(), cancellationToken);
        var lines = new List<ProductLine>();
        foreach (var item in requested)
        {
            if (!found.TryGetValue(item.ProductId, out var product) || product.IsDeleted)
            {
                throw new InvalidOperationException($"El producto {item.ProductId} no existe.");
            }

            if (!product.IsActive)
            {
                throw new InvalidOperationException($"El producto {product.Name} está inactivo.");
            }

            lines.Add(new ProductLine(product, new BoxQuantity(item.Boxes, item.LooseUnits)));
        }

        return lines;
    }
}

/// <summary>Textos y numeración en español.</summary>
internal static class SpanishText
{
    private static readonly CultureInfo Spanish = new("es-ES");

    /// <summary>Nombre del mes en español con mayúscula inicial ("Octubre").</summary>
    public static string MonthName(int month)
    {
        string name = Spanish.DateTimeFormat.GetMonthName(month);
        return char.ToUpper(name[0], Spanish) + name[1..];
    }

    /// <summary>"Factura Octubre 2026 #3" / "Regalía Octubre 2026 #1" (el año evita choques entre años).</summary>
    public static string InvoiceNumber(InvoiceType type, int year, int month, int sequence) =>
        $"{(type == InvoiceType.Sale ? "Factura" : "Regalía")} {MonthName(month)} {year} #{sequence}";

    public static string Quantity(int boxes, int units) => $"{boxes} cajas + {units} und";
}

/// <summary>Registro de auditoría desde los servicios.</summary>
internal static class AuditExtensions
{
    public static Task AuditAsync(
        this IAuditService audit,
        string action,
        string module,
        string description,
        string? entityName = null,
        string? entityId = null,
        decimal? amount = null,
        object? before = null,
        object? after = null,
        object? metadata = null,
        CancellationToken cancellationToken = default) =>
        audit.RecordAsync(new AuditEntry(action, module, description, entityName, entityId, amount, before, after, metadata), cancellationToken);
}
