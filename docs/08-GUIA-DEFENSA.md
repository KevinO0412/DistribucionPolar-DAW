# Guía de defensa

Guion para la sustentación individual: demostración de 15 minutos de las Fases 4 y 5 y de las observaciones del profesor. Todos los comandos se verificaron el 15 de octubre de 2026 sobre `main`.

## 1. Antes de entrar (preparación, 10 minutos antes)

```bash
cd ~/Desktop/projects/DistribucionPolar
git checkout main && git pull
[ -f .env ] || cp .env.example .env      # la clave JWT llega desde .env; nunca está en el código
docker compose down -v                    # base limpia (borra el volumen de Development)
docker compose up --build -d
docker ps --filter label=com.docker.compose.project=distribucion-polar
scripts/demo-operacion.sh                 # 25 cargas de camión y unas 18 facturas reales a través de la API
scripts/demo-fechas.sh                    # reparte esas facturas en los últimos 14 días (solo Development)
```

- Que db, api y spa digan `(healthy)` en `docker ps`. La api tarda unos 15 s y la spa arranca después.
- Que `demo-operacion.sh` muestre algunas "venta rechazada (400): Inventario insuficiente en el camión…". Es normal: es la regla de stock del camión funcionando.
- Abrir en el navegador: http://localhost:8080 (SPA), http://localhost:5000/swagger (API) y la [última ejecución del CI](https://github.com/KevinO0412/DistribucionPolar-DAW/actions).

| Usuario | Clave | Para mostrar |
| --- | --- | --- |
| `admin` | `Admin2026!` | Dashboard, límite de crédito, respaldos, historial de liquidaciones |
| `empleado` | `Empleado2026!` | Control por rol (sin dashboard ni botones de eliminar), preventa, liquidación |

## 2. Guion de 15 minutos

