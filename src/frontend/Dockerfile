# syntax=docker/dockerfile:1
# SPA React. Construir desde la raíz del repositorio:
#   docker build -f src/frontend/Dockerfile -t distribucion-polar-spa .

# ---------- Etapa 1: compilación con Node.js (se descarta) ----------
FROM node:20-alpine AS build
WORKDIR /app
COPY src/frontend/package.json src/frontend/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY src/frontend/ .
RUN npm run build

# ---------- Etapa 2: archivos estáticos servidos por Nginx ----------
FROM nginx:1.27-alpine AS final
COPY src/frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=3 CMD wget -qO- http://127.0.0.1/healthz > /dev/null || exit 1
CMD ["nginx", "-g", "daemon off;"]
