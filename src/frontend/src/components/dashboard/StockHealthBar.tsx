import type { DashboardResponse, StockHealthStatus } from '../../api/types';
import { cn } from '../../lib/cn';
import { formatInt } from '../../lib/format';
import { STATUS_META } from '../ui/StatusBadge';

const ORDER: StockHealthStatus[] = ['OutOfStock', 'CriticalRisk', 'Understocked', 'Optimal', 'Overstocked', 'Excessive'];

/** Distribución de los productos por estado de stock (MinStock / MaxStock), con la leyenda como tabla legible. */
export function StockHealthBar({ alerts }: { alerts: DashboardResponse['stockAlerts'] }) {
  const counts = new Map(alerts.map((a) => [a.status, a.count]));
  const total = alerts.reduce((sum, a) => sum + a.count, 0);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex h-4 w-full overflow-hidden rounded-full bg-sunken" role="img" aria-label="Distribución de productos por estado de stock">
        {ORDER.map((status) => {
          const count = counts.get(status) ?? 0;
          return count > 0 ? (
            <div key={status} className={STATUS_META[status].dot} style={{ width: `${(count / total) * 100}%` }} title={`${STATUS_META[status].label}: ${count}`} />
          ) : null;
        })}
      </div>
      <dl className="grid grid-cols-2 gap-x-4 gap-y-2 sm:grid-cols-3">
        {ORDER.map((status) => (
          <div key={status} className="flex items-center justify-between gap-2 text-sm">
            <dt className="flex items-center gap-2 text-muted">
              <span className={cn('h-2.5 w-2.5 rounded-full', STATUS_META[status].dot)} aria-hidden />
              {STATUS_META[status].label}
            </dt>
            <dd className="font-semibold">{formatInt(counts.get(status) ?? 0)}</dd>
          </div>
        ))}
      </dl>
    </div>
  );
}
