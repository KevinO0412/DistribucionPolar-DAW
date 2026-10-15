using Core.Domain.Common;
using Core.Domain.Enums;

namespace Core.Domain.Entities;

/// <summary>Movimiento del estado de cuenta de un cliente. Es inmutable: solo se crea.</summary>
public sealed class ClientMovement : BaseEntity
{
    public const int TitleMaxLength = 100;
    public const int DescriptionMaxLength = 2000;

    // Constructor para EF Core.
    private ClientMovement()
    {
    }

    public ClientMovement(Guid clientId, ClientMovementType type, string title, string description, decimal amount, Guid? invoiceId)
    {
        ClientId = Guard.NotEmpty(clientId, "cliente");
        Type = type;
        Title = Guard.RequiredText(title, "título del movimiento", TitleMaxLength);
        Description = Guard.RequiredText(description, "descripción del movimiento", DescriptionMaxLength);
        Amount = amount;
        InvoiceId = invoiceId;
    }

    public Guid ClientId { get; private set; }

    public ClientMovementType Type { get; private set; }

    public string Title { get; private set; } = default!;

    public string Description { get; private set; } = default!;

    public decimal Amount { get; private set; }

    public Guid? InvoiceId { get; private set; }
}
