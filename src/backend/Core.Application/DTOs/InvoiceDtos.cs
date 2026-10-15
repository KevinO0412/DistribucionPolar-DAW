using Core.Domain.Enums;

namespace Core.Application.DTOs;

/// <summary>Factura de venta.</summary>
/// <param name="ClientId">Cliente.</param>
/// <param name="DispatchOrigin" example="Truck">Truck (camión, por defecto en la operación) o Warehouse (galpón).</param>
/// <param name="TruckId">Camión, obligatorio si el origen es Truck.</param>
/// <param name="EmployeeId">Vendedor que atiende; solo si el usuario puede elegirlo.</param>
/// <param name="Items">Productos (al menos uno).</param>
/// <param name="Payment" example="40.00">Abono recibido (mayor o igual que 0).</param>
/// <param name="PaymentObservation">Observación de pago (monedas, tasa, referencia).</param>
/// <param name="ReturnedEmpties">Vacíos que el cliente entrega en esta factura, por grupo.</param>
public sealed record CreateSaleRequest(
    Guid ClientId, StockOrigin DispatchOrigin, Guid? TruckId, Guid? EmployeeId,
    IReadOnlyList<LineRequest> Items, decimal Payment, string? PaymentObservation,
    IReadOnlyList<EmptyGroupQuantity>? ReturnedEmpties);

/// <summary>Regalía a cliente: solo productos autorizados, sin cobro.</summary>
/// <param name="ClientId">Cliente.</param>
/// <param name="DispatchOrigin" example="Truck">Truck o Warehouse.</param>
/// <param name="TruckId">Camión, obligatorio si el origen es Truck.</param>
/// <param name="Items">Productos autorizados para regalía.</param>
public sealed record CreateGiftRequest(Guid ClientId, StockOrigin DispatchOrigin, Guid? TruckId, IReadOnlyList<LineRequest> Items);

/// <summary>Anulación de factura o regalía (solo Admin).</summary>
/// <param name="Reason" example="Error en la cantidad despachada">Motivo, obligatorio.</param>
public sealed record CancelInvoiceRequest(string Reason);

/// <summary>Línea de factura (copia del producto al emitirla).</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="SKU">SKU.</param>
/// <param name="ProductName">Nombre.</param>
/// <param name="PriceBox">Precio por caja aplicado.</param>
/// <param name="PriceUnit">Precio por unidad aplicado.</param>
/// <param name="UnitsPerBox">Unidades por caja.</param>
/// <param name="Boxes">Cajas.</param>
/// <param name="LooseUnits">Unidades sueltas.</param>
/// <param name="TotalUnits">Unidades totales.</param>
/// <param name="QuantityDisplay">Cantidad en texto.</param>
/// <param name="Subtotal">Subtotal (0 en regalías).</param>
public sealed record InvoiceItemResponse(
    Guid ProductId, string SKU, string ProductName, decimal PriceBox, decimal PriceUnit, int UnitsPerBox,
    int Boxes, int LooseUnits, int TotalUnits, string QuantityDisplay, decimal Subtotal);

/// <summary>Vacíos de un grupo en la factura.</summary>
/// <param name="GroupKey">Grupo.</param>
/// <param name="GroupName">Nombre del grupo.</param>
/// <param name="GeneratedBoxes">Cajas generadas.</param>
/// <param name="GeneratedUnits">Unidades generadas.</param>
/// <param name="ReturnedBoxes">Cajas devueltas.</param>
/// <param name="ReturnedUnits">Unidades devueltas.</param>
/// <param name="PendingBoxes">Cajas pendientes (generadas − devueltas).</param>
/// <param name="PendingUnits">Unidades pendientes.</param>
public sealed record InvoiceEmptyGroupResponse(
    string GroupKey, string GroupName, int GeneratedBoxes, int GeneratedUnits, int ReturnedBoxes, int ReturnedUnits, int PendingBoxes, int PendingUnits);

