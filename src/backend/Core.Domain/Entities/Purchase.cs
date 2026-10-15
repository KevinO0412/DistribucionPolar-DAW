using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Exceptions;

namespace Core.Domain.Entities;

/// <summary>Entrada de mercancía al galpón (compra, nota de entrega, entrada manual o ajuste).</summary>
public sealed class Purchase : BaseEntity
{
    public const int NumberMaxLength = 20;
    public const int DocumentNumberMaxLength = 50;
    public const int EmployeeNameMaxLength = 100;

    private readonly List<PurchaseItem> items = [];

    // Constructor para EF Core.
    private Purchase()
    {
    }

    public Purchase(
        int sequence,
        Employee employee,
        PurchaseDocumentType documentType,
        string? documentNumber,
        IReadOnlyList<ProductLine> lines,
        DateTime purchasedAt)
    {
        Number = FormatNumber(Guard.Positive(sequence, "secuencia de compra"));
        EmployeeId = employee.Id;
        EmployeeName = employee.Name;
        DocumentType = Enum.IsDefined(documentType) ? documentType : throw new DomainException("Tipo de documento inválido.");
        DocumentNumber = Guard.OptionalText(documentNumber, "número de documento", DocumentNumberMaxLength);
        PurchasedAt = purchasedAt;

        if (lines.Count == 0)
        {
            throw new DomainException("La compra debe tener al menos un producto.");
        }

        foreach (var line in lines)
        {
            if (line.Quantity.IsEmpty)
            {
                throw new DomainException($"Indique cajas o unidades para {line.Product.Name}.");
            }

            items.Add(new PurchaseItem(Id, line.Product, line.Quantity));
        }

        TotalUnits = items.Sum(i => i.TotalUnits);
        TotalAmount = items.Sum(i => i.Subtotal);
    }

    /// <summary>Número legible: "Compra #0001".</summary>
    public string Number { get; private set; } = default!;

    public DateTime PurchasedAt { get; private set; }

    public Guid EmployeeId { get; private set; }

    public string EmployeeName { get; private set; } = default!;

    public PurchaseDocumentType DocumentType { get; private set; }

    public string? DocumentNumber { get; private set; }

    public int TotalUnits { get; private set; }

    public decimal TotalAmount { get; private set; }

    public IReadOnlyCollection<PurchaseItem> Items => items.AsReadOnly();

    public static string FormatNumber(int sequence) => $"Compra #{sequence:D4}";
}
