namespace Core.Application.DTOs;

/// <summary>Nuevo evento a consignación: se entrega mercancía del galpón.</summary>
/// <param name="Name" example="Feria del Colegio San José">Nombre del evento.</param>
/// <param name="Responsible" example="María González">Responsable.</param>
/// <param name="EventDate">Fecha del evento (UTC); si se omite, ahora.</param>
/// <param name="Items">Productos entregados (si un producto se repite, se acumula).</param>
public sealed record CreateConsignmentRequest(string Name, string Responsible, DateTime? EventDate, IReadOnlyList<LineRequest> Items);

/// <summary>Devolución de mercancía de un evento abierto.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="Boxes" example="0">Cajas devueltas.</param>
/// <param name="LooseUnits" example="30">Unidades devueltas.</param>
public sealed record ConsignmentReturnRequest(Guid ProductId, int Boxes, int LooseUnits);

/// <summary>Producto de un evento a consignación.</summary>
/// <param name="ProductId">Producto.</param>
/// <param name="ProductName">Nombre.</param>
/// <param name="UnitsPerBox">Unidades por caja.</param>
/// <param name="DeliveredUnits">Unidades entregadas.</param>
/// <param name="ReturnedUnits">Unidades devueltas.</param>
/// <param name="PendingUnits">Pendientes por devolver.</param>
/// <param name="SoldUnits">Vendidas (entregadas − devueltas).</param>
/// <param name="SoldDisplay">Vendidas en texto.</param>
/// <param name="SoldAmount">Monto vendido.</param>
public sealed record ConsignmentItemResponse(
    Guid ProductId, string ProductName, int UnitsPerBox, int DeliveredUnits, int ReturnedUnits, int PendingUnits, int SoldUnits, string SoldDisplay, decimal SoldAmount);

/// <summary>Evento a consignación.</summary>
/// <param name="Id">Identificador.</param>
/// <param name="Name">Nombre.</param>
/// <param name="Responsible">Responsable.</param>
/// <param name="EventDate">Fecha del evento (UTC).</param>
/// <param name="IsClosed">Cerrado.</param>
/// <param name="ClosedAt">Fecha de cierre (UTC).</param>
/// <param name="TotalSold">Total a cobrar.</param>
/// <param name="Items">Productos.</param>
public sealed record ConsignmentResponse(
    Guid Id, string Name, string Responsible, DateTime EventDate, bool IsClosed, DateTime? ClosedAt, decimal TotalSold, IReadOnlyList<ConsignmentItemResponse> Items);

/// <summary>Indicadores de consignación.</summary>
/// <param name="OpenEvents">Eventos abiertos.</param>
/// <param name="ClosedEvents">Eventos cerrados.</param>
/// <param name="ReceivableTotal">Por cobrar total.</param>
/// <param name="ReceivableOpen">Por cobrar de eventos abiertos.</param>
/// <param name="ReceivableClosed">Por cobrar de eventos cerrados.</param>
/// <param name="DeliveredUnits">Unidades entregadas.</param>
/// <param name="ReturnedUnits">Unidades devueltas.</param>
/// <param name="SoldUnits">Unidades vendidas.</param>
public sealed record ConsignmentIndicatorsResponse(
    int OpenEvents, int ClosedEvents, decimal ReceivableTotal, decimal ReceivableOpen, decimal ReceivableClosed, int DeliveredUnits, int ReturnedUnits, int SoldUnits);
