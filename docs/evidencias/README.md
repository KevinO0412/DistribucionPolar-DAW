# Evidencias de la sustentación

| Evidencia | Archivo |
| --- | --- |
| Contenedores en estado healthy, redes y volumen | [docker-ps.txt](docker-ps.txt) |
| Imágenes Docker de menos de 120 MB | [docker-images.txt](docker-images.txt) y [07-ci-verde.png](capturas/07-ci-verde.png) |
| Reporte del Test Runner de xUnit (unitarias y de integración) | [pruebas-backend.txt](pruebas-backend.txt) |
| Reporte de cobertura (xUnit + Moq, coverlet + ReportGenerator) | [cobertura/index.html](cobertura/index.html) |
| Pruebas del frontend (Vitest) | [pruebas-frontend.txt](pruebas-frontend.txt) |
| Colección de Postman (50 peticiones, 97 aserciones) | [postman-newman.txt](postman-newman.txt) |

## Capturas

| Captura | Qué muestra |
| --- | --- |
| [01-login.jpg](capturas/01-login.jpg) | Pantalla de inicio de sesión de la SPA (http://localhost:8080) |
| [02-dashboard-admin.jpg](capturas/02-dashboard-admin.jpg) | Dashboard KPI como Admin: filtros por fecha y categoría y tarjetas de ventas, rotación, margen y stock crítico |
| [03-sesion-tras-recargar.jpg](capturas/03-sesion-tras-recargar.jpg) | La sesión JWT sigue activa después de recargar la página (token guardado en `localStorage`) |
| [04-ocupacion-flota.jpg](capturas/04-ocupacion-flota.jpg) | Ocupación de la flota: tarjetas por camión con su carga y sus ventas |
| [05-employee-sin-dashboard.jpg](capturas/05-employee-sin-dashboard.jpg) | Catálogo como Employee: el menú no tiene Dashboard y la tabla no tiene botones de editar ni de eliminar |
| [06-employee-sin-permiso.jpg](capturas/06-employee-sin-permiso.jpg) | El Employee abre `/dashboard` por URL y `ProtectedRoute` lo lleva a "sin permiso" |
| [07-ci-verde.png](capturas/07-ci-verde.png) | Último run de GitHub Actions en `main`: backend, frontend, artefactos e imágenes en verde |
| [08-preventa-sin-conexion.jpg](capturas/08-preventa-sin-conexion.jpg) | Preventa con el servidor apagado: la app abre desde el service worker y el pedido queda guardado ("Sin conexión: 1 pedido pendiente") |
| [09-liquidacion-ciega.jpg](capturas/09-liquidacion-ciega.jpg) | Liquidación ciega: el chofer declara dinero y mercancía sin ver lo esperado |
| [10-planilla-cuadre.jpg](capturas/10-planilla-cuadre.jpg) | Planilla de cuadre con lo esperado, lo declarado y las diferencias |
| [11-recibo-58mm.jpg](capturas/11-recibo-58mm.jpg) | Recibo de cobro de un abono en papel térmico de 58 mm |

## Imágenes Docker

Según el CI (`docker image inspect`, suma de capas sin comprimir), la API pesa 58 MB y la SPA 54 MB. La medición local de [docker-images.txt](docker-images.txt) puede variar unos megabytes por la versión de la imagen base (`nginx:alpine`, `runtime-deps:10.0-alpine`) descargada en cada máquina. Con el almacén containerd de Docker, la columna SIZE de `docker images` suma además el contenido comprimido, por eso muestra más.
