namespace Core.Application.DTOs;

/// <summary>Entrada del evaluador de salud de stock (reto 1 de la Fase 1). Cantidades en unidades.</summary>
/// <param name="Sku" example="TAL-650-01">SKU a evaluar.</param>
/// <param name="CurrentStock" example="3">Stock actual.</param>
/// <param name="MinStock" example="10">Stock mínimo de seguridad.</param>
/// <param name="MaxStock" example="50">Capacidad máxima; debe superar al mínimo.</param>
public sealed record HealthCalculatorRequest(string Sku, int CurrentStock, int MinStock, int MaxStock);

/// <summary>Entrada del generador de SKU corporativo (reto 2 de la Fase 1).</summary>
/// <param name="RawProductName" example="Taladro Percutor 1/2 Pulg 650W Bosch">Nombre del producto sin normalizar.</param>
/// <param name="CategoryName" example="Herramientas Eléctricas">Nombre de la categoría.</param>
/// <param name="SequenceNumber" example="7">Secuencia entre 1 y 9999.</param>
public sealed record SkuGeneratorRequest(string RawProductName, string CategoryName, int SequenceNumber);
