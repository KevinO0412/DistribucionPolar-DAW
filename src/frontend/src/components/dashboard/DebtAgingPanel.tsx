import type { DebtAging } from '../../api/types';
import { cn } from '../../lib/cn';
import { formatInt, formatMoney } from '../../lib/format';
import { EmptyState } from '../ui/States';
import { DataTable } from './DataTable';

/** Antigüedad de la deuda: barras por tramo y clientes con deuda vencida (bloqueados para vender a crédito). */
export function DebtAgingPanel({ aging }: { aging: DebtAging }) {
  if (aging.totalDebt === 0) return <EmptyState title="Ningún cliente debe">No hay dinero en la calle.</EmptyState>;
  const max = Math.max(...aging.buckets.map((b) => b.amount), 1);

  return (
    <div className="flex flex-col gap-5">
      <dl className="flex flex-wrap gap-x-8 gap-y-2 text-sm">
        <div>
          <dt className="text-muted">Dinero en la calle</dt>
          <dd className="font-display text-xl font-semibold">{formatMoney(aging.totalDebt)}</dd>
        </div>
        <div>
          <dt className="text-muted">Vencido (más de {aging.maxOverdueDays} días)</dt>
          <dd className={cn('font-display text-xl font-semibold', aging.overdueDebt > 0 && 'text-status-out')}>{formatMoney(aging.overdueDebt)}</dd>
        </div>
        <div>
          <dt className="text-muted">Clientes que deben</dt>
          <dd className="font-display text-xl font-semibold">{formatInt(aging.debtors)}</dd>
        </div>
      </dl>

      <ul className="flex flex-col gap-2" aria-label="Deuda por tramo de antigüedad">
        {aging.buckets.map((bucket) => {
          const overdue = bucket.fromDays > aging.maxOverdueDays;
          return (
            <li key={bucket.label} className="grid grid-cols-[7rem_minmax(0,1fr)_auto] items-center gap-3 text-sm">
              <span className="text-muted">{bucket.label}</span>
              <span className="h-3 overflow-hidden rounded-full bg-sunken">
                <span className={cn('block h-full rounded-full', overdue ? 'bg-status-out' : 'bg-brand')} style={{ width: `${(bucket.amount / max) * 100}%` }} />
              </span>
              <span className="text-right tabular-nums">
                <span className="font-semibold">{formatMoney(bucket.amount)}</span> <span className="text-muted">({bucket.clients})</span>
              </span>
            </li>
          );
        })}
      </ul>

      {aging.topDebtors.length > 0 && (
        <DataTable
          caption="Clientes con deuda vencida"
          rows={aging.topDebtors}
          rowKey={(c) => c.clientId}
          columns={[
            { header: 'Cliente con deuda vencida', cell: (c) => <><p className="font-medium">{c.code} {c.name}</p><p className="text-muted">{c.oldestDays >= 999 ? 'Saldo anterior sin factura' : `La más antigua tiene ${c.oldestDays} días`}</p></> },
            { header: 'Vencido', align: 'right', cell: (c) => <span className="text-status-out">{formatMoney(c.overdueDebt)}</span> },
            { header: 'Deuda total', align: 'right', cell: (c) => formatMoney(c.debt) },
          ]}
        />
      )}
    </div>
  );
}
