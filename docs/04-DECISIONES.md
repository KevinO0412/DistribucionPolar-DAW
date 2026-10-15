# Registro de decisiones de arquitectura (ADR)

Cada decisión indica qué se decidió, por qué, qué alternativa se descartó y dónde se aplica.
Antes de cambiar una de estas decisiones, conviene agregar una entrada nueva que explique el motivo.

| # | Decisión | Tema |
| --- | --- | --- |
| [01](#01-arquitectura-cebolla-con-cuatro-proyectos) | Arquitectura Cebolla con cuatro proyectos | Arquitectura |
| [02](#02-interfaces-de-repositorio-en-coreapplication) | Interfaces de repositorio en Core.Application | Arquitectura |
| [03](#03-entidades-ricas-con-invariantes) | Entidades ricas con invariantes | Dominio |
| [04](#04-stock-en-unidades-con-el-value-object-boxquantity) | Stock en unidades con el value object `BoxQuantity` | Dominio |
| [05](#05-precio-por-caja-y-precio-por-unidad-independientes) | Precio por caja y por unidad independientes | Dominio |
| [06](#06-sku-generado-por-el-sistema) | SKU generado por el sistema | Dominio |
| [07](#07-claves-primarias-uuid) | Claves primarias UUID | Persistencia |
| [08](#08-montos-en-decimal-y-numeric182) | Montos en `decimal` y `numeric(18,2)` | Persistencia |
| [09](#09-fluent-api-sin-data-annotations) | Fluent API sin Data Annotations | Persistencia |
| [10](#10-borrado-lógico-con-filtro-global) | Borrado lógico con filtro global | Persistencia |
| [11](#11-repositorio--unidad-de-trabajo-asnotracking-en-lecturas) | Repositorio + unidad de trabajo, `AsNoTracking` en lecturas | Persistencia |
| [12](#12-siembra-declarativa-para-el-catálogo-y-dinámica-para-los-usuarios) | Siembra declarativa para el catálogo, dinámica para los usuarios | Persistencia |
| [13](#13-validación-en-tres-niveles) | Validación en tres niveles | Calidad |
| [14](#14-errores-centralizados-en-un-middleware-rfc-7807) | Errores centralizados en un middleware RFC 7807 | API |
| [15](#15-controladores-en-lugar-de-minimal-apis) | Controladores en lugar de Minimal APIs | API |
| [16](#16-jwt-stateless-firmado-con-hmac-sha256) | JWT stateless con HMAC-SHA256 | Seguridad |
| [17](#17-autenticación-obligatoria-por-defecto) | Autenticación obligatoria por defecto | Seguridad |
| [18](#18-claves-con-pbkdf2-hmac-sha256) | Claves con PBKDF2-HMAC-SHA256 | Seguridad |
| [19](#19-matriz-de-permisos-employee-registra-productos-pero-no-los-edita-ni-elimina) | Employee registra productos pero no los edita ni elimina | Seguridad |
| [20](#20-ciclos-de-vida-de-inyección-de-dependencias) | Ciclos de vida de inyección de dependencias | Infraestructura |
| [21](#21-comportamiento-por-entorno-mediante-configuración) | Comportamiento por entorno mediante configuración | Operación |
| [22](#22-docker-compose-con-un-archivo-base-y-uno-por-entorno) | Docker Compose: un archivo base y uno por entorno | Operación |
| [23](#23-openapi-nativo-de-net-10-con-swagger-ui) | OpenAPI nativo de .NET 10 + Swagger UI | API |
| [24](#24-pruebas-unitarias-con-moq) | Pruebas unitarias con Moq | Calidad |
| [25](#25-una-transacción-explícita-por-proceso-de-negocio) | Una transacción explícita por proceso de negocio | Persistencia |
| [26](#26-concurrencia-optimista-con-xmin) | Concurrencia optimista con `xmin` | Persistencia |
| [27](#27-numeración-atómica-en-la-base-de-datos-con-el-año-en-el-número) | Numeración atómica en la base, con el año en el número | Dominio |
| [28](#28-snapshot-del-producto-en-las-líneas-de-documento) | Snapshot del producto en las líneas de documento | Dominio |
| [29](#29-saldo-de-vacíos-por-grupo-como-dato-estructurado) | Saldo de vacíos por grupo como dato estructurado | Dominio |
| [30](#30-autorización-para-regalía-como-dato-del-producto) | Autorización para regalía como dato del producto | Dominio |
| [31](#31-vaciar-un-camión-devuelve-la-mercancía-al-galpón) | Vaciar un camión devuelve la mercancía al galpón | Dominio |
| [32](#32-no-se-elimina-un-cliente-con-saldos-pendientes) | No se elimina un cliente con saldos pendientes | Dominio |
| [33](#33-la-anulación-solo-revierte-el-pendiente) | La anulación solo revierte el pendiente | Dominio |
| [34](#34-auditoría-después-del-commit-con-un-dbcontext-propio) | Auditoría después del commit, con un `DbContext` propio | Infraestructura |
| [35](#35-resiliencia-con-polly-y-circuit-breaker-en-el-servicio-externo) | Resiliencia con Polly y Circuit Breaker en el servicio externo | Infraestructura |
| [36](#36-rate-limiting-y-cors-nativos-configurados-por-entorno) | Rate limiting y CORS nativos, configurados por entorno | Seguridad |
| [37](#37-reloj-del-negocio-con-zona-horaria) | Reloj del negocio con zona horaria | Dominio |
| [38](#38-pdf-con-questpdf) | PDF con QuestPDF | Infraestructura |
| [39](#39-pruebas-de-integración-con-postgresql-real-testcontainers) | Pruebas de integración con PostgreSQL real (Testcontainers) | Calidad |
| [40](#40-reportes-como-consultas-de-solo-lectura-proyectadas) | Reportes como consultas de solo lectura proyectadas | Persistencia |
| [41](#41-dashboard-kpi-calculado-en-la-capa-de-aplicación) | Dashboard KPI calculado en la capa de aplicación | Aplicación |
| [42](#42-capacidad-del-camión-en-cajas) | Capacidad del camión en cajas | Dominio |
| [43](#43-spa-con-context-api-jwt-en-localstorage-y-mismo-origen) | SPA con Context API, JWT en localStorage y mismo origen | Frontend |
| [44](#44-tailwind-css-3-con-tokens-semánticos) | Tailwind CSS 3 con tokens semánticos | Frontend |

---

### 01. Arquitectura Cebolla con cuatro proyectos
- **Decisión:** separar la solución en `Core.Domain`, `Core.Application`, `Infrastructure` y `Presentation.API`, con las dependencias apuntando hacia el dominio.
- **Por qué:** las reglas de negocio quedan aisladas de frameworks y bases de datos. Se prueban sin infraestructura, y cambiar la base de datos o la interfaz no toca el núcleo. Al ser proyectos separados, el compilador impide violar la regla de dependencia.
- **Descartado:** un proyecto monolítico con carpetas. Es más simple, pero nada impide que un controlador use EF Core directamente.
- **Dónde:** los `.csproj` y `DistribucionPolar.slnx`.

### 02. Interfaces de repositorio en Core.Application
- **Decisión:** `IProductRepository`, `ICategoryRepository`, `IUserRepository` e `IUnitOfWork` se declaran en `Core.Application/Abstractions`.
- **Por qué:** son *puertos* que necesitan los casos de uso. Hablan de consultas concretas (stock bajo, SKU existente) y de cancelación asíncrona, que no son reglas del negocio. Así el dominio queda formado solo por entidades y reglas, y la infraestructura sigue dependiendo de una abstracción (inversión de dependencias).
- **Descartado:** ponerlas en `Core.Domain`, que también es válido en Onion. Se prefirió un dominio sin conceptos de acceso a datos.

### 03. Entidades ricas con invariantes
- **Decisión:** las entidades tienen setters privados, constructores que validan y métodos de negocio (`AddStock`, `RemoveStock`, `UpdateDetails`, `MarkAsDeleted`). Si se viola una regla, se lanza `DomainException`.
- **Por qué:** una entidad nunca puede quedar en un estado inválido, venga la orden de la API, de una prueba o de un proceso futuro.
- **Descartado:** entidades anémicas (solo propiedades) con reglas en los servicios. Las reglas terminan duplicadas o se olvidan.
- **Dónde:** `Core.Domain/Entities`, `Core.Domain/Common/Guard.cs`.

### 04. Stock en unidades con el value object `BoxQuantity`
- **Decisión:** `Product.StockUnits` guarda unidades. `BoxQuantity` (cajas + sueltas) solo convierte y da formato.
- **Por qué:** con una sola unidad de medida se suma y resta sin ambigüedad, y el valor de la caja nunca queda desincronizado. El value object reúne toda la aritmética de cajas en un único lugar inmutable y probado.
- **Descartado:** guardar cajas y unidades en dos columnas. Obliga a normalizar en cada operación (por ejemplo, 40 sueltas con cajas de 36).
- **Dónde:** `Core.Domain/ValueObjects/BoxQuantity.cs`.

### 05. Precio por caja y precio por unidad independientes
- **Decisión:** dos campos (`PriceBox`, `PriceUnit`) en lugar de un único `Price`.
- **Por qué:** en la distribución mayorista la unidad suelta no cuesta *precio de caja ÷ unidades*; suele llevar recargo. Un solo precio no puede representar la venta mixta (cajas + sueltas).
- **Consecuencia:** el subtotal se calcula como `cajas × PriceBox + sueltas × PriceUnit`.

### 06. SKU generado por el sistema
- **Decisión:** el cliente de la API no envía el SKU. `ProductService` lo genera con `CorporateSkuGenerator` usando como secuencia el total de productos, incluidos los eliminados, + 1.
- **Por qué:** garantiza el formato `CAT-PRO-0000` y la unicidad. Contar los eliminados evita reasignar el SKU de un producto borrado.
- **Respaldo:** índice único sobre `SKU` en PostgreSQL.

### 07. Claves primarias UUID
- **Decisión:** `Guid` en todas las entidades, tipo `uuid` en PostgreSQL. En la siembra se usan UUID fijos.
- **Por qué:** los identificadores no son predecibles, lo que evita la enumeración de recursos. Se pueden generar en la aplicación sin consultar la base. Los UUID fijos de la siembra hacen que las migraciones sean deterministas.
- **Descartado:** enteros autoincrementales. Son predecibles y dependen de la base para generarse.

### 08. Montos en `decimal` y `numeric(18,2)`
- **Decisión:** todos los montos son `decimal` en C# y `numeric(18,2)` en la base. Los validadores rechazan más de 2 decimales.
- **Por qué:** `float` y `double` introducen errores de redondeo binario inaceptables en dinero.
- **Dónde:** `ProductConfiguration` (`HasPrecision(18, 2)`), `ProductValidators` (`PrecisionScale`).

### 09. Fluent API sin Data Annotations
- **Decisión:** todo el mapeo relacional va en clases `IEntityTypeConfiguration<T>` dentro de Infrastructure, cargadas con `ApplyConfigurationsFromAssembly`.
- **Por qué:** el dominio queda libre de atributos de EF, y el mapeo de cada entidad queda en un único archivo.
- **Dónde:** `Infrastructure/Persistence/Configurations`.

### 10. Borrado lógico con filtro global
- **Decisión:** `DELETE` marca `IsDeleted = true`. Un filtro global (`HasQueryFilter(e => !e.IsDeleted)`) oculta esos registros en todas las consultas.
- **Por qué:** conserva el historial y la trazabilidad, evita perder datos por error y mantiene los SKU reservados.
- **Consecuencias:**
  - El índice único del nombre de categoría es parcial (`WHERE "IsDeleted" = false`), así que se puede reutilizar el nombre de una categoría eliminada.
  - La FK `ON DELETE RESTRICT` sigue protegiendo ante borrados físicos.
- **Dónde:** `BaseEntityConfiguration`, `BaseEntity.MarkAsDeleted`.

### 11. Repositorio + unidad de trabajo, `AsNoTracking` en lecturas
- **Decisión:**
  - Los repositorios exponen consultas de lectura con `AsNoTracking()` y un `GetForUpdateAsync` con seguimiento para modificar.
  - `ApplicationDbContext` implementa `IUnitOfWork`, y los cambios se confirman con un único `SaveChangesAsync`, en una sola transacción.
- **Por qué:** las lecturas, que son la mayoría de las peticiones, no pagan el costo del ChangeTracker. Los servicios no dependen de EF Core.
- **Descartado:** inyectar el `DbContext` directamente en los servicios. Acopla la aplicación a EF Core.

### 12. Siembra declarativa para el catálogo y dinámica para los usuarios
- **Decisión:** las categorías y los productos se siembran con `HasData` (forman parte de la migración). Los usuarios los crea `DbInitializer` al arrancar, solo si la tabla está vacía.
- **Por qué:** el catálogo es estable y versionable. Los usuarios necesitan un hash con sal aleatoria, que cambiaría la migración en cada generación, y además dependen del entorno (en producción, solo el administrador definido por variable de entorno).

### 13. Validación en tres niveles
- **Decisión:** FluentValidation sobre los DTOs, invariantes en el dominio y check constraints e índices en PostgreSQL.
- **Por qué:** cada nivel cumple un objetivo distinto:
  - FluentValidation da una respuesta 400 con todos los errores por campo.
  - El dominio garantiza la consistencia aunque la orden no venga de la API.
  - La base protege ante accesos externos o errores de programación.
- **Dónde:** `Core.Application/Validators`, `Core.Domain/Common/Guard.cs`, `ProductConfiguration`.

### 14. Errores centralizados en un middleware RFC 7807
- **Decisión:** las capas lanzan excepciones estándar (`KeyNotFoundException`, `InvalidOperationException`, `DomainException`, `ValidationException`, `UnauthorizedAccessException`). Un único `ExceptionMiddleware` las traduce a Problem Details.
- **Por qué:** formato de error uniforme y predecible para cualquier cliente, sin `try/catch` repetidos en los controladores, y sin exponer el stack trace (el detalle queda solo en el log).
- **Dónde:** `Presentation.API/Middleware`.

### 15. Controladores en lugar de Minimal APIs
- **Decisión:** los endpoints de negocio son controladores `[ApiController]`.
- **Por qué:** agrupan los endpoints por recurso. Permiten declarar permisos con `[Authorize(Roles = ...)]` a nivel de clase y de método, y documentar respuestas con `[ProducesResponseType]` y comentarios XML.
- **Excepción:** `/`, `/health` y `/api/info` son endpoints mínimos porque no tienen lógica.

### 16. JWT stateless firmado con HMAC-SHA256
- **Decisión:**
  - El token incluye los claims del usuario y su rol.
  - Se valida emisor, audiencia, firma, expiración y algoritmo (`HS256`).
  - La clave se configura por entorno y la aplicación no arranca si mide menos de 32 bytes.
- **Por qué:** el servidor no guarda sesiones, lo que permite escalar horizontalmente. Fijar el algoritmo evita ataques de sustitución (`alg: none`).
- **Dónde:** `Infrastructure/Security/JwtTokenService.cs`, `Presentation.API/Security/AuthenticationSetup.cs`.

### 17. Autenticación obligatoria por defecto
- **Decisión:** la política de respaldo (`FallbackPolicy`) exige un usuario autenticado. Las rutas públicas se marcan con `[AllowAnonymous]`.
- **Por qué:** un endpoint nuevo queda protegido aunque se olvide el atributo (*secure by default*).
- **Consecuencia:** una ruta inexistente sin token responde 401 en lugar de 404.

### 18. Claves con PBKDF2-HMAC-SHA256
- **Decisión:** PBKDF2 con HMAC-SHA256, 100.000 iteraciones, sal aleatoria de 16 bytes y comparación en tiempo constante.
- **Por qué:** un SHA-256 simple se calcula a miles de millones por segundo y, sin sal, cae con tablas precalculadas. PBKDF2 aplica SHA-256 de forma iterada y salada, así que cumple el requisito de "SHA-256 o superior" con resistencia real a la fuerza bruta. Es una primitiva incluida en .NET, sin dependencias externas.
- **Dónde:** `Infrastructure/Security/Pbkdf2PasswordHasher.cs`.

### 19. Matriz de permisos: Employee registra productos pero no los edita ni elimina
- **Decisión:**
  - Employee: consulta catálogos y registra productos.
  - Admin: además, edita y elimina productos y gestiona categorías.
- **Por qué:** registrar mercancía nueva es parte de la operación diaria. Cambiar precios, eliminar registros o reorganizar el catálogo son decisiones administrativas.
- **Dónde:** atributos `[Authorize(Roles = ...)]` en `ProductsController` y `CategoriesController`.

### 20. Ciclos de vida de inyección de dependencias
- **Decisión:**
  - Singleton: los servicios sin estado (hash, validadores, `TimeProvider`).
  - Scoped: `DbContext`, repositorios y servicios de aplicación.
  - Transient: el emisor de tokens.
- **Por qué:** dentro de una petición, todos los repositorios comparten el mismo `DbContext`, que actúa como unidad de trabajo. Ningún Singleton depende de un Scoped, así que no hay dependencias cautivas.
- **Dónde:** `DependencyInjection.cs` de Application e Infrastructure.

### 21. Comportamiento por entorno mediante configuración
- **Decisión:** las opciones `Swagger:Enabled` y `Database:MigrateOnStartup`, más los secretos, se definen en `appsettings.{Entorno}.json` y en variables de entorno, en lugar de condicionar el código con `IsDevelopment()`.
- **Por qué:** el mismo binario se comporta distinto en cada entorno sin recompilar, y cada comportamiento se puede activar de forma independiente (por ejemplo, Swagger en Staging pero no en Production).

### 22. Docker Compose con un archivo base y uno por entorno
- **Decisión:** `docker-compose.yml` define los servicios comunes. `override` (Development), `staging` y `prod` agregan solo lo que cambia.
- **Por qué:**
  - No se repite configuración.
  - Development arranca con un solo comando.
  - Staging y Production exigen sus secretos (`${VAR:?}`) y no exponen la base de datos.
  - Cada entorno usa su propio nombre de proyecto, así que no comparte red ni volumen con los demás.
- **Dónde:** `docker-compose*.yml`, `.env.*.example`, `Dockerfile` (multietapa y usuario sin privilegios).

### 23. OpenAPI nativo de .NET 10 con Swagger UI
- **Decisión:** el documento se genera con `Microsoft.AspNetCore.OpenApi` (OpenAPI 3.1), se documenta con comentarios XML y se visualiza con Swagger UI. El esquema Bearer habilita el botón *Authorize*.
- **Por qué:** es el generador oficial del framework, y los comentarios XML mantienen la documentación junto al código. El contrato se publica en JSON y YAML (`docs/openapi/`).

### 24. Pruebas unitarias con Moq
- **Decisión:** las pruebas de `Core.Application` simulan los repositorios y servicios de infraestructura con Moq (`Setup` para el comportamiento y `Verify` para las interacciones). Reemplaza a los dobles escritos a mano (`Fakes.cs`) de las fases anteriores.
- **Por qué:** la Fase 4 exige xUnit + Moq. Además, `Verify` comprueba lo que un doble en memoria no muestra: que un proceso rechazado no guarda (`SaveChangesAsync` nunca se llama), no consume número de factura ni registra auditoría.
- **Criterio:** se verifica el estado del dominio (stock, saldos) y solo las interacciones que forman parte de la regla (guardar, numerar, auditar), no cada llamada interna.

### 25. Una transacción explícita por proceso de negocio
- **Decisión:** cada proceso que toca varias tablas (facturar, anular, cargar camión, compra, avería, consignación…) se ejecuta con `IUnitOfWork.ExecuteInTransactionAsync`. Ese método abre `BEGIN … COMMIT` dentro de la estrategia de reintentos de Npgsql.
- **Por qué:** garantiza atomicidad. Una factura descuenta el inventario, carga el saldo, crea los movimientos y consume el número, o no hace nada. Sin transacción, una falla a mitad de camino deja el inventario descontado y la factura sin crear.
- **Descartado:** varios `SaveChanges` con reversión manual. Es frágil y no protege ante una caída del proceso.
- **Dónde:** `ApplicationDbContext.ExecuteInTransactionAsync`, servicios de aplicación.

### 26. Concurrencia optimista con `xmin`
- **Decisión:** `Product`, `Truck`, `Client`, `Invoice` y `ConsignmentEvent` usan la columna de sistema `xmin` de PostgreSQL como token de concurrencia (`IsRowVersion`).
- **Por qué:** dos ventas simultáneas de las últimas unidades leerían el mismo stock y ambas descontarían. Con el token, la segunda escritura falla con 409 y el inventario nunca queda negativo ni se vende dos veces. `xmin` lo mantiene PostgreSQL, sin agregar columnas.
- **Descartado:** bloqueo pesimista (`SELECT … FOR UPDATE`) en cada lectura. Reduce la concurrencia y alarga las transacciones.
- **Verificado:** `ConcurrentSales_OfTheLastUnits_NeverOversell`.

### 27. Numeración atómica en la base de datos, con el año en el número
- **Decisión:**
  - Los correlativos se obtienen con un único `INSERT … ON CONFLICT DO UPDATE … RETURNING` sobre `DocumentCounters`, dentro de la transacción del proceso.
  - El número incluye el año: `Factura Octubre 2026 #3`.
- **Por qué:** es atómico (dos usuarios nunca reciben el mismo número) y una factura fallida no consume número. Sin el año, "Factura Octubre #1" chocaría con la del año siguiente.
- **Descartado:** "máximo + 1" calculado en la aplicación, que se duplica con usuarios concurrentes; contadores locales por dispositivo.

### 28. Snapshot del producto en las líneas de documento
- **Decisión:** `InvoiceItem`, `PurchaseItem` y `ConsignmentItem` copian nombre, SKU, precio por caja, precio por unidad y unidades por caja.
- **Por qué:** un documento emitido es un registro histórico y debe mostrar el precio con el que se hizo. Si los precios cambian mañana, las facturas viejas no pueden cambiar. Es una desnormalización deliberada, que no viola la 3FN del catálogo.

### 29. Saldo de vacíos por grupo como dato estructurado
- **Decisión:** la tabla `ClientEmptyBalances (cliente, grupo, cajas, unidades)` se actualiza en cada factura, devolución y anulación.
- **Por qué:** el saldo por grupo se consulta directamente y siempre es consistente con el total. Reconstruirlo leyendo textos de movimientos es lento y se rompe si cambia el formato.

### 30. Autorización para regalía como dato del producto
- **Decisión:** `Product.IsGiftEligible`, editable por el administrador. En la siembra se calculó con la regla del negocio (cervezas retornables, la malta retornable de 222 ml y las Pepsi de 2 L x6).
- **Por qué:** una regla basada en textos del nombre se rompe con cualquier producto nuevo o un cambio de nombre. Como dato, se ajusta sin tocar código.

### 31. Vaciar un camión devuelve la mercancía al galpón
- **Decisión:** "vaciar camión" (solo Admin) reintegra cada línea al stock del galpón.
- **Por qué:** borrar el inventario del camión sin devolverlo a ningún lado haría desaparecer mercancía. Para dar de baja mercancía existen las averías.

### 32. No se elimina un cliente con saldos pendientes
- **Decisión:** el borrado lógico de un cliente se rechaza si su saldo de dinero o de vacíos es distinto de 0.
- **Por qué:** eliminar un cliente que debe ocultaría esa deuda de las cuentas por cobrar y del balance.

### 33. La anulación solo revierte el pendiente
- **Decisión:** al anular, `saldo += pendiente`. Lo cobrado al facturar no se convierte en saldo a favor.
- **Por qué:** es la regla de negocio definida (el dinero recibido se trata aparte). Con una factura de total 100 y abono 40, el saldo pasa de −60 a 0.
- **Consecuencia:** si el negocio decide devolver o acreditar lo cobrado, se hace con un ajuste de saldo o un abono, que quedan auditados.

### 34. Auditoría después del commit, con un `DbContext` propio
- **Decisión:** `AuditService` escribe en `AuditLogs` con un `DbContext` creado por la fábrica, después de confirmar el proceso, y captura sus propios errores.
- **Por qué:** la regla del negocio es que si la auditoría falla, el proceso no falla. Usar el mismo contexto haría que un error de auditoría revirtiera la venta.
- **Modelo:** PK BIGINT identity (crecimiento intensivo), antes y después en **jsonb** (consultable e indexable), IP en **inet** y fecha por defecto `NOW()`.

### 35. Resiliencia con Polly y Circuit Breaker en el servicio externo
- **Decisión:** el `HttpClient` del proveedor de tasas tiene timeout por intento, reintentos con espera exponencial y Circuit Breaker. Si falla, se devuelve el último valor conocido; si no lo hay, 503.
- **Por qué:** un proveedor lento o caído no debe bloquear hilos de la API (falla en cascada). El circuito abierto falla en milisegundos y deja recuperarse al proveedor. Se aplica donde hay una dependencia HTTP saliente; la base de datos tiene su propia estrategia de reintentos.
- **Verificado:** con el proveedor caído, la primera llamada hace 3 intentos y abre el circuito; las siguientes fallan en ~10 ms sin llamar al proveedor.

### 36. Rate limiting y CORS nativos, configurados por entorno
- **Decisión:** se usa el limitador de tasa de ASP.NET Core (ventana fija):
  - Login: por IP; 5, 10 o 30 por minuto según el entorno.
  - Global: por usuario o IP.

  CORS solo permite los orígenes de `Cors:AllowedOrigins`.
- **Por qué:**
  - El límite del login frena los ataques de fuerza bruta contra las claves.
  - El límite global evita abusos.
  - CORS deja que solo el frontend autorizado consuma la API desde un navegador.

  Ambos son componentes del framework, sin dependencias extra.

### 37. Reloj del negocio con zona horaria
- **Decisión:** `IClock` calcula "hoy" y los rangos de los reportes en la zona de la empresa (`America/Caracas`, configurable). Si el sistema no tiene la base de zonas horarias, usa UTC−4 fijo.
- **Por qué:** el servidor trabaja en UTC. Sin conversión, una venta de las 9 p. m. caería en el reporte del día siguiente, y la numeración mensual cambiaría de mes cuatro horas antes.

### 38. PDF con QuestPDF
- **Decisión:** ticket de factura (80 mm) y liquidación de consignación con QuestPDF (licencia Community).
- **Por qué:** genera PDF desde código C# tipado, sin navegadores ni plantillas HTML externas. La licencia Community cubre el uso académico.

### 39. Pruebas de integración con PostgreSQL real (Testcontainers)
- **Decisión:** `Integration.Tests` levanta la API real con `WebApplicationFactory` y un PostgreSQL 15 desechable en Docker.
- **Por qué:** las reglas más delicadas (transacciones, concurrencia, numeración atómica, check constraints, filtros de borrado lógico) dependen del motor real. Un proveedor en memoria no las reproduciría. Cada ejecución parte de una base limpia y no toca la de desarrollo.

### 40. Reportes como consultas de solo lectura proyectadas
- **Decisión:** los reportes (`IReportQueries`) calculan sumas y conteos en PostgreSQL y proyectan directamente a DTO con `Select`, sin materializar entidades.
- **Por qué:** es más rápido y usa menos memoria que cargar miles de facturas para sumarlas en C#. Además, los reportes son de solo lectura y no necesitan el comportamiento de las entidades (separación de lecturas y escrituras, al estilo CQRS ligero).

### 41. Dashboard KPI calculado en la capa de aplicación
- **Decisión:** `GET /api/reports/dashboard` (solo Admin) recibe rango de fechas y categoría. `IReportQueries.GetDashboardDataAsync` trae de PostgreSQL filas ya proyectadas (productos activos con su stock en camiones, renglones vendidos y camiones con su carga) y `ReportService` calcula los KPI.
- **Por qué:** la serie diaria agrupa por **día local** (`IClock`), y el estado de cada producto usa el `ProductHealthEvaluator` del dominio: así el dashboard aplica las mismas reglas MinStock/MaxStock que `GET /api/products/{id}/health`. Calcularlo en C# deja la lógica probada con Moq, sin base de datos (`ReportServiceTests`).
- **Definiciones:** valor al costo = unidades × (costo por caja / unidades por caja); valor de venta = unidades × precio por unidad; rotación = unidades vendidas en el rango / existencia actual (galpón + camiones). Las facturas anuladas no cuentan.

### 42. Capacidad del camión en cajas
- **Decisión:** `Truck.CapacityBoxes` (1–5000, 500 por defecto) y la ocupación = cajas equivalentes cargadas (unidades / unidades por caja) / capacidad. Migración `TruckCapacity`.
- **Por qué:** el KPI de ocupación de flota que sugirió el profesor necesita una capacidad. Se mide en cajas porque es la unidad de carga del negocio; el peso o el volumen no existen en el catálogo.
- **Consecuencia:** la ocupación puede pasar de 100 % y se muestra como "Sobrecargado"; no se bloquea la carga porque el negocio no definió esa regla.

### 43. SPA con Context API, JWT en localStorage y mismo origen
- **Decisión:** `AuthContext` guarda el JWT en `localStorage`, lo decodifica con `jwt-decode` y cierra la sesión al vencer o ante un 401. `ThemeContext` guarda el tema. La SPA siempre llama a `/api` relativo: Vite (desarrollo) y Nginx (Docker) reenvían a la API.
- **Por qué:** el curso pide Context API y persistencia en `localStorage`. El mismo origen evita depender de CORS y de URLs de la API compiladas en el bundle.
- **Riesgo aceptado:** un token en `localStorage` es legible por un script inyectado (XSS). Se mitiga con React (escapa el contenido), sin HTML sin sanitizar y con tokens de vida corta; una cookie `HttpOnly` requeriría protección CSRF y cambiar la API.

### 44. Tailwind CSS 3 con tokens semánticos
- **Decisión:** Tailwind 3.4 con `darkMode: 'class'` y el color `unet.blue #003366`, como en el material del curso. Los colores de la interfaz son variables CSS (`--brand`, `--surface`, `--status-*`) que cambian con la clase `dark`.
- **Por qué:** los componentes no repiten pares `bg-x dark:bg-y`; un solo token cambia ambos temas. Tailwind 4 cambia la configuración (sin `tailwind.config.js`) y se aleja de la guía del curso.

