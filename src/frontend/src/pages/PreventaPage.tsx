import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { CloudOff, Plus, RotateCcw, Send, Trash2 } from 'lucide-react';
import { clientsApi, productsApi, trucksApi } from '../api/endpoints';
import type { Client, LineRequest, Product, Truck } from '../api/types';
import { PageHeader } from '../components/layout/PageHeader';
import { Button } from '../components/ui/Button';
import { SelectField, TextField } from '../components/ui/Field';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { formatDateTime, formatMoney } from '../lib/format';
import { cachedFetch } from '../offline/catalogCache';
import { useOffline } from '../offline/OfflineContext';
import { discardOrder, newOrderId, retryOrder } from '../offline/queue';

interface Line {
  productId: string;
  boxes: string;
  looseUnits: string;
}

interface Catalog {
  clients: Client[];
  trucks: Truck[];
  products: Product[];
  savedAt: string | null;
}

/** Catálogo, clientes y camiones; sin conexión, la última copia guardada en el dispositivo. */
async function loadCatalog(): Promise<Catalog> {
  const [clients, trucks, products] = await Promise.all([
    cachedFetch('clients', () => clientsApi.list()),
    cachedFetch('trucks', () => trucksApi.list()),
    cachedFetch('products', () => productsApi.list()),
  ]);
  const savedAt = [clients.savedAt, trucks.savedAt, products.savedAt].filter(Boolean).sort()[0] ?? null;
  return { clients: clients.data, trucks: trucks.data, products: products.data, savedAt };
}

const toInt = (value: string) => (value.trim() === '' ? 0 : Number(value));

/**
 * Preventa offline-first (observación del docente): el vendedor registra pedidos aunque no haya cobertura.
 * Cada pedido se guarda en IndexedDB con una clave de idempotencia y se envía a POST /api/invoices/sales en cuanto
 * vuelve la conexión, sin riesgo de duplicar facturas.
 */
