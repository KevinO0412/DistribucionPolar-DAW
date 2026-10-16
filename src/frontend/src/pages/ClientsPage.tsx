import { useState, type FormEvent } from 'react';
import { Banknote, History, PackageOpen, Printer, Search, ShieldCheck } from 'lucide-react';
import { toApiError } from '../api/client';
import { clientsApi } from '../api/endpoints';
import type { Client } from '../api/types';
import { ClientStatementDialog } from '../components/clients/ClientStatementDialog';
import { DataTable } from '../components/dashboard/DataTable';
import { PageHeader } from '../components/layout/PageHeader';
import { PaymentReceipt, type PaymentReceiptData } from '../components/print/PaymentReceipt';
import { PaperWidthToggle, usePaperWidth } from '../components/print/ThermalPaper';
import { Button } from '../components/ui/Button';
import { TextField } from '../components/ui/Field';
import { Modal } from '../components/ui/Modal';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { useAuth } from '../hooks/useAuth';
import { useDebounced } from '../hooks/useDebounced';
import { cn } from '../lib/cn';
import { formatInt, formatMoney } from '../lib/format';
import { EMPTY_GROUPS } from './ProductForm';

const num = (value: string) => Number(value.replace(',', '.'));

function MoneyBalance({ client }: { client: Client }) {
  const tone = client.moneyBalance < 0 ? 'text-status-out' : client.moneyBalance > 0 ? 'text-status-ok' : 'text-muted';
  return <span className={cn('font-semibold', tone)}>{client.moneyStatus}</span>;
}

/** Saldo de vacíos por grupo de envase: generados en facturas menos devueltos (negativo = el cliente debe). */
function EmptiesBalance({ client }: { client: Client }) {
  if (client.emptyBalances.length === 0) return <span className="text-muted">Sin vacíos pendientes</span>;
  return (
    <ul className="flex flex-col gap-0.5">
      {client.emptyBalances.map((b) => (
        <li key={b.groupKey} className={b.boxes < 0 || b.units < 0 ? 'text-status-out' : 'text-status-ok'}>
          {b.groupName}: {b.boxes < 0 || b.units < 0 ? 'debe' : 'a favor'} {Math.abs(b.boxes)} cj + {Math.abs(b.units)} und
        </li>
      ))}
    </ul>
  );
}

function PaymentDialog({ client, onClose, onPaid }: { client: Client; onClose: () => void; onPaid: (receipt: PaymentReceiptData) => void }) {
  const { user } = useAuth();
  const [amount, setAmount] = useState('');
  const [observation, setObservation] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!(num(amount) > 0)) {
      setError('El abono debe ser mayor que 0.');
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      const updated = await clientsApi.registerPayment(client.id, num(amount), observation.trim() || null);
      onPaid({
        client: updated,
        amount: num(amount),
        balanceBefore: client.moneyBalance,
        observation: observation.trim() || null,
        receivedAt: new Date().toISOString(),
        receivedBy: user?.fullName ?? '',
      });
    } catch (err) {
      setError(toApiError(err).message);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open
      onClose={onClose}
      title={`Abono de ${client.name}`}
      description={client.moneyStatus}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="payment-form" isLoading={isSaving}>
            Registrar abono
          </Button>
        </>
      }
    >
      <form id="payment-form" onSubmit={submit} noValidate className="flex flex-col gap-4">
        <TextField label="Monto (USD)" inputMode="decimal" value={amount} onChange={(e) => setAmount(e.target.value)} error={error ?? undefined} autoFocus />
        <TextField label="Observación (moneda, tasa, referencia)" value={observation} onChange={(e) => setObservation(e.target.value)} maxLength={500} />
      </form>
    </Modal>
  );
}

function ReceiptDialog({ data, onClose }: { data: PaymentReceiptData; onClose: () => void }) {
  const { width, setWidth } = usePaperWidth();
  return (
    <Modal
      open
      onClose={onClose}
      title="Recibo de cobro"
      description="Vista previa para impresora térmica de 80 o 58 mm."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cerrar
          </Button>
          <Button onClick={() => window.print()}>
            <Printer className="h-4 w-4" aria-hidden />
            Imprimir recibo
          </Button>
        </>
      }
    >
      <div className="mb-3 flex justify-end">
        <PaperWidthToggle width={width} onChange={setWidth} />
      </div>
      <div className="rounded-md bg-sunken p-3">
        <PaymentReceipt data={data} width={width} />
      </div>
    </Modal>
  );
}

