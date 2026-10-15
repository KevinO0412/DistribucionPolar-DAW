using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Application.DTOs;

/// <summary>Línea del inventario de un camión.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="SKU">SKU.</param>
/// <param name="ProductName">Nombre del producto.</param>
/// <param name="CategoryName">Categoría.</param>
/// <param name="UnitsPerBox">Unidades por caja.</param>
/// <param name="StockUnits">Unidades en el camión.</param>
/// <param name="StockDisplay">Cantidad en texto ("2 cajas + 5 und").</param>
/// <param name="PriceUnit">Precio por unidad actual.</param>
/// <param name="Value">Valor: unidades × precio por unidad.</param>
public sealed record TruckStockLineResponse(
    Guid ProductId, string SKU, string ProductName, string? CategoryName, int UnitsPerBox,
    int StockUnits, string StockDisplay, decimal PriceUnit, decimal Value);

/// <summary>Camión de reparto con su inventario.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Name">Nombre o modelo.</param>
/// <param name="Plate">Placa.</param>
/// <param name="Status">Estado: En galpón, Cargando, En ruta o Liquidado.</param>
/// <param name="TotalUnits">Unidades totales en el camión.</param>
/// <param name="InventoryValue">Valor del inventario del camión.</param>
/// <param name="Stock">Líneas de inventario.</param>
/// <param name="CapacityBoxes">Capacidad de carga en cajas.</param>
/// <param name="LoadedBoxes">Carga actual en cajas equivalentes (unidades / unidades por caja).</param>
/// <param name="OccupancyPercent">Ocupación: carga / capacidad × 100.</param>
/// <param name="DispatchedAt">Salida a la ruta actual o la última (UTC).</param>
/// <param name="TripFrom">Desde cuándo se cuentan las ventas: la salida a ruta, o el inicio de hoy si está en galpón o cargando.</param>
/// <param name="TripInvoices">Facturas de venta del camión desde <c>TripFrom</c>.</param>
/// <param name="TripSales">Monto facturado desde <c>TripFrom</c>.</param>
/// <param name="TripCollected">Cobrado al facturar desde <c>TripFrom</c>.</param>
public sealed record TruckResponse(
    Guid Id, string Name, string Plate, string Status, int TotalUnits, decimal InventoryValue,
    IReadOnlyList<TruckStockLineResponse> Stock, int CapacityBoxes, decimal LoadedBoxes, decimal OccupancyPercent,
    DateTime? DispatchedAt = null, DateTime? TripFrom = null, int TripInvoices = 0, decimal TripSales = 0m, decimal TripCollected = 0m);

/// <summary>Alta o edición de camión (solo Admin).</summary>
/// <param name="Name" example="NPR">Nombre o modelo.</param>
/// <param name="Plate" example="AB123CD">Placa única.</param>
/// <param name="CapacityBoxes" example="500">Capacidad de carga en cajas (1–5000, por defecto 500).</param>
public sealed record TruckRequest(string Name, string Plate, int CapacityBoxes = Truck.DefaultCapacityBoxes);

/// <summary>Modo de carga de un camión.</summary>
public enum TruckLoadMode
{
    /// <summary>Agrega la cantidad indicada.</summary>
    Add = 1,

    /// <summary>La cantidad indicada es la meta final; se carga la diferencia.</summary>
    Target = 2
}

/// <summary>Carga de un camión desde el galpón.</summary>
/// <param name="ProductId">Producto a cargar.</param>
/// <param name="Mode" example="Add">Add: agrega la cantidad. Target: la cantidad es la meta final en el camión.</param>
/// <param name="Boxes" example="2">Cajas.</param>
/// <param name="LooseUnits" example="0">Unidades sueltas.</param>
/// <param name="Observation">Observación opcional.</param>
public sealed record LoadTruckRequest(Guid ProductId, TruckLoadMode Mode, int Boxes, int LooseUnits, string? Observation);

/// <summary>Transferencia (desmontaje) desde un camión al galpón o a otro camión.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="Boxes" example="1">Cajas.</param>
/// <param name="LooseUnits" example="0">Unidades sueltas.</param>
/// <param name="Destination" example="Warehouse">Warehouse (galpón) o Truck (otro camión).</param>
/// <param name="DestinationTruckId">Camión destino, obligatorio si Destination = Truck.</param>
/// <param name="Observation">Observación opcional.</param>
public sealed record TransferFromTruckRequest(
    Guid ProductId, int Boxes, int LooseUnits, StockOrigin Destination, Guid? DestinationTruckId, string? Observation);

/// <summary>Ajuste manual de inventario (solo Admin): fija la cantidad absoluta.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="Boxes" example="3">Cajas de la cantidad final.</param>
/// <param name="LooseUnits" example="0">Unidades sueltas de la cantidad final.</param>
/// <param name="Reason" example="Conteo físico">Motivo, obligatorio.</param>
public sealed record AdjustStockRequest(Guid ProductId, int Boxes, int LooseUnits, string Reason);

