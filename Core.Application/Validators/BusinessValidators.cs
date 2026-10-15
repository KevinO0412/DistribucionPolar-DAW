using Core.Application.DTOs;
using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using FluentValidation;

namespace Core.Application.Validators;

/// <summary>Producto + cantidad: cajas y sueltas no negativas y no ambas en 0.</summary>
public sealed class LineRequestValidator : AbstractValidator<LineRequest>
{
    public LineRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.Boxes > 0 || x.LooseUnits > 0)
            .WithName("Cantidad")
            .WithMessage("Indique cajas o unidades sueltas (no pueden ser ambas 0).");
    }
}

public sealed class EmptyGroupQuantityValidator : AbstractValidator<EmptyGroupQuantity>
{
    public EmptyGroupQuantityValidator()
    {
        RuleFor(x => x.GroupKey)
            .Must(EmptyReturnGroups.Exists)
            .WithMessage($"El grupo de vacíos no existe. Valores permitidos: {string.Join(", ", EmptyReturnGroups.All.Keys)}.");
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Units).GreaterThanOrEqualTo(0);
    }
}

public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Employee.NameMaxLength);
        RuleFor(x => x.Role).MaximumLength(Employee.RoleMaxLength);
    }
}

public sealed class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Employee.NameMaxLength);
        RuleFor(x => x.Role).MaximumLength(Employee.RoleMaxLength);
    }
}

public sealed class TruckRequestValidator : AbstractValidator<TruckRequest>
{
    public TruckRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Truck.NameMaxLength);
        RuleFor(x => x.Plate).NotEmpty().MaximumLength(Truck.PlateMaxLength);
        RuleFor(x => x.CapacityBoxes).InclusiveBetween(1, Truck.MaxCapacityBoxes);
    }
}

public sealed class LoadTruckRequestValidator : AbstractValidator<LoadTruckRequest>
{
    public LoadTruckRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Mode).IsInEnum();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.Boxes > 0 || x.LooseUnits > 0)
            .WithName("Cantidad")
            .WithMessage("Indique cajas o unidades sueltas (no pueden ser ambas 0).");
        RuleFor(x => x.Observation).MaximumLength(TruckLoad.ObservationMaxLength);
    }
}

public sealed class TransferFromTruckRequestValidator : AbstractValidator<TransferFromTruckRequest>
{
    public TransferFromTruckRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.Boxes > 0 || x.LooseUnits > 0)
            .WithName("Cantidad")
            .WithMessage("Indique cajas o unidades sueltas (no pueden ser ambas 0).");
        RuleFor(x => x.Destination).IsInEnum();
        RuleFor(x => x.DestinationTruckId)
            .NotEmpty()
            .When(x => x.Destination == StockOrigin.Truck)
            .WithMessage("Indique el camión destino.");
        RuleFor(x => x.Observation).MaximumLength(TruckLoad.ObservationMaxLength);
    }
}

public sealed class AdjustStockRequestValidator : AbstractValidator<AdjustStockRequest>
{
    public AdjustStockRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class AdjustWarehouseStockRequestValidator : AbstractValidator<AdjustWarehouseStockRequest>
{
    public AdjustWarehouseStockRequestValidator()
    {
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.Rif).NotEmpty().MaximumLength(Client.RifMaxLength);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Client.NameMaxLength);
        RuleFor(x => x.Address).MaximumLength(Client.AddressMaxLength);
        RuleFor(x => x.Nickname).MaximumLength(Client.NicknameMaxLength);
    }
}

public sealed class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(x => x.Address).MaximumLength(Client.AddressMaxLength);
        RuleFor(x => x.Nickname).MaximumLength(Client.NicknameMaxLength);
        RuleFor(x => x.Type).MaximumLength(Client.TypeMaxLength);
    }
}

public sealed class ClientPaymentRequestValidator : AbstractValidator<ClientPaymentRequest>
{
    public ClientPaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.Observation).MaximumLength(500);
    }
}

public sealed class ClientEmptiesReturnRequestValidator : AbstractValidator<ClientEmptiesReturnRequest>
{
    public ClientEmptiesReturnRequestValidator()
    {
        RuleFor(x => x.Groups).NotEmpty();
        RuleForEach(x => x.Groups).SetValidator(new EmptyGroupQuantityValidator());
        RuleFor(x => x.Groups)
            .Must(g => g is not null && g.Any(q => q.Boxes > 0 || q.Units > 0))
            .WithMessage("Indique al menos una caja o unidad devuelta.");
        RuleFor(x => x.Groups)
            .Must(g => g is null || g.Select(q => q.GroupKey).Distinct().Count() == g.Count)
            .WithMessage("Cada grupo de vacíos debe aparecer una sola vez.");
        RuleFor(x => x.Observation).MaximumLength(500);
    }
}

