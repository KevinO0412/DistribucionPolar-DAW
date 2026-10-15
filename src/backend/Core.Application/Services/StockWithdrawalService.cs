using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using FluentValidation;

namespace Core.Application.Services;

/// <summary>Averías y consumos internos (solo Admin): descuentan stock del galpón o de un camión.</summary>
public sealed class StockWithdrawalService(
    IDamagedProductRepository damages,
    IInternalConsumptionRepository consumptions,
    IProductRepository products,
    ITruckRepository trucks,
    IEmployeeRepository employees,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IClock clock,
    IValidator<CreateDamageRequest> damageValidator,
    IValidator<CreateInternalConsumptionRequest> consumptionValidator) : IStockWithdrawalService
{
    public async Task<DamageResponse> RegisterDamageAsync(CreateDamageRequest request, CancellationToken cancellationToken = default)
    {
        await damageValidator.ValidateAndThrowAsync(request, cancellationToken);

        var damage = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var (product, truck, quantity) = await LoadAndRemoveAsync(request.ProductId, request.Boxes, request.LooseUnits, request.Origin, request.TruckId, ct);
            var created = new DamagedProduct(product, quantity, request.Origin, truck, request.Type, request.Reason,
                request.ActionTaken, request.Observation, clock.UtcNow);
            damages.Add(created);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("damage_registered", "damages", $"Avería de {damage.TotalUnits} und de {damage.ProductName}",
            nameof(DamagedProduct), damage.Id.ToString(), damage.EstimatedLoss,
            metadata: new { damage.Type, damage.Reason, damage.ActionTaken, damage.Origin, damage.TruckName }, cancellationToken: cancellationToken);
        return damage.ToResponse();
    }

    public async Task<WithdrawalListResponse<DamageResponse>> ListDamagesAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var (fromUtc, toUtc) = Range(from, to);
        var list = (await damages.ListAsync(fromUtc, toUtc, cancellationToken)).Select(d => d.ToResponse()).ToList();
        return new WithdrawalListResponse<DamageResponse>(list, list.Count, list.Sum(d => d.EstimatedLoss));
    }

    public async Task<InternalConsumptionResponse> RegisterConsumptionAsync(CreateInternalConsumptionRequest request, CancellationToken cancellationToken = default)
    {
        await consumptionValidator.ValidateAndThrowAsync(request, cancellationToken);

        var consumption = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var employee = await employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new InvalidOperationException($"El empleado {request.EmployeeId} no existe.");
            if (!employee.IsActive)
            {
                throw new InvalidOperationException($"El empleado {employee.Name} está inactivo.");
            }

            var (product, truck, quantity) = await LoadAndRemoveAsync(request.ProductId, request.Boxes, request.LooseUnits, request.Origin, request.TruckId, ct);
            var created = new InternalConsumption(product, quantity, request.Origin, truck, employee, request.Type, request.Observation, clock.UtcNow);
            consumptions.Add(created);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("internal_consumption_registered", "internal_consumptions",
            $"{consumption.Type}: {consumption.TotalUnits} und de {consumption.ProductName} para {consumption.EmployeeName}",
            nameof(InternalConsumption), consumption.Id.ToString(), consumption.EstimatedValue, cancellationToken: cancellationToken);
        return consumption.ToResponse();
    }

    public async Task<WithdrawalListResponse<InternalConsumptionResponse>> ListConsumptionsAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var (fromUtc, toUtc) = Range(from, to);
        var list = (await consumptions.ListAsync(fromUtc, toUtc, cancellationToken)).Select(c => c.ToResponse()).ToList();
        return new WithdrawalListResponse<InternalConsumptionResponse>(list, list.Count, list.Sum(c => c.EstimatedValue));
    }

    /// <summary>Carga producto y camión y descuenta el stock; si no alcanza falla con "Inventario insuficiente".</summary>
    private async Task<(Product Product, Truck? Truck, BoxQuantity Quantity)> LoadAndRemoveAsync(
        Guid productId, int boxes, int looseUnits, Core.Domain.Enums.StockOrigin origin, Guid? truckId, CancellationToken cancellationToken)
    {
        var product = await products.GetForUpdateAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"El producto {productId} no existe.");
        var truck = await StockOperations.LoadOriginTruckAsync(trucks, origin, truckId, cancellationToken);
        var quantity = new BoxQuantity(boxes, looseUnits);

        StockOperations.EnsureAvailable(origin, truck, [new ProductLine(product, quantity)]);
        StockOperations.Remove(origin, truck, product, quantity);
        return (product, truck, quantity);
    }

    private (DateTime? FromUtc, DateTime? ToUtc) Range(DateOnly? from, DateOnly? to) =>
        (from is null ? null : clock.GetDayRangeUtc(from.Value).StartUtc, to is null ? null : clock.GetDayRangeUtc(to.Value).EndUtc);
}