| Min. | Qué hacer | Qué decir |
| --- | --- | --- |
| 0–1 | Abrir el README | Distribuidora de bebidas y alimentos con galpón y camiones. API .NET 10 con Arquitectura Cebolla, SPA React y todo en Docker. Fases 1 a 5 y las 11 observaciones del 9 de octubre |
| 1–3 | Terminal: `docker ps` | Tres servicios con healthcheck, `depends_on` por salud, volumen persistente, puertos 8080/5000/5432 y dos redes: la SPA no ve la base de datos |
| 3–4 | `docker images` y [evidencias](evidencias/README.md#imágenes-docker) | Imágenes multi-stage: API unos 60 MB (self-contained, recortada, sobre `runtime-deps` Alpine), SPA unos 60 MB (Nginx Alpine). El CI falla si alguna llega a 120 MB |
| 4–7 | SPA como **admin**: dashboard | Tarjetas KPI (ventas, rotación, margen, stock bajo el mínimo). Filtro por fecha y categoría, y al pulsar una barra se filtra todo. Ocupación de flota por camión. Antigüedad de la deuda. Descargar respaldo |
| 7–8 | Recargar la página y cerrar sesión | El JWT persiste al recargar (`AuthContext`, `localStorage`) y se borra al cerrar sesión. Cambiar a modo oscuro (`ThemeContext`) |
| 8–9 | SPA como **empleado** | El menú no tiene Dashboard; `/dashboard` por URL lleva a "sin permiso" (`ProtectedRoute`); Productos sin editar ni eliminar. La API responde 403 igual |
| 9–11 | Preventa **sin conexión** | `docker stop distribucion-polar-spa`, recargar (abre desde el service worker), registrar un pedido: "Sin conexión: 1 pedido pendiente". `docker start distribucion-polar-spa` y pulsar "Enviar ahora" (o esperar 30 s): se factura una sola vez (Idempotency-Key) |
| 11–12 | Camiones: "Salir a ruta" y "Liquidar ruta" | Liquidación ciega: el chofer declara sin ver lo esperado; la planilla muestra las diferencias. Imprimir en 58 mm |
| 12–13 | Clientes: "Abono" y recibo de 58 mm | Saldos con signo, vacíos por grupo, recibo de cobro. Como admin, el límite de crédito (bloquea la venta a crédito) |
| 13–14 | GitHub Actions y [cobertura](evidencias/cobertura/index.html) | CI en push y PR a `main` y `develop`: pruebas unitarias e integración, build de la SPA, artefactos solo si todo pasa, imágenes y prueba de humo con Compose. Cobertura del núcleo alrededor del 86 % |
| 14–15 | Preguntas | Ver §5 |

Si el tiempo se acorta, priorizar: `docker ps`, el dashboard, el control por rol, la preventa sin conexión y el CI.

## 3. Qué mostrar para cada criterio

### Fase 4 (80 pts)

| Criterio | Pts | Mostrar | Dónde está |
| --- | --- | --- | --- |
| 4.1 SPA con React, Vite y Tailwind | 20 | Navegación sin recargas, vista a 375 px sin desplazamiento horizontal | `src/frontend` (React 18.3, Vite 6, Tailwind 3.4 con `unet.blue #003366` y `darkMode: 'class'`) |
| 4.2 Context API y temas | 20 | Recargar sin perder la sesión; modo oscuro persistente | `src/frontend/src/context/AuthContext.tsx`, `ThemeContext.tsx`; captura `03` |
| 4.3 Dashboard KPI | 15 | Costo frente a venta, rotación, MinStock/MaxStock, filtros, flota | `components/dashboard/PowerBIDashboard.tsx` sobre `GET /api/reports/dashboard` |
| 4.4 xUnit y Moq | 15 | `ProductServiceTests` con mock de `IProductRepository` (éxito y excepción) | `tests/UnitTests/Application/ProductServiceTests.cs`; `evidencias/pruebas-backend.txt` |
| 4.5 Integración y RBAC | 10 | Employee sin dashboard ni eliminar; errores RFC 7807 en los formularios | `components/ProtectedRoute.tsx`, `layout/navigation.ts`; capturas `05` y `06` |

### Fase 5 (80 pts)

| Criterio | Pts | Mostrar | Dónde está |
| --- | --- | --- | --- |
| 5.1 Multi-stage, menos de 120 MB | 20 | `docker images`; los Dockerfiles | `src/backend/Presentation.API/Dockerfile`, `src/frontend/Dockerfile`, [ADR 45](04-DECISIONES.md#45-imagen-de-la-api-self-contained-y-recortada-sobre-alpine) |
| 5.2 Compose | 20 | `docker ps` (healthy), redes y volumen | `docker-compose.yml`, `docker-compose.override.yml`; `evidencias/docker-ps.txt` |
| 5.3 CI | 15 | Run verde en GitHub Actions | `.github/workflows/ci.yml`; captura `07` |
| 5.4 Modelo de datos | 15 | Las 4 dimensiones y el diccionario | [07-MODELO-DATOS.md](07-MODELO-DATOS.md); pgAdmin en http://localhost:5050 |
| 5.5 Sustentación | 10 | Este guion | — |

## 4. Observaciones del profesor (9 de octubre)

Detalle con el texto literal y la fuente: [09-OBSERVACIONES-PROFESOR.md](09-OBSERVACIONES-PROFESOR.md).

| # | Observación | Dónde mostrarla |
| --- | --- | --- |
| 1 | No presentó la Fase 4; coordinar la recuperación | Esta defensa individual |
| 2 | Liquidación ciega de ruta | Camiones → Liquidar ruta → planilla (`RouteSettlement.cs`, `TrucksPage.tsx`) |
| 3 | Control estricto de crédito | Clientes → límite (admin); una venta a crédito sobre el límite responde "Crédito bloqueado…" (`Client.EnsureCreditAvailable`, `DebtAging.cs`) |
| 4 | Envases retornables bidireccionales | Clientes → columna Vacíos y devolución por grupo |
| 5 | Tarjetas de flota con estados | Camiones: En galpón, Cargando, En ruta, Liquidado; ocupación y ventas en el dashboard |
| 6 | Impresión térmica de 80 y 58 mm, tickets y recibos | Facturas → Ticket; Clientes → Abono → recibo; selector 80/58 mm |
| 7 | Preventa offline-first (PWA, IndexedDB) | Preventa con el contenedor `spa` apagado (`src/frontend/src/offline/`) |
| 8 | Sincronización idempotente | `Idempotency-Key` en `POST /api/invoices/sales` (`InvoiceService.CreateSaleAsync`) |
| 9 | Respaldos JSON y CSV | Dashboard → Descargar respaldo |
| 10 | RFC 7807 con Axios para faltantes de stock | Pedido de preventa rechazado con "Inventario insuficiente en el camión…" (`api/client.ts`) |
| 11 | KPI de flota, rotación y antigüedad de la deuda | Dashboard → Ocupación de la flota, Rotación por producto, Antigüedad de la deuda |

## 5. Piezas clave, explicadas en una línea

| Pieza | Qué hace | Ruta |
| --- | --- | --- |
| Arquitectura Cebolla | Dominio en el centro; las dependencias apuntan hacia adentro | `src/backend/Core.Domain` → `Core.Application` → `Infrastructure` → `Presentation.API` |
| Entidad rica | `Product` valida sus reglas (precios > 0, máximo > mínimo) y maneja su stock | `src/backend/Core.Domain/Entities/Product.cs` |
| Cajas y unidades | Todo se guarda en unidades; "2 cajas + 5 und" es solo cómo se muestra | `src/backend/Core.Domain/ValueObjects/BoxQuantity.cs` |
| Salud del stock | `switch` sobre tuplas: agotado, bajo el mínimo, óptimo, sobre el máximo | `src/backend/Core.Domain/Services/ProductHealthEvaluator.cs` |
| Facturación | Una transacción: stock, numeración, saldo, vacíos y crédito; idempotente | `src/backend/Core.Application/Services/InvoiceService.cs` |
| Antigüedad de la deuda | Los abonos saldan primero lo más viejo (FIFO) | `src/backend/Core.Domain/Services/DebtAging.cs` |
| Liquidación ciega | Compara lo declarado con lo esperado y arma la planilla | `src/backend/Core.Domain/Entities/RouteSettlement.cs` |
| Dashboard | KPI calculados en Aplicación sobre consultas proyectadas | `src/backend/Core.Application/Services/ReportService.cs` |
| Persistencia | EF Core 10 con Fluent API, `numeric(18,2)`, `Restrict`, índices únicos | `src/backend/Infrastructure/Persistence/Configurations/` |
| Seguridad | JWT HS256 con claims y expiración; claves con PBKDF2-HMAC-SHA256 | `src/backend/Infrastructure/Security/JwtTokenService.cs`, `Pbkdf2PasswordHasher.cs` |
| Errores RFC 7807 | Un middleware convierte las excepciones en `application/problem+json` | `src/backend/Presentation.API/Middleware/ExceptionMiddleware.cs` |
| Sesión en la SPA | Guarda y decodifica el JWT; cierra la sesión al vencer o ante un 401 | `src/frontend/src/context/AuthContext.tsx` |
| Cliente HTTP | Axios con interceptor Bearer y conversión de Problem Details | `src/frontend/src/api/client.ts` |
| Rutas por rol | Sin sesión → login; rol insuficiente → "sin permiso" | `src/frontend/src/components/ProtectedRoute.tsx` |
| Preventa sin conexión | Cola en IndexedDB que se envía al reconectar, con clave de idempotencia | `src/frontend/src/offline/queue.ts`, `OfflineContext.tsx` |
| PWA | Manifest y service worker (precaché y caché del catálogo) | `src/frontend/vite.config.ts` |
| Nginx | Sirve la SPA, fallback a `index.html` y proxy de `/api` | `src/frontend/nginx.conf` |
| Contenedores | Multi-stage; la API sin SDK ni runtime compartido | `src/backend/Presentation.API/Dockerfile`, `src/frontend/Dockerfile` |
| Orquestación | db, api y spa con healthchecks y redes separadas | `docker-compose.yml` |
| CI | Pruebas, cobertura, build, artefactos e imágenes | `.github/workflows/ci.yml` |

## 6. Preguntas probables

1. **¿Por qué Arquitectura Cebolla?** El negocio no depende de EF Core, HTTP ni PostgreSQL; se prueba sin base de datos y se puede cambiar la infraestructura sin tocarlo.
2. **¿Dónde están las interfaces de los repositorios?** En `Core.Application/Abstractions` (los puertos); las implementa `Infrastructure` (ADR 02).
3. **¿Qué es RFC 7807?** Un formato estándar de error (`type`, `title`, `status`, `detail`); el middleware lo aplica a todas las respuestas de error.
4. **¿Por qué Fluent API y no Data Annotations?** Para no acoplar el dominio a EF Core; el mapeo vive en `Infrastructure/Persistence/Configurations`.
5. **¿Para qué `AsNoTracking`?** En las lecturas evita que EF guarde copias para detectar cambios: menos memoria y CPU.
6. **¿Cómo garantizan que una factura no quede a medias?** Cada proceso corre en una transacción (`ExecuteInTransactionAsync`): si algo falla, se revierte todo.
7. **¿Por qué PBKDF2 y no SHA-256?** SHA-256 solo se calcula muy rápido; PBKDF2 con sal y 100.000 iteraciones encarece la fuerza bruta (cumple "SHA-256 o superior").
8. **¿Qué lleva el JWT?** `sub`, `unique_name`, `email`, `name`, `role`, `exp`, `iss`, `aud`, `jti`, firmado con HMAC-SHA256.
9. **¿Dónde está la clave JWT?** En la variable `JWT_KEY` desde `.env` (ignorado por Git); Compose no arranca sin ella. Nunca en el código.
10. **¿Diferencia entre 401 y 403?** 401: no se sabe quién eres (sin token o vencido). 403: se sabe, pero tu rol no puede.
11. **¿El control por rol de la SPA es seguridad?** No, es comodidad de interfaz; la seguridad la aplica la API con `[Authorize(Roles = ...)]`.
12. **¿Por qué el JWT en `localStorage`?** Lo pide el curso y no requiere CSRF; el riesgo es XSS, mitigado porque React escapa el contenido y el token dura poco (ADR 43).
13. **¿Cómo se calcula la rotación?** Unidades vendidas en el rango / existencia actual (galpón + camiones).
14. **¿Cómo se valoriza el inventario?** Al costo: unidades × costo por caja / unidades por caja. A precio de venta: unidades × precio por unidad. La diferencia es el margen bruto.
15. **¿Qué pruebas usan Moq?** Las 100 de `tests/UnitTests/Application`: `Setup` simula los repositorios y `Verify` comprueba, por ejemplo, que una venta rechazada no guarda ni audita.
16. **¿Y las de integración?** 20 pruebas con la API real contra un PostgreSQL desechable (Testcontainers): transacciones, permisos, concurrencia, idempotencia.
17. **¿Qué es un build multi-stage?** Una etapa compila con el SDK y otra ejecuta solo lo publicado; el SDK no llega a la imagen final.
18. **¿Cómo bajaron la API de 120 MB?** La imagen `aspnet` Alpine ya pesa unos 128 MB. Se publica self-contained con recorte parcial y archivo único comprimido sobre `runtime-deps` Alpine (unos 60 MB), y se verificó con la colección de Postman (ADR 45).
19. **¿Qué hacen los healthchecks?** `pg_isready`, `/health` (incluye la conexión a PostgreSQL) y `/healthz`; con `depends_on: service_healthy` la API espera a la base y la SPA a la API.
20. **¿Por qué dos redes?** `backend` (db, api) y `frontend` (api, spa): la SPA no puede llegar a la base.
21. **¿Por qué Nginx reenvía `/api`?** El navegador ve un solo origen, no hace falta CORS y la API no necesita exponerse en producción.
22. **¿Qué hace el CI?** En cada push y PR a `main` y `develop`: compila, pruebas unitarias con cobertura, integración, build de la SPA, artefactos solo si todo pasa, imágenes con control de tamaño y prueba de humo.
23. **¿Cómo funciona la preventa sin conexión?** El service worker abre la app sin red. El catálogo está en caché y en IndexedDB, y los pedidos se guardan en IndexedDB y se envían al volver la conexión.
24. **¿Y si el pedido se envía dos veces?** Lleva su id como `Idempotency-Key`; la API devuelve la factura existente (200) en lugar de crear otra.
25. **¿Qué pasa si al sincronizar falta stock en el camión?** El pedido queda "rechazado" con el mensaje RFC 7807; el vendedor lo reintenta o lo descarta.
26. **¿Qué es la liquidación ciega?** El chofer declara dinero y mercancía sin ver lo esperado; el sistema calcula las diferencias y deja la planilla de cuadre.
27. **¿Cómo se bloquea el crédito?** Si el cliente tiene deuda con más de 30 días (configurable) o la venta supera su límite, se rechaza; de contado sí puede comprar.
28. **¿Cómo saben qué deuda está vencida si hay un saldo único?** Los abonos se imputan a lo más antiguo (FIFO), así que la deuda actual corresponde a las facturas más recientes con pendiente.
29. **¿Por qué los vacíos van por grupo?** Solo ciertos envases son retornables y no son intercambiables (222 ml x36, 330 ml x24, Pepsi 1,25 L x6, 350 ml x24); el saldo se guarda por cliente y grupo.
30. **¿Qué hace `demo-fechas.sh` y no es "dato simulado"?** Las ventas son reales (salen de la API con todas sus reglas); el script solo reparte sus fechas en varios días para el gráfico diario, y únicamente en Development.

## 7. Si algo falla en vivo

| Problema | Qué hacer |
| --- | --- |
| `docker compose` dice que falta `JWT_KEY` o `POSTGRES_PASSWORD` | `cp .env.example .env` y repetir `docker compose up --build -d` |
| La api queda `unhealthy` o se reinicia | `docker logs distribucion-polar-api --tail 50`. Si no conecta a la base: `docker compose restart api` |
| Un contenedor dice que el nombre ya está en uso | `docker compose down` y `docker compose up -d` (sin `-v` para no perder datos) |
| El puerto 8080, 5000 o 5432 está ocupado | `docker ps` para ver qué lo usa y detenerlo, o cerrar la otra aplicación |
| El login responde "Demasiados intentos" (429) | Esperar un minuto (límite de 30 intentos por minuto en Development) |
| El dashboard sale sin ventas | Correr `scripts/demo-operacion.sh` y luego `scripts/demo-fechas.sh`; o elegir el período "Hoy" |
| La preventa no muestra "Sin conexión" | Entrar primero a Preventa con conexión (para que se guarde el catálogo), después `docker stop distribucion-polar-spa` y registrar el pedido |
| El pedido no se envía al reconectar | `docker start distribucion-polar-spa`, esperar `(healthy)` y pulsar "Enviar ahora" en la cabecera |
| La SPA muestra una versión vieja | Recargar con Ctrl+Shift+R (el service worker se actualiza solo en la siguiente carga) |
| No hay internet en la sala | Todo corre en local con Docker. Para el CI, usar la captura `07-ci-verde.png` y las [evidencias](evidencias/README.md) |
| Hay que correr las pruebas sin el SDK de .NET 10 | `docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test tests/UnitTests`, o mostrar `evidencias/pruebas-backend.txt` |
| Se cuelga el navegador | Cerrar la pestaña y abrir http://localhost:8080 de nuevo: la sesión sigue guardada |
