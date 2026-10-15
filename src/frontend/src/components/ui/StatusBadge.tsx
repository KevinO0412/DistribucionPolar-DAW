import type { StockHealthStatus } from '../../api/types';
import { cn } from '../../lib/cn';

export const STATUS_META: Record<StockHealthStatus, { label: string; tone: string; dot: string }> = {
  OutOfStock: { label: 'Agotado', tone: 'text-status-out bg-status-out/10', dot: 'bg-status-out' },
  CriticalRisk: { label: 'Bajo el mínimo', tone: 'text-status-critical bg-status-critical/10', dot: 'bg-status-critical' },
  Understocked: { label: 'Bajo', tone: 'text-status-low bg-status-low/10', dot: 'bg-status-low' },
  Optimal: { label: 'Óptimo', tone: 'text-status-ok bg-status-ok/10', dot: 'bg-status-ok' },
  Overstocked: { label: 'En el tope', tone: 'text-status-full bg-status-full/10', dot: 'bg-status-full' },
  Excessive: { label: 'Sobre el máximo', tone: 'text-status-excess bg-status-excess/10', dot: 'bg-status-excess' },
};

/** Estado del stock con texto, no solo color. */
export function StatusBadge({ status }: { status: StockHealthStatus }) {
  const meta = STATUS_META[status];
  return (
    <span className={cn('inline-flex items-center gap-1.5 whitespace-nowrap rounded px-2 py-0.5 text-sm font-medium', meta.tone)}>
      <span className={cn('h-2 w-2 rounded-full', meta.dot)} aria-hidden />
      {meta.label}
    </span>
  );
}

/** Estado del stock de galpón según MinStock/MaxStock (mismas reglas que ProductHealthEvaluator del dominio). */
export function stockStatus(stock: number, min: number, max: number): StockHealthStatus {
  if (stock <= 0) return 'OutOfStock';
  if (stock <= min) return 'CriticalRisk';
  const middle = min + Math.trunc((max - min) / 2);
  if (stock <= middle) return 'Understocked';
  if (stock < max) return 'Optimal';
  if (stock === max) return 'Overstocked';
  return 'Excessive';
}
