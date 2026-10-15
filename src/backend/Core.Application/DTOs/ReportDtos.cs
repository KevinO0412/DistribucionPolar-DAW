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
/// <param name="DebtAging">Antigüedad de la deuda de los clientes a la fecha (no depende del filtro).</param>
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
    DebtAgingResponse DebtAging);

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

/// <summary>Antigüedad de la deuda (los abonos saldan primero lo más antiguo).</summary>
/// <param name="TotalDebt">Dinero en la calle.</param>
/// <param name="Debtors">Clientes que deben.</param>
/// <param name="MaxOverdueDays">Días a partir de los cuales la deuda bloquea el crédito.</param>
/// <param name="OverdueDebt">Deuda con más de <paramref name="MaxOverdueDays"/> días.</param>
/// <param name="Buckets">Deuda por tramo de antigüedad.</param>
/// <param name="TopDebtors">Clientes con más deuda vencida (máximo 10).</param>
public sealed record DebtAgingResponse(
    decimal TotalDebt, int Debtors, int MaxOverdueDays, decimal OverdueDebt,
    IReadOnlyList<DebtAgingBucket> Buckets, IReadOnlyList<ClientDebtAging> TopDebtors);

/// <summary>Tramo de antigüedad: "0–7 días", "8–15 días", "16–30 días" o "Más de 30 días".</summary>
public sealed record DebtAgingBucket(string Label, int FromDays, int? ToDays, decimal Amount, int Clients);

/// <summary>Deuda de un cliente por antigüedad.</summary>
public sealed record ClientDebtAging(
    Guid ClientId, string Code, string Name, decimal Debt, decimal OverdueDebt, int OldestDays, decimal? CreditLimit);
