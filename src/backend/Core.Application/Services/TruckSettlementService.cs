using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

/// <summary>
/// Liquidación ciega de ruta (observación 1). El formulario solo muestra qué productos llevó el camión; el chofer
/// declara el efectivo y la mercancía que devuelve y, al enviar, el sistema calcula las diferencias contra lo registrado.
/// </summary>
public sealed class TruckSettlementService(
    ITruckRepository trucks,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    ICurrentUser currentUser,
    IClock clock,
    IValidator<TruckSettlementRequest> validator) : ITruckSettlementService
{
    /// <summary>Productos a contar, sin cantidades ni cobros esperados: así el conteo es ciego.</summary>
    public async Task<SettlementFormResponse> GetFormAsync(Guid truckId, CancellationToken cancellationToken = default)
    {
        var truck = await trucks.GetByIdAsync(truckId, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
        if (!truck.IsOnRoute)
        {
            throw new InvalidOperationException($"El camión {truck.Name} no está en ruta; no hay ruta que liquidar.");
        }

        var productsToCount = new List<SettlementFormProduct>();
        foreach (var line in truck.Stock)
        {
            var product = line.Product ?? await products.GetByIdAsync(line.ProductId, cancellationToken)
                ?? throw new KeyNotFoundException($"No existe el producto {line.ProductId}.");
            productsToCount.Add(new SettlementFormProduct(product.Id, product.SKU, product.Name, product.UnitsPerBox));
        }

        productsToCount.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        return new SettlementFormResponse(truck.Id, truck.Name, truck.Plate, truck.Status, truck.DispatchedAt, productsToCount);
    }

    public async Task<TruckSettlementResponse> SettleAsync(Guid truckId, TruckSettlementRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var settlement = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var truck = await trucks.GetForUpdateAsync(truckId, ct)
                ?? throw new KeyNotFoundException($"No existe el camión {truckId}.");
            if (!truck.IsOnRoute || truck.DispatchedAt is null)
            {
                throw new InvalidOperationException($"El camión {truck.Name} no está en ruta; solo se liquida un camión que salió a ruta.");
            }

            var ids = truck.Stock.Select(s => s.ProductId).Concat(request.Items.Select(i => i.ProductId)).Distinct().ToList();
            var found = await products.GetManyForUpdateAsync(ids, ct);
            var counted = new Dictionary<Guid, int>();
            foreach (var item in request.Items)
            {
                var product = found.GetValueOrDefault(item.ProductId)
                    ?? throw new KeyNotFoundException($"No existe el producto {item.ProductId}.");
                counted[item.ProductId] = new BoxQuantity(item.Boxes, item.LooseUnits).ToUnits(product.UnitsPerBox);
            }

            var tripSales = await trucks.GetTripSalesAsync(new Dictionary<Guid, DateTime> { [truck.Id] = truck.DispatchedAt.Value }, ct);
            decimal expectedCash = tripSales.GetValueOrDefault(truck.Id)?.Collected ?? 0m;

            var created = TruckSettlement.Create(truck, expectedCash, request.CashCounted, counted, found, clock.UtcNow,
                currentUser.Username, request.Observation);
            trucks.AddSettlement(created);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("truck_settled", "trucks",
            $"Liquidación de {settlement.TruckName}: efectivo {BusinessMappings.Money(settlement.CashDifference)}, " +
            $"mercancía {BusinessMappings.Money(settlement.MerchandiseDifferenceValue)}",
            nameof(TruckSettlement), settlement.Id.ToString(), settlement.CashDifference,
            metadata: new { settlement.ExpectedCash, settlement.CountedCash, settlement.MerchandiseDifferenceValue, settlement.IsBalanced },
            cancellationToken: cancellationToken);
        return settlement.ToResponse();
    }

    public async Task<TruckSettlementResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var settlement = await trucks.GetSettlementAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la liquidación {id}.");
        return settlement.ToResponse();
    }

    public async Task<IReadOnlyList<TruckSettlementResponse>> ListAsync(Guid? truckId, CancellationToken cancellationToken = default)
    {
        var list = await trucks.GetSettlementsAsync(truckId, cancellationToken);
        return list.Select(s => s.ToResponse()).ToList();
    }
}
