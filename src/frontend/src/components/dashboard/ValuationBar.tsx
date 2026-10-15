import type { DashboardResponse } from '../../api/types';
import { formatMoney, formatPercent } from '../../lib/format';

/** Costo frente a precio de venta del inventario: la diferencia es el margen bruto proyectado. */
export function ValuationBar({ inventory }: { inventory: DashboardResponse['inventory'] }) {
  const costShare = inventory.saleValue > 0 ? Math.min(100, (inventory.costValue / inventory.saleValue) * 100) : 0;

  return (
    <div className="flex flex-col gap-4">
      <div className="relative h-10 w-full overflow-hidden rounded-md bg-status-ok/20" role="img" aria-label={`Costo ${formatMoney(inventory.costValue)} de un valor de venta de ${formatMoney(inventory.saleValue)}`}>
        <div className="absolute inset-y-0 left-0 bg-brand" style={{ width: `${costShare}%` }} />
      </div>
      <dl className="flex flex-wrap gap-x-8 gap-y-3 text-sm">
        <div>
          <dt className="flex items-center gap-2 text-muted">
            <span className="h-2.5 w-2.5 rounded-sm bg-brand" aria-hidden />
            Valor al costo
          </dt>
          <dd className="font-display text-xl font-semibold">{formatMoney(inventory.costValue)}</dd>
        </div>
        <div>
          <dt className="flex items-center gap-2 text-muted">
            <span className="h-2.5 w-2.5 rounded-sm bg-status-ok/40" aria-hidden />
            Valor a precio de venta
          </dt>
          <dd className="font-display text-xl font-semibold">{formatMoney(inventory.saleValue)}</dd>
        </div>
        <div>
          <dt className="text-muted">Margen bruto proyectado</dt>
          <dd className="font-display text-xl font-semibold text-status-ok">
            {formatMoney(inventory.grossMargin)} <span className="text-sm font-medium">({formatPercent(inventory.grossMarginPercent)})</span>
          </dd>
        </div>
      </dl>
    </div>
  );
}
