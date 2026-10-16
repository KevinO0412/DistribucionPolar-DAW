import { useMemo, useState } from 'react';
import { clientsApi } from '../../api/endpoints';
import type { Client, ClientMovementType } from '../../api/types';
import { useApiQuery } from '../../hooks/useApiQuery';
import { cn } from '../../lib/cn';
import { formatDate, formatDateTime, formatMoney } from '../../lib/format';
import { debtAging, moneyEffect, MOVEMENT_LABELS, statementRows } from '../../lib/statement';
import { DataTable } from '../dashboard/DataTable';
import { Button } from '../ui/Button';
import { SelectField, TextField } from '../ui/Field';
import { Modal } from '../ui/Modal';
import { EmptyState, ErrorState, LoadingState } from '../ui/States';

const MOVEMENT_TONES: Record<ClientMovementType, string> = {
  Invoice: 'text-status-out bg-status-out/10',
  Gift: 'text-status-full bg-status-full/10',
  Payment: 'text-status-ok bg-status-ok/10',
  EmptiesGenerated: 'text-status-low bg-status-low/10',
  EmptiesReturned: 'text-status-ok bg-status-ok/10',
  InvoiceCancelled: 'text-status-excess bg-status-excess/10',
  BalanceAdjustment: 'text-muted bg-sunken',
};

/** Tipo de movimiento con texto, no solo color (mismo formato que StatusBadge). */
export function MovementBadge({ type }: { type: ClientMovementType }) {
  return (
    <span className={cn('inline-flex whitespace-nowrap rounded px-2 py-0.5 text-sm font-medium', MOVEMENT_TONES[type])}>
      {MOVEMENT_LABELS[type]}
    </span>
  );
}

const balanceTone = (value: number) => (value < 0 ? 'text-status-out' : value > 0 ? 'text-status-ok' : 'text-muted');

/** Saldo con signo como lo lee el negocio: "Debe $60,00", "A favor $20,00" o "Sin saldo". */
export const balanceText = (value: number) =>
  value < 0 ? `Debe ${formatMoney(-value)}` : value > 0 ? `A favor ${formatMoney(value)}` : 'Sin saldo';

/** Fecha local (YYYY-MM-DD) de un instante UTC, para comparar con los filtros. */
const localDay = (iso: string) => {
  const d = new Date(iso);
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};

type TypeFilter = '' | 'Invoice' | 'Gift' | 'Payment' | 'InvoiceCancelled' | 'Empties' | 'BalanceAdjustment';

/**
 * Historial (estado de cuenta) del cliente: datos, saldo, límite de crédito, vacíos por grupo, antigüedad de la deuda
 * y movimientos con su saldo resultante. Usa GET /api/clients/{id}/statement (Admin y Employee).
 */
