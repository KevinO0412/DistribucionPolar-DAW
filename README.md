# Distribución Polar

API REST para la operación de una distribuidora mayorista de alimentos, bebidas y productos de limpieza.
Está construida con **ASP.NET Core 10** y **PostgreSQL 15** sobre **Arquitectura Cebolla (Onion)**, y es el proyecto
integrador de Desarrollo de Aplicaciones Web (DAW-0423807T, UNET).

| Fase | Tema | Dónde está |
| --- | --- | --- |
| 1 | Onion, dominio, retos C#, middleware RFC 7807, resiliencia, xUnit | `Core.Domain/`, `Presentation.API/Middleware/`, `*.Tests/` |
| 2 | EF Core 10 + PostgreSQL 15, Fluent API, migraciones, siembra, `AsNoTracking`, transacciones, auditoría | `Infrastructure/Persistence/`, `docs/fase2/schema.sql` |
| 3 | JWT, RBAC Admin/Employee, FluentValidation, hash de claves, rate limiting, CORS, Postman | `Infrastructure/Security/`, `Presentation.API/`, `Core.Application/Validators/`, `postman/` |

Sobre esa base, el sistema implementa la operación completa del negocio:
- Inventario en galpón y camiones.
- Clientes con saldos y estado de cuenta.
- Facturación, regalías y anulación con control de envases retornables.
- Compras, averías, consumos internos y consignación.
- Reportes, auditoría, ticket PDF y respaldo.

## Inicio rápido

```bash
docker compose up -d --build
```

| Servicio | URL |
| --- | --- |
| Swagger UI | http://localhost:8080/swagger |
| pgAdmin | http://localhost:5050 (Servers → Distribución Polar) |

| Usuario | Clave | Rol |
| --- | --- | --- |
| `admin` | `Admin2026!` | Admin |
| `empleado` | `Empleado2026!` | Employee |
| `inactivo` | `Inactivo2026!` | Employee desactivado (el login responde 401) |

## Documentación

| Documento | Contenido |
| --- | --- |
| [Guía de incorporación](docs/01-ONBOARDING.md) | Puesta en marcha, recorrido del código, flujo de una petición, cómo agregar una funcionalidad, convenciones |
| [Arquitectura](docs/02-ARQUITECTURA.md) | Capas, transacciones y concurrencia, errores, validación, seguridad, persistencia, entornos, resiliencia, inyección de dependencias, pruebas |
| [Lógica de negocio](docs/03-LOGICA-NEGOCIO.md) | Cada regla del negocio, su razón, el archivo donde vive y ejemplos verificados |
| [Decisiones de arquitectura](docs/04-DECISIONES.md) | 40 decisiones (ADR) con su justificación y la alternativa descartada |
| [Requisitos por fase](docs/05-REQUISITOS-POR-FASE.md) | Requisitos de las Fases 1, 2 y 3, su implementación y cómo verificarlos |
| [Fundamentos](docs/06-FUNDAMENTOS.md) | Conceptos teóricos de cada fase y cómo se aplican en el código |
| [Contrato OpenAPI](docs/openapi/openapi.yaml) | Especificación OpenAPI 3.1 de los 78 endpoints (también en JSON) |
| [Script SQL](docs/fase2/schema.sql) | Esquema completo y datos iniciales (idempotente) |

## Estructura

```text
DistribucionPolar.slnx
├── Core.Domain/             Negocio puro: entidades, value objects, reglas (sin dependencias)
├── Core.Application/        Casos de uso, DTOs, validadores, interfaces (puertos)
├── Infrastructure/          EF Core + PostgreSQL, migraciones, repositorios, JWT, Polly, PDF, auditoría
├── Presentation.API/        Controladores, middleware RFC 7807, autenticación, CORS, rate limiting, Swagger
├── Core.Domain.Tests/       Pruebas unitarias del dominio
├── Core.Application.Tests/  Pruebas unitarias de validadores y servicios
├── Integration.Tests/       API real contra PostgreSQL desechable (Testcontainers)
├── docs/                    Documentación, script SQL y contrato OpenAPI
├── postman/                 Colección de pruebas de la API
└── docker-compose*.yml      Orquestación: base + Development, Staging y Production
```

