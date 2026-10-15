# Requisitos por fase

Relación entre cada requisito de las fases del proyecto, dónde está implementado y cómo se verifica.
Todas las verificaciones se pueden repetir con el entorno de Development en marcha (`docker compose up -d --build`).

## Fase 1: Fundamentos arquitectónicos, .NET 10 y resiliencia REST

| Requisito | Implementación | Verificación |
| --- | --- | --- |
| Solución con 4 proyectos y referencias Onion | `DistribucionPolar.slnx` y los `.csproj` (ver [Arquitectura §1](02-ARQUITECTURA.md#1-visión-general)) | `dotnet build DistribucionPolar.slnx` sin advertencias ni errores |
| `BaseEntity` con `Id`, `CreatedAt`, `LastModifiedAt`, `IsDeleted` | `src/backend/Core.Domain/Common/BaseEntity.cs` | `ProductTests.MarkAsDeleted_SetsSoftDeleteFlag` |
| Entidad `Product` con reglas de negocio | `src/backend/Core.Domain/Entities/Product.cs` | `ProductTests` |
| Middleware global de excepciones RFC 7807 registrado en el pipeline | `src/backend/Presentation.API/Middleware/ExceptionMiddleware.cs`, `Program.cs` | `GET /api/demo/exception` → 500 `application/problem+json` sin stack trace |
| Reto 1: evaluador de salud con *switch* sobre tuplas | `src/backend/Core.Domain/Services/ProductHealthEvaluator.cs` | `ProductHealthEvaluatorTests`; `POST /api/demo/csharp/health-calculator` |
| Reto 2: generador de SKU con expresiones regulares y rangos | `src/backend/Core.Domain/Services/CorporateSkuGenerator.cs` | `CorporateSkuGeneratorTests`; `POST /api/demo/csharp/sku-generator` → `HER-TAL-0007` |
| Inyección de dependencias con Transient, Scoped y Singleton | `DependencyInjection.cs` de Application e Infrastructure ([Arquitectura §11](02-ARQUITECTURA.md#11-inyección-de-dependencias)) | Revisión de código |
| Pruebas unitarias xUnit pasando al 100% | `tests/UnitTests` (80 de dominio y 88 de aplicación), además de `tests/IntegrationTests` (16) | `dotnet test DistribucionPolar.slnx` → 184/184 |

**Adaptaciones al dominio respecto de la guía de laboratorio:**
- `Product` usa `PriceBox`/`PriceUnit` en lugar de `Price`, por la venta por caja y por unidad ([ADR 05](04-DECISIONES.md#05-precio-por-caja-y-precio-por-unidad-independientes)).
- Usa `StockUnits` en lugar de `StockQuantity` ([ADR 04](04-DECISIONES.md#04-stock-en-unidades-con-el-value-object-boxquantity)).
- Usa `AddStock`/`RemoveStock`, que validan la cantidad y el stock disponible, en lugar de `UpdateStock`.
- `Id` es `Guid`, como pide la Fase 2.

## Fase 2: Persistencia y siembra de datos

| Requisito | Implementación | Verificación |
| --- | --- | --- |
| EF Core 10 sobre PostgreSQL 15 | Npgsql en `Infrastructure`; `postgres:15-alpine` en `docker-compose.yml` | pgAdmin → Servers → Distribución Polar |
| Una `IEntityTypeConfiguration<T>` por entidad | `src/backend/Infrastructure/Persistence/Configurations/` (`Category`, `Product`, `User`) | Revisión de código |
| Claves primarias UUID y nombres de tabla explícitos | `BaseEntityConfiguration` (`uuid`), `ToTable("Categories" / "Products" / "Users")` | pgAdmin → Tables |
| Longitudes máximas de texto | `HasMaxLength` en cada configuración | Columnas `character varying(n)` |
| Precisión `numeric(18,2)` en montos | `ProductConfiguration`: `HasPrecision(18, 2)` en `PriceBox`, `PriceUnit` y `CostPrice` | pgAdmin → Products → Columns |
| Índices únicos | SKU (`Products`); `Username` y `Email` (`Users`); nombre de categoría vigente (`Categories`) | pgAdmin → Indexes |
| Relación 1:N con `DeleteBehavior.Restrict` | `ProductConfiguration`: `HasOne(Category).WithMany(Products).OnDelete(Restrict)` | Constraint `FK_Products_Categories_CategoryId … ON DELETE RESTRICT` |
| `ApplicationDbContext` con `ApplyConfigurationsFromAssembly` | `src/backend/Infrastructure/Persistence/ApplicationDbContext.cs` | Revisión de código |
| Migraciones generadas por CLI y versionadas | `src/backend/Infrastructure/Persistence/Migrations/` (`InitialCatalog`, `BusinessOperations`); herramienta fijada en `dotnet-tools.json` | `dotnet ef migrations list --project src/backend/Infrastructure --startup-project src/backend/Presentation.API` |
| Siembra: mínimo 2 categorías y 4 productos con precio, costo, stock y SKU | `HasData` con 3 categorías y 275 productos (`CatalogSeedData.cs`) | `SELECT count(*) FROM "Products"` → 275 |
| Repositorio desacoplado por interfaces con `.AsNoTracking()` en lecturas | Interfaces en `src/backend/Core.Application/Abstractions`; implementaciones en `src/backend/Infrastructure/Persistence/Repositories` | Revisión de código |
| Script SQL o evidencia de la base | `docs/fase2/schema.sql` (idempotente, ambas migraciones) | Ejecutable en cualquier PostgreSQL 15 |

Complementos de la teoría de la fase también aplicados:
- Check constraints sobre precios y stock.
- Auditoría en `timestamptz`.
- Normalización 3FN (el depósito vive solo en `Categories`).
- Transacción única por caso de uso con `IUnitOfWork`.

## Fase 3: Seguridad stateless (JWT), RBAC y validación

| Requisito | Implementación | Verificación |
| --- | --- | --- |
| `POST /api/auth/login` con username/email y clave | `AuthController`, `AuthService`, `LoginRequest` | Postman → *Login Admin* / *Login Employee* |
| Validación de credenciales contra el hash guardado | `Pbkdf2PasswordHasher` (PBKDF2-HMAC-SHA256, [ADR 18](04-DECISIONES.md#18-claves-con-pbkdf2-hmac-sha256)) | Postman → *Login con clave incorrecta* → 401 |
| Respuesta con token, username, email y rol | `AuthResponse` | Postman: verifica `role` y los claims del token |
| JWT firmado con HMAC-SHA256, con claims y expiración | `JwtTokenService`; validación en `AuthenticationSetup` | Postman: verifica `alg = HS256` y los claims `sub`, `unique_name`, `email`, `role`, `exp`, `iss`, `aud`, `jti` |
| Clave secreta fuera del código | `JWT_KEY` en `.env` / `.env.staging` / `.env.production` (ignorados por Git) o user-secrets en desarrollo sin Docker; ninguna clave en `appsettings` | Ningún entorno arranca sin `JWT_KEY` |
| DELETE y gestión de categorías solo para Admin | `[Authorize(Roles = Roles.Admin)]` en `ProductsController` y `CategoriesController` | Postman → *Employee elimina producto* → 403; *Crear categoría (Employee)* → 403 |
| Employee consulta catálogos y registra productos | `[Authorize(Roles = Roles.AdminOrEmployee)]` | Postman → *Registrar producto (Employee)* → 201 |
| Petición sin token → 401 | Política de respaldo autenticada ([ADR 17](04-DECISIONES.md#17-autenticación-obligatoria-por-defecto)) | Postman → *Endpoint protegido sin token* → 401 |
| Validadores FluentValidation en Core.Application para los DTOs de creación y edición | `src/backend/Core.Application/Validators/` | `ValidatorTests` |
| Reglas: precio > 0, stock ≥ 0, stock máximo > mínimo, textos obligatorios con límite | `ProductValidators.cs`, `CategoryValidators.cs`, `AuthValidators.cs` | Postman → *Producto con precio negativo* → 400 con `errors.priceBox` |
| 400 con el detalle de cada error | `ExceptionMiddleware` → `ValidationProblemDetails` | Respuesta con el objeto `errors` por campo |
| Colección de Postman con los 4 escenarios | `postman/collections/DistribucionPolar.postman_collection.json` (carpetas 1 y 2; la carpeta 5 cubre la operación completa) | `npx newman run …` → 50 peticiones, 97 aserciones |

## Fase 4: Frontend SPA, dashboard KPI y pruebas unitarias

| Requisito | Implementación | Verificación |
| --- | --- | --- |
| SPA desacoplada con React 18+, Vite y Tailwind CSS | `src/frontend` (React 18.3, Vite 6, TypeScript, Tailwind 3.4 con `unet.blue #003366` y `darkMode: 'class'`) | `npm run build`; navegación sin recargas con React Router |
| Componentes modulares y diseño adaptable | Átomos en `components/ui`, layout con menú colapsable y menú lateral en móvil, tablas con `overflow-x-auto` | Sin desplazamiento horizontal de la página a 375 px |
| `AuthContext`: sesión, JWT decodificado y persistido | `src/context/AuthContext.tsx` (`jwt-decode`, `localStorage`, cierre al vencer o ante un 401) | `AuthContext.test.tsx`: guarda, recupera al recargar, descarta vencido, cierra sola |
| `ThemeContext`: Azul UNET y modo oscuro persistido | `src/context/ThemeContext.tsx` | `ThemeContext.test.tsx` |
| Consumo de la API con Bearer y manejo de errores | `src/api/client.ts`: interceptor `Authorization: Bearer`, normaliza RFC 7807 (`title`, `detail`, `errors` por campo) | `client.test.ts`; los errores 400 se muestran junto a cada campo del formulario |
| Dashboard KPI (`PowerBIDashboard`) con datos reales | `components/dashboard/PowerBIDashboard.tsx` sobre `GET /api/reports/dashboard` ([ADR 41](04-DECISIONES.md#41-dashboard-kpi-calculado-en-la-capa-de-aplicación)) | `ReportServiceTests`; `Dashboard_ReflectsTruckSaleOccupancyAndRotation_AndIsAdminOnly` |
| Valorización costo frente a precio de venta | Tarjeta de margen y barra "Valorización del inventario" | KPI `inventory.costValue` / `saleValue` |
| Rotación de stock | KPI de rotación y tabla "Rotación por producto" (unidades vendidas / existencia) | KPI `sales.rotationIndex` y `topProducts` |
| Alertas MinStock / MaxStock | Distribución por estado y tabla "Stock crítico" con unidades a reponer | `ProductHealthEvaluator` del dominio |
| Gráficos interactivos y filtros por fecha y categoría | Recharts: ventas por día y por categoría; al pulsar una barra se filtra todo el dashboard. Filtros en la URL | Postman / navegador: `?from=&to=&categoryId=` |
| Al menos tres tarjetas KPI | Ventas del período, rotación, margen bruto, productos bajo el mínimo | Pantalla del dashboard |
| Contención contra desbordamiento | `ResponsiveContainer` y `DataTable` con `overflow-x-auto` | Vista a 375 px |
| CRUD de productos y categorías según el rol | `ProductsPage`, `CategoriesPage` | Employee no ve los botones de editar ni eliminar |
| Dashboard solo para Admin | Oculto del menú para Employee (`navigation.ts`) y protegido con `ProtectedRoute role="Admin"`; la API responde 403 | `ProtectedRoute.test.tsx`; prueba de integración del dashboard |
| Pruebas xUnit + Moq de `ProductService` con mock de `IProductRepository` | `tests/UnitTests/Application/ProductServiceTests.cs`: `GetByIdAsync_ExistingId_ReturnsProductDto`, `GetProductByIdAsync_WhenNotFound_ThrowsKeyNotFoundException` y 13 casos más | `dotnet test` |
| Casos exitosos y de excepción, aislados de la base de datos | 88 pruebas de aplicación con Moq (`Setup`/`Verify`) en todos los servicios | Reporte de cobertura (coverlet + ReportGenerator): 85,8 % de líneas del núcleo |

**Ideas del profesor incorporadas:** tarjetas por camión con ocupación ([ADR 42](04-DECISIONES.md#42-capacidad-del-camión-en-cajas)), KPI de ocupación de flota y ventas por camión, ticket de 80 mm imprimible desde el navegador (`Ticket.tsx`, `@media print`) y menú lateral colapsable.

**Datos de la demostración:** `scripts/demo-operacion.sh` carga camiones y emite facturas a través de la API; no inserta datos en la base directamente. Como la API emite cada factura con la fecha del momento, `scripts/demo-fechas.sh` las reparte después en los últimos días (junto con sus movimientos de saldo) para que el dashboard muestre ventas por día. Es el único paso que modifica la base: solo cambia fechas y se usa únicamente en Development.

## Fase 5: Contenerización multi-stage, Docker Compose, CI/CD y modelo de datos

| Requisito | Implementación | Verificación |
| --- | --- | --- |
| Estructura del repositorio oficial | `src/backend/` (4 proyectos), `src/frontend/`, `tests/UnitTests/` (con `Application/ProductServiceTests.cs`), `tests/IntegrationTests/`, `.github/workflows/ci.yml`, `docker-compose.yml` | Árbol del [README](../README.md#estructura) |
| Dockerfile multi-stage de la API | `src/backend/Presentation.API/Dockerfile`: `sdk:10.0-alpine` → `runtime-deps:10.0-alpine` ([ADR 45](04-DECISIONES.md#45-imagen-de-la-api-self-contained-y-recortada-sobre-alpine)) | `docker images`: ~60 MB sin comprimir |
| Dockerfile multi-stage de la SPA | `src/frontend/Dockerfile`: `node:20-alpine` → `nginx:alpine`, con fallback a `index.html` (`nginx.conf`) | `docker images`: ~59 MB sin comprimir; `GET /dashboard` en el puerto 8080 → 200 |
| Imágenes de menos de 120 MB, sin SDK | Etapas finales sin SDK ni Node.js | El job `docker-images` del CI falla si alguna pesa 120 MB o más; cifras en [evidencias](evidencias/README.md#imágenes-docker) |
| `docker-compose.yml` con db, api y spa | `postgres:15-alpine`, API .NET 10 y SPA Nginx | `docker compose up --build -d` |
| Healthchecks y `depends_on` | `pg_isready`, `GET /health` (incluye PostgreSQL), `GET /healthz`; `api` espera a `db` sana y `spa` a `api` sana | `docker ps` → los tres `(healthy)` |
| Volumen persistente y puertos 8080 / 5000 / 5432 | Volumen `postgres-data`; puertos en `docker-compose.override.yml` | Los datos sobreviven a `docker compose down` (sin `-v`) |
| Red interna aislada | Redes `backend` (db, api) y `frontend` (api, spa) | `docker network inspect`: la SPA no ve a `db` |
| Clave JWT fuera del código | `JWT_KEY` desde `.env` (ignorado por Git, `.env.example` versionado) | `git grep` sin claves; Compose no arranca sin `JWT_KEY` |
| CI en push y pull request a `main` (y `develop`) | `.github/workflows/ci.yml` ([Arquitectura §9](02-ARQUITECTURA.md#integración-continua-githubworkflowsciyml)) | Pestaña Actions de GitHub |
| Restore, build y pruebas xUnit + Moq | Job `backend-ci` (unitarias con cobertura e integración) | Resumen del job con la cobertura |
| Artefacto publicado solo si las pruebas pasan | Job `publish-artifacts` con `needs: [backend-ci, frontend-ci]` | Artefactos `api-publish` y `spa-dist` |
| Build del frontend | Job `frontend-ci` (`npm ci`, Vitest, `npm run build`) | — |
| Las 4 dimensiones operacionales | [Modelo de datos §1](07-MODELO-DATOS.md#1-las-cuatro-dimensiones-operacionales) | — |
| Diccionario de datos de Categories, Products y Users (PK, FK e índices) | [Modelo de datos §3](07-MODELO-DATOS.md#3-diccionario-de-datos), sacado del esquema real | pgAdmin; `docs/fase2/schema.sql` |
| Proyecto integrador "otro, notificado al facilitador" | Distribución Polar (logística de flota, preventa y envases retornables), evaluado como Grupo 1 | — |
| Dashboard KPI específico de la propuesta | Ocupación de la flota por camión y ventas por camión ([ADR 42](04-DECISIONES.md#42-capacidad-del-camión-en-cajas)) | Dashboard en http://localhost:8080 |
| Sustentación: demostración en vivo | `docker compose down -v`, `docker compose up --build -d`, `docker ps`, `scripts/demo-operacion.sh`, `scripts/demo-fechas.sh`, login Admin y Employee en la SPA | [Evidencias](evidencias/README.md) |

## Teoría de las fases implementada

Conceptos del material de lectura que no estaban en las tareas evaluadas y también están implementados.
El detalle de cada uno está en [Fundamentos](06-FUNDAMENTOS.md).

| Fase | Concepto | Implementación |
| --- | --- | --- |
| 1 | Resiliencia REST: reintentos, timeout y **Circuit Breaker** | Polly sobre el `HttpClient` de tasas de cambio, con fallback; reintentos de Npgsql |
| 2 | Transacciones **ACID** explícitas | `IUnitOfWork.ExecuteInTransactionAsync` en cada proceso de negocio |
| 2 | Tabla de **auditoría** con BIGINT, `jsonb` e `inet` | `AuditLogs`; consulta en `GET /api/reports/audit` |
| 2 | `AsNoTrackingWithIdentityResolution` y **proyecciones a DTO** | Camiones, facturas, listados y reportes |
| 2 | Aislamiento y concurrencia | Concurrencia optimista con `xmin` (409) |
| 1–3 | **CORS** | Política por entorno (`Cors:AllowedOrigins`) |
| 3 | Protección contra fuerza bruta | **Rate limiting** del login (429) y límite global |

## Además de lo requerido

| Elemento | Dónde |
| --- | --- |
| Operación completa del negocio: camiones, clientes y saldos, facturación, regalías, anulación, vacíos, compras, averías, consumos, consignación, reportes, usuarios | [Lógica de negocio](03-LOGICA-NEGOCIO.md) |
| Ticket PDF, liquidación PDF y respaldo JSON/CSV | `QuestPdfGenerator`, `BackupExporter` |
| Entornos Development, Staging y Production, cada uno con su SPA, API y base | `docker-compose*.yml`, `appsettings.{Entorno}.json`, `.env*.example` |
| Documentación OpenAPI 3.1 de los 79 endpoints + Swagger UI | `/swagger`, `docs/openapi/` |
| Pruebas de integración contra PostgreSQL real | `tests/IntegrationTests` (Testcontainers) |
| Sondas de salud y de entorno | `/health`, `/api/info` |
| pgAdmin preconfigurado en Development | `docker-compose.override.yml`, `docker/pgadmin/` |
| Prueba de humo con Compose y control del tamaño de las imágenes en el CI | Job `docker-images` |
| Datos de demostración generados a través de la API, sin insertar en la base, y repartidos en los últimos días | `scripts/demo-operacion.sh`, `scripts/demo-fechas.sh` |
