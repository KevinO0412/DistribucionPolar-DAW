namespace Core.Domain.Enums;

/// <summary>Tipo de documento de despacho a cliente.</summary>
public enum InvoiceType
{
    /// <summary>Factura de venta: afecta el saldo de dinero del cliente.</summary>
    Sale = 1,

    /// <summary>Regalía: productos autorizados sin cobro; solo genera vacíos.</summary>
    Gift = 2
}
