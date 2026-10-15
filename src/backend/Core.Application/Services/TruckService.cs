using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

public sealed class TruckService(
    ITruckRepository trucks,
    IProductRepository products,
    IInvoiceRepository invoices,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    ICurrentUser currentUser,
    IClock clock,
    IValidator<TruckRequest> truckValidator,
    IValidator<LoadTruckRequest> loadValidator,
    IValidator<TransferFromTruckRequest> transferValidator,
    IValidator<AdjustStockRequest> adjustValidator,
    IValidator<SettleRouteRequest> settleValidator) : ITruckService
{
    public async Task<IReadOnlyList<TruckResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await trucks.GetAllAsync(cancellationToken);
        return list.Select(t => t.ToResponse()).ToList();
    }

    public async Task<TruckResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var truck = await trucks.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el camión {id}.");
        return truck.ToResponse();
    }

    public async Task<TruckResponse> CreateAsync(TruckRequest request, CancellationToken cancellationToken = default)
    {
        await truckValidator.ValidateAndThrowAsync(request, cancellationToken);
        if (await trucks.PlateExistsAsync(request.Plate.Trim().ToUpperInvariant(), null, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe un camión con la placa {request.Plate}.");
        }

        var truck = new Truck(request.Name, request.Plate, request.CapacityBoxes);
        trucks.Add(truck);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("truck_created", "trucks", $"Camión creado: {truck.Name} ({truck.Plate})",
            nameof(Truck), truck.Id.ToString(), cancellationToken: cancellationToken);
        return truck.ToResponse();
    }

    public async Task<TruckResponse> UpdateAsync(Guid id, TruckRequest request, CancellationToken cancellationToken = default)
    {
        await truckValidator.ValidateAndThrowAsync(request, cancellationToken);
        var truck = await trucks.GetForUpdateAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el camión {id}.");
        if (await trucks.PlateExistsAsync(request.Plate.Trim().ToUpperInvariant(), id, cancellationToken))
        {
            throw new InvalidOperationException($"Ya existe un camión con la placa {request.Plate}.");
        }

        truck.Update(request.Name, request.Plate, request.CapacityBoxes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("truck_updated", "trucks", $"Camión actualizado: {truck.Name} ({truck.Plate})",
            nameof(Truck), truck.Id.ToString(), cancellationToken: cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    /// <summary>
    /// Carga desde el galpón. Modo Add: agrega la cantidad. Modo Target: la cantidad es la meta final
    /// y se carga la diferencia; si el camión ya tiene esa cantidad o más, se rechaza.
    /// </summary>
    public async Task<TruckResponse> LoadAsync(Guid truckId, LoadTruckRequest request, CancellationToken cancellationToken = default)
    {
        await loadValidator.ValidateAndThrowAsync(request, cancellationToken);

        var load = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var truck = await trucks.GetForUpdateAsync(truckId, ct)
                ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
            var product = await products.GetForUpdateAsync(request.ProductId, ct)
                ?? throw new InvalidOperationException($"El producto {request.ProductId} no existe.");

            int requested = new BoxQuantity(request.Boxes, request.LooseUnits).ToUnits(product.UnitsPerBox);
            int before = truck.GetUnits(product.Id);
            int toLoad = request.Mode == TruckLoadMode.Target ? requested - before : requested;
            if (toLoad <= 0)
            {
                throw new DomainException("El camión ya tiene esa cantidad o más.");
            }

            product.RemoveStock(BoxQuantity.FromUnits(toLoad, product.UnitsPerBox));
            truck.AddStock(product.Id, toLoad);
            truck.MarkLoading();

            var record = new TruckLoad(truck, product, toLoad, before, truck.GetUnits(product.Id),
                TruckLoadSource.Warehouse, null, request.Observation, currentUser.Username);
            trucks.AddLoad(record);
            await unitOfWork.SaveChangesAsync(ct);
            return record;
        }, cancellationToken);

        await audit.AuditAsync("truck_loaded", "trucks",
            $"Carga de {load.TotalUnits} und de {load.ProductName} al camión {load.TruckName} desde el galpón",
            nameof(Truck), truckId.ToString(), metadata: load.ToResponse(), cancellationToken: cancellationToken);
        return await GetByIdAsync(truckId, cancellationToken);
    }

    /// <summary>Desmonta mercancía del camión hacia el galpón o hacia otro camión.</summary>
    public async Task<TruckResponse> TransferAsync(Guid truckId, TransferFromTruckRequest request, CancellationToken cancellationToken = default)
    {
        await transferValidator.ValidateAndThrowAsync(request, cancellationToken);
        if (request.Destination == StockOrigin.Truck && request.DestinationTruckId == truckId)
        {
            throw new InvalidOperationException("El camión destino debe ser distinto del camión de origen.");
        }

        string description = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var truck = await trucks.GetForUpdateAsync(truckId, ct)
                ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
            var product = await products.GetForUpdateAsync(request.ProductId, ct)
                ?? throw new InvalidOperationException($"El producto {request.ProductId} no existe.");

            int units = new BoxQuantity(request.Boxes, request.LooseUnits).ToUnits(product.UnitsPerBox);
            truck.RemoveStock(product.Id, units);

            string destinationName;
            if (request.Destination == StockOrigin.Warehouse)
            {
                product.AddStock(BoxQuantity.FromUnits(units, product.UnitsPerBox));
                destinationName = "el galpón";
            }
            else
            {
                var destination = await trucks.GetForUpdateAsync(request.DestinationTruckId!.Value, ct)
                    ?? throw new InvalidOperationException($"El camión destino {request.DestinationTruckId} no existe.");
                int before = destination.GetUnits(product.Id);
                destination.AddStock(product.Id, units);
                trucks.AddLoad(new TruckLoad(destination, product, units, before, destination.GetUnits(product.Id),
                    TruckLoadSource.Truck, truck.Id, request.Observation, currentUser.Username));
                destinationName = $"el camión {destination.Name}";
            }

            await unitOfWork.SaveChangesAsync(ct);
            return $"Transferencia de {units} und de {product.Name} desde el camión {truck.Name} hacia {destinationName}";
        }, cancellationToken);

        await audit.AuditAsync("truck_transfer", "trucks", description, nameof(Truck), truckId.ToString(),
            metadata: request, cancellationToken: cancellationToken);
        return await GetByIdAsync(truckId, cancellationToken);
    }

    /// <summary>Ajuste manual (solo Admin): fija la cantidad absoluta de un producto en el camión.</summary>
    public async Task<TruckResponse> AdjustAsync(Guid truckId, AdjustStockRequest request, CancellationToken cancellationToken = default)
    {
        await adjustValidator.ValidateAndThrowAsync(request, cancellationToken);

        var (productName, before, after) = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var truck = await trucks.GetForUpdateAsync(truckId, ct)
                ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
            var product = await products.GetByIdAsync(request.ProductId, ct)
                ?? throw new InvalidOperationException($"El producto {request.ProductId} no existe.");

            int previous = truck.GetUnits(product.Id);
            truck.SetStock(product.Id, new BoxQuantity(request.Boxes, request.LooseUnits).ToUnits(product.UnitsPerBox));
            await unitOfWork.SaveChangesAsync(ct);
            return (product.Name, previous, truck.GetUnits(product.Id));
        }, cancellationToken);

        await audit.AuditAsync("truck_stock_adjusted", "trucks",
            $"Ajuste manual de {productName} en camión: {before} und → {after} und. Motivo: {request.Reason}",
            nameof(Truck), truckId.ToString(), before: new { Units = before }, after: new { Units = after },
            metadata: new { request.Reason }, cancellationToken: cancellationToken);
        return await GetByIdAsync(truckId, cancellationToken);
    }

    /// <summary>Vacía el camión devolviendo toda su mercancía al galpón (solo Admin).</summary>
    public async Task<TruckResponse> UnloadAsync(Guid truckId, CancellationToken cancellationToken = default)
    {
        var unloaded = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var truck = await trucks.GetForUpdateAsync(truckId, ct)
                ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
            var lines = truck.Unload();
            var found = await products.GetManyForUpdateAsync(lines.Select(l => l.ProductId), ct);
            foreach (var (productId, units) in lines)
            {
                var product = found[productId];
                product.AddStock(BoxQuantity.FromUnits(units, product.UnitsPerBox));
            }

            await unitOfWork.SaveChangesAsync(ct);
            return lines;
        }, cancellationToken);

        await audit.AuditAsync("truck_unloaded", "trucks",
            $"Camión vaciado: {unloaded.Sum(l => l.Units)} und devueltas al galpón",
            nameof(Truck), truckId.ToString(), metadata: unloaded.Select(l => new { l.ProductId, l.Units }),
            cancellationToken: cancellationToken);
        return await GetByIdAsync(truckId, cancellationToken);
    }

    /// <summary>Sale a ruta: el camión pasa a "En ruta" y empieza el período que se liquidará.</summary>
    public async Task<TruckResponse> DispatchAsync(Guid truckId, CancellationToken cancellationToken = default)
    {
        var truck = await trucks.GetForUpdateAsync(truckId, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
        truck.Dispatch(clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await audit.AuditAsync("truck_dispatched", "trucks", $"Camión {truck.Name} sale a ruta con {truck.TotalUnits} und",
            nameof(Truck), truckId.ToString(), cancellationToken: cancellationToken);
        return await GetByIdAsync(truckId, cancellationToken);
    }

    /// <summary>
    /// Liquidación ciega: compara lo declarado con lo esperado (abonos de las facturas del camión desde su salida y la
    /// mercancía que debía traer), reintegra al galpón lo contado y deja la planilla de cuadre. Todo en una transacción.
    /// </summary>
    public async Task<RouteSettlementResponse> SettleAsync(Guid truckId, SettleRouteRequest request, CancellationToken cancellationToken = default)
    {
        await settleValidator.ValidateAndThrowAsync(request, cancellationToken);

        var settlement = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var truck = await trucks.GetForUpdateAsync(truckId, ct)
                ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
            if (!truck.IsOnRoute || truck.DispatchedAt is not { } dispatchedAt)
            {
                throw new DomainException($"Solo se liquida un camión en ruta; {truck.Name} está \"{truck.Status}\".");
            }

            var sales = await invoices.GetTruckRouteSalesAsync(truckId, dispatchedAt, ct);
            var expected = truck.Settle();
            var found = await products.GetManyForUpdateAsync(expected.Select(e => e.ProductId).Union(request.Returned.Select(r => r.ProductId)), ct);

            var counted = new Dictionary<Guid, int>();
            foreach (var line in request.Returned)
            {
                var product = found.GetValueOrDefault(line.ProductId)
                    ?? throw new InvalidOperationException($"El producto {line.ProductId} no existe.");
                counted[product.Id] = counted.GetValueOrDefault(product.Id) + new BoxQuantity(line.Boxes, line.LooseUnits).ToUnits(product.UnitsPerBox);
            }

            var created = RouteSettlement.Create(truck, dispatchedAt, found, expected, counted, request.CountedCash,
                sales.Payments, sales.Total, sales.Count, clock.UtcNow, currentUser.Username, request.Observation);

            // Lo contado físicamente vuelve al galpón; los faltantes quedan registrados en la planilla.
            foreach (var (productId, units) in counted.Where(c => c.Value > 0))
            {
                var product = found[productId];
                product.AddStock(BoxQuantity.FromUnits(units, product.UnitsPerBox));
            }

            trucks.AddSettlement(created);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("route_settled", "trucks",
            $"Liquidación de {settlement.TruckName}: dinero {(settlement.CashDifference >= 0 ? "+" : "")}{settlement.CashDifference:0.00}, " +
            $"faltante de mercancía {settlement.MissingGoodsValue:0.00}",
            nameof(RouteSettlement), settlement.Id.ToString(), settlement.CountedCash,
            metadata: new { settlement.ExpectedCash, settlement.CashDifference, settlement.MissingGoodsValue, settlement.IsBalanced },
            cancellationToken: cancellationToken);
        return settlement.ToResponse();
    }

    public async Task<IReadOnlyList<RouteSettlementResponse>> GetSettlementsAsync(
        Guid? truckId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        DateTime? fromUtc = from is null ? null : clock.GetDayRangeUtc(from.Value).StartUtc;
        DateTime? toUtc = to is null ? null : clock.GetDayRangeUtc(to.Value).EndUtc;
        var list = await trucks.GetSettlementsAsync(truckId, fromUtc, toUtc, cancellationToken);
        return list.Select(s => s.ToResponse()).ToList();
    }

    public async Task<IReadOnlyList<TruckLoadResponse>> GetLoadsAsync(
        Guid? truckId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        DateTime? fromUtc = from is null ? null : clock.GetDayRangeUtc(from.Value).StartUtc;
        DateTime? toUtc = to is null ? null : clock.GetDayRangeUtc(to.Value).EndUtc;
        var loads = await trucks.GetLoadsAsync(truckId, fromUtc, toUtc, cancellationToken);
        return loads.Select(l => l.ToResponse()).ToList();
    }
}
