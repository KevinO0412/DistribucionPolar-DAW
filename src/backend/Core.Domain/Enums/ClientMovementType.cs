namespace Core.Domain.Enums;

/// <summary>Tipo de movimiento del estado de cuenta de un cliente.</summary>
public enum ClientMovementType
{
    /// <summary>Factura de venta (monto = total).</summary>
    Invoice = 1,

    /// <summary>Regalía (monto = 0).</summary>
    Gift = 2,

    /// <summary>Abono de dinero, suelto o al facturar.</summary>
    Payment = 3,

    /// <summary>Vacíos generados y devueltos en una factura o regalía.</summary>
    EmptiesGenerated = 4,

    /// <summary>Devolución de vacíos fuera de factura.</summary>
    EmptiesReturned = 5,

    /// <summary>Anulación de una factura o regalía.</summary>
    InvoiceCancelled = 6,

    /// <summary>Ajuste manual de saldos realizado por un administrador.</summary>
    BalanceAdjustment = 7
}
