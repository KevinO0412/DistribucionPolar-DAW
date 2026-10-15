using System.Globalization;
using System.Text;
using System.Text.Json;
using Core.Application.Abstractions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

/// <summary>Respaldo de datos en JSON (todas las tablas de negocio) y CSV (por conjunto).</summary>
public sealed class BackupExporter(ApplicationDbContext context, TimeProvider timeProvider) : IBackupExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public async Task<byte[]> ExportJsonAsync(CancellationToken cancellationToken = default)
    {
        var backup = new
        {
            GeneratedAt = timeProvider.GetUtcNow().UtcDateTime,
            Categories = await context.Categories.AsNoTracking().ToListAsync(cancellationToken),
            Products = await context.Products.AsNoTracking().Select(p => new
            {
                p.Id, p.SKU, p.Name, p.Brand, p.CategoryId, p.PriceBox, p.PriceUnit, p.CostPrice, p.UnitsPerBox,
                p.StockUnits, p.MinStock, p.MaxStock, p.IsReturnable, p.EmptyGroupKey, p.IsGiftEligible, p.IsActive
            }).ToListAsync(cancellationToken),
            Employees = await context.Employees.AsNoTracking().ToListAsync(cancellationToken),
            Trucks = await context.Trucks.AsNoTracking().Include(t => t.Stock)
                .Select(t => new { t.Id, t.Name, t.Plate, t.Status, Stock = t.Stock.Select(s => new { s.ProductId, s.StockUnits }) })
                .ToListAsync(cancellationToken),
            TruckLoads = await context.TruckLoads.AsNoTracking().ToListAsync(cancellationToken),
            Clients = await context.Clients.AsNoTracking().Include(c => c.EmptyBalances).ToListAsync(cancellationToken),
            ClientMovements = await context.ClientMovements.AsNoTracking().ToListAsync(cancellationToken),
            Invoices = await context.Invoices.AsNoTracking().Include(i => i.Items).Include(i => i.EmptyGroups).AsSplitQuery().ToListAsync(cancellationToken),
            Purchases = await context.Purchases.AsNoTracking().Include(p => p.Items).ToListAsync(cancellationToken),
            DamagedProducts = await context.DamagedProducts.AsNoTracking().ToListAsync(cancellationToken),
            InternalConsumptions = await context.InternalConsumptions.AsNoTracking().ToListAsync(cancellationToken),
            ConsignmentEvents = await context.ConsignmentEvents.AsNoTracking().Include(c => c.Items).ToListAsync(cancellationToken),
            Users = await context.Users.AsNoTracking()
                .Select(u => new { u.Id, u.Username, u.Email, u.FullName, u.Role, u.IsActive, u.AttendantName, u.CanChangeAttendant })
                .ToListAsync(cancellationToken)
        };

        return JsonSerializer.SerializeToUtf8Bytes(backup, JsonOptions);
    }

    public async Task<byte[]> ExportCsvAsync(string dataset, CancellationToken cancellationToken = default)
    {
        IEnumerable<IEnumerable<object?>> rows;
        string[] header;
        switch (dataset.ToLowerInvariant())
        {
            case "products":
                header = ["SKU", "Nombre", "Marca", "Categoria", "PrecioCaja", "PrecioUnidad", "UnidadesPorCaja", "StockUnidades", "Minimo", "Maximo"];
                rows = (await context.Products.AsNoTracking().OrderBy(p => p.SKU)
                        .Select(p => new { p.SKU, p.Name, p.Brand, Category = p.Category!.Name, p.PriceBox, p.PriceUnit, p.UnitsPerBox, p.StockUnits, p.MinStock, p.MaxStock })
                        .ToListAsync(cancellationToken))
                    .Select(p => new object?[] { p.SKU, p.Name, p.Brand, p.Category, p.PriceBox, p.PriceUnit, p.UnitsPerBox, p.StockUnits, p.MinStock, p.MaxStock });
                break;
            case "clients":
                header = ["Codigo", "RIF", "Nombre", "Direccion", "Referencia", "Tipo", "SaldoDinero", "SaldoCajasVacios", "SaldoUnidadesVacios"];
                rows = (await context.Clients.AsNoTracking().OrderBy(c => c.Code).ToListAsync(cancellationToken))
                    .Select(c => new object?[] { c.FormattedCode, c.Rif, c.Name, c.Address, c.Nickname, c.Type, c.MoneyBalance, c.EmptyBoxesBalance, c.EmptyUnitsBalance });
                break;
            case "invoices":
                header = ["Numero", "Tipo", "Fecha", "Cliente", "Origen", "Vendedor", "Total", "Abono", "Pendiente", "Anulada"];
                rows = (await context.Invoices.AsNoTracking().OrderBy(i => i.IssuedAt).ToListAsync(cancellationToken))
                    .Select(i => new object?[] { i.Number, i.Type, i.IssuedAt.ToString("O"), i.ClientName, i.DispatchOrigin, i.AttendantName, i.Total, i.Payment, i.Pending, i.IsCancelled });
                break;
            case "movements":
                header = ["Fecha", "ClienteId", "Tipo", "Titulo", "Monto", "Descripcion"];
                rows = (await context.ClientMovements.AsNoTracking().OrderBy(m => m.CreatedAt).ToListAsync(cancellationToken))
                    .Select(m => new object?[] { m.CreatedAt.ToString("O"), m.ClientId, m.Type, m.Title, m.Amount, m.Description });
                break;
            default:
                throw new InvalidOperationException("Conjunto no válido. Use: products, clients, invoices o movements.");
        }

        var csv = new StringBuilder();
        csv.AppendLine(string.Join(';', header));
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(';', row.Select(Escape)));
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
    }

    private static string Escape(object? value)
    {
        string text = value switch
        {
            null => string.Empty,
            decimal d => d.ToString(CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
        return text.Contains(';') || text.Contains('"') || text.Contains('\n') ? $"\"{text.Replace("\"", "\"\"")}\"" : text;
    }
}
