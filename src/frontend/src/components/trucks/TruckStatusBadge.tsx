import { cn } from '../../lib/cn';

/** Estado de la ruta del camión, con texto y color (el color nunca es la única señal). */
export const TRUCK_STATUS: Record<string, { tone: string; hint: string }> = {
  'En galpón': { tone: 'bg-sunken text-muted', hint: 'Sin carga' },
  Cargando: { tone: 'bg-status-low/15 text-status-low', hint: 'Preparando la salida' },
  'En ruta': { tone: 'bg-brand/10 text-brand', hint: 'Vendiendo en la calle' },
  Liquidado: { tone: 'bg-status-ok/15 text-status-ok', hint: 'Ruta cerrada y cuadrada' },
};

export function TruckStatusBadge({ status }: { status: string }) {
  const meta = TRUCK_STATUS[status] ?? TRUCK_STATUS['En galpón'];
  return (
    <span className={cn('inline-flex items-center rounded px-2 py-0.5 text-xs font-semibold', meta.tone)} title={meta.hint}>
      {status}
    </span>
  );
}
