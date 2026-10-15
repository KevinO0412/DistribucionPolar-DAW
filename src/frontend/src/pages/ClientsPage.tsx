import { useState } from 'react';
import { CreditCard, Search } from 'lucide-react';
import { clientsApi } from '../api/endpoints';
import { toApiError, type ApiError } from '../api/client';
import type { Client } from '../api/types';
import { AGING_BUCKETS, DebtAgingBar } from '../components/clients/DebtAgingBar';
import { DataTable } from '../components/dashboard/DataTable';
import { PageHeader } from '../components/layout/PageHeader';
import { Button } from '../components/ui/Button';
import { Modal } from '../components/ui/Modal';
import { TextField } from '../components/ui/Field';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { useAuth } from '../hooks/useAuth';
import { useDebounced } from '../hooks/useDebounced';
import { cn } from '../lib/cn';
import { formatInt, formatMoney } from '../lib/format';

export const creditLabel = (client: Pick<Client, 'creditLimit' | 'creditDays'>) =>
  `${client.creditLimit === null ? 'Sin límite' : `Límite ${formatMoney(client.creditLimit)}`} · ${client.creditDays} días`;

/** Situación de crédito del cliente; el Admin además fija el límite y los días. */
function CreditDialog({ client, onClose, onSaved }: { client: Client; onClose: () => void; onSaved: () => void }) {
  const { isAdmin } = useAuth();
  const notify = useToast();
  const credit = useApiQuery((signal) => clientsApi.credit(client.id, signal), [client.id]);
  const [limit, setLimit] = useState(client.creditLimit === null ? '' : String(client.creditLimit));
  const [days, setDays] = useState(String(client.creditDays));
  const [error, setError] = useState<ApiError | null>(null);
  const [saving, setSaving] = useState(false);

  async function save() {
    setSaving(true);
    setError(null);
    try {
      await clientsApi.setCredit(client.id, { creditLimit: limit.trim() === '' ? null : Number(limit), creditDays: Number(days) });
      notify(`Crédito de ${client.name} actualizado.`);
      credit.reload();
      onSaved();
    } catch (err) {
      setError(toApiError(err));
    } finally {
      setSaving(false);
    }
  }

  const status = credit.data;
  return (
    <Modal open onClose={onClose} size="lg" title={`Crédito de ${client.name}`} description={`${client.code} · ${client.rif}`}
      footer={<Button variant="secondary" onClick={onClose}>Cerrar</Button>}>
      {credit.error ? (
        <ErrorState error={credit.error} onRetry={credit.reload} />
      ) : !status ? (
        <LoadingState />
      ) : (
        <div className="flex flex-col gap-5">
          {status.isBlocked && (
            <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
              <span className="font-semibold">Crédito bloqueado.</span> {status.blockReason}
            </p>
          )}
          <dl className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            {[
              ['Deuda', formatMoney(status.debt)],
              ['Vencido', formatMoney(status.overdueAmount)],
              ['Disponible', status.available === null ? 'Sin límite' : formatMoney(status.available)],
              ['Deuda más vieja', status.oldestDebtDays === null ? (status.debt > 0 ? 'Sin factura' : '—') : `${formatInt(status.oldestDebtDays)} días`],
            ].map(([label, value]) => (
              <div key={label}>
                <dt className="text-sm text-muted">{label}</dt>
                <dd className="font-display text-xl font-semibold tabular-nums">{value}</dd>
              </div>
            ))}
          </dl>
          <section aria-label="Envejecimiento de la deuda">
            <h3 className="mb-2 text-sm font-medium">Envejecimiento de la deuda</h3>
            <DebtAgingBar aging={status} />
          </section>
          {isAdmin ? (
            <form
              className="grid grid-cols-1 gap-4 border-t border-line pt-4 sm:grid-cols-[1fr_1fr_auto] sm:items-end"
              onSubmit={(e) => {
                e.preventDefault();
                void save();
              }}
            >
              <TextField label="Límite de crédito (USD)" type="number" min={0} step="0.01" inputMode="decimal" value={limit}
                onChange={(e) => setLimit(e.target.value)} hint="Vacío = sin límite; 0 = solo contado." error={error?.fieldError('creditLimit')} />
              <TextField label="Días de crédito" type="number" min={1} max={365} inputMode="numeric" value={days}
                onChange={(e) => setDays(e.target.value)} hint="Deuda más vieja = vencida." error={error?.fieldError('creditDays')} />
              <Button type="submit" isLoading={saving}>Guardar</Button>
              {error && !error.fieldError('creditLimit') && !error.fieldError('creditDays') && (
                <p className="text-sm text-status-out sm:col-span-3" role="alert">{error.message}</p>
              )}
            </form>
          ) : (
            <p className="border-t border-line pt-4 text-sm text-muted">{creditLabel(status)}. Solo un administrador cambia estas condiciones.</p>
          )}
        </div>
      )}
    </Modal>
  );
}

