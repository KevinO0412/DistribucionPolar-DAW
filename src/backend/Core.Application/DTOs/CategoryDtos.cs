namespace Core.Application.DTOs;

/// <summary>Categoría del catálogo con el depósito del galpón donde se almacena.</summary>
/// <param name="Id">Identificador UUID de la categoría.</param>
/// <param name="Name">Nombre único de la categoría.</param>
/// <param name="Description">Descripción opcional.</param>
/// <param name="Deposit">Depósito del galpón asignado a la categoría.</param>
/// <param name="CreatedAt">Fecha de creación (UTC).</param>
/// <param name="LastModifiedAt">Fecha de la última modificación (UTC), si existe.</param>
public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string Deposit,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

/// <summary>Datos para crear una categoría (solo Admin).</summary>
/// <param name="Name" example="Licores">Nombre único, obligatorio, máximo 50 caracteres.</param>
/// <param name="Description" example="Rones, whiskys y vinos">Descripción opcional, máximo 250 caracteres.</param>
/// <param name="Deposit" example="Depósito #4 - Licores">Depósito del galpón, obligatorio, máximo 100 caracteres.</param>
public sealed record CreateCategoryRequest(string Name, string? Description, string Deposit);

/// <summary>Datos para editar una categoría (solo Admin).</summary>
/// <param name="Name" example="Licores y Vinos">Nombre único, obligatorio, máximo 50 caracteres.</param>
/// <param name="Description" example="Rones, whiskys y vinos">Descripción opcional, máximo 250 caracteres.</param>
/// <param name="Deposit" example="Depósito #4 - Licores">Depósito del galpón, obligatorio, máximo 100 caracteres.</param>
public sealed record UpdateCategoryRequest(string Name, string? Description, string Deposit);
