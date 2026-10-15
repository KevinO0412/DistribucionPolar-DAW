namespace Core.Application.DTOs;

/// <summary>Ajuste manual del stock del galpón (solo Admin): fija la cantidad absoluta.</summary>
/// <param name="Boxes" example="10">Cajas de la cantidad final.</param>
/// <param name="LooseUnits" example="0">Unidades sueltas de la cantidad final.</param>
/// <param name="Reason" example="Conteo físico mensual">Motivo, obligatorio.</param>
public sealed record AdjustWarehouseStockRequest(int Boxes, int LooseUnits, string Reason);

/// <summary>Unidades y valor de una categoría en galpón y camiones.</summary>
/// <param name="CategoryName">Categoría.</param>
/// <param name="WarehouseUnits">Unidades en galpón.</param>
/// <param name="WarehouseValue">Valor en galpón.</param>
/// <param name="TruckUnits">Unidades en camiones.</param>
/// <param name="TruckValue">Valor en camiones.</param>
public sealed record CategoryInventoryResponse(string CategoryName, int WarehouseUnits, decimal WarehouseValue, int TruckUnits, decimal TruckValue);

/// <summary>Unidades y valor de un camión.</summary>
/// <param name="TruckId">Camión.</param>
/// <param name="TruckName">Nombre.</param>
/// <param name="Units">Unidades.</param>
/// <param name="Value">Valor.</param>
public sealed record TruckInventoryResponse(Guid TruckId, string TruckName, int Units, decimal Value);

/// <summary>Resumen del inventario total: galpón + camiones.</summary>
/// <param name="WarehouseUnits">Unidades en galpón.</param>
/// <param name="WarehouseValue">Valor del galpón (unidades × precio unidad).</param>
/// <param name="TruckUnits">Unidades en camiones.</param>
/// <param name="TruckValue">Valor en camiones.</param>
/// <param name="TotalUnits">Unidades totales.</param>
/// <param name="TotalValue">Valor total.</param>
/// <param name="ByCategory">Desglose por categoría.</param>
/// <param name="ByTruck">Desglose por camión.</param>
public sealed record InventorySummaryResponse(
    int WarehouseUnits, decimal WarehouseValue, int TruckUnits, decimal TruckValue, int TotalUnits, decimal TotalValue,
    IReadOnlyList<CategoryInventoryResponse> ByCategory, IReadOnlyList<TruckInventoryResponse> ByTruck);

/// <summary>Dónde está un producto.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="ProductName">Nombre.</param>
/// <param name="WarehouseUnits">Unidades en galpón.</param>
/// <param name="Trucks">Camiones que lo llevan.</param>
/// <param name="TotalUnits">Unidades totales.</param>
public sealed record ProductLocationResponse(Guid ProductId, string ProductName, int WarehouseUnits, IReadOnlyList<TruckInventoryResponse> Trucks, int TotalUnits);
