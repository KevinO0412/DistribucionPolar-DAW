namespace Core.Domain.Enums;

/// <summary>Origen de una entrada de mercancía a un camión (historial de cargas).</summary>
public enum TruckLoadSource
{
    /// <summary>Carga desde el galpón.</summary>
    Warehouse = 1,

    /// <summary>Transferencia desde otro camión.</summary>
    Truck = 2,

    /// <summary>Reverso por anulación de una factura despachada desde el camión.</summary>
    InvoiceCancellation = 3
}
