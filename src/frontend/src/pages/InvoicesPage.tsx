import { useState } from 'react';
import { Printer } from 'lucide-react';
import { invoicesApi } from '../api/endpoints';
import { DataTable } from '../components/dashboard/DataTable';
import { presetRange } from '../components/dashboard/DashboardFilters';
import { PageHeader } from '../components/layout/PageHeader';
import { Ticket } from '../components/trucks/Ticket';
import { Button } from '../components/ui/Button';
import { Modal } from '../components/ui/Modal';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useApiQuery } from '../hooks/useApiQuery';
import { cn } from '../lib/cn';
import { formatDateTime, formatInt, formatMoney } from '../lib/format';

const RANGES = [
  { days: 1, label: 'Hoy' },
  { days: 7, label: '7 días' },
  { days: 30, label: '30 días' },
];

function TicketDialog({ invoiceId, onClose }: { invoiceId: string; onClose: () => void }) {
  const invoice = useApiQuery(() => invoicesApi.get(invoiceId), [invoiceId]);
  return (
    <Modal
      open
      onClose={onClose}
      title="Ticket de 80 mm"
      description="Vista previa del ticket para impresora térmica."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cerrar
          </Button>
          <Button onClick={() => window.print()} disabled={!invoice.data}>
            <Printer className="h-4 w-4" aria-hidden />
            Imprimir ticket
          </Button>
        </>
      }
    >
      <div className="rounded-md bg-sunken p-3">
        {invoice.error ? <ErrorState error={invoice.error} onRetry={invoice.reload} /> : invoice.data ? <Ticket invoice={invoice.data} /> : <LoadingState />}
      </div>
    </Modal>
  );
}

export function InvoicesPage() {
  const [days, setDays] = useState(7);
  const [includeCancelled, setIncludeCancelled] = useState(false);
  const [ticketId, setTicketId] = useState<string | null>(null);
  const range = presetRange(days);
  const invoices = useApiQuery(
    () => invoicesApi.list({ ...range, includeCancelled, pageSize: 200 }),
    [range.from, range.to, includeCancelled],
  );

  return (
    <>
      <PageHeader title="Facturas" description="Facturas y regalías emitidas, con su ticket listo para imprimir." />
      <div className="panel mb-4 flex flex-wrap items-center gap-4 p-4">
        <div className="flex rounded-md border border-line bg-sunken p-0.5" role="group" aria-label="Período">
          {RANGES.map((r) => (
            <button
              key={r.days}
              type="button"
              aria-pressed={days === r.days}
              onClick={() => setDays(r.days)}
              className={cn(
                'h-9 cursor-pointer rounded px-3 text-sm font-medium',
                days === r.days ? 'bg-surface text-ink shadow-sm' : 'text-muted hover:text-ink',
              )}
            >
              {r.label}
            </button>
          ))}
        </div>
        <label className="flex cursor-pointer items-center gap-2 text-sm">
          <input type="checkbox" checked={includeCancelled} onChange={(e) => setIncludeCancelled(e.target.checked)} className="h-4 w-4 accent-[rgb(var(--brand))]" />
          Incluir anuladas
        </label>
        {invoices.data && <p className="ml-auto text-sm text-muted">{formatInt(invoices.data.totalCount)} documentos</p>}
      </div>

      {invoices.error ? (
        <ErrorState error={invoices.error} onRetry={invoices.reload} />
      ) : !invoices.data ? (
        <LoadingState />
      ) : invoices.data.items.length === 0 ? (
        <EmptyState title="No hay facturas en este período">Elige un período más largo.</EmptyState>
      ) : (
        <div className="panel p-4 sm:p-2">
          <DataTable
            caption="Facturas y regalías"
            rows={invoices.data.items}
            rowKey={(i) => i.id}
            columns={[
              {
                header: 'Documento',
                cell: (i) => (
                  <>
                    <p className={cn('font-medium', i.isCancelled && 'line-through')}>{i.number}</p>
                    <p className="text-muted">{formatDateTime(i.issuedAt)}</p>
                  </>
                ),
              },
              { header: 'Cliente', cell: (i) => <><p>{i.clientName}</p><p className="text-muted">{i.dispatchOrigin === 'Truck' ? 'Camión' : 'Galpón'}, {i.attendantName}</p></> },
              { header: 'Total', align: 'right', cell: (i) => formatMoney(i.total) },
              { header: 'Pendiente', align: 'right', cell: (i) => (i.type === 'Gift' ? 'Regalía' : formatMoney(i.pending)) },
              {
                header: 'Ticket',
                align: 'right',
                cell: (i) => (
                  <Button variant="secondary" size="sm" onClick={() => setTicketId(i.id)} aria-label={`Ver ticket de ${i.number}`}>
                    <Printer className="h-4 w-4" aria-hidden />
                    Ticket
                  </Button>
                ),
              },
            ]}
          />
        </div>
      )}
      {ticketId && <TicketDialog invoiceId={ticketId} onClose={() => setTicketId(null)} />}
    </>
  );
}