Regla de dependencia: `Presentation.API → Core.Application, Infrastructure`; `Infrastructure → Core.Application → Core.Domain`.

## Entornos

| Archivo | Entorno | API | Swagger | BD expuesta | Secretos |
| --- | --- | --- | :-: | :-: | --- |
| `docker-compose.yml` | Base común (PostgreSQL + API) | — | — | — | — |
| `docker-compose.override.yml` | **Development** (+ pgAdmin) | http://localhost:8080 | ✔ | ✔ | valores de desarrollo |
| `docker-compose.staging.yml` | **Staging** | http://localhost:8081 | ✔ | ✘ | `.env.staging` (obligatorio) |
| `docker-compose.prod.yml` | **Production** | http://localhost:8082 | ✘ | ✘ | `.env.production` (obligatorio) |

```bash
# Development (carga docker-compose.override.yml automáticamente)
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
- Staging y Production no arrancan si faltan los secretos.
- Detalle de la configuración por entorno: [Arquitectura §9](docs/02-ARQUITECTURA.md#9-entornos-y-despliegue).

### Desarrollo sin Docker para la API

```bash
docker compose up -d postgres
dotnet tool restore
dotnet ef database update --project Infrastructure --startup-project Presentation.API
dotnet run --project Presentation.API          # http://localhost:5249
```

## API

78 endpoints, todos documentados en Swagger. Por módulo:

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
| Reportes, auditoría, respaldo | `/api/reports` | Admin |
| Tasas de cambio | `/api/exchange-rates` | Ambos |
| Fase 1 (demostración) | `/api/demo` | Público |
| Sistema | `/health`, `/api/info` | Público |

Todos los errores responden `application/problem+json` (RFC 7807) con los códigos 400, 401, 403, 404, 409, 429, 500 y 503.

## Pruebas

```bash
dotnet test DistribucionPolar.slnx                                         # 184 pruebas (las de integración requieren Docker)
scripts/cobertura.sh                                                       # reporte HTML de cobertura en docs/evidencias/cobertura
(cd src/frontend && npm test)                                              # 25 pruebas del frontend
npx newman run postman/collections/DistribucionPolar.postman_collection.json  # 50 peticiones, 97 aserciones
```

| Suite | Pruebas | Qué cubre |
| --- | ---: | --- |
| `Core.Domain.Tests` | 80 | Reglas del dominio |
| `Core.Application.Tests` | 88 | Validadores y servicios, con Moq |
| `Integration.Tests` | 16 | Procesos completos contra PostgreSQL real: casos de referencia, concurrencia, permisos, auditoría, CORS y rate limiting |
| `src/frontend` (Vitest) | 25 | Sesión, rutas por rol, tema y errores de la API |
| Postman | 50 / 97 | Escenarios obligatorios de la Fase 3 y operación completa |

Para Staging, la colección acepta `--env-var baseUrl=http://localhost:8081` y las credenciales de `.env.staging`
(`adminUsername`, `adminPassword`, `employeeUsername`, `employeePassword`). La prueba del usuario inactivo solo
aplica en Development.

## Base de datos

- Migraciones (CLI): `dotnet ef database update --project Infrastructure --startup-project Presentation.API`.
- Script idempotente: `dotnet ef migrations script --idempotent --project Infrastructure --startup-project Presentation.API -o docs/fase2/schema.sql`.
- **Datos de demostración:** el catálogo de referencia no trae costo ni niveles de stock. Valores ficticios: costo = 80% del precio por caja; mínimo = 5 cajas; máximo = 40 cajas; stock entre 0 y 44 cajas. Los empleados, camiones y clientes iniciales son ficticios.

## Commits

[Conventional Commits](https://www.conventionalcommits.org/): `<tipo>(<alcance>): <descripción>`, con tipos `feat`, `fix`,
`refactor`, `docs`, `test`, `build` y `chore`. Un commit por cambio coherente, en imperativo y sin secretos ni
archivos de `bin/` u `obj/`.
