namespace Core.Domain.Enums;

/// <summary>Documento que respalda una entrada de mercancía.</summary>
public enum PurchaseDocumentType
{
    Invoice = 1,
    DeliveryNote = 2,
    ManualEntry = 3,
    InventoryAdjustment = 4
}
