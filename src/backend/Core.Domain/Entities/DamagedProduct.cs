using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>Producto averiado, no apto o desechado. Su valor estimado es la pérdida.</summary>
public sealed class DamagedProduct : StockWithdrawal
{
    // Constructor para EF Core.
    private DamagedProduct()
    {
    }

    public DamagedProduct(
        Product product,
        BoxQuantity quantity,
        StockOrigin origin,
        Truck? truck,
        DamageType type,
        DamageReason reason,
        DamageAction action,
        string? observation,
        DateTime occurredAt)
        : base(product, quantity, origin, truck, observation, occurredAt)
    {
        Type = Enum.IsDefined(type) ? type : throw new DomainException("Tipo de avería inválido.");
        Reason = Enum.IsDefined(reason) ? reason : throw new DomainException("Motivo de avería inválido.");
        ActionTaken = Enum.IsDefined(action) ? action : throw new DomainException("Acción tomada inválida.");
    }

    public DamageType Type { get; private set; }

    public DamageReason Reason { get; private set; }

    public DamageAction ActionTaken { get; private set; }

    /// <summary>Pérdida estimada (igual al valor estimado de la mercancía).</summary>
    public decimal EstimatedLoss => EstimatedValue;
}