function EmptiesDialog({ client, onClose, onSaved }: { client: Client; onClose: () => void; onSaved: (c: Client) => void }) {
  const [values, setValues] = useState<Record<string, { boxes: string; units: string }>>({});
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    const groups = EMPTY_GROUPS.map((g) => ({ groupKey: g.key, boxes: num(values[g.key]?.boxes || '0'), units: num(values[g.key]?.units || '0') }));
    if (groups.some((g) => !Number.isInteger(g.boxes) || !Number.isInteger(g.units) || g.boxes < 0 || g.units < 0)) {
      setError('Las cantidades deben ser enteros mayores o iguales que 0.');
      return;
    }
    if (groups.every((g) => g.boxes === 0 && g.units === 0)) {
      setError('Indica al menos una caja o unidad devuelta.');
      return;
    }
    setIsSaving(true);
    try {
      onSaved(await clientsApi.returnEmpties(client.id, groups.filter((g) => g.boxes > 0 || g.units > 0), null));
    } catch (err) {
      setError(toApiError(err).message);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open
      onClose={onClose}
      title={`Devolución de vacíos de ${client.name}`}
      description={client.emptiesStatus}
      size="lg"
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="empties-form" isLoading={isSaving}>
            Registrar devolución
          </Button>
        </>
      }
    >
      <form id="empties-form" onSubmit={submit} noValidate className="flex flex-col gap-3">
        {error && (
          <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
            {error}
          </p>
        )}
        {EMPTY_GROUPS.map((g) => (
          <div key={g.key} className="grid grid-cols-[minmax(0,1fr)_5rem_5rem] items-end gap-2">
            <p className="pb-3 text-sm font-medium">{g.name}</p>
            <TextField label="Cajas" inputMode="numeric" value={values[g.key]?.boxes ?? ''} onChange={(e) => setValues((v) => ({ ...v, [g.key]: { boxes: e.target.value, units: v[g.key]?.units ?? '' } }))} />
            <TextField label="Und" inputMode="numeric" value={values[g.key]?.units ?? ''} onChange={(e) => setValues((v) => ({ ...v, [g.key]: { boxes: v[g.key]?.boxes ?? '', units: e.target.value } }))} />
          </div>
        ))}
      </form>
    </Modal>
  );
}

function CreditLimitDialog({ client, onClose, onSaved }: { client: Client; onClose: () => void; onSaved: (c: Client) => void }) {
  const [limit, setLimit] = useState(client.creditLimit === null ? '' : String(client.creditLimit));
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    const value = limit.trim() === '' ? null : num(limit);
    if (value !== null && !(value >= 0)) {
      setError('El límite no puede ser negativo.');
      return;
    }
    setIsSaving(true);
    try {
      onSaved(await clientsApi.setCreditLimit(client.id, value));
    } catch (err) {
      setError(toApiError(err).message);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open
      onClose={onClose}
      title={`Límite de crédito de ${client.name}`}
      description="Una venta que deje la deuda por encima del límite se rechaza. Vacío = sin límite; 0 = solo de contado."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="credit-form" isLoading={isSaving}>
            Guardar límite
          </Button>
        </>
      }
    >
      <form id="credit-form" onSubmit={submit} noValidate>
        <TextField label="Límite (USD)" inputMode="decimal" value={limit} onChange={(e) => setLimit(e.target.value)} error={error ?? undefined} autoFocus />
      </form>
    </Modal>
  );
}

type Dialog =
  | { kind: 'payment'; client: Client }
  | { kind: 'receipt'; data: PaymentReceiptData }
  | { kind: 'empties'; client: Client }
  | { kind: 'credit'; client: Client }
  | { kind: 'statement'; client: Client }
  | null;

/**
 * Clientes y cuentas por cobrar: saldos con signo, vacíos por grupo (generados frente a devueltos), abonos con recibo
 * imprimible y, para el Admin, el límite de crédito.
 */
