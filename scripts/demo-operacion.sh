#!/usr/bin/env bash
# Genera una jornada de operación real a través de la API (no inserta datos en la base directamente):
# carga camiones desde el galpón y emite facturas de venta desde los camiones y desde el galpón.
# Así el dashboard KPI muestra ventas, rotación y ocupación de flota con datos de verdad.
#
#   scripts/demo-operacion.sh [URL_API] [USUARIO] [CLAVE]
#   scripts/demo-operacion.sh http://localhost:5000/api admin 'Admin2026!'
#
# Requiere curl y jq. Ejecutarlo varias veces suma más operaciones (cada factura toma su número correlativo).
set -euo pipefail

API="${1:-http://localhost:5000/api}"
USERNAME="${2:-admin}"
PASSWORD="${3:-Admin2026!}"
RANDOM=2026

token=$(curl -fsS -X POST "$API/auth/login" -H 'Content-Type: application/json' \
  -d "$(jq -n --arg u "$USERNAME" --arg p "$PASSWORD" '{username: $u, password: $p}')" | jq -r .token)
auth=(-H "Authorization: Bearer $token" -H 'Content-Type: application/json')

get() { curl -fsS "${auth[@]}" "$API$1"; }
post() { curl -sS -o /tmp/dp-demo-response.json -w '%{http_code}' -X POST "${auth[@]}" "$API$1" -d "$2"; }

# Productos con al menos 15 cajas en galpón, de todas las categorías.
mapfile -t products < <(get /products | jq -r '[.[] | select(.stockUnits >= .unitsPerBox * 15)] | sort_by(.categoryName, .name) | .[] | "\(.id) \(.unitsPerBox) \(.name)"')
mapfile -t trucks < <(get /trucks | jq -r '.[] | select(.capacityBoxes >= 150) | .id')
mapfile -t clients < <(get /clients | jq -r '.[].id' | head -n 25)

if (( ${#products[@]} < 8 || ${#trucks[@]} == 0 || ${#clients[@]} == 0 )); then
  echo "No hay suficientes productos con stock, camiones o clientes para la demostración." >&2
  exit 1
fi

echo "Cargando ${#trucks[@]} camiones…"
loads=0
declare -A truck_products
for t in "${!trucks[@]}"; do
  truck=${trucks[$t]}
  for k in 0 1 2 3 4; do
    read -r pid _ name <<<"${products[$(( (t * 5 + k * 3) % ${#products[@]} ))]}"
    boxes=$(( 4 + RANDOM % 9 ))
    code=$(post "/trucks/$truck/load" "$(jq -n --arg p "$pid" --argjson b "$boxes" '{productId: $p, mode: "Add", boxes: $b, looseUnits: 0, observation: "Carga de la ruta"}')")
    if [[ $code == 200 ]]; then
      loads=$((loads + 1))
      truck_products[$truck]+="$pid "
    else
      echo "  carga rechazada ($code) de $name: $(jq -r '.detail // .title' /tmp/dp-demo-response.json)"
    fi
  done
done

echo "Facturando…"
sales=0
for n in $(seq 1 18); do
  client=${clients[$(( RANDOM % ${#clients[@]} ))]}
  truck=${trucks[$(( RANDOM % ${#trucks[@]} ))]}
  read -ra loaded <<<"${truck_products[$truck]:-}"
  (( ${#loaded[@]} == 0 )) && continue
  items=$(for pid in "${loaded[@]:0:$(( 1 + RANDOM % 3 ))}"; do
    jq -n --arg p "$pid" --argjson b "$(( 1 + RANDOM % 2 ))" --argjson l "$(( RANDOM % 4 ))" '{productId: $p, boxes: $b, looseUnits: $l}'
  done | jq -s .)
  payment=$(( RANDOM % 3 == 0 ? 0 : 20 + RANDOM % 60 ))
  code=$(post /invoices/sales "$(jq -n --arg c "$client" --arg t "$truck" --argjson items "$items" --argjson pay "$payment" \
    '{clientId: $c, dispatchOrigin: "Truck", truckId: $t, items: $items, payment: $pay, paymentObservation: "Recibió en $: \($pay)"}')")
  [[ $code == 201 ]] && sales=$((sales + 1)) || echo "  venta rechazada ($code): $(jq -r '.detail // .title' /tmp/dp-demo-response.json)"
done

for n in 1 2 3; do
  read -r pid _ _ <<<"${products[$(( RANDOM % ${#products[@]} ))]}"
  code=$(post /invoices/sales "$(jq -n --arg c "${clients[$n]}" --arg p "$pid" '{clientId: $c, dispatchOrigin: "Warehouse", items: [{productId: $p, boxes: 3, looseUnits: 0}], payment: 0}')")
  [[ $code == 201 ]] && sales=$((sales + 1)) || echo "  venta de galpón rechazada ($code): $(jq -r '.detail // .title' /tmp/dp-demo-response.json)"
done

rm -f /tmp/dp-demo-response.json
echo "Listo: $loads cargas de camión y $sales facturas registradas."
