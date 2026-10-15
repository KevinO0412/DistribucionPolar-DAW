# syntax=docker/dockerfile:1
# API REST .NET 10. Construir desde la raíz del repositorio:
#   docker build -f src/backend/Presentation.API/Dockerfile -t distribucion-polar-api .

# ---------- Etapa 1: compilación (SDK completo, se descarta) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

# Restaurar solo con los .csproj aprovecha la caché de capas mientras no cambien las dependencias.
COPY src/backend/Core.Domain/Core.Domain.csproj Core.Domain/
COPY src/backend/Core.Application/Core.Application.csproj Core.Application/
COPY src/backend/Infrastructure/Infrastructure.csproj Infrastructure/
COPY src/backend/Presentation.API/Presentation.API.csproj Presentation.API/
RUN dotnet restore Presentation.API/Presentation.API.csproj -r linux-musl-x64 -p:PublishTrimmed=true

COPY src/backend/ .

# Publicación para la imagen de menos de 120 MB:
# - linux-musl-x64: solo las librerías nativas de Alpine (QuestPDF/Skia trae las de todas las plataformas).
# - Self-contained con recorte parcial: el runtime viaja con la app sin las partes del framework que no usa;
#   los ensamblados que usan reflexión se conservan completos (TrimmerRootAssembly en Presentation.API.csproj).
# - Un solo archivo comprimido con las librerías administradas; las nativas quedan al lado.
# - EnableConfigurationBindingGenerator=false: las opciones (JwtSettings…) usan propiedades init y se enlazan
#   por reflexión en todos los proyectos.
# - Sin soporte de depurador, EventSource ni recarga en caliente.
RUN dotnet publish Presentation.API/Presentation.API.csproj -c Release -r linux-musl-x64 --self-contained true \
      -p:PublishTrimmed=true -p:TrimMode=partial -p:SuppressTrimAnalysisWarnings=true \
      -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true \
      -p:EnableConfigurationBindingGenerator=false \
      -p:DebuggerSupport=false -p:EventSourceSupport=false -p:MetadataUpdaterSupport=false -p:DebugType=None \
      --no-restore -o /app/publish

# Soporte de depuración del runtime y recolectores de basura alternativos: no se usan en producción.
RUN rm -f /app/publish/libmscordaccore.so /app/publish/libmscordbi.so /app/publish/createdump \
          /app/publish/libclrgc.so /app/publish/libclrgcexp.so

# ---------- Etapa 2: ejecución (solo dependencias nativas de Alpine, sin SDK ni runtime compartido) ----------
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-alpine AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5000 \
    DOTNET_GCHeapHardLimitPercent=75
EXPOSE 5000

# Usuario sin privilegios de la imagen oficial. El healthcheck usa wget de busybox (Alpine no trae curl).
USER $APP_UID
HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 CMD wget -qO- http://127.0.0.1:5000/health > /dev/null || exit 1
ENTRYPOINT ["./Presentation.API"]
