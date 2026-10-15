namespace Core.Domain.Enums;

/// <summary>Ubicación física desde donde sale o a donde entra la mercancía.</summary>
public enum StockOrigin
{
    /// <summary>Galpón (almacén central).</summary>
    Warehouse = 1,

    /// <summary>Camión de reparto.</summary>
    Truck = 2
}
