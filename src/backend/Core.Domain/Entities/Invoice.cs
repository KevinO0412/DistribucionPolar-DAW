using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Exceptions;
using Core.Domain.ValueObjects;

namespace Core.Domain.Entities;

/// <summary>Línea solicitada para un documento: producto + cantidad en cajas y unidades sueltas.</summary>
public readonly record struct ProductLine(Product Product, BoxQuantity Quantity);

/// <summary>
/// Factura de venta o regalía. Guarda una copia (snapshot) del producto en cada línea, así que
/// cambiar precios después no altera documentos ya emitidos.
/// </summary>
public sealed class Invoice : BaseEntity
{
    public const int NumberMaxLength = 60;
    public const int AttendantMaxLength = 100;
    public const int ObservationMaxLength = 1000;
    public const int CancellationReasonMaxLength = 500;
    public const string GiftObservation = "Regalía a cliente - productos autorizados";

    private readonly List<InvoiceItem> items = [];
    private readonly List<InvoiceEmptyGroup> emptyGroups = [];

    // Constructor para EF Core.
    private Invoice()
    {
    }

    private Invoice(
        InvoiceType type,
        string number,
        Client client,
        StockOrigin origin,
        Truck? truck,
        Guid? employeeId,
        string attendantName,
        DateTime issuedAt)
    {
        Type = type;
        Number = Guard.RequiredText(number, "número de documento", NumberMaxLength);
        ClientId = client.Id;
        ClientName = client.Name;
        ClientRif = client.Rif;
        DispatchOrigin = origin;
        if (origin == StockOrigin.Truck)
        {
            if (truck is null)
            {
                throw new DomainException("El camión es obligatorio cuando el despacho sale de un camión.");
            }

            TruckId = truck.Id;
            TruckName = truck.Name;
            TruckPlate = truck.Plate;
        }

        EmployeeId = employeeId;
        AttendantName = Guard.RequiredText(attendantName, "vendedor que atiende", AttendantMaxLength);
        IssuedAt = issuedAt;
    }

    public string Number { get; private set; } = default!;

    public InvoiceType Type { get; private set; }

    public Guid ClientId { get; private set; }

    public string ClientName { get; private set; } = default!;

    public string ClientRif { get; private set; } = default!;

    public DateTime IssuedAt { get; private set; }

    public StockOrigin DispatchOrigin { get; private set; }

    public Guid? TruckId { get; private set; }

    public string? TruckName { get; private set; }

    public string? TruckPlate { get; private set; }

    public Guid? EmployeeId { get; private set; }

    public string AttendantName { get; private set; } = default!;

    public decimal Total { get; private set; }

    public decimal Payment { get; private set; }

    /// <summary>Total − abono. Negativo cuando el abono supera el total (queda saldo a favor).</summary>
    public decimal Pending { get; private set; }

    public string? PaymentObservation { get; private set; }

    public int GeneratedEmptyBoxes { get; private set; }

    public int GeneratedEmptyUnits { get; private set; }

    public int ReturnedEmptyBoxes { get; private set; }

    public int ReturnedEmptyUnits { get; private set; }

    public bool IsCancelled { get; private set; }

