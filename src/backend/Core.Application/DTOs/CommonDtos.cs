namespace Core.Application.DTOs;

/// <summary>Producto y cantidad en cajas + unidades sueltas.</summary>
/// <param name="ProductId">Identificador del producto.</param>
/// <param name="Boxes" example="2">Cajas, mayor o igual que 0.</param>
/// <param name="LooseUnits" example="5">Unidades sueltas, mayor o igual que 0. Cajas y sueltas no pueden ser ambas 0.</param>
public sealed record LineRequest(Guid ProductId, int Boxes, int LooseUnits);

/// <summary>Cantidad de vacíos de un grupo de envase.</summary>
/// <param name="GroupKey" example="RET_222ML_X36">Grupo: RET_222ML_X36, RET_POLAR_PILSEN_330ML_X24, PEPSI_125L_X6 o PEPSI_350ML_X24.</param>
/// <param name="Boxes" example="2">Cajas de vacíos, mayor o igual que 0.</param>
/// <param name="Units" example="0">Unidades de vacíos, mayor o igual que 0.</param>
public sealed record EmptyGroupQuantity(string GroupKey, int Boxes, int Units);

/// <summary>Saldo de vacíos de un grupo de envase, con signo (negativo = el cliente debe).</summary>
/// <param name="GroupKey">Clave del grupo.</param>
/// <param name="GroupName">Nombre legible del grupo.</param>
/// <param name="Boxes">Cajas con signo.</param>
/// <param name="Units">Unidades con signo.</param>
public sealed record EmptyGroupBalance(string GroupKey, string GroupName, int Boxes, int Units);

/// <summary>Página de resultados.</summary>
/// <param name="Items">Elementos de la página.</param>
/// <param name="Page">Número de página (desde 1).</param>
/// <param name="PageSize">Tamaño de página.</param>
/// <param name="TotalCount">Total de elementos.</param>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
