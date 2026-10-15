# Evidencias de la sustentación

| Evidencia | Archivo |
| --- | --- |
| Contenedores en estado healthy, redes y volumen | [docker-ps.txt](docker-ps.txt) |
| Imágenes Docker de menos de 120 MB | [docker-images.txt](docker-images.txt) |
| Reporte de cobertura (xUnit + Moq) | [cobertura/index.html](cobertura/index.html) |
| Pruebas del frontend (Vitest) | [pruebas-frontend.txt](pruebas-frontend.txt) |
| Colección de Postman (50 peticiones, 97 aserciones) | [postman-newman.txt](postman-newman.txt) |
| Capturas de la SPA (login, dashboard, RBAC, sesión JWT) | [capturas/](capturas/) |

## Imágenes Docker
Ver [docker-images.txt](docker-images.txt): API ≈ 51 MB y SPA ≈ 57 MB sin comprimir. Con el almacén containerd de Docker, la columna SIZE de `docker images` suma además el contenido comprimido, por eso muestra más.