/** Clientes y cuentas por cobrar con su situación de crédito (observación 2). */
export function ClientsPage() {
  const [search, setSearch] = useState('');
  const [onlyDebtors, setOnlyDebtors] = useState(false);
  const [selected, setSelected] = useState<Client | null>(null);
  const debouncedSearch = useDebounced(search.trim());
  const clients = useApiQuery(() => clientsApi.list(debouncedSearch), [debouncedSearch]);
  const rows = (clients.data ?? []).filter((c) => !onlyDebtors || c.moneyBalance < 0);

  return (
    <>
      <PageHeader title="Clientes" description="Saldos, condiciones de crédito y antigüedad de la deuda de cada cliente." />
      <div className="panel mb-4 flex flex-wrap items-end gap-4 p-4">
        <div className="flex min-w-[14rem] flex-1 flex-col gap-1.5">
          <label htmlFor="client-search" className="text-sm font-medium">Buscar</label>
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" aria-hidden />
            <input id="client-search" type="search" placeholder="Nombre, RIF o referencia" value={search} onChange={(e) => setSearch(e.target.value)}
              className="h-11 w-full rounded-md border border-line bg-surface pl-9 pr-3" />
          </div>
        </div>
        <label className="flex h-11 cursor-pointer items-center gap-2 text-sm">
          <input type="checkbox" checked={onlyDebtors} onChange={(e) => setOnlyDebtors(e.target.checked)} className="h-4 w-4 accent-[rgb(var(--brand))]" />
          Solo con deuda
        </label>
      </div>
      {clients.error ? (
        <ErrorState error={clients.error} onRetry={clients.reload} />
      ) : clients.isLoading && !clients.data ? (
        <LoadingState />
      ) : rows.length === 0 ? (
        <EmptyState title="Ningún cliente coincide" />
      ) : (
        <div className="panel p-4 sm:p-2">
          <DataTable
            caption="Clientes"
            rows={rows}
            rowKey={(c) => c.id}
            columns={[
              { header: 'Cliente', cell: (c) => <><p className="font-medium">{c.name}</p><p className="text-muted">{c.code} · {c.rif}</p></> },
              { header: 'Saldo', align: 'right', cell: (c) => <span className={cn(c.moneyBalance < 0 && 'font-semibold text-status-out')}>{c.moneyStatus}</span> },
              { header: 'Crédito', cell: (c) => <span className="text-muted">{creditLabel(c)}</span> },
              {
                header: 'Acciones',
                align: 'right',
                cell: (c) => (
                  <Button variant="secondary" size="sm" onClick={() => setSelected(c)} aria-label={`Crédito de ${c.name}`}>
                    <CreditCard className="h-4 w-4" aria-hidden />
                    Crédito
                  </Button>
                ),
              },
            ]}
          />
          <p className="px-4 pt-3 text-xs text-muted sm:px-3">
            La antigüedad usa el criterio FIFO: los abonos pagan primero las facturas más viejas. Tramos: {AGING_BUCKETS.map((b) => b.label).join(', ')}.
          </p>
        </div>
      )}
      {selected && <CreditDialog client={selected} onClose={() => setSelected(null)} onSaved={clients.reload} />}
    </>
  );
}
