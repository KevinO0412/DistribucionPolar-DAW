namespace Core.Application.DTOs;

/// <summary>Reporte diario / cierre de caja. Excluye documentos anulados.</summary>
/// <param name="Date">Día local de la empresa.</param>
/// <param name="SalesTotal">Ventas del día (suma de totales de facturas de venta).</param>
/// <param name="SalesCount">Número de facturas de venta.</param>
/// <param name="GiftsCount">Número de regalías.</param>
/// <param name="CollectedOnInvoices">Cobrado al facturar (suma de abonos de facturas).</param>
/// <param name="CreditGenerated">Fiado generado (suma de pendientes).</param>
/// <param name="PaymentsTotal">Abonos del día (incluye los abonos al facturar).</param>
/// <param name="EmptiesGeneratedBoxes">Cajas de vacíos generadas.</param>
/// <param name="EmptiesGeneratedUnits">Unidades de vacíos generadas.</param>
/// <param name="EmptiesReturnedBoxes">Cajas de vacíos devueltas en facturas.</param>
/// <param name="EmptiesReturnedUnits">Unidades de vacíos devueltas en facturas.</param>
/// <param name="InternalConsumptionValue">Valor del consumo interno del día.</param>
/// <param name="DamageLoss">Pérdida por averías del día.</param>
public sealed record DailyReportResponse(
    DateOnly Date, decimal SalesTotal, int SalesCount, int GiftsCount, decimal CollectedOnInvoices, decimal CreditGenerated,
    decimal PaymentsTotal, int EmptiesGeneratedBoxes, int EmptiesGeneratedUnits, int EmptiesReturnedBoxes, int EmptiesReturnedUnits,
    decimal InternalConsumptionValue, decimal DamageLoss);

/// <summary>Balance general.</summary>
/// <param name="MoneyOnStreet">Dinero en la calle: suma de deudas de clientes.</param>
/// <param name="EmptyBoxesOnStreet">Cajas de vacíos adeudadas.</param>
/// <param name="EmptyUnitsOnStreet">Unidades de vacíos adeudadas.</param>
/// <param name="ReceivableClients">Clientes con alguna deuda.</param>
/// <param name="Inventory">Inventario de galpón y camiones.</param>
/// <param name="ConsignmentReceivable">Por cobrar en consignaciones.</param>
public sealed record BalanceResponse(
    decimal MoneyOnStreet, int EmptyBoxesOnStreet, int EmptyUnitsOnStreet, int ReceivableClients,
    InventorySummaryResponse Inventory, decimal ConsignmentReceivable);

/// <summary>
/// Dashboard KPI del rango indicado. Las ventas excluyen documentos anulados; con categoría,
/// las ventas son la suma de los renglones de esa categoría.
/// </summary>
/// <param name="From">Desde (día local, inclusive).</param>
/// <param name="To">Hasta (día local, inclusive).</param>
/// <param name="CategoryId">Categoría filtrada, o null para todas.</param>
/// <param name="Inventory">Valorización del inventario: costo frente a precio de venta.</param>
/// <param name="Sales">Totales de ventas del rango.</param>
/// <param name="StockAlerts">Conteo de productos por estado de salud del stock.</param>
/// <param name="CriticalProducts">Productos agotados, en riesgo crítico o en exceso (máximo 25, los más urgentes primero).</param>
/// <param name="SalesByDay">Serie diaria de ventas, un punto por día del rango.</param>
/// <param name="SalesByCategory">Ventas y unidades por categoría.</param>
/// <param name="TopProducts">Productos con más unidades vendidas y su índice de rotación (máximo 10).</param>
/// <param name="Fleet">Ocupación y ventas por camión.</param>
/// <param name="Receivables">Cuentas por cobrar con el envejecimiento de la deuda al día de hoy (no depende del rango ni de la categoría).</param>
public sealed record DashboardResponse(
    DateOnly From,
    DateOnly To,
    Guid? CategoryId,
    DashboardInventoryKpi Inventory,
    DashboardSalesKpi Sales,
    IReadOnlyList<StockAlertCount> StockAlerts,
    IReadOnlyList<CriticalProductResponse> CriticalProducts,
    IReadOnlyList<SalesPoint> SalesByDay,
    IReadOnlyList<CategorySales> SalesByCategory,
    IReadOnlyList<ProductRotation> TopProducts,
    DashboardFleetKpi Fleet,
    DashboardReceivablesKpi Receivables);

/// <summary>Cuentas por cobrar y envejecimiento de la deuda (criterio FIFO), en tramos de 0-30, 31-60, 61-90 y más de 90 días.</summary>
/// <param name="TotalDebt">Deuda total de los clientes en USD.</param>
/// <param name="Debtors">Clientes que deben dinero.</param>
/// <param name="Overdue">Deuda vencida según los días de crédito de cada cliente.</param>
/// <param name="BlockedClients">Clientes que hoy solo pueden comprar de contado.</param>
/// <param name="Days0To30">Deuda de 0 a 30 días.</param>
/// <param name="Days31To60">Deuda de 31 a 60 días.</param>
/// <param name="Days61To90">Deuda de 61 a 90 días.</param>
/// <param name="Over90">Deuda de más de 90 días.</param>
/// <param name="TopDebtors">Clientes con más deuda (máximo 10) y su deuda por tramo.</param>
public sealed record DashboardReceivablesKpi(
    decimal TotalDebt, int Debtors, decimal Overdue, int BlockedClients,
    decimal Days0To30, decimal Days31To60, decimal Days61To90, decimal Over90,
    IReadOnlyList<ClientDebtAging> TopDebtors);

