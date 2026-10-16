# Observaciones del profesor

Condición de la defensa individual: incorporar todas las observaciones que el docente hizo al Grupo 1 (Distribución Polar).

## Fuentes revisadas

Se revisó todo `DAW-Material` (fases 1 a 5, guías, tareas, rúbricas y evaluaciones). Solo hay dos documentos dirigidos al Grupo 1, ambos de la sesión del **9 de octubre de 2026 (Fase 4)**:

| Fuente | Contenido |
| --- | --- |
| `fases/fase4/Evaluacion_Grupo1_09_Octubre.txt` | Evaluación oficial del Grupo 1: "No Presentó", observaciones del docente y recomendaciones de estrategia, frontend e integración |
| `fases/fase4/Instrucciones y Evaluación 09 Octubre .txt` | Instrumento REAF-F4: la columna G1 sin puntaje (—) y en "Observaciones y Retroalimentación": "Grupo 1 (G1): No Presentó" |

No hay actas ni observaciones de las Fases 1, 2, 3 o 5 para el Grupo 1. `fases/LOGICA_DE_NEGOCIO.md` también menciona a Polar, pero es la especificación del negocio, no una evaluación. Sus "problemas detectados" (§9) ya están resueltos en el diseño ([Decisiones](04-DECISIONES.md)).

## Observaciones y su estado

Texto literal de `Evaluacion_Grupo1_09_Octubre.txt` (número de línea entre paréntesis).

