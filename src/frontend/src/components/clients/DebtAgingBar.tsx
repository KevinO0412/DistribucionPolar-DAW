import { formatMoney } from '../../lib/format';

export interface AgingBuckets {
  days0To30: number;
  days31To60: number;
  days61To90: number;
  over90: number;
}

/** Tramos de antigüedad de la deuda; el color va de neutro a rojo y cada tramo lleva su texto (no solo color). */
export const AGING_BUCKETS = [
  { key: 'days0To30', label: '0-30 días', tone: 'bg-status-ok' },
  { key: 'days31To60', label: '31-60 días', tone: 'bg-status-low' },
  { key: 'days61To90', label: '61-90 días', tone: 'bg-status-critical' },
  { key: 'over90', label: 'Más de 90 días', tone: 'bg-status-out' },
] as const;

export function DebtAgingBar({ aging }: { aging: AgingBuckets }) {
  const total = AGING_BUCKETS.reduce((sum, b) => sum + aging[b.key], 0);
  return (
    <div className="flex flex-col gap-2">
      <div className="flex h-3 w-full overflow-hidden rounded-full bg-sunken" role="img" aria-label="Deuda por antigüedad">
        {total > 0 &&
          AGING_BUCKETS.map((b) =>
            aging[b.key] > 0 ? <div key={b.key} className={b.tone} style={{ width: `${(aging[b.key] / total) * 100}%` }} /> : null,
          )}
      </div>
      <dl className="grid grid-cols-2 gap-x-4 gap-y-1 text-sm sm:grid-cols-4">
        {AGING_BUCKETS.map((b) => (
          <div key={b.key} className="flex flex-col">
            <dt className="flex items-center gap-1.5 text-muted">
              <span className={`h-2 w-2 rounded-full ${b.tone}`} aria-hidden />
              {b.label}
            </dt>
            <dd className="font-semibold tabular-nums">{formatMoney(aging[b.key])}</dd>
          </div>
        ))}
      </dl>
    </div>
  );
}