export function PreventaPage() {
  const notify = useToast();
  const { isOnline, orders, pendingCount, isSyncing, enqueue, sync, refresh, lastResult } = useOffline();
  const catalog = useApiQuery(() => loadCatalog(), []);

  const [truckId, setTruckId] = useState('');
  const [clientSearch, setClientSearch] = useState('');
  const [clientId, setClientId] = useState('');
  const [lines, setLines] = useState<Line[]>([{ productId: '', boxes: '1', looseUnits: '0' }]);
  const [payment, setPayment] = useState('0');
  const [observation, setObservation] = useState('');
  const [formError, setFormError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const trucks = catalog.data?.trucks ?? [];
  const truck = trucks.find((t) => t.id === truckId);
  const productsById = useMemo(() => new Map((catalog.data?.products ?? []).map((p) => [p.id, p])), [catalog.data]);
  // Desde un camión solo se vende lo que lleva; desde el galpón, todo el catálogo con existencia.
  const available = useMemo(
    () =>
      truck
        ? truck.stock.map((s) => ({ id: s.productId, name: s.productName, stockUnits: s.stockUnits, stockDisplay: s.stockDisplay }))
        : (catalog.data?.products ?? []).filter((p) => p.stockUnits > 0).map((p) => ({ id: p.id, name: p.name, stockUnits: p.stockUnits, stockDisplay: p.stockDisplay })),
    [truck, catalog.data],
  );
  const clients = useMemo(() => {
    const term = clientSearch.trim().toLowerCase();
    const list = catalog.data?.clients ?? [];
    return term ? list.filter((c) => `${c.name} ${c.rif} ${c.nickname ?? ''} ${c.code}`.toLowerCase().includes(term)) : list;
  }, [catalog.data, clientSearch]);
  const client = catalog.data?.clients.find((c) => c.id === clientId);

  const total = lines.reduce((sum, line) => {
    const product = productsById.get(line.productId);
    return product ? sum + toInt(line.boxes) * product.priceBox + toInt(line.looseUnits) * product.priceUnit : sum;
  }, 0);

  useEffect(() => {
    if (lastResult?.sent.length) notify(`${lastResult.sent.length} ${lastResult.sent.length === 1 ? 'pedido enviado' : 'pedidos enviados'}: ${lastResult.sent.map((s) => s.invoice.number).join(', ')}.`);
    if (lastResult?.rejected.length) notify(`${lastResult.rejected.length} ${lastResult.rejected.length === 1 ? 'pedido rechazado' : 'pedidos rechazados'} por la API. Revísalos abajo.`, 'error');
  }, [lastResult, notify]);

  const setLine = (index: number, patch: Partial<Line>) => setLines((list) => list.map((l, i) => (i === index ? { ...l, ...patch } : l)));

  function validate(): string | null {
    if (!clientId) return 'Elige el cliente.';
    const chosen = lines.filter((l) => l.productId);
    if (chosen.length === 0) return 'Agrega al menos un producto.';
    for (const line of chosen) {
      const boxes = toInt(line.boxes);
      const loose = toInt(line.looseUnits);
      if (!Number.isInteger(boxes) || !Number.isInteger(loose) || boxes < 0 || loose < 0) return 'Las cantidades deben ser enteros mayores o iguales que 0.';
      if (boxes === 0 && loose === 0) return 'Cada producto necesita cajas o unidades.';
    }
    const pay = Number(payment.replace(',', '.'));
    if (!Number.isFinite(pay) || pay < 0) return 'El abono no puede ser negativo.';
    return null;
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const error = validate();
    setFormError(error);
    if (error || !client) return;

    const items: LineRequest[] = lines.filter((l) => l.productId).map((l) => ({ productId: l.productId, boxes: toInt(l.boxes), looseUnits: toInt(l.looseUnits) }));
    setIsSaving(true);
    try {
      const result = await enqueue({
        id: newOrderId(),
        createdAt: new Date().toISOString(),
        request: {
          clientId,
          dispatchOrigin: truck ? 'Truck' : 'Warehouse',
          truckId: truck?.id ?? null,
          employeeId: null,
          items,
          payment: Number(payment.replace(',', '.')),
          paymentObservation: observation.trim() || null,
          returnedEmpties: null,
        },
        summary: { clientName: client.name, origin: truck ? `Camión ${truck.name}` : 'Galpón', total, lines: items.length },
      });
      if (!result || result.offline) notify('Sin conexión: el pedido quedó guardado y se enviará al recuperar la señal.');
      setLines([{ productId: '', boxes: '1', looseUnits: '0' }]);
      setPayment('0');
      setObservation('');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <>
      <PageHeader
        title="Preventa"
        description="Registra pedidos en la ruta aunque no haya cobertura: se guardan en el dispositivo y se facturan al volver la señal."
      />

      {!isOnline && (
        <p className="mb-4 flex items-center gap-2 rounded-md border border-crate/60 bg-crate/15 px-4 py-3 text-sm" role="status">
          <CloudOff className="h-4 w-4 shrink-0" aria-hidden />
          Sin conexión. Los pedidos se guardan aquí y se envían solos cuando vuelva la señal.
        </p>
      )}
      {catalog.data?.savedAt && (
        <p className="mb-4 text-sm text-muted">Catálogo, clientes y camiones guardados el {formatDateTime(catalog.data.savedAt)}.</p>
      )}

      <div className="grid grid-cols-1 gap-5 xl:grid-cols-[minmax(0,1.4fr)_minmax(0,1fr)]">
        <section className="panel p-4 sm:p-5" aria-label="Nuevo pedido">
          <h2 className="mb-4 font-display text-xl font-semibold">Nuevo pedido</h2>
          {catalog.error ? (
            <ErrorState error={catalog.error} onRetry={catalog.reload} />
          ) : !catalog.data ? (
            <LoadingState label="Cargando catálogo…" />
          ) : (
            <form onSubmit={submit} noValidate className="flex flex-col gap-4">
              {formError && (
                <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
                  {formError}
                </p>
              )}
              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <SelectField label="Despacho" value={truckId} onChange={(e) => { setTruckId(e.target.value); setLines([{ productId: '', boxes: '1', looseUnits: '0' }]); }}>
                  <option value="">Galpón</option>
                  {trucks.map((t) => (
                    <option key={t.id} value={t.id}>
                      Camión {t.name} ({t.status})
                    </option>
                  ))}
                </SelectField>
                <TextField label="Buscar cliente" type="search" value={clientSearch} onChange={(e) => setClientSearch(e.target.value)} placeholder="Nombre, RIF o código" />
              </div>
              <SelectField label="Cliente" value={clientId} onChange={(e) => setClientId(e.target.value)}
                hint={client ? `${client.moneyStatus}${client.creditLimit !== null ? `, límite ${formatMoney(client.creditLimit)}` : ''}` : undefined}>
                <option value="">Elige un cliente ({clients.length})</option>
                {clients.slice(0, 200).map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.code} {c.name}
                  </option>
                ))}
              </SelectField>

              <fieldset className="flex flex-col gap-3">
                <legend className="mb-1 text-sm font-medium">Productos</legend>
                {lines.map((line, index) => (
                  <div key={index} className="grid grid-cols-[minmax(0,1fr)_4.5rem_4.5rem_auto] items-end gap-2">
                    <SelectField label={index === 0 ? 'Producto' : `Producto ${index + 1}`} value={line.productId} onChange={(e) => setLine(index, { productId: e.target.value })}>
                      <option value="">Elige…</option>
                      {available.map((p) => (
                        <option key={p.id} value={p.id}>
                          {p.name} ({p.stockDisplay})
                        </option>
                      ))}
                    </SelectField>
                    <TextField label="Cajas" inputMode="numeric" value={line.boxes} onChange={(e) => setLine(index, { boxes: e.target.value })} />
                    <TextField label="Und" inputMode="numeric" value={line.looseUnits} onChange={(e) => setLine(index, { looseUnits: e.target.value })} />
                    <Button variant="ghost" onClick={() => setLines((l) => (l.length > 1 ? l.filter((_, i) => i !== index) : l))} aria-label={`Quitar producto ${index + 1}`} disabled={lines.length === 1}>
                      <Trash2 className="h-4 w-4" aria-hidden />
                    </Button>
                  </div>
                ))}
                <Button variant="secondary" size="sm" className="self-start" onClick={() => setLines((l) => [...l, { productId: '', boxes: '1', looseUnits: '0' }])}>
                  <Plus className="h-4 w-4" aria-hidden />
                  Agregar producto
                </Button>
              </fieldset>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                <TextField label="Abono (USD)" inputMode="decimal" value={payment} onChange={(e) => setPayment(e.target.value)} hint="Lo que el cliente paga ahora." />
                <TextField label="Observación de pago" value={observation} onChange={(e) => setObservation(e.target.value)} maxLength={1000} />
              </div>

              <div className="flex flex-wrap items-center justify-between gap-3 border-t border-line pt-4">
                <p>
                  Total estimado <span className="font-display text-2xl font-semibold">{formatMoney(total)}</span>
                </p>
                <Button type="submit" isLoading={isSaving}>
                  <Send className="h-4 w-4" aria-hidden />
                  {isOnline ? 'Registrar pedido' : 'Guardar pedido sin conexión'}
                </Button>
              </div>
            </form>
          )}
        </section>

        <section className="panel p-4 sm:p-5" aria-label="Pedidos por enviar">
          <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
            <h2 className="font-display text-xl font-semibold">Pedidos por enviar</h2>
            <Button variant="secondary" size="sm" onClick={() => void sync()} isLoading={isSyncing} disabled={pendingCount === 0}>
              <RotateCcw className="h-4 w-4" aria-hidden />
              Enviar ahora
            </Button>
          </div>
          {orders.length === 0 ? (
            <EmptyState title="No hay pedidos pendientes">Los pedidos que registres sin conexión aparecerán aquí hasta que se envíen.</EmptyState>
          ) : (
            <ul className="flex flex-col gap-3">
              {orders.map((order) => (
                <li key={order.id} className="rounded-md border border-line p-3">
                  <div className="flex items-start justify-between gap-3">
                    <div className="min-w-0">
                      <p className="font-semibold">{order.summary.clientName}</p>
                      <p className="text-sm text-muted">
                        {order.summary.origin}, {order.summary.lines} {order.summary.lines === 1 ? 'producto' : 'productos'}, {formatDateTime(order.createdAt)}
                      </p>
                    </div>
                    <p className="shrink-0 font-display text-lg font-semibold">{formatMoney(order.summary.total)}</p>
                  </div>
                  {order.status === 'rejected' ? (
                    <div className="mt-2 flex flex-col gap-2">
                      <p className="text-sm text-status-out" role="alert">
                        Rechazado: {order.error}
                      </p>
                      <div className="flex gap-2">
                        <Button size="sm" variant="secondary" onClick={() => void retryOrder(order.id).then(refresh).then(() => sync())}>
                          Reintentar
                        </Button>
                        <Button size="sm" variant="ghost" className="text-status-out" onClick={() => void discardOrder(order.id).then(refresh)}>
                          Descartar
                        </Button>
                      </div>
                    </div>
                  ) : (
                    <p className="mt-2 text-sm text-muted">{isOnline ? 'Pendiente de envío' : 'Esperando conexión'}{order.attempts > 0 ? ` (intentos: ${order.attempts})` : ''}</p>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </>
  );
}