    public string? CancellationReason { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public string? CancelledBy { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => items.AsReadOnly();

    public IReadOnlyCollection<InvoiceEmptyGroup> EmptyGroups => emptyGroups.AsReadOnly();

    /// <summary>Cajas de vacíos que quedan pendientes por esta operación (puede ser negativo).</summary>
    public int PendingEmptyBoxes => GeneratedEmptyBoxes - ReturnedEmptyBoxes;

    public int PendingEmptyUnits => GeneratedEmptyUnits - ReturnedEmptyUnits;

    public bool HasEmptiesMovement => emptyGroups.Count > 0;

    /// <summary>Crea una factura de venta.</summary>
    public static Invoice CreateSale(
        string number,
        Client client,
        StockOrigin origin,
        Truck? truck,
        Guid? employeeId,
        string attendantName,
        IReadOnlyList<ProductLine> lines,
        decimal payment,
        string? paymentObservation,
        IReadOnlyDictionary<string, BoxQuantity> returnedEmpties,
        DateTime issuedAt)
    {
        if (payment < 0)
        {
            throw new DomainException("El abono no puede ser negativo.");
        }

        var invoice = new Invoice(InvoiceType.Sale, number, client, origin, truck, employeeId, attendantName, issuedAt);
        invoice.AddLines(lines, chargeable: true);
        invoice.Payment = payment;
        invoice.Pending = invoice.Total - payment;
        invoice.PaymentObservation = Guard.OptionalText(paymentObservation, "observación de pago", ObservationMaxLength);
        invoice.BuildEmptyGroups(returnedEmpties);
        return invoice;
    }

    /// <summary>
    /// Crea una regalía: solo productos autorizados, sin cobro (total, abono y pendiente = 0).
    /// Sí genera vacíos, y en la regalía no se reciben devoluciones.
    /// </summary>
    public static Invoice CreateGift(
        string number,
        Client client,
        StockOrigin origin,
        Truck? truck,
        string attendantName,
        IReadOnlyList<ProductLine> lines,
        DateTime issuedAt)
    {
        var notEligible = lines.Where(l => !l.Product.IsGiftEligible).Select(l => l.Product.Name).ToList();
        if (notEligible.Count > 0)
        {
            throw new DomainException($"Productos no autorizados para regalía: {string.Join(", ", notEligible)}.");
        }

        var invoice = new Invoice(InvoiceType.Gift, number, client, origin, truck, null, attendantName, issuedAt);
        invoice.AddLines(lines, chargeable: false);
        invoice.PaymentObservation = GiftObservation;
        invoice.BuildEmptyGroups(new Dictionary<string, BoxQuantity>());
        return invoice;
    }

    public void Cancel(string reason, string? cancelledBy, DateTime cancelledAt)
    {
        if (IsCancelled)
        {
            throw new DomainException($"El documento {Number} ya está anulado.");
        }

        CancellationReason = Guard.RequiredText(reason, "motivo de anulación", CancellationReasonMaxLength);
        CancelledBy = cancelledBy;
        CancelledAt = cancelledAt;
        IsCancelled = true;
        Touch();
    }

    private void AddLines(IReadOnlyList<ProductLine> lines, bool chargeable)
    {
        if (lines.Count == 0)
        {
            throw new DomainException("El documento debe tener al menos un producto.");
        }

        foreach (var line in lines)
        {
            if (line.Quantity.IsEmpty)
            {
                throw new DomainException($"Indique cajas o unidades para {line.Product.Name}.");
            }

            items.Add(new InvoiceItem(Id, line.Product, line.Quantity, chargeable));
        }

        Total = items.Sum(i => i.Subtotal);
    }

    private void BuildEmptyGroups(IReadOnlyDictionary<string, BoxQuantity> returnedEmpties)
    {
        foreach (var key in returnedEmpties.Keys.Where(k => !EmptyReturnGroups.Exists(k)))
        {
            throw new DomainException($"El grupo de vacíos '{key}' no existe.");
        }

        var generated = items
            .Where(i => i.EmptyGroupKey is not null)
            .GroupBy(i => i.EmptyGroupKey!)
            .ToDictionary(g => g.Key, g => (Boxes: g.Sum(i => i.Boxes), Units: g.Sum(i => i.LooseUnits)));

        foreach (var key in generated.Keys.Union(returnedEmpties.Keys).Distinct())
        {
            var gen = generated.GetValueOrDefault(key);
            var ret = returnedEmpties.GetValueOrDefault(key);
            var group = new InvoiceEmptyGroup(Id, key, gen.Boxes, gen.Units, ret.Boxes, ret.LooseUnits);
            if (group.HasMovement)
            {
                emptyGroups.Add(group);
            }
        }

        GeneratedEmptyBoxes = emptyGroups.Sum(g => g.GeneratedBoxes);
        GeneratedEmptyUnits = emptyGroups.Sum(g => g.GeneratedUnits);
        ReturnedEmptyBoxes = emptyGroups.Sum(g => g.ReturnedBoxes);
        ReturnedEmptyUnits = emptyGroups.Sum(g => g.ReturnedUnits);
    }
}