export function ClientsPage() {
  const { isAdmin } = useAuth();
  const notify = useToast();
  const [search, setSearch] = useState('');
  const [onlyReceivable, setOnlyReceivable] = useState(false);
  const debounced = useDebounced(search.trim());
  const clients = useApiQuery(() => clientsApi.list(debounced), [debounced]);
  const [dialog, setDialog] = useState<Dialog>(null);

  const list = (clients.data ?? []).filter((c) => !onlyReceivable || c.isReceivable);
  const debt = (clients.data ?? []).reduce((sum, c) => sum + (c.moneyBalance < 0 ? -c.moneyBalance : 0), 0);
  const emptiesOwed = (clients.data ?? []).reduce((sum, c) => sum + (c.emptyBoxesBalance < 0 ? -c.emptyBoxesBalance : 0), 0);

  return (
    <>
      <PageHeader title="Clientes" description="Saldos, historial de movimientos, vacíos por grupo de envase, abonos con recibo y límites de crédito." />

      <div className="panel mb-4 flex flex-wrap items-end gap-4 p-4">
        <div className="flex min-w-[14rem] flex-1 flex-col gap-1.5">
          <label htmlFor="client-search" className="text-sm font-medium">
            Buscar
          </label>
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" aria-hidden />
            <input id="client-search" type="search" placeholder="Nombre, RIF, dirección o referencia" value={search} onChange={(e) => setSearch(e.target.value)} className="h-11 w-full rounded-md border border-line bg-surface pl-9 pr-3" />
          </div>
        </div>
        <label className="flex h-11 cursor-pointer items-center gap-2 text-sm">
          <input type="checkbox" checked={onlyReceivable} onChange={(e) => setOnlyReceivable(e.target.checked)} className="h-4 w-4 accent-[rgb(var(--brand))]" />
          Solo cuentas por cobrar
        </label>
        <p className="ml-auto self-center text-sm text-muted" aria-live="polite">
          Deben {formatMoney(debt)} y {formatInt(emptiesOwed)} cajas de vacíos
        </p>
      </div>

      {clients.error ? (
        <ErrorState error={clients.error} onRetry={clients.reload} />
      ) : !clients.data ? (
        <LoadingState />
      ) : list.length === 0 ? (
        <EmptyState title="Ningún cliente coincide">Prueba con otro texto o quita el filtro.</EmptyState>
      ) : (
        <div className="panel p-4 sm:p-2">
          <DataTable
            caption="Clientes"
            rows={list}
            rowKey={(c) => c.id}
            columns={[
              {
                header: 'Cliente',
                cell: (c) => (
                  <>
                    <button type="button" onClick={() => setDialog({ kind: 'statement', client: c })} className="cursor-pointer text-left font-medium text-brand underline-offset-2 hover:underline" title="Ver historial">
                      {c.code} {c.name}
                    </button><p className="text-muted">{c.rif}{c.nickname ? `, ${c.nickname}` : ''}</p>
                  </>
                ),
              },
              { header: 'Dinero', cell: (c) => <><MoneyBalance client={c} /><p className="text-muted">{c.creditLimit === null ? 'Sin límite' : `Límite ${formatMoney(c.creditLimit)}`}</p></> },
              { header: 'Vacíos', cell: (c) => <EmptiesBalance client={c} /> },
              {
                header: 'Acciones',
                align: 'right',
                cell: (c) => (
                  <div className="flex flex-wrap justify-end gap-1">
                    <Button size="sm" variant="ghost" onClick={() => setDialog({ kind: 'statement', client: c })} aria-label={`Historial de ${c.name}`}>
                      <History className="h-4 w-4" aria-hidden />
                      Historial
                    </Button>
                    <Button size="sm" variant="secondary" onClick={() => setDialog({ kind: 'payment', client: c })}>
                      <Banknote className="h-4 w-4" aria-hidden />
                      Abono
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setDialog({ kind: 'empties', client: c })} aria-label={`Registrar vacíos devueltos por ${c.name}`}>
                      <PackageOpen className="h-4 w-4" aria-hidden />
                    </Button>
                    {isAdmin && (
                      <Button size="sm" variant="ghost" onClick={() => setDialog({ kind: 'credit', client: c })} aria-label={`Límite de crédito de ${c.name}`}>
                        <ShieldCheck className="h-4 w-4" aria-hidden />
                      </Button>
                    )}
                  </div>
                ),
              },
            ]}
          />
        </div>
      )}

      {dialog?.kind === 'payment' && (
        <PaymentDialog
          client={dialog.client}
          onClose={() => setDialog(null)}
          onPaid={(data) => {
            notify(`Abono de ${formatMoney(data.amount)} registrado.`);
            clients.reload();
            setDialog({ kind: 'receipt', data });
          }}
        />
      )}
      {dialog?.kind === 'statement' && <ClientStatementDialog client={dialog.client} onClose={() => setDialog(null)} />}
      {dialog?.kind === 'receipt' && <ReceiptDialog data={dialog.data} onClose={() => setDialog(null)} />}
      {dialog?.kind === 'empties' && (
        <EmptiesDialog client={dialog.client} onClose={() => setDialog(null)} onSaved={(c) => { notify(`Vacíos registrados. ${c.emptiesStatus}.`); clients.reload(); setDialog(null); }} />
      )}
      {dialog?.kind === 'credit' && (
        <CreditLimitDialog client={dialog.client} onClose={() => setDialog(null)} onSaved={(c) => { notify(`Límite de ${c.name}: ${c.creditLimit === null ? 'sin límite' : formatMoney(c.creditLimit)}.`); clients.reload(); setDialog(null); }} />
      )}
    </>
  );
}