/// <summary>Factura o regalía completa.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Number">Número ("Factura Octubre 2026 #3").</param>
/// <param name="Type">Sale o Gift.</param>
/// <param name="ClientId">Cliente.</param>
/// <param name="ClientName">Nombre del cliente.</param>
/// <param name="ClientRif">RIF del cliente.</param>
/// <param name="IssuedAt">Fecha de emisión (UTC).</param>
/// <param name="DispatchOrigin">Origen del despacho.</param>
/// <param name="TruckId">Camión.</param>
/// <param name="TruckName">Nombre del camión.</param>
/// <param name="TruckPlate">Placa del camión.</param>
/// <param name="AttendantName">Vendedor que atendió.</param>
/// <param name="Total">Total.</param>
/// <param name="Payment">Abono.</param>
/// <param name="Pending">Pendiente (negativo = saldo a favor).</param>
/// <param name="PaymentObservation">Observación de pago.</param>
/// <param name="GeneratedEmptyBoxes">Cajas de vacíos generadas.</param>
/// <param name="GeneratedEmptyUnits">Unidades de vacíos generadas.</param>
/// <param name="ReturnedEmptyBoxes">Cajas de vacíos devueltas.</param>
/// <param name="ReturnedEmptyUnits">Unidades de vacíos devueltas.</param>
/// <param name="Items">Líneas.</param>
/// <param name="EmptyGroups">Vacíos por grupo.</param>
/// <param name="IsCancelled">Anulada.</param>
/// <param name="CancellationReason">Motivo de anulación.</param>
/// <param name="CancelledAt">Fecha de anulación (UTC).</param>
/// <param name="CancelledBy">Usuario que anuló.</param>
public sealed record InvoiceResponse(
    Guid Id, string Number, InvoiceType Type, Guid ClientId, string ClientName, string ClientRif, DateTime IssuedAt,
    StockOrigin DispatchOrigin, Guid? TruckId, string? TruckName, string? TruckPlate, string AttendantName,
    decimal Total, decimal Payment, decimal Pending, string? PaymentObservation,
    int GeneratedEmptyBoxes, int GeneratedEmptyUnits, int ReturnedEmptyBoxes, int ReturnedEmptyUnits,
    IReadOnlyList<InvoiceItemResponse> Items, IReadOnlyList<InvoiceEmptyGroupResponse> EmptyGroups,
    bool IsCancelled, string? CancellationReason, DateTime? CancelledAt, string? CancelledBy);

/// <summary>Resumen de factura para listados (proyección directa desde la base).</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Number">Número.</param>
/// <param name="Type">Sale o Gift.</param>
/// <param name="ClientId">Cliente.</param>
/// <param name="ClientName">Nombre del cliente.</param>
/// <param name="IssuedAt">Fecha (UTC).</param>
/// <param name="DispatchOrigin">Origen.</param>
/// <param name="AttendantName">Vendedor.</param>
/// <param name="Total">Total.</param>
/// <param name="Payment">Abono.</param>
/// <param name="Pending">Pendiente.</param>
/// <param name="IsCancelled">Anulada.</param>
public sealed record InvoiceSummaryResponse(
    Guid Id, string Number, InvoiceType Type, Guid ClientId, string ClientName, DateTime IssuedAt,
    StockOrigin DispatchOrigin, string AttendantName, decimal Total, decimal Payment, decimal Pending, bool IsCancelled);

/// <summary>Opciones de vendedor para facturar.</summary>
/// <param name="CanChoose">El usuario puede elegir el vendedor.</param>
/// <param name="DefaultAttendant">Vendedor por defecto del usuario.</param>
/// <param name="Options">Empleados activos elegibles (vacío si no puede elegir).</param>
public sealed record AttendantOptionsResponse(bool CanChoose, string DefaultAttendant, IReadOnlyList<EmployeeResponse> Options);
