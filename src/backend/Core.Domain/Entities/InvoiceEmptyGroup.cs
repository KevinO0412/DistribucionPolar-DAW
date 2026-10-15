using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>Vacíos generados y devueltos en una factura para un grupo de envase.</summary>
public sealed class InvoiceEmptyGroup
{
    // Constructor para EF Core.
    private InvoiceEmptyGroup()
    {
    }

    internal InvoiceEmptyGroup(Guid invoiceId, string groupKey, int generatedBoxes, int generatedUnits, int returnedBoxes, int returnedUnits)
    {
        if (returnedBoxes < 0 || returnedUnits < 0)
        {
            throw new DomainException("Los vacíos devueltos no pueden ser negativos.");
        }

        InvoiceId = invoiceId;
        GroupKey = groupKey;
        GeneratedBoxes = generatedBoxes;
        GeneratedUnits = generatedUnits;
        ReturnedBoxes = returnedBoxes;
        ReturnedUnits = returnedUnits;
    }

    public Guid InvoiceId { get; private set; }

    public string GroupKey { get; private set; } = default!;

    public int GeneratedBoxes { get; private set; }

    public int GeneratedUnits { get; private set; }

    public int ReturnedBoxes { get; private set; }

    public int ReturnedUnits { get; private set; }

    /// <summary>Pendiente de esta operación: generados − devueltos (puede ser negativo).</summary>
    public int PendingBoxes => GeneratedBoxes - ReturnedBoxes;

    public int PendingUnits => GeneratedUnits - ReturnedUnits;

    public bool HasMovement => GeneratedBoxes > 0 || GeneratedUnits > 0 || ReturnedBoxes > 0 || ReturnedUnits > 0;
}
