#!/usr/bin/env bash
# Importa clientes desde un CSV a la base de DESARROLLO a través de la API (POST /api/clients), con el login de admin.
# El CSV no se copia al repositorio (*.csv está en .gitignore): solo se lee desde la ruta indicada.
#
#   scripts/importar-clientes.sh RUTA.csv [URL_API] [USUARIO] [CLAVE]
#   scripts/importar-clientes.sh ~/Downloads/clientes.csv http://localhost:5000/api admin 'Admin2026!'
#
# Columnas esperadas (encabezado, en cualquier orden): nombre, rif, apodo, direccion. Otras columnas (por ejemplo
# telefono) se ignoran porque el modelo de cliente no las tiene. Separador "," o ";" (se detecta), UTF-8.
# Cada fila que la API rechaza (RIF vacío, RIF repetido, textos demasiado largos…) se reporta por número de fila y
# campo, sin imprimir sus datos, y la importación sigue con la siguiente.
#
# Requiere curl, jq y python3.
set -euo pipefail

CSV="${1:?Indique la ruta del CSV: scripts/importar-clientes.sh RUTA.csv [URL_API] [USUARIO] [CLAVE]}"
API="${2:-http://localhost:5000/api}"
USERNAME="${3:-admin}"
PASSWORD="${4:-Admin2026!}"

[[ -r "$CSV" ]] || { echo "No se puede leer el archivo: $CSV" >&2; exit 1; }

token=$(curl -fsS -X POST "$API/auth/login" -H 'Content-Type: application/json' \
  -d "$(jq -n --arg u "$USERNAME" --arg p "$PASSWORD" '{username: $u, password: $p}')" | jq -r .token)
response=$(mktemp)
trap 'rm -f "$response"' EXIT

created=0
rejected=0
total=0

# Python convierte cada fila en un JSON de CreateClientRequest (una por línea) con el número de fila delante.
while IFS=$'\t' read -r row body; do
  total=$((total + 1))
  code=$(curl -sS -o "$response" -w '%{http_code}' -X POST "$API/clients" \
    -H "Authorization: Bearer $token" -H 'Content-Type: application/json' -d "$body")
  if [[ $code == 201 ]]; then
    created=$((created + 1))
  else
    rejected=$((rejected + 1))
    # Solo el motivo general y los campos con error; nunca los valores de la fila.
    reason=$(jq -r 'if .errors then "campos inválidos: " + (.errors | keys | join(", "))
                    elif (.detail // "" | test("Ya existe")) then "RIF o cédula repetido"
                    else (.title // "error") end' "$response" 2>/dev/null || echo "HTTP $code")
    echo "  fila $row rechazada ($code): $reason"
  fi
done < <(python3 -I - "$CSV" <<'PY'
import csv, json, sys

path = sys.argv[1]
with open(path, newline='', encoding='utf-8-sig') as fh:
    sample = fh.read(4096)
    fh.seek(0)
    dialect = csv.Sniffer().sniff(sample, delimiters=',;\t')
    reader = csv.DictReader(fh, dialect=dialect)
    fields = {(name or '').strip().lower(): name for name in reader.fieldnames or []}
    missing = [c for c in ('nombre', 'rif') if c not in fields]
    if missing:
        sys.exit(f"Faltan columnas en el CSV: {', '.join(missing)}")

    def value(row, column):
        key = fields.get(column)
        text = (row.get(key) or '').strip() if key else ''
        return text or None

    for number, row in enumerate(reader, start=2):  # la fila 1 es el encabezado
        body = {
            'rif': (value(row, 'rif') or '').upper(),
            'name': value(row, 'nombre') or '',
            'address': value(row, 'direccion'),
            'nickname': value(row, 'apodo'),
        }
        print(f"{number}\t{json.dumps(body, ensure_ascii=False)}")
PY
)

echo "Listo: $created clientes creados, $rejected filas rechazadas de $total."
