namespace Core.Application.DTOs;

/// <summary>Producto del catálogo. El stock se expresa en unidades y también en cajas + unidades sueltas.</summary>
/// <param name="Id">Identificador UUID del producto.</param>
/// <param name="SKU">Código corporativo único con formato [CAT]-[PRO]-[SEQ], por ejemplo BEB-MAL-0006.</param>
/// <param name="Name">Nombre comercial del producto.</param>
/// <param name="Brand">Marca.</param>
/// <param name="CategoryId">Identificador de la categoría.</param>
/// <param name="CategoryName">Nombre de la categoría.</param>
/// <param name="Deposit">Depósito del galpón donde se almacena (según la categoría).</param>
/// <param name="PriceBox">Precio de venta por caja (USD).</param>
/// <param name="PriceUnit">Precio de venta por unidad suelta (USD), independiente del precio por caja.</param>
/// <param name="CostPrice">Costo por caja (USD).</param>
/// <param name="UnitsPerBox">Unidades que trae cada caja.</param>
/// <param name="StockUnits">Existencia total en unidades.</param>
/// <param name="StockBoxes">Cajas completas de la existencia.</param>
/// <param name="StockLooseUnits">Unidades sueltas de la existencia.</param>
/// <param name="StockDisplay">Existencia en texto, por ejemplo "2 cajas + 5 und".</param>
/// <param name="MinStock">Stock mínimo de seguridad, en unidades.</param>
/// <param name="MaxStock">Capacidad máxima de almacenamiento, en unidades.</param>
/// <param name="InventoryValue">Valor del inventario: unidades × precio por unidad (USD).</param>
/// <param name="IsReturnable">Indica si el envase es retornable.</param>
/// <param name="EmptyGroupKey">Grupo de vacíos al que pertenece, o null si no genera vacíos.</param>
/// <param name="EmptyGroupName">Nombre legible del grupo de vacíos.</param>
/// <param name="IsGiftEligible">Indica si se puede entregar como regalía a clientes.</param>
/// <param name="IsActive">Indica si el producto está activo para la venta.</param>
/// <param name="CreatedAt">Fecha de creación (UTC).</param>
/// <param name="LastModifiedAt">Fecha de la última modificación (UTC), si existe.</param>
public sealed record ProductResponse(
    Guid Id,
    string SKU,
    string Name,
    string Brand,
    Guid CategoryId,
    string? CategoryName,
    string? Deposit,
    decimal PriceBox,
    decimal PriceUnit,
    decimal CostPrice,
    int UnitsPerBox,
    int StockUnits,
    int StockBoxes,
    int StockLooseUnits,
    string StockDisplay,
    int MinStock,
    int MaxStock,
    decimal InventoryValue,
    bool IsReturnable,
    string? EmptyGroupKey,
    string? EmptyGroupName,
    bool IsGiftEligible,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>
/// Alta de producto. El SKU se genera automáticamente; el stock inicial se captura en cajas + unidades sueltas.
/// MinStock y MaxStock se expresan en unidades.
/// </summary>
/// <param name="Name" example="HARINA PAN 1KGx20UN">Nombre, obligatorio, máximo 150 caracteres.</param>
/// <param name="Brand" example="P.A.N.">Marca, obligatoria, máximo 80 caracteres.</param>
/// <param name="CategoryId" example="a1111111-1111-4111-8111-111111111111">Categoría existente.</param>
/// <param name="PriceBox" example="18.50">Precio por caja, mayor que 0, máximo 2 decimales.</param>
/// <param name="PriceUnit" example="0.95">Precio por unidad, mayor que 0, máximo 2 decimales.</param>
/// <param name="CostPrice" example="14.80">Costo por caja, mayor o igual que 0, máximo 2 decimales.</param>
/// <param name="UnitsPerBox" example="20">Unidades por caja, mayor que 0.</param>
/// <param name="InitialBoxes" example="2">Cajas del stock inicial, mayor o igual que 0.</param>
/// <param name="InitialLooseUnits" example="5">Unidades sueltas del stock inicial, mayor o igual que 0.</param>
/// <param name="MinStock" example="100">Stock mínimo en unidades, mayor o igual que 0.</param>
/// <param name="MaxStock" example="800">Stock máximo en unidades, mayor que el mínimo.</param>
/// <param name="IsReturnable" example="false">Indica si el envase es retornable.</param>
/// <param name="EmptyGroupKey" example="null">Grupo de vacíos: RET_222ML_X36, RET_POLAR_PILSEN_330ML_X24, PEPSI_125L_X6, PEPSI_350ML_X24 o null.</param>
/// <param name="IsGiftEligible" example="false">Autorizado para regalías a clientes (por defecto false).</param>
public sealed record CreateProductRequest(
    string Name,
    string Brand,
    Guid CategoryId,
    decimal PriceBox,
    decimal PriceUnit,
    decimal CostPrice,
    int UnitsPerBox,
    int InitialBoxes,
    int InitialLooseUnits,
    int MinStock,
    int MaxStock,
    bool IsReturnable,
    string? EmptyGroupKey,
    bool IsGiftEligible = false);

/// <summary>
/// Edición de producto (solo Admin). El stock no se edita aquí: cambia solo por operaciones de inventario.
/// </summary>
/// <param name="Name" example="HARINA PAN 1KGx20UN">Nombre, obligatorio, máximo 150 caracteres.</param>
/// <param name="Brand" example="P.A.N.">Marca, obligatoria, máximo 80 caracteres.</param>
/// <param name="CategoryId" example="a1111111-1111-4111-8111-111111111111">Categoría existente.</param>
/// <param name="PriceBox" example="19.00">Precio por caja, mayor que 0, máximo 2 decimales.</param>
/// <param name="PriceUnit" example="0.98">Precio por unidad, mayor que 0, máximo 2 decimales.</param>
/// <param name="CostPrice" example="15.20">Costo por caja, mayor o igual que 0, máximo 2 decimales.</param>
/// <param name="UnitsPerBox" example="20">Unidades por caja, mayor que 0.</param>
/// <param name="MinStock" example="100">Stock mínimo en unidades, mayor o igual que 0.</param>
/// <param name="MaxStock" example="800">Stock máximo en unidades, mayor que el mínimo.</param>
/// <param name="IsReturnable" example="false">Indica si el envase es retornable.</param>
/// <param name="EmptyGroupKey" example="null">Grupo de vacíos o null.</param>
/// <param name="IsGiftEligible" example="false">Autorizado para regalías a clientes.</param>
public sealed record UpdateProductRequest(
    string Name,
    string Brand,
    Guid CategoryId,
    decimal PriceBox,
    decimal PriceUnit,
    decimal CostPrice,
    int UnitsPerBox,
    int MinStock,
    int MaxStock,
    bool IsReturnable,
    string? EmptyGroupKey,
    bool IsGiftEligible = false);
