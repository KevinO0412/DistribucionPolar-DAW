# Pruebas

| Proyecto | Tipo | Pruebas | Necesita |
| --- | --- | ---: | --- |
| `UnitTests/` | Unitarias del núcleo con xUnit + Moq | 168 | Solo el SDK de .NET 10 |
| `IntegrationTests/` | API real (`WebApplicationFactory`) contra PostgreSQL 15 desechable (Testcontainers) | 16 | Docker |
| `../src/frontend` | SPA con Vitest + Testing Library | 31 | Node.js 20 |

```bash
dotnet test tests/UnitTests                      # unitarias, sin base de datos
dotnet test tests/IntegrationTests               # integración (levanta un PostgreSQL en Docker)
dotnet test DistribucionPolar.slnx               # todo el backend
scripts/cobertura.sh                             # cobertura HTML en docs/evidencias/cobertura/index.html
(cd src/frontend && npm test)                    # frontend
```

## Pruebas unitarias (`UnitTests/`)

Un solo proyecto, como en la estructura del curso, con dos carpetas:

| Carpeta | Pruebas | Qué cubre |
| --- | ---: | --- |
| `Domain/` | 80 | Reglas puras del dominio: cajas ↔ unidades, precios, SKU, salud del stock, saldos y vacíos del cliente, camiones (incluida la capacidad), facturas, regalías, anulación, consignación, compras y averías |
| `Application/` | 88 | Servicios y validadores con **todas** las dependencias simuladas con Moq. Ninguna toca PostgreSQL ni EF Core |

### Prueba de `ProductService` con mock de `IProductRepository`

Los nombres coinciden con los del material del curso: `ProductService` (`src/backend/Core.Application/Services`) e `IProductRepository` (`src/backend/Core.Application/Abstractions`). La prueba está en [`UnitTests/Application/ProductServiceTests.cs`](UnitTests/Application/ProductServiceTests.cs):

```csharp
[Fact]
public async Task GetByIdAsync_ExistingId_ReturnsProductDto()
{
    // Arrange
    var product = TestDoubles.HarinaPan(category.Id);
    mockRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

    // Act
    var result = await service.GetByIdAsync(product.Id);

    // Assert
    Assert.NotNull(result);
    Assert.Equal("HARINA PAN 1KGx20UN", result.Name);
    mockRepo.Verify(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
}
```

Dos diferencias con el ejemplo del curso, ambas por el diseño de la Fase 2:
- `ProductService` recibe también `ICategoryRepository`, `IUnitOfWork` y los validadores. Todos son mocks o validadores reales sin estado.
- Los métodos del repositorio reciben un `CancellationToken`, por eso los `Setup` usan `It.IsAny<CancellationToken>()`.

| Caso | Prueba |
| --- | --- |
| Éxito: obtener por id | `GetByIdAsync_ExistingId_ReturnsProductDto` |
| Excepción: id inexistente → `KeyNotFoundException` | `GetProductByIdAsync_WhenNotFound_ThrowsKeyNotFoundException` |
| Éxito: listar con filtros, buscar por SKU normalizado, stock bajo, salud del stock | `GetAllAsync_…`, `GetBySkuAsync_NormalizesSkuBeforeQuerying`, `GetLowStockAsync_…`, `GetHealthAsync_StockUnderMinimum_IsCriticalRisk` |
| Éxito: crear (SKU generado, stock en unidades) y saltar un SKU ocupado | `CreateAsync_GeneratesSkuAndStoresStockInUnits`, `CreateAsync_SkipsSkuAlreadyTaken` |
| Excepción: precio negativo → `ValidationException`, sin tocar el repositorio | `CreateAsync_WithNegativePrice_ThrowsValidation_AndNeverTouchesRepository` |
| Excepción: categoría inexistente, producto inexistente al editar o eliminar | `CreateAsync_WithUnknownCategory_…`, `UpdateAsync_WhenNotFound_…`, `DeleteAsync_WhenNotFound_…` |
| Éxito: editar precios y borrado lógico | `UpdateAsync_ChangesPrices_AndSaves`, `DeleteAsync_IsSoftDelete` |

### Resto de servicios

| Archivo | Qué verifica |
| --- | --- |
| `InvoiceServiceTests` | Venta con abono (saldo −60), abono mayor al total (saldo +20), falta de stock sin cambios ni número consumido, vacíos (−1 caja, −5 und), venta desde camión, vendedor no permitido, regalía sin efecto en dinero, anulación y doble anulación |
| `TruckServiceTests` | Carga en modo Agregar, modo Meta (10 → 36 carga 26), meta ya alcanzada, stock insuficiente, placa duplicada |
| `ClientServiceTests` | Abonos (−60 + 25 = −35), saldo a favor, monto 0, cliente inexistente |
| `ReportServiceTests` | Dashboard: rango por defecto, valorización costo/venta, ventas sin regalías, agrupación por día local, alertas MinStock/MaxStock, rotación, ocupación de flota, filtro de categoría, rangos inválidos |
| `OperationServicesTests` | Consignación (entrega 2 cajas, devuelve 30 und, cobra 18 und), averías y consumos con stock insuficiente, compras numeradas, ajuste de galpón |
| `CategoryServiceTests`, `AuthServiceTests`, `UserAndEmployeeServiceTests` | Unicidad, login por username o email, usuario inactivo, hash de claves, autoprotección del admin, desactivar en lugar de borrar |
| `ValidatorTests` | Reglas FluentValidation: precios > 0, stock ≥ 0, máximo > mínimo, textos obligatorios |

`TestDoubles.cs` arma la unidad de trabajo simulada: `ExecuteInTransactionAsync` ejecuta la operación recibida para que el servicio corra su lógica completa.

## Pruebas de integración (`IntegrationTests/`)

Cada ejecución levanta un PostgreSQL 15 nuevo y la API real con una clave JWT aleatoria. Cubren:
- los casos de referencia de la lógica de negocio de punta a punta;
- el dashboard (ventas desde un camión, ocupación, rotación, solo Admin, rango inválido);
- la concurrencia optimista, los permisos, la auditoría, CORS y el rate limiting.

## Cobertura

`scripts/cobertura.sh` corre las pruebas unitarias con coverlet (`coverlet.runsettings` mide solo `Core.Domain` y `Core.Application`) y genera el reporte con ReportGenerator. El CI lo publica como artefacto y lo resume en cada ejecución.

Resultado actual: 85,8 % de líneas y 78,5 % de ramas del núcleo, con todos los servicios de aplicación entre el 85,7 % y el 100 % ([reporte](../docs/evidencias/cobertura/index.html)).
