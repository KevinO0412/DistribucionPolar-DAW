using Core.Application.Abstractions;
using Core.Application.DTOs;
using Core.Application.Mappings;
using Core.Domain.Entities;
using FluentValidation;

namespace Core.Application.Services;

public sealed class PurchaseService(
    IPurchaseRepository purchases,
    IProductRepository products,
    IEmployeeRepository employees,
    IDocumentNumberGenerator numbers,
    IUnitOfWork unitOfWork,
    IAuditService audit,
    IClock clock,
    IValidator<CreatePurchaseRequest> validator) : IPurchaseService
{
    /// <summary>Entrada de mercancía (solo Admin): suma el stock al galpón y numera "Compra #0001".</summary>
    public async Task<PurchaseResponse> CreateAsync(CreatePurchaseRequest request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var purchase = await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var employee = await employees.GetByIdAsync(request.EmployeeId, ct)
                ?? throw new InvalidOperationException($"El empleado {request.EmployeeId} no existe.");
            if (!employee.IsActive)
            {
                throw new InvalidOperationException($"El empleado {employee.Name} está inactivo.");
            }

            var lines = await StockOperations.LoadLinesAsync(products, request.Items, ct);
            int sequence = await numbers.NextAsync(DocumentSequences.Purchase, 0, 0, 0, ct);
            var created = new Purchase(sequence, employee, request.DocumentType, request.DocumentNumber, lines, clock.UtcNow);
            foreach (var line in lines)
            {
                line.Product.AddStock(line.Quantity);
            }

            purchases.Add(created);
            await unitOfWork.SaveChangesAsync(ct);
            return created;
        }, cancellationToken);

        await audit.AuditAsync("purchase_created", "purchases", $"{purchase.Number}: {purchase.TotalUnits} und",
            nameof(Purchase), purchase.Id.ToString(), purchase.TotalAmount,
            metadata: new { purchase.DocumentType, purchase.DocumentNumber, purchase.EmployeeName }, cancellationToken: cancellationToken);
        return purchase.ToResponse();
    }

    public async Task<PurchaseResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var purchase = await purchases.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No existe la compra {id}.");
        return purchase.ToResponse();
    }

    public async Task<IReadOnlyList<PurchaseResponse>> ListAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        DateTime? fromUtc = from is null ? null : clock.GetDayRangeUtc(from.Value).StartUtc;
        DateTime? toUtc = to is null ? null : clock.GetDayRangeUtc(to.Value).EndUtc;
        var list = await purchases.ListAsync(fromUtc, toUtc, cancellationToken);
        return list.Select(p => p.ToResponse()).ToList();
    }
}