/// <summary>Deuda de un cliente por antigüedad.</summary>
/// <param name="ClientId">Cliente.</param>
/// <param name="Code">Código.</param>
/// <param name="Name">Nombre.</param>
/// <param name="Type">Tipo de cliente.</param>
/// <param name="Debt">Deuda total.</param>
/// <param name="Days0To30">De 0 a 30 días.</param>
/// <param name="Days31To60">De 31 a 60 días.</param>
/// <param name="Days61To90">De 61 a 90 días.</param>
/// <param name="Over90">Más de 90 días.</param>
/// <param name="OldestDays">Días de la deuda más vieja (null si parte no tiene factura).</param>
/// <param name="Overdue">Vencido según sus días de crédito.</param>
/// <param name="IsBlocked">Solo puede comprar de contado.</param>
public sealed record ClientDebtAging(
    Guid ClientId, string Code, string Name, string Type, decimal Debt,
    decimal Days0To30, decimal Days31To60, decimal Days61To90, decimal Over90, int? OldestDays, decimal Overdue, bool IsBlocked);

/// <summary>Valorización del inventario (galpón + camiones).</summary>
/// <param name="ActiveProducts">Productos activos considerados.</param>
/// <param name="StockUnits">Unidades en galpón y camiones.</param>
/// <param name="CostValue">Valor al costo: unidades × (costo por caja / unidades por caja).</param>
/// <param name="SaleValue">Valor a precio de venta: unidades × precio por unidad.</param>
/// <param name="GrossMargin">Margen bruto proyectado: venta − costo.</param>
/// <param name="GrossMarginPercent">Margen bruto sobre el valor de venta, en %.</param>
public sealed record DashboardInventoryKpi(
    int ActiveProducts, int StockUnits, decimal CostValue, decimal SaleValue, decimal GrossMargin, decimal GrossMarginPercent);

/// <summary>Ventas del rango.</summary>
/// <param name="SalesTotal">Ventas en USD.</param>
/// <param name="InvoicesCount">Facturas de venta.</param>
/// <param name="GiftsCount">Regalías.</param>
/// <param name="AverageTicket">Ticket promedio: ventas / facturas.</param>
/// <param name="UnitsSold">Unidades vendidas (facturas y regalías).</param>
/// <param name="CollectedOnInvoices">Cobrado al facturar (sin filtro de categoría).</param>
/// <param name="CreditGenerated">Fiado generado (sin filtro de categoría).</param>
/// <param name="RotationIndex">Rotación del período: unidades vendidas / unidades en existencia.</param>
public sealed record DashboardSalesKpi(
    decimal SalesTotal, int InvoicesCount, int GiftsCount, decimal AverageTicket, int UnitsSold,
    decimal CollectedOnInvoices, decimal CreditGenerated, decimal RotationIndex);

/// <summary>Cantidad de productos en un estado de salud del stock.</summary>
/// <param name="Status">OutOfStock, CriticalRisk, Understocked, Optimal, Overstocked o Excessive.</param>
/// <param name="Count">Productos en ese estado.</param>
public sealed record StockAlertCount(string Status, int Count);

/// <summary>Producto que requiere atención por su nivel de stock en galpón.</summary>
public sealed record CriticalProductResponse(
    Guid Id, string SKU, string Name, string? CategoryName, int StockUnits, string StockDisplay,
    int MinStock, int MaxStock, string Status, int RecommendedPurchaseUnits);

/// <summary>Ventas de un día local.</summary>
public sealed record SalesPoint(DateOnly Date, decimal Total, int Invoices, int Units);

/// <summary>Ventas de una categoría en el rango.</summary>
public sealed record CategorySales(Guid CategoryId, string CategoryName, decimal Total, int Units);

/// <summary>Rotación de un producto: unidades vendidas en el rango frente a la existencia actual.</summary>
public sealed record ProductRotation(
    Guid ProductId, string SKU, string Name, string? CategoryName, int UnitsSold, decimal Revenue, int StockUnits, decimal RotationIndex);

/// <summary>Ocupación de la flota.</summary>
/// <param name="CapacityBoxes">Capacidad total de los camiones, en cajas.</param>
/// <param name="LoadedBoxes">Carga total en cajas equivalentes.</param>
/// <param name="OccupancyPercent">Ocupación de la flota en %.</param>
/// <param name="Trucks">Detalle por camión.</param>
public sealed record DashboardFleetKpi(int CapacityBoxes, decimal LoadedBoxes, decimal OccupancyPercent, IReadOnlyList<TruckOccupancy> Trucks);

/// <summary>Ocupación y ventas despachadas desde un camión en el rango.</summary>
public sealed record TruckOccupancy(
    Guid TruckId, string Name, string Plate, int CapacityBoxes, decimal LoadedBoxes, decimal OccupancyPercent,
    int StockUnits, decimal InventoryValue, decimal SalesTotal, int Invoices);
