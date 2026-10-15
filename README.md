# Distribución Polar

[![CI Pipeline](https://github.com/KevinO0412/DistribucionPolar-DAW/actions/workflows/ci.yml/badge.svg)](https://github.com/KevinO0412/DistribucionPolar-DAW/actions/workflows/ci.yml)

Sistema para la operación de una distribuidora mayorista de alimentos, bebidas y productos de limpieza: inventario en galpón y camiones, facturación con envases retornables, clientes a crédito y un dashboard de indicadores.

- API REST en **ASP.NET Core 10** sobre **PostgreSQL 15**, con **Arquitectura Cebolla (Onion)**.
- SPA en **React 18 + Vite + Tailwind CSS**.
- Todo contenerizado con **Docker Compose**.

Es el proyecto integrador de Desarrollo de Aplicaciones Web (DAW-0423807T, UNET).

| Fase | Tema | Dónde está |
| --- | --- | --- |
| 1 | Onion, dominio, retos C#, middleware RFC 7807, resiliencia, xUnit | `src/backend/Core.Domain/`, `src/backend/Presentation.API/Middleware/` |
| 2 | EF Core 10 + PostgreSQL 15, Fluent API, migraciones, siembra, `AsNoTracking`, transacciones, auditoría | `src/backend/Infrastructure/Persistence/`, `docs/fase2/schema.sql` |
| 3 | JWT, RBAC Admin/Employee, FluentValidation, hash de claves, rate limiting, CORS, Postman | `src/backend/Infrastructure/Security/`, `src/backend/Core.Application/Validators/`, `postman/` |
| 4 | SPA React + Vite + Tailwind, Context API (sesión JWT y tema), dashboard KPI, pruebas xUnit + Moq | `src/frontend/`, `tests/UnitTests/` |
| 5 | Dockerfiles multi-stage (< 120 MB), Docker Compose, CI con GitHub Actions, modelo de datos | `src/*/Dockerfile`, `docker-compose*.yml`, `.github/workflows/ci.yml`, `docs/07-MODELO-DATOS.md` |

Sobre esa base, el sistema implementa la operación completa del negocio:
- Inventario en galpón y camiones.
- Clientes con saldos y estado de cuenta.
- Facturación, regalías y anulación con control de envases retornables.
- Compras, averías, consumos internos y consignación.
- Reportes, auditoría, ticket PDF y respaldo.

## Inicio rápido

```bash
cp .env.example .env                  # y cambiar JWT_KEY: openssl rand -base64 48
docker compose up --build -d
docker ps                             # db, api y spa en estado (healthy)
scripts/demo-operacion.sh             # opcional: carga camiones y factura a través de la API para poblar el dashboard
```

| Servicio | URL |
| --- | --- |
| SPA (dashboard, catálogo, camiones, facturas) | http://localhost:8080 |
| API REST | http://localhost:5000/api |
| Swagger UI | http://localhost:5000/swagger |
| PostgreSQL | localhost:5432 |
| pgAdmin | http://localhost:5050 (Servers → Distribución Polar) |

| Usuario | Clave | Rol |
| --- | --- | --- |
| `admin` | `Admin2026!` | Admin: todo, incluido el dashboard |
| `empleado` | `Empleado2026!` | Employee: catálogo, camiones y facturas, sin editar ni eliminar |
| `inactivo` | `Inactivo2026!` | Employee desactivado (el login responde 401) |

Estos usuarios solo existen en Development. Los de Staging y Production salen de `.env.staging` y `.env.production`.

## Documentación

| Documento | Contenido |
| --- | --- |
| [Guía de incorporación](docs/01-ONBOARDING.md) | Puesta en marcha, recorrido del código, flujo de una petición, cómo agregar una funcionalidad, convenciones |
| [Arquitectura](docs/02-ARQUITECTURA.md) | Capas, transacciones y concurrencia, errores, validación, seguridad, persistencia, entornos, contenedores, CI, pruebas |
| [Lógica de negocio](docs/03-LOGICA-NEGOCIO.md) | Cada regla del negocio, su razón, el archivo donde vive y ejemplos verificados |
| [Decisiones de arquitectura](docs/04-DECISIONES.md) | 48 decisiones (ADR) con su justificación y la alternativa descartada |
| [Requisitos por fase](docs/05-REQUISITOS-POR-FASE.md) | Requisitos de las Fases 1 a 5, su implementación y cómo verificarlos |
| [Fundamentos](docs/06-FUNDAMENTOS.md) | Conceptos teóricos de cada fase y cómo se aplican en el código |
| [Modelo de datos](docs/07-MODELO-DATOS.md) | Las cuatro dimensiones del dominio y el diccionario de datos de Categories, Products y Users |
| [Evidencias](docs/evidencias/README.md) | Capturas y reportes para la sustentación: pruebas, cobertura, imágenes, `docker ps`, sesión JWT |
| [Contrato OpenAPI](docs/openapi/openapi.yaml) | Especificación OpenAPI 3.1 de los 79 endpoints (también en JSON) |
| [Script SQL](docs/fase2/schema.sql) | Esquema completo y datos iniciales (idempotente) |

## Estructura

```text
DistribucionPolar.slnx
├── .github/workflows/ci.yml       CI: compilación, pruebas, cobertura, artefactos e imágenes Docker
├── src/
│   ├── backend/
│   │   ├── Core.Domain/           Negocio puro: entidades, value objects, reglas (sin dependencias)
│   │   ├── Core.Application/      Casos de uso, DTOs, validadores, interfaces (puertos)
│   │   ├── Infrastructure/        EF Core + PostgreSQL, migraciones, repositorios, JWT, Polly, PDF, auditoría
│   │   └── Presentation.API/      Controladores, middleware RFC 7807, autenticación, CORS, rate limiting, Swagger
│   │       └── Dockerfile         Multi-stage: SDK Alpine → runtime-deps Alpine (~60 MB)
│   └── frontend/                  SPA React + Vite + TypeScript + Tailwind
│       ├── src/                   api, context (Auth, Theme), components, pages
│       ├── nginx.conf             Archivos estáticos, fallback del SPA y proxy de /api
│       └── Dockerfile             Multi-stage: node:20-alpine → nginx:alpine
├── tests/
│   ├── UnitTests/                 xUnit + Moq: Domain/ y Application/ (sin base de datos)
│   └── IntegrationTests/          API real contra PostgreSQL desechable (Testcontainers)
├── docs/                          Documentación, evidencias, script SQL y contrato OpenAPI
├── postman/                       Colección de pruebas de la API
├── scripts/                       Cobertura y datos de demostración
└── docker-compose*.yml            Orquestación: base (db, api, spa) + Development, Staging y Production
```

Regla de dependencia: `Presentation.API → Core.Application, Infrastructure`; `Infrastructure → Core.Application → Core.Domain`.

## Contenedores

| Servicio | Imagen | Puerto | Healthcheck |
| --- | --- | --- | --- |
| `db` | `postgres:15-alpine` + volumen `postgres-data` | 5432 | `pg_isready` |
| `api` | `distribucion-polar-api` (self-contained, recorte parcial, Alpine) | 5000 | `GET /health` (incluye la conexión a PostgreSQL) |
| `spa` | `distribucion-polar-spa` (Nginx Alpine) | 8080 → 80 | `GET /healthz` |

- Orden de arranque: `api` espera a que `db` esté sana, y `spa` espera a la `api`.
- Redes: `backend` une `db` y `api`; `frontend` une `api` y `spa`. La SPA no alcanza la base de datos.
- La SPA llama a `/api` en su mismo origen y Nginx lo reenvía a la API, así que no depende de CORS.
- Tamaños medidos: [evidencias](docs/evidencias/README.md#imágenes-docker).

## Entornos

| Archivo | Entorno | SPA | API | Swagger | BD expuesta | Secretos |
| --- | --- | --- | --- | :-: | :-: | --- |
| `docker-compose.yml` | Base común (db, api, spa) | — | — | — | — | — |
| `docker-compose.override.yml` | **Development** (+ pgAdmin) | http://localhost:8080 | http://localhost:5000 | ✔ | ✔ | `.env` |
| `docker-compose.staging.yml` | **Staging** | http://localhost:8091 | http://localhost:8081 | ✔ | ✘ | `.env.staging` (obligatorio) |
| `docker-compose.prod.yml` | **Production** | http://localhost:8090 | http://localhost:8082 | ✘ | ✘ | `.env.production` (obligatorio) |

```bash
# Development (carga docker-compose.override.yml automáticamente)
cp .env.example .env
docker compose up -d --build

# Staging
cp .env.staging.example .env.staging          # y reemplazar los secretos
docker compose -p distribucion-polar-staging --env-file .env.staging \
  -f docker-compose.yml -f docker-compose.staging.yml up -d --build

# Production
cp .env.production.example .env.production    # y reemplazar los secretos (openssl rand -base64 48)
docker compose -p distribucion-polar-prod --env-file .env.production \
  -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

- Cada entorno tiene su propia red y su propio volumen de datos, así que los tres pueden correr a la vez.
- Ningún entorno arranca sin `JWT_KEY` ni `POSTGRES_PASSWORD`.
- La clave JWT nunca está escrita en el código ni en `appsettings`.
- Detalle de la configuración por entorno: [Arquitectura §9](docs/02-ARQUITECTURA.md#9-entornos-contenedores-y-ci).

### Desarrollo sin Docker

```bash
docker compose up -d db
dotnet tool restore
dotnet user-secrets set "JwtSettings:Key" "$(openssl rand -base64 48)" --project src/backend/Presentation.API
dotnet ef database update --project src/backend/Infrastructure --startup-project src/backend/Presentation.API
dotnet run --project src/backend/Presentation.API          # http://localhost:5000

cd src/frontend && npm ci && npm run dev                   # http://localhost:5173 (reenvía /api al puerto 5000)
```

## API

79 endpoints, todos documentados en Swagger. Por módulo:

| Módulo | Ruta base | Acceso |
| --- | --- | --- |
| Autenticación | `/api/auth` (login, me, change-password) | Público / autenticado |
| Catálogo | `/api/categories`, `/api/products` | Consulta: ambos roles; registrar producto: ambos; editar y eliminar: Admin |
| Inventario y camiones | `/api/inventory`, `/api/trucks` | Consulta, carga y transferencia: ambos; ajustes, vaciado y alta de camiones: Admin |
| Clientes | `/api/clients` | Ambos; eliminar y ajustar saldos: Admin |
| Facturación | `/api/invoices` (sales, gifts, cancel, ticket) | Emitir y consultar: ambos; anular: Admin |
| Consignación | `/api/consignments` | Ambos |
| Compras, averías, consumos | `/api/purchases`, `/api/damages`, `/api/internal-consumptions` | Admin |
| Empleados y usuarios | `/api/employees`, `/api/users` | Consulta de empleados: ambos; gestión: Admin |
| Reportes, dashboard, auditoría, respaldo | `/api/reports` (`dashboard?from=&to=&categoryId=`) | Admin |
| Tasas de cambio | `/api/exchange-rates` | Ambos |
| Fase 1 (demostración) | `/api/demo` | Público |
| Sistema | `/health`, `/api/info` | Público |

Todos los errores responden `application/problem+json` (RFC 7807) con los códigos 400, 401, 403, 404, 409, 429, 500 y 503.

## Pruebas

```bash
dotnet test DistribucionPolar.slnx                                            # 184 pruebas (las de integración requieren Docker)
scripts/cobertura.sh                                                          # reporte HTML en docs/evidencias/cobertura
(cd src/frontend && npm test)                                                 # 31 pruebas del frontend
npx newman run postman/collections/DistribucionPolar.postman_collection.json  # 50 peticiones, 97 aserciones
```

| Suite | Pruebas | Qué cubre |
| --- | ---: | --- |
| `tests/UnitTests/Domain` | 80 | Reglas del dominio |
| `tests/UnitTests/Application` | 88 | Validadores y servicios con Moq: `ProductServiceTests` (mock de `IProductRepository`), facturación, camiones, clientes, dashboard y operaciones |
| `tests/IntegrationTests` | 16 | Procesos completos contra PostgreSQL real: casos de referencia, dashboard, concurrencia, permisos, auditoría, CORS y rate limiting |
| `src/frontend` (Vitest) | 31 | Sesión JWT, rutas por rol, tema, errores de la API y validaciones del formulario |
| Postman | 50 / 97 | Escenarios obligatorios de la Fase 3 y operación completa |

Detalle de las pruebas y de la cobertura: [tests/README.md](tests/README.md).

Para Staging, la colección acepta `--env-var baseUrl=http://localhost:8081` y las credenciales de `.env.staging`
(`adminUsername`, `adminPassword`, `employeeUsername`, `employeePassword`). La prueba del usuario inactivo solo
aplica en Development.

## Base de datos

- Migraciones (CLI): `dotnet ef database update --project src/backend/Infrastructure --startup-project src/backend/Presentation.API`.
- Script idempotente: `dotnet ef migrations script --idempotent --project src/backend/Infrastructure --startup-project src/backend/Presentation.API -o docs/fase2/schema.sql`.
- Diccionario de datos y dimensiones del dominio: [docs/07-MODELO-DATOS.md](docs/07-MODELO-DATOS.md).
- **Datos de demostración:** el catálogo de referencia no trae costo ni niveles de stock, así que esos valores son ficticios:
  - costo = 80 % del precio por caja;
  - mínimo = 5 cajas y máximo = 40 cajas;
  - stock entre 0 y 44 cajas.

  Los empleados, camiones y clientes iniciales también son ficticios. Las ventas del dashboard salen de `scripts/demo-operacion.sh`, que opera a través de la API.

## Commits

[Conventional Commits](https://www.conventionalcommits.org/): `<tipo>(<alcance>): <descripción>`, con tipos `feat`, `fix`,
`refactor`, `docs`, `test`, `build`, `ci` y `chore`. Un commit por cambio coherente, en imperativo y sin secretos ni
archivos de `bin/` u `obj/`.
