using Core.Domain.Enums;

namespace Core.Application.DTOs;

/// <summary>Entrada de mercancía al galpón (solo Admin).</summary>
/// <param name="EmployeeId">Empleado que recibe la mercancía.</param>
/// <param name="DocumentType" example="Invoice">Invoice, DeliveryNote, ManualEntry o InventoryAdjustment.</param>
/// <param name="DocumentNumber" example="F-000123">Número del documento del proveedor.</param>
/// <param name="Items">Productos recibidos.</param>
public sealed record CreatePurchaseRequest(Guid EmployeeId, PurchaseDocumentType DocumentType, string? DocumentNumber, IReadOnlyList<LineRequest> Items);

/// <summary>Línea de compra.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="ProductName">Nombre.</param>
/// <param name="Boxes">Cajas.</param>
/// <param name="LooseUnits">Unidades sueltas.</param>
/// <param name="TotalUnits">Unidades totales.</param>
/// <param name="Subtotal">Subtotal.</param>
public sealed record PurchaseItemResponse(Guid ProductId, string ProductName, int Boxes, int LooseUnits, int TotalUnits, decimal Subtotal);

/// <summary>Compra registrada.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Number">Número ("Compra #0001").</param>
/// <param name="PurchasedAt">Fecha (UTC).</param>
/// <param name="EmployeeName">Empleado que recibió.</param>
/// <param name="DocumentType">Tipo de documento.</param>
/// <param name="DocumentNumber">Número de documento.</param>
/// <param name="TotalUnits">Unidades totales.</param>
/// <param name="TotalAmount">Monto total.</param>
/// <param name="Items">Líneas.</param>
public sealed record PurchaseResponse(
    Guid Id, string Number, DateTime PurchasedAt, string EmployeeName, PurchaseDocumentType DocumentType, string? DocumentNumber,
    int TotalUnits, decimal TotalAmount, IReadOnlyList<PurchaseItemResponse> Items);

/// <summary>Registro de avería (solo Admin).</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="Boxes" example="0">Cajas.</param>
/// <param name="LooseUnits" example="3">Unidades sueltas.</param>
/// <param name="Origin" example="Warehouse">Warehouse o Truck.</param>
/// <param name="TruckId">Camión, obligatorio si el origen es Truck.</param>
/// <param name="Type" example="Damaged">Damaged, Unfit o Discarded.</param>
/// <param name="Reason" example="Broken">Expired, Broken, Dented, Spilled, Wet, OpenPackage u Other.</param>
/// <param name="ActionTaken" example="Discarded">Discarded, InternalConsumption, Gifted o ReturnedToSupplier.</param>
/// <param name="Observation">Observación.</param>
public sealed record CreateDamageRequest(
    Guid ProductId, int Boxes, int LooseUnits, StockOrigin Origin, Guid? TruckId,
    DamageType Type, DamageReason Reason, DamageAction ActionTaken, string? Observation);

/// <summary>Avería registrada.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="OccurredAt">Fecha (UTC).</param>
/// <param name="Type">Tipo.</param>
/// <param name="Reason">Motivo.</param>
/// <param name="ActionTaken">Acción tomada.</param>
/// <param name="Origin">Origen.</param>
/// <param name="TruckName">Camión, si aplica.</param>
/// <param name="ProductId">Producto.</param>
/// <param name="ProductName">Nombre del producto.</param>
/// <param name="Boxes">Cajas.</param>
/// <param name="LooseUnits">Unidades sueltas.</param>
/// <param name="TotalUnits">Unidades totales.</param>
/// <param name="EstimatedLoss">Pérdida estimada.</param>
/// <param name="Observation">Observación.</param>
public sealed record DamageResponse(
    Guid Id, DateTime OccurredAt, DamageType Type, DamageReason Reason, DamageAction ActionTaken, StockOrigin Origin, string? TruckName,
    Guid ProductId, string ProductName, int Boxes, int LooseUnits, int TotalUnits, decimal EstimatedLoss, string? Observation);

/// <summary>Consumo interno o regalía a empleado (solo Admin).</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="Boxes" example="0">Cajas.</param>
/// <param name="LooseUnits" example="6">Unidades sueltas.</param>
/// <param name="Origin" example="Warehouse">Warehouse o Truck.</param>
/// <param name="TruckId">Camión, obligatorio si el origen es Truck.</param>
/// <param name="EmployeeId">Empleado, obligatorio.</param>
/// <param name="Type" example="InternalConsumption">InternalConsumption o Gift.</param>
/// <param name="Observation">Observación.</param>
public sealed record CreateInternalConsumptionRequest(
    Guid ProductId, int Boxes, int LooseUnits, StockOrigin Origin, Guid? TruckId, Guid EmployeeId, InternalConsumptionType Type, string? Observation);

/// <summary>Consumo interno registrado.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="OccurredAt">Fecha (UTC).</param>
/// <param name="Type">Tipo.</param>
/// <param name="EmployeeName">Empleado.</param>
/// <param name="Origin">Origen.</param>
/// <param name="TruckName">Camión, si aplica.</param>
/// <param name="ProductId">Producto.</param>
/// <param name="ProductName">Nombre del producto.</param>
/// <param name="Boxes">Cajas.</param>
/// <param name="LooseUnits">Unidades sueltas.</param>
/// <param name="TotalUnits">Unidades totales.</param>
/// <param name="EstimatedValue">Valor estimado.</param>
/// <param name="Observation">Observación.</param>
public sealed record InternalConsumptionResponse(
    Guid Id, DateTime OccurredAt, InternalConsumptionType Type, string EmployeeName, StockOrigin Origin, string? TruckName,
    Guid ProductId, string ProductName, int Boxes, int LooseUnits, int TotalUnits, decimal EstimatedValue, string? Observation);

/// <summary>Listado con total acumulado.</summary>
/// <param name="Items">Registros.</param>
/// <param name="Count">Cantidad de registros.</param>
/// <param name="TotalValue">Suma del valor estimado (pérdida o consumo).</param>
public sealed record WithdrawalListResponse<T>(IReadOnlyList<T> Items, int Count, decimal TotalValue);