export function ClientStatementDialog({ client, onClose }: { client: Client; onClose: () => void }) {
  const statement = useApiQuery((signal) => clientsApi.statement(client.id, signal), [client.id]);
  const [from, setFrom] = useState('');
  const [to, setTo] = useState('');
  const [type, setType] = useState<TypeFilter>('');

  const rows = useMemo(() => (statement.data ? statementRows(statement.data) : []), [statement.data]);
  const aging = useMemo(() => (statement.data ? debtAging(statement.data) : null), [statement.data]);
  // La API no filtra por fecha: se filtra aquí, después de calcular el saldo resultante con la historia completa.
  const visible = rows.filter((r) => {
    const day = localDay(r.date);
    const typeOk = !type || (type === 'Empties' ? r.type === 'EmptiesGenerated' || r.type === 'EmptiesReturned' : r.type === type);
    return typeOk && (!from || day >= from) && (!to || day <= to);
  });

  const current = statement.data?.client ?? client;

  return (
    <Modal open onClose={onClose} size="lg" title={`Historial de ${current.name}`} description={`${current.code} · ${current.rif}`}
      footer={<Button variant="secondary" onClick={onClose}>Cerrar</Button>}>
      {statement.error ? (
        <ErrorState error={statement.error} onRetry={statement.reload} />
      ) : !statement.data || !aging ? (
        <LoadingState label="Cargando el estado de cuenta…" />
      ) : (
        <div className="flex flex-col gap-5">
          <dl className="grid grid-cols-1 gap-x-6 gap-y-3 text-sm sm:grid-cols-2" aria-label="Datos del cliente">
            <div>
              <dt className="text-muted">Dirección</dt>
              <dd>{current.address}{current.nickname ? ` (${current.nickname})` : ''}</dd>
            </div>
            <div>
              <dt className="text-muted">Tipo y última compra</dt>
              <dd>{current.type}, {current.lastPurchaseAt ? formatDate(current.lastPurchaseAt) : 'sin compras'}</dd>
            </div>
            <div>
              <dt className="text-muted">Saldo actual</dt>
              <dd className={cn('font-display text-xl font-semibold', balanceTone(current.moneyBalance))}>{balanceText(current.moneyBalance)}</dd>
            </div>
            <div>
              <dt className="text-muted">Límite de crédito</dt>
              <dd className="font-display text-xl font-semibold">
                {current.creditLimit === null ? 'Sin límite' : current.creditLimit === 0 ? 'Solo de contado' : formatMoney(current.creditLimit)}
              </dd>
            </div>
            <div>
              <dt className="text-muted">Vacíos por grupo</dt>
              <dd>
                {current.emptyBalances.length === 0 ? (
                  <span className="text-muted">Sin vacíos pendientes</span>
                ) : (
                  <ul>
                    {current.emptyBalances.map((b) => (
                      <li key={b.groupKey} className={b.boxes < 0 || b.units < 0 ? 'text-status-out' : 'text-status-ok'}>
                        {b.groupName}: {b.boxes < 0 || b.units < 0 ? 'debe' : 'a favor'} {Math.abs(b.boxes)} cj + {Math.abs(b.units)} und
                      </li>
                    ))}
                  </ul>
                )}
              </dd>
            </div>
            <div>
              <dt className="text-muted">Antigüedad de la deuda</dt>
              <dd>
                {aging.oldestDays === null ? (
                  <span className="text-muted">No debe</span>
                ) : (
                  <>
                    <p className={cn(aging.oldestDays > 30 && 'font-semibold text-status-out')}>
                      {aging.oldestDays >= 999 ? 'Incluye saldo anterior sin factura' : `La más antigua tiene ${aging.oldestDays} días`}
                    </p>
                    <ul className="text-muted">
                      {aging.buckets.filter((b) => b.amount > 0).map((b) => (
                        <li key={b.label}>{b.label}: {formatMoney(b.amount)}</li>
                      ))}
                    </ul>
                  </>
                )}
              </dd>
            </div>
          </dl>

          <div className="flex flex-wrap items-end gap-3 border-t border-line pt-4">
            <SelectField label="Tipo" value={type} onChange={(e) => setType(e.target.value as TypeFilter)} wrapperClassName="min-w-[11rem] flex-1">
              <option value="">Todos los movimientos</option>
              <option value="Invoice">Facturas</option>
              <option value="Gift">Regalías</option>
              <option value="Payment">Abonos</option>
              <option value="InvoiceCancelled">Anulaciones</option>
              <option value="Empties">Vacíos</option>
              <option value="BalanceAdjustment">Ajustes</option>
            </SelectField>
            <TextField label="Desde" type="date" value={from} max={to || undefined} onChange={(e) => setFrom(e.target.value)} />
            <TextField label="Hasta" type="date" value={to} min={from || undefined} onChange={(e) => setTo(e.target.value)} />
          </div>

          {visible.length === 0 ? (
            <EmptyState title={rows.length === 0 ? 'Sin movimientos' : 'Ningún movimiento coincide'}>
              {rows.length === 0 ? 'Este cliente todavía no tiene facturas, abonos ni vacíos.' : 'Cambia el tipo o el rango de fechas.'}
            </EmptyState>
          ) : (
            <DataTable
              caption={`Movimientos de ${current.name}, del más reciente al más antiguo`}
              rows={visible}
              rowKey={(r) => r.id}
              columns={[
                { header: 'Fecha', cell: (r) => <span className="whitespace-nowrap">{formatDateTime(r.date)}</span> },
                { header: 'Tipo', cell: (r) => <MovementBadge type={r.type} /> },
                { header: 'Detalle', cell: (r) => <><p className="font-medium">{r.title}</p><p className="max-w-[18rem] break-words text-muted">{r.description}</p></> },
                {
                  header: 'Monto',
                  align: 'right',
                  cell: (r) => {
                    const effect = moneyEffect(r);
                    if (r.type === 'EmptiesGenerated' || r.type === 'EmptiesReturned') return <span className="text-muted">—</span>;
                    return <span className={balanceTone(effect)}>{effect > 0 ? '+' : effect < 0 ? '−' : ''}{formatMoney(Math.abs(effect))}</span>;
                  },
                },
                { header: 'Saldo', align: 'right', cell: (r) => <span className={cn('whitespace-nowrap', balanceTone(r.balanceAfter))}>{balanceText(r.balanceAfter)}</span> },
              ]}
            />
          )}
        </div>
      )}
    </Modal>
  );
}