| # | Observación (texto del docente) | Fecha y fuente | Estado | Dónde está implementada |
| --- | --- | --- | --- | --- |
| 1 | "El equipo no realizó la presentación técnica pautada para la sesión del 9 de octubre. Debe coordinar con el docente la recuperación o sustentación extraordinaria antes del cierre de la Fase 5." (l. 17–18) | 09/10/2026, Evaluación G1 e instrumento REAF-F4 (l. 129–130) | ✅ Coordinada | Defensa individual aceptada por el docente. La Fase 4 está completa: [Requisitos por fase](05-REQUISITOS-POR-FASE.md#fase-4-frontend-spa-dashboard-kpi-y-pruebas-unitarias) |
| 2 | "**Liquidación Ciega de Ruta (Blind Settlement)**. Al finalizar el día, el chofer/vendedor ingresa en el sistema el conteo físico de dinero recaudado y mercancía devuelta sin ver el valor esperado. El sistema calcula las discrepancias y genera la planilla de cuadre." (l. 25–26) | 09/10/2026, Evaluación G1 | ✅ Implementada | `RouteSettlement` (dominio), `TruckService.SettleAsync`, `POST /api/trucks/{id}/settlement`, `GET /api/trucks/settlements` (Admin). SPA: "Liquidar ruta" en Camiones (no muestra lo esperado) y planilla imprimible (`SettlementSheet.tsx`). Capturas `09` y `10`. [ADR 49](04-DECISIONES.md#49-liquidación-ciega-de-ruta) |
| 3 | "**Control Estricto de Crédito y Cuentas por Cobrar**. Bloquear automáticamente la emisión de nuevas facturas a clientes con facturas vencidas de más de N días o que excedan su límite de crédito en divisas/bolívares." (l. 28–29) | 09/10/2026, Evaluación G1 | ✅ Implementada | `Client.EnsureCreditAvailable`, `DebtAging` (dominio); `InvoiceService.EnsureCreditAsync`; `Credit:MaxOverdueDays` (30 por defecto); `PUT /api/clients/{id}/credit-limit` (Admin). SPA: límite en Clientes. El límite se lleva en USD (moneda de trabajo); el cobro en Bs o COP se anota en la observación de pago. [ADR 50](04-DECISIONES.md#50-control-de-crédito-con-deuda-vencida-y-límite) |
| 4 | "**Gestión de Envases Retornables (Empty Groups)**. Monitoreo bidireccional del inventario de cajas de refrescos/cervezas y gaveras entregadas en calidad de depósito vs. devueltas por el cliente." (l. 31–32) | 09/10/2026, Evaluación G1 | ✅ Implementada (la vista en la SPA es nueva) | Backend desde la Fase 3: vacíos generados y devueltos por grupo en cada factura, saldo por cliente y grupo (`ClientEmptyBalances`), devolución fuera de factura, vacíos en la calle en el balance. Nuevo: pantalla Clientes con el saldo de vacíos por grupo y el registro de devoluciones (`ClientsPage.tsx`) |
| 5 | "**Tablero Visual de Flotas (Truck Cards)**. Panel en React 18 con tarjetas de estado por camión (Cargando, En Ruta, Liquidado), barra de progreso de capacidad volumétrica/peso y monto total facturado." (l. 37–38) | 09/10/2026, Evaluación G1 | ✅ Implementada | Tarjetas, ocupación y monto facturado desde la Fase 4 (`TruckCard.tsx`, [ADR 42](04-DECISIONES.md#42-capacidad-del-camión-en-cajas)). Nuevo: estados En galpón → Cargando → En ruta → Liquidado (`Truck.MarkLoading`, `Dispatch`, `Settle`) con su etiqueta en cada tarjeta. La capacidad se mide en cajas: el catálogo no tiene peso ni volumen |
| 6 | "**Módulo de Impresión Térmica (80mm / 58mm)**. Generación de tickets de entrega y recibos de cobro directamente desde el navegador con CSS print optimizado para impresoras POS portátiles." (l. 40–41) | 09/10/2026, Evaluación G1 | ✅ Implementada | `ThermalPaper.tsx` (80 o 58 mm, `@page` dinámico). Ticket de entrega en Facturas, **recibo de cobro** al registrar un abono en Clientes (`PaymentReceipt.tsx`) y planilla de cuadre. Captura `11` |
| 7 | "**Preventa Offline-First (PWA / IndexedDB)**. Almacenamiento local de catálogo y pedidos para que el vendedor registre órdenes en zonas sin cobertura y las sincronice al reconectarse." (l. 43–44) | 09/10/2026, Evaluación G1 | ✅ Implementada | `vite-plugin-pwa` (manifest y service worker con precaché y caché del catálogo), `src/frontend/src/offline/` (IndexedDB, cola, sincronización), pantalla Preventa e indicador "Sin conexión / pedidos pendientes". Verificado en el navegador con el servidor apagado. Captura `08`. [ADR 51](04-DECISIONES.md#51-preventa-offline-first-con-pwa-e-indexeddb) |
| 8 | "Sincronización de pedidos mediante endpoints Scoped en .NET 10 con idempotencia (evitar facturas duplicadas)." (l. 50) | 09/10/2026, Evaluación G1 | ✅ Implementada | Encabezado `Idempotency-Key` en `POST /api/invoices/sales` (201 la primera vez, 200 con la misma factura al reintentar), índice único `IX_Invoices_IdempotencyKey`, servicios Scoped. La preventa usa el id del pedido como clave. [ADR 52](04-DECISIONES.md#52-ventas-idempotentes) |
| 9 | "Descarga de respaldos y balances contables en JSON/CSV ya implementados en el backend." (l. 51) | 09/10/2026, Evaluación G1 | ✅ Implementada | Backend desde la Fase 3 (`BackupExporter`). Nuevo: botón "Descargar respaldo" en el dashboard (JSON completo y CSV de productos, clientes, facturas y movimientos) |
| 10 | "Manejo de errores RFC 7807 interceptados por Axios para notificar faltantes de stock en camión." (l. 52) | 09/10/2026, Evaluación G1 | ✅ Implementada | Interceptor de Axios desde la Fase 4 (`api/client.ts`). Nuevo: la preventa muestra el `detail` RFC 7807 en el pedido rechazado (por ejemplo "Inventario insuficiente en el camión…") y avisa con una notificación |
| 11 | "KPIs de utilización de flota (% ocupación por camión), rotación de productos estrella y envejecimiento de deuda de clientes mayoristas." (l. 54) | 09/10/2026, Evaluación G1 | ✅ Implementada | Ocupación y rotación desde la Fase 4. Nuevo: sección "Antigüedad de la deuda" del dashboard (tramos de 0–7, 8–15, 16–30 y más de 30 días, y clientes con deuda vencida) |

## Lo que no se modeló y por qué

- **Capacidad por peso o volumen** (obs. 5): el catálogo no tiene peso ni volumen de los productos, así que la ocupación se mide en cajas, que es la unidad de carga del negocio.
- **Límite de crédito en bolívares** (obs. 3): la moneda de trabajo es el dólar (lógica de negocio §1). El límite se fija en USD; los pagos en Bs o COP se registran en la observación de pago con su tasa.
- **Liquidación totalmente ciega en todas las pantallas** (obs. 2): el formulario de liquidación no muestra lo esperado, pero la pantalla Camiones sigue mostrando la carga a cualquier usuario, como en las fases anteriores. Para ocultarla al chofer haría falta un rol propio, que el negocio no tiene (solo Admin y Employee).

## Verificación

| Prueba | Resultado |
| --- | --- |
| Unitarias (`tests/UnitTests`) | Dominio: antigüedad de la deuda, crédito, estados de ruta y planilla. Aplicación: venta idempotente, bloqueo por deuda vencida y por límite, contado permitido, despacho, liquidación, límite de crédito y KPI de antigüedad |
| Integración (`tests/IntegrationTests/ProfessorObservationTests.cs`) | Mismo pedido enviado dos veces → 201 y luego 200 con la misma factura, stock descontado una sola vez; límite de crédito (Employee 403, venta a crédito bloqueada, contado permitido); liquidación ciega de punta a punta; antigüedad de la deuda en el dashboard |
| Frontend (Vitest) | Cola en IndexedDB (orden, idempotencia, sin conexión, rechazos RFC 7807, reintento y descarte), caché del catálogo sin red, indicador de conexión, recibo de 58 mm y planilla de cuadre |
| Navegador (pila de Docker) | Con el contenedor `spa` apagado, la app abrió desde el service worker, el pedido quedó en IndexedDB con "Sin conexión: 1 pedido pendiente"; al levantar el contenedor se envió solo y la API registró **una** factura con esa clave (`Factura Octubre 2026 #21`). También se probaron salir a ruta, liquidar, la planilla y el recibo de 58 mm |
