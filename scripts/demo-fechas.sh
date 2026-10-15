#!/usr/bin/env bash
# Reparte en los últimos días las facturas de demostración, que la API emite siempre con la fecha de hoy.
# Así el dashboard muestra ventas por día. Solo para la base de Development: modifica la base directamente.
#
#   scripts/demo-operacion.sh        # primero: genera las ventas a través de la API
#   scripts/demo-fechas.sh [DÍAS]    # luego: las reparte entre hoy y los DÍAS-1 anteriores (por defecto 14, máximo 180)
#   scripts/demo-fechas.sh 150       # deuda con más de 90 días: llena todos los tramos del envejecimiento
#
# Las facturas se ordenan por fecha y número: las más antiguas quedan en los días más lejanos y las últimas hoy,
# entre las 8:00 y las 18:00 de Caracas. Los movimientos de saldo de cada factura se mueven con ella.
# Volver a ejecutarlo da el mismo resultado; si después se emiten más ventas, se reparten todas de nuevo.
set -euo pipefail

DAYS="${1:-14}"
CONTAINER="${DB_CONTAINER:-distribucion-polar-db}"
cd "$(dirname "$0")/.."
[[ -f .env ]] && set -a && . ./.env && set +a
DB_USER="${POSTGRES_USER:-polar}"
DB_NAME="${POSTGRES_DB:-distribucion_polar}"

if ! [[ "$DAYS" =~ ^[0-9]+$ ]] || (( DAYS < 1 || DAYS > 180 )); then
  echo "DÍAS debe ser un número entre 1 y 180." >&2
  exit 1
fi

docker exec -i "$CONTAINER" psql -U "$DB_USER" -d "$DB_NAME" -v ON_ERROR_STOP=1 -q -v days="$DAYS" <<'SQL'
BEGIN;

CREATE TEMP TABLE demo_dates ON COMMIT DROP AS
WITH ordered AS (
    SELECT "Id", row_number() OVER (ORDER BY "IssuedAt", "Number") - 1 AS k, count(*) OVER () AS n
    FROM "Invoices"
)
SELECT "Id",
       LEAST(
           (((now() AT TIME ZONE 'America/Caracas')::date - ((n - 1 - k) * :days / n)::int)
               + time '08:00' + ((abs(hashtext("Id"::text)) % 600) * interval '1 minute'))
               AT TIME ZONE 'America/Caracas',
           now()) AS issued
FROM ordered;

UPDATE "Invoices" i
SET "IssuedAt" = d.issued,
    "CreatedAt" = d.issued,
    "CancelledAt" = CASE WHEN i."CancelledAt" IS NULL THEN NULL ELSE d.issued + interval '1 hour' END
FROM demo_dates d
WHERE i."Id" = d."Id";

UPDATE "ClientMovements" m
SET "CreatedAt" = d.issued
FROM demo_dates d
WHERE m."InvoiceId" = d."Id";

COMMIT;

SELECT to_char("IssuedAt" AT TIME ZONE 'America/Caracas', 'YYYY-MM-DD') AS "Día",
       count(*) AS "Facturas",
       sum("Total") AS "Total"
FROM "Invoices"
WHERE NOT "IsCancelled"
GROUP BY 1
ORDER BY 1;
SQL
