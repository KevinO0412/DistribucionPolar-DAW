using Core.Domain.Common;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>
/// Evento a consignación: se entrega mercancía del galpón, se registran devoluciones y al cerrar
/// se cobra solo lo vendido (entregado − devuelto).
/// </summary>
public sealed class ConsignmentEvent : BaseEntity
{
    public const int NameMaxLength = 150;
    public const int ResponsibleMaxLength = 100;

    private readonly List<ConsignmentItem> items = [];

    // Constructor para EF Core.
    private ConsignmentEvent()
    {
    }

    public ConsignmentEvent(string name, string responsible, DateTime eventDate)
    {
        Name = Guard.RequiredText(name, "nombre del evento", NameMaxLength);
        Responsible = Guard.RequiredText(responsible, "responsable", ResponsibleMaxLength);
        EventDate = eventDate;
    }

    public string Name { get; private set; } = default!;

    public string Responsible { get; private set; } = default!;

    public DateTime EventDate { get; private set; }

    public bool IsClosed { get; private set; }

    public DateTime? ClosedAt { get; private set; }

    public IReadOnlyCollection<ConsignmentItem> Items => items.AsReadOnly();

    /// <summary>Total a cobrar: suma de lo vendido de cada producto.</summary>
    public decimal TotalSold => items.Sum(i => i.SoldAmount);

    /// <summary>Entrega mercancía; si el producto ya está en el evento, acumula.</summary>
    public void Deliver(Product product, BoxQuantity quantity)
    {
        EnsureOpen();
        int units = quantity.ToUnits(product.UnitsPerBox);
        Guard.Positive(units, "cantidad entregada");

        var item = items.FirstOrDefault(i => i.ProductId == product.Id);
        if (item is null)
        {
            items.Add(new ConsignmentItem(Id, product, units));
        }
        else
        {
            item.AddDelivered(units);
        }

        Touch();
    }

    /// <summary>Registra una devolución; no puede superar lo pendiente por devolver.</summary>
    public int RegisterReturn(Guid productId, BoxQuantity quantity)
    {
        EnsureOpen();
        var item = items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new DomainException("El producto no forma parte del evento.");

        int units = quantity.ToUnits(item.UnitsPerBox);
        Guard.Positive(units, "cantidad devuelta");
        if (units > item.PendingUnits)
        {
            throw new DomainException(
                $"La devolución ({units} und) supera lo pendiente por devolver de {item.ProductName} ({item.PendingUnits} und).");
        }

        item.AddReturned(units);
        Touch();
        return units;
    }

    public void Close(DateTime closedAt)
    {
        EnsureOpen();
        IsClosed = true;
        ClosedAt = closedAt;
        Touch();
    }

    private void EnsureOpen()
    {
        if (IsClosed)
        {
            throw new DomainException($"El evento '{Name}' está cerrado y no admite cambios.");
        }
    }
}