/// <summary>Registro histórico de una carga de camión.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="TruckId">Camión.</param>
/// <param name="TruckName">Nombre del camión.</param>
/// <param name="ProductId">Producto.</param>
/// <param name="ProductName">Nombre del producto.</param>
/// <param name="TotalUnits">Unidades cargadas.</param>
/// <param name="StockBefore">Unidades del producto en el camión antes.</param>
/// <param name="StockAfter">Unidades del producto en el camión después.</param>
/// <param name="Source">Origen: Warehouse, Truck o InvoiceCancellation.</param>
/// <param name="Observation">Observación.</param>
/// <param name="Username">Usuario que realizó la carga.</param>
/// <param name="CreatedAt">Fecha (UTC).</param>
public sealed record TruckLoadResponse(
    Guid Id, Guid TruckId, string TruckName, Guid ProductId, string ProductName, int TotalUnits,
    int StockBefore, int StockAfter, TruckLoadSource Source, string? Observation, string? Username, DateTime CreatedAt);

/// <summary>Conteo de un producto en la liquidación ciega.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="Boxes">Cajas contadas.</param>
/// <param name="LooseUnits">Unidades sueltas contadas.</param>
public sealed record SettlementCountLine(Guid ProductId, int Boxes, int LooseUnits);

/// <summary>Liquidación ciega: lo que el chofer cuenta al volver, sin ver lo esperado.</summary>
/// <param name="CashCounted" example="350.00">Efectivo recaudado que entrega, en USD.</param>
/// <param name="Items">Mercancía que devuelve, por producto (lo que no se cuente se toma como 0).</param>
/// <param name="Observation">Observación opcional.</param>
public sealed record TruckSettlementRequest(decimal CashCounted, IReadOnlyList<SettlementCountLine> Items, string? Observation);

/// <summary>Producto que el camión llevó, para el conteo ciego (sin cantidades).</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="SKU">SKU.</param>
/// <param name="Name">Nombre.</param>
/// <param name="UnitsPerBox">Unidades por caja.</param>
public sealed record SettlementFormProduct(Guid ProductId, string SKU, string Name, int UnitsPerBox);

/// <summary>Formulario de la liquidación ciega: solo los productos, sin cantidades esperadas ni cobros.</summary>
/// <param name="TruckId">Camión.</param>
/// <param name="TruckName">Nombre.</param>
/// <param name="TruckPlate">Placa.</param>
/// <param name="Status">Estado del camión.</param>
/// <param name="DispatchedAt">Salida a ruta.</param>
/// <param name="Products">Productos a contar.</param>
public sealed record SettlementFormResponse(
    Guid TruckId, string TruckName, string TruckPlate, string Status, DateTime? DispatchedAt, IReadOnlyList<SettlementFormProduct> Products);

/// <summary>Producto en la planilla de cuadre.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="SKU">SKU.</param>
/// <param name="ProductName">Nombre.</param>
/// <param name="UnitsPerBox">Unidades por caja.</param>
/// <param name="ExpectedUnits">Unidades que el sistema esperaba de vuelta.</param>
/// <param name="ExpectedDisplay">Lo esperado en cajas y unidades.</param>
/// <param name="CountedUnits">Unidades contadas.</param>
/// <param name="CountedDisplay">Lo contado en cajas y unidades.</param>
/// <param name="DifferenceUnits">Contado − esperado (negativo = faltante).</param>
/// <param name="DifferenceValue">Diferencia valorizada a precio por unidad.</param>
public sealed record SettlementLineResponse(
    Guid ProductId, string SKU, string ProductName, int UnitsPerBox, int ExpectedUnits, string ExpectedDisplay,
    int CountedUnits, string CountedDisplay, int DifferenceUnits, decimal DifferenceValue);

/// <summary>Planilla de cuadre de la ruta: esperado frente a contado, en efectivo y en mercancía.</summary>
/// <param name="Id">Liquidación.</param>
/// <param name="TruckId">Camión.</param>
/// <param name="TruckName">Nombre.</param>
/// <param name="TruckPlate">Placa.</param>
/// <param name="TripStartedAt">Salida a ruta.</param>
/// <param name="SettledAt">Liquidación.</param>
/// <param name="SettledBy">Usuario que liquidó.</param>
/// <param name="Observation">Observación.</param>
/// <param name="ExpectedCash">Cobrado en las facturas de la ruta según el sistema.</param>
/// <param name="CountedCash">Efectivo declarado.</param>
/// <param name="CashDifference">Contado − esperado (negativo = falta dinero).</param>
/// <param name="ExpectedMerchandiseValue">Valor de la mercancía esperada de vuelta.</param>
/// <param name="CountedMerchandiseValue">Valor de la mercancía contada.</param>
/// <param name="MerchandiseDifferenceValue">Diferencia valorizada (negativo = falta mercancía).</param>
/// <param name="IsBalanced">La ruta cuadra sin diferencias.</param>
/// <param name="Lines">Productos con diferencias primero.</param>
public sealed record TruckSettlementResponse(
    Guid Id, Guid TruckId, string TruckName, string TruckPlate, DateTime TripStartedAt, DateTime SettledAt, string? SettledBy,
    string? Observation, decimal ExpectedCash, decimal CountedCash, decimal CashDifference, decimal ExpectedMerchandiseValue,
    decimal CountedMerchandiseValue, decimal MerchandiseDifferenceValue, bool IsBalanced, IReadOnlyList<SettlementLineResponse> Lines);