public sealed class ClientBalanceAdjustmentRequestValidator : AbstractValidator<ClientBalanceAdjustmentRequest>
{
    public ClientBalanceAdjustmentRequestValidator()
    {
        RuleFor(x => x.MoneyBalance).PrecisionScale(18, 2, true);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.DispatchOrigin).IsInEnum();
        RuleFor(x => x.TruckId)
            .NotEmpty()
            .When(x => x.DispatchOrigin == StockOrigin.Truck)
            .WithMessage("Indique el camión cuando el despacho sale de un camión.");
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new LineRequestValidator());
        RuleFor(x => x.Items)
            .Must(items => items is null || items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Cada producto debe aparecer una sola vez en la factura.");
        RuleFor(x => x.Payment).GreaterThanOrEqualTo(0).PrecisionScale(18, 2, true);
        RuleFor(x => x.PaymentObservation).MaximumLength(Invoice.ObservationMaxLength);
        RuleForEach(x => x.ReturnedEmpties).SetValidator(new EmptyGroupQuantityValidator());
        RuleFor(x => x.ReturnedEmpties)
            .Must(g => g is null || g.Select(q => q.GroupKey).Distinct().Count() == g.Count)
            .WithMessage("Cada grupo de vacíos debe aparecer una sola vez.");
    }
}

public sealed class CreateGiftRequestValidator : AbstractValidator<CreateGiftRequest>
{
    public CreateGiftRequestValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.DispatchOrigin).IsInEnum();
        RuleFor(x => x.TruckId)
            .NotEmpty()
            .When(x => x.DispatchOrigin == StockOrigin.Truck)
            .WithMessage("Indique el camión cuando el despacho sale de un camión.");
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new LineRequestValidator());
        RuleFor(x => x.Items)
            .Must(items => items is null || items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Cada producto debe aparecer una sola vez en la regalía.");
    }
}

public sealed class CancelInvoiceRequestValidator : AbstractValidator<CancelInvoiceRequest>
{
    public CancelInvoiceRequestValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(Invoice.CancellationReasonMaxLength);
    }
}

public sealed class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.DocumentType).IsInEnum();
        RuleFor(x => x.DocumentNumber).MaximumLength(Purchase.DocumentNumberMaxLength);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new LineRequestValidator());
        RuleFor(x => x.Items)
            .Must(items => items is null || items.Select(i => i.ProductId).Distinct().Count() == items.Count)
            .WithMessage("Cada producto debe aparecer una sola vez en la compra.");
    }
}

public sealed class CreateDamageRequestValidator : AbstractValidator<CreateDamageRequest>
{
    public CreateDamageRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.Boxes > 0 || x.LooseUnits > 0)
            .WithName("Cantidad")
            .WithMessage("Indique cajas o unidades sueltas (no pueden ser ambas 0).");
        RuleFor(x => x.Origin).IsInEnum();
        RuleFor(x => x.TruckId).NotEmpty().When(x => x.Origin == StockOrigin.Truck).WithMessage("Indique el camión.");
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Reason).IsInEnum();
        RuleFor(x => x.ActionTaken).IsInEnum();
        RuleFor(x => x.Observation).MaximumLength(StockWithdrawal.ObservationMaxLength);
    }
}

public sealed class CreateInternalConsumptionRequestValidator : AbstractValidator<CreateInternalConsumptionRequest>
{
    public CreateInternalConsumptionRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.Boxes > 0 || x.LooseUnits > 0)
            .WithName("Cantidad")
            .WithMessage("Indique cajas o unidades sueltas (no pueden ser ambas 0).");
        RuleFor(x => x.Origin).IsInEnum();
        RuleFor(x => x.TruckId).NotEmpty().When(x => x.Origin == StockOrigin.Truck).WithMessage("Indique el camión.");
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Observation).MaximumLength(StockWithdrawal.ObservationMaxLength);
    }
}

public sealed class CreateConsignmentRequestValidator : AbstractValidator<CreateConsignmentRequest>
{
    public CreateConsignmentRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(ConsignmentEvent.NameMaxLength);
        RuleFor(x => x.Responsible).NotEmpty().MaximumLength(ConsignmentEvent.ResponsibleMaxLength);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new LineRequestValidator());
    }
}

public sealed class ConsignmentReturnRequestValidator : AbstractValidator<ConsignmentReturnRequest>
{
    public ConsignmentReturnRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Boxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LooseUnits).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.Boxes > 0 || x.LooseUnits > 0)
            .WithName("Cantidad")
            .WithMessage("Indique cajas o unidades sueltas (no pueden ser ambas 0).");
    }
}

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(User.UsernameMaxLength);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(User.EmailMaxLength);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(User.FullNameMaxLength);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(128);
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.AttendantName).MaximumLength(User.AttendantNameMaxLength);
    }
}

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(User.EmailMaxLength);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(User.FullNameMaxLength);
        RuleFor(x => x.Role).IsInEnum();
        RuleFor(x => x.AttendantName).MaximumLength(User.AttendantNameMaxLength);
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(128);
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(128)
            .NotEqual(x => x.CurrentPassword).WithMessage("La nueva clave debe ser distinta de la actual.");
    }
}
