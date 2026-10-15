# Observaciones del profesor

Fuente: "Recomendaciones del profesor, entregadas el 15/10/2026". Es la lista oficial que se incorpora para la defensa.
Cada observación se revisó contra `main` antes de tocar código; las que faltaban se implementaron en este orden de prioridad:
6 + 7, 2, 10, 4, 5 y 1, con un commit por observación.

## Estado

| # | Observación | Antes | Estado | Dónde está | Cómo se verifica |
| --- | --- | --- | --- | --- | --- |
| 1 | Liquidación ciega de ruta | No existía | Implementada (versión acotada, ver abajo) | `TruckSettlement` (dominio), `TruckSettlementService`, `POST /api/trucks/{id}/settlements`, página **Camiones → Liquidar ruta** | `TruckSettlementTests`, `TruckSettlementServiceTests`, `BlindSettlement_ComparesTheCount_WithTheRoute_AndClosesIt`, `SettlementPage.test.tsx` |
| 2 | Control de crédito y cuentas por cobrar | No existía | Implementada | `Client.CreditLimit` / `CreditDays`, `CreditPolicy` y `DebtAging` (dominio), `GET`/`PUT /api/clients/{id}/credit`, página **Clientes** | `DebtAgingTests`, `InvoiceServiceTests` (crédito), `ClientServiceTests`, `CreditLimit_BlocksCreditSales_ButAllowsCash_AndOnlyAdminSetsIt`, `ClientsPage.test.tsx` |
| 3 | Envases retornables (Empty Groups) | Ya existía | Sin cambios | Saldos por grupo (`ClientEmptyBalance`), vacíos generados y devueltos en cada factura, `POST /api/clients/{id}/empties-returns` ([Lógica de negocio §5–6](03-LOGICA-NEGOCIO.md#5-clientes-y-estado-de-cuenta)) | Pruebas existentes de vacíos (unitarias e integración) |
| 4 | Tablero visual de flotas | Tarjeta con capacidad; sin estados ni monto | Implementada | `Truck.Status` (En galpón, Cargando, En ruta, Liquidado), `POST /api/trucks/{id}/dispatch`, ventas de la ruta en `GET /api/trucks`, página **Camiones** | `TruckStatusTests`, `TruckServiceTests`, `Truck_GoesLoadingThenOnRoute_AndShowsTheTripSales`, `TrucksPage.test.tsx` |
| 5 | Impresión térmica 80 y 58 mm | Solo ticket de 80 mm | Implementada | `lib/print.ts` (`printThermal`), selector de ancho en ticket, recibo de cobro (**Clientes → Cobrar**) y planilla de cuadre | `print.test.ts`, `Ticket.test.tsx`, `ClientsPage.test.tsx`; PDF de impresión de 58 mm generado en Chromium |
| 6 | Preventa offline-first (PWA / IndexedDB) | No existía | Implementada | Página **Preventa**, `src/offline/` (IndexedDB y sincronización), `public/sw.js`, `manifest.webmanifest` | `sync.test.ts`, `presale.test.ts`, `PresalePage.test.tsx`; en Chromium: recarga sin conexión, pedido en cola y envío al reconectar |
| 7 | Sincronización idempotente (Idempotency-Key) | No existía | Implementada | Encabezado `Idempotency-Key` en `POST /api/invoices/sales`, tabla `IdempotencyKeys` | `InvoiceServiceTests` (idempotencia), `OfflineOrderResent_WithSameIdempotencyKey_IsInvoicedOnlyOnce` |
| 8 | Respaldos y balances en JSON/CSV | Ya existía | Sin cambios | `GET /api/reports/backup/json`, `GET /api/reports/backup/csv/{dataset}`, `GET /api/reports/balance` (Admin) | Swagger |
| 9 | Errores RFC 7807 interceptados por Axios | Ya existía | Se aprovecha en la preventa | `src/api/client.ts` normaliza el Problem Details; la preventa muestra el detalle de un faltante de stock en el camión | `client.test.ts`, `sync.test.ts` ("un faltante de stock (400 RFC 7807) rechaza ese pedido con el detalle") |
| 10 | KPIs: ocupación, rotación y envejecimiento de deuda | Ocupación y rotación existían | Implementada (envejecimiento) | `receivables` en `GET /api/reports/dashboard`, sección **Envejecimiento de la deuda** del dashboard | `ReportServiceTests`, `DebtAgingBar.test.tsx`, `Dashboard_IncludesTheDebtAgingOfTheClients` |

## Rúbrica REAF-F4 (evaluación del 9 de octubre de 2026)

La Fase 4 figura como "No Presentó" en esa evaluación; esta tabla relaciona cada subcriterio con su evidencia para la recuperación.

| Criterio | Subcriterio | Pts | Evidencia en el proyecto | Cómo se verifica |
| --- | --- | ---: | --- | --- |
| 1. Frontend SPA y componentización | Estructura React + Vite, enrutamiento y Tailwind | 10 | React 18.3 + Vite 6 + TypeScript, React Router 6 (rutas protegidas por rol, carga diferida del dashboard), Tailwind 3.4 con tema Azul UNET y modo oscuro | `npm run build`; navegación sin recargas; `ProtectedRoute.test.tsx` |
| | Modularidad y gestión de estado de componentes | 10 | Átomos en `components/ui`, módulos por dominio (`dashboard`, `trucks`, `clients`, `layout`), hooks propios (`useApiQuery`, `useAuth`, `useOnline`, `useDebounced`), estado local con `useState` y filtros del dashboard en la URL | `src/frontend/README.md` (estructura); 54 pruebas de Vitest |
| 2. Consumo de APIs y estado | Peticiones HTTP asíncronas y errores | 10 | Axios con interceptores (`src/api/client.ts`): Bearer en cada petición y normalización de RFC 7807 (`title`, `detail`, errores por campo); cancelación con `AbortController`; cola idempotente de la preventa | `client.test.ts`, `sync.test.ts`; errores 400 junto a cada campo del formulario |
| | Autenticación con JWT y estado local/global | 10 | `AuthContext` (estado global): login, JWT decodificado con `jwt-decode`, persistido en `localStorage`, cierre al vencer o ante un 401; `ThemeContext` global; IndexedDB para la preventa sin conexión | `AuthContext.test.tsx`, `ThemeContext.test.tsx`, `PresalePage.test.tsx` |
| 3. Dashboard KPI y métricas | Tarjetas de métricas y KPI clave | 10 | Ventas del período, rotación, margen bruto, productos bajo el mínimo, ocupación de la flota y ventas por camión, cuentas por cobrar, vencido y clientes solo contado | `ReportServiceTests`; `Dashboard_ReflectsTruckSaleOccupancyAndRotation_AndIsAdminOnly` |
| | Gráficos interactivos y filtros dinámicos | 10 | Recharts con tooltips; al pulsar una barra de categoría se filtra todo el dashboard; filtros de período, fechas y categoría guardados en la URL (`useSearchParams`); barra de envejecimiento de la deuda | Dashboard en el navegador: `?from=&to=&categoryId=` |
| 4. Pruebas unitarias (xUnit y Moq) | Lógica de negocio en xUnit | 10 | 96 pruebas de dominio (cajas y unidades, precios, SKU, salud del stock, saldos y vacíos, facturas, camiones y sus estados, crédito, envejecimiento, liquidación) | `dotnet test tests/UnitTests` |
| | Aislamiento de capas y mocks con Moq | 10 | 108 pruebas de aplicación con todas las dependencias simuladas con Moq (`Setup`/`Verify`), entre ellas `ProductServiceTests` con mock de `IProductRepository`; ninguna toca PostgreSQL ni EF Core | Reporte de cobertura: 87,1 % de líneas del núcleo |

## Cómo funciona cada una

### 6 y 7. Preventa sin conexión con ventas idempotentes

- El pedido se guarda **primero** en IndexedDB (`src/offline/store.ts`) con un id UUID y después se envía.
- El envío usa ese id como `Idempotency-Key`. La API guarda la clave y el hash SHA-256 del cuerpo en `IdempotencyKeys`, en la misma transacción que la factura.
- Si la respuesta se pierde y el teléfono reenvía el pedido, la API devuelve la misma factura (200 y `Idempotency-Replayed: true`) sin descontar stock otra vez. Si la clave llega con otro cuerpo, responde 422.
- Sin señal, el pedido queda **Pendiente de envío**. Al volver la conexión (evento `online`) la cola se envía sola, en orden.
- Un rechazo de la API (por ejemplo, faltante de stock en el camión o crédito bloqueado) deja el pedido **Rechazado** con el detalle RFC 7807. Los errores 401, 409, 429 y 5xx se reintentan.
- El service worker guarda la aplicación para abrirla sin conexión, y el catálogo, los clientes y los camiones se guardan en IndexedDB.
- Los endpoints de la API son servicios Scoped de .NET 10 (una instancia por petición); la idempotencia no depende de memoria del servidor sino de la base de datos.

### 2. Control de crédito

- Cada cliente tiene un **límite de crédito** en USD (vacío = sin límite; 0 = solo contado) y **días de crédito** (30 por defecto). Solo el Admin los cambia.
- La deuda se envejece con criterio **FIFO**: los abonos pagan primero las facturas más viejas, así que la deuda actual corresponde a los pendientes de las facturas más recientes. Si la deuda supera esos pendientes (un ajuste manual), el resto cuenta como la deuda más antigua.
- Se bloquean las **ventas a crédito** (las que dejan saldo pendiente) si el cliente tiene deuda con más días que sus días de crédito o si la venta supera su límite. **Las ventas de contado se permiten.** El control corre antes de numerar la factura: un rechazo no consume número.
- Los montos están en USD (divisas), la moneda de todo el sistema; la tasa en bolívares ya está disponible en `/api/exchange-rates`.

### 10. Envejecimiento de la deuda

- El dashboard muestra la deuda total, lo vencido según los días de crédito de cada cliente, los clientes que solo pueden comprar de contado y la deuda en tramos de **0-30, 31-60, 61-90 y más de 90 días**, con los 10 clientes que más deben.
- Es una foto al día de hoy: no depende del rango de fechas ni de la categoría del dashboard.
- Todos los clientes compran al mayor (bodegas, abastos, licorerías); el tipo de cliente se muestra en la tabla.
- Para mostrar todos los tramos en la demostración: `scripts/demo-operacion.sh` y después `scripts/demo-fechas.sh 150`. Con la deuda envejecida, los clientes con 30 días de crédito quedan **solo contado**, que es justamente lo que pide la observación 2.

### 4. Estados del camión

| Estado | Cuándo |
| --- | --- |
| En galpón | Sin carga (estado inicial y después de vaciarlo) |
| Cargando | Al cargarlo desde el galpón |
| En ruta | Al pulsar **Despachar a ruta** (`POST /api/trucks/{id}/dispatch`); se guarda la hora de salida |
| Liquidado | Al hacer la liquidación ciega |

La tarjeta muestra el estado, la barra de capacidad, lo facturado y lo cobrado en la ruta (desde la salida; si no salió, lo de hoy). Una base existente se migra dejando **Cargando** a los camiones que ya tenían carga.

### 5. Impresión térmica

- El ticket de factura, el recibo de cobro y la planilla de cuadre se imprimen en **80 mm o 58 mm** con un selector.
- `printThermal` copia el ticket fuera del diálogo y fija `@page` con el ancho del papel y el alto real del ticket. Antes el ticket de 80 mm salía recortado dentro del diálogo y en hoja carta, porque Chrome ignora `size: 80mm auto`.
- **Recibo de cobro**: desde **Clientes → Cobrar** se registra un abono y queda el recibo con el monto, la observación, el saldo anterior y el saldo actual.

### 1. Liquidación ciega

- El formulario (`GET /api/trucks/{id}/settlement-form`) solo trae los productos que llevó el camión, **sin cantidades ni cobros esperados**.
- El chofer o vendedor declara el efectivo y la mercancía que devuelve. Al confirmar, el sistema compara:
  - el efectivo con lo cobrado en las facturas del camión desde su salida;
  - la mercancía con el inventario registrado en el camión (lo no contado cuenta como 0 y lo que el camión no tenía aparece como sobrante).
- Guarda la **planilla de cuadre** con las diferencias en efectivo, en valor de mercancía y por producto, y deja el camión **Liquidado**. La planilla se imprime para firmarla.
- Mientras el camión está en ruta, el Employee no ve en **Camiones** lo cobrado ni el detalle de la carga.

**Versión acotada:**
- La liquidación registra las diferencias pero **no mueve inventario**: la descarga al galpón sigue siendo la operación de vaciado del Admin.
- La API sigue entregando la carga del camión a cualquier usuario autenticado, porque la preventa la usa para saber qué productos lleva cada camión. El conteo es ciego en la interfaz, no en la API.
- La tarjeta del camión sigue mostrando la ocupación total en cajas (observación 4).

## Endpoints nuevos

| Método | Ruta | Acceso |
| --- | --- | --- |
| GET | `/api/clients/{id}/credit` | Admin y Employee |
| PUT | `/api/clients/{id}/credit` | Admin |
| POST | `/api/trucks/{id}/dispatch` | Admin y Employee |
| GET | `/api/trucks/{id}/settlement-form` | Admin y Employee |
| POST | `/api/trucks/{id}/settlements` | Admin y Employee |
| GET | `/api/trucks/{id}/settlements` | Admin y Employee |
| GET | `/api/trucks/settlements/{settlementId}` | Admin y Employee |

`POST /api/invoices/sales` acepta además el encabezado opcional `Idempotency-Key`. El contrato completo está en [docs/openapi](openapi/openapi.yaml).

## Migraciones nuevas

| Migración | Cambio |
| --- | --- |
| `SaleIdempotencyKeys` | Tabla `IdempotencyKeys` (clave, hash del cuerpo, factura) |
| `ClientCreditTerms` | `Clients.CreditLimit` y `Clients.CreditDays` con check constraint |
| `TruckRouteStatus` | `Trucks.DispatchedAt` y check constraint de los estados |
| `TruckSettlements` | Tablas `TruckSettlements` y `TruckSettlementLines` |

Se aplican solas al arrancar en Development y Staging (`MigrateOnStartup`).

## Pruebas

| Suite | Antes | Ahora |
| --- | ---: | ---: |
| Unitarias de dominio | 80 | 96 |
| Unitarias de aplicación (Moq) | 88 | 108 |
| Integración (PostgreSQL real) | 16 | 21 |
| Frontend (Vitest) | 31 | 54 |
