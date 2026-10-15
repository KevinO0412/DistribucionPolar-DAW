import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { CloudOff, Plus, RefreshCw, Send, Trash2, Wifi } from 'lucide-react';
import { clientsApi, invoicesApi, productsApi, trucksApi } from '../api/endpoints';
import { toApiError, type ApiError } from '../api/client';
import type { Client, Product, Truck } from '../api/types';
import { PageHeader } from '../components/layout/PageHeader';
import { Button } from '../components/ui/Button';
import { SelectField, TextField } from '../components/ui/Field';
import { ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useOnline } from '../hooks/useOnline';
import { cn } from '../lib/cn';
import { formatDateTime, formatMoney } from '../lib/format';
import { buildSaleRequest, estimateTotal, fetchWithCache, mergeLines, type DraftLine } from '../offline/presale';
import { idbOrderStore, newOrderId, type OrderStore, type PendingOrder } from '../offline/store';
import { syncOrders } from '../offline/sync';

interface Catalog {
  clients: Client[];
  products: Product[];
  trucks: Truck[];
  savedAt: string | null;
}

const STATUS_LABEL: Record<PendingOrder['status'], { label: string; tone: string }> = {
  pending: { label: 'Pendiente de envío', tone: 'bg-status-critical/10 text-status-critical' },
  synced: { label: 'Facturado', tone: 'bg-status-ok/10 text-status-ok' },
  rejected: { label: 'Rechazado', tone: 'bg-status-out/10 text-status-out' },
};

/**
 * Preventa offline-first: el pedido se guarda primero en IndexedDB y después se envía a la API con su id como
 * Idempotency-Key. Sin señal, queda en cola y se envía solo al volver la conexión; reenviarlo nunca duplica la factura.
 */
export function PresalePage({ store = idbOrderStore }: { store?: OrderStore }) {
  const online = useOnline();
  const notify = useToast();
  const [catalog, setCatalog] = useState<Catalog>();
  const [loadError, setLoadError] = useState<ApiError | null>(null);
  const [orders, setOrders] = useState<PendingOrder[]>([]);
  const [syncing, setSyncing] = useState(false);
  const syncingRef = useRef(false);

  const [clientId, setClientId] = useState('');
  const [clientSearch, setClientSearch] = useState('');
  const [truckId, setTruckId] = useState('');
  const [productId, setProductId] = useState('');
  const [boxes, setBoxes] = useState('1');
  const [looseUnits, setLooseUnits] = useState('0');
  const [lines, setLines] = useState<DraftLine[]>([]);
  const [payment, setPayment] = useState('0');
  const [observation, setObservation] = useState('');
  const [formError, setFormError] = useState<string>();

  const loadCatalog = useCallback(async () => {
    setLoadError(null);
    try {
      const [clients, products, trucks] = await Promise.all([
        fetchWithCache('clients', () => clientsApi.list()),
        fetchWithCache('products', () => productsApi.list()),
        fetchWithCache('trucks', () => trucksApi.list()),
      ]);
      const savedAt = [clients.savedAt, products.savedAt, trucks.savedAt].find(Boolean) ?? null;
      setCatalog({ clients: clients.data, products: products.data, trucks: trucks.data, savedAt });
    } catch (err) {
      setLoadError(toApiError(err));
    }
  }, []);

  const refreshOrders = useCallback(async () => setOrders(await store.all()), [store]);

  const sync = useCallback(async (manual = false) => {
    if (syncingRef.current) return;
    syncingRef.current = true;
    setSyncing(true);
    try {
      const result = await syncOrders(store, (order) => invoicesApi.createSale(order.request, order.id));
      await refreshOrders();
      if (result.synced > 0) notify(`${result.synced} pedido(s) facturado(s).`);
      if (result.rejected > 0) notify(`${result.rejected} pedido(s) rechazado(s): revisa el detalle.`, 'error');
      if (manual && result.offline) notify('Sin conexión: los pedidos siguen guardados en el dispositivo.', 'error');
    } finally {
      syncingRef.current = false;
      setSyncing(false);
    }
  }, [notify, refreshOrders, store]);

  useEffect(() => {
    void loadCatalog();
    void refreshOrders().then(() => sync());
  }, [loadCatalog, refreshOrders, sync]);

  // Al volver la señal: se envía la cola y se refresca la copia local del catálogo.
  useEffect(() => {
    if (online) {
      void sync();
      void loadCatalog();
    }
  }, [online, sync, loadCatalog]);

  const truck = catalog?.trucks.find((t) => t.id === truckId);
  const productOptions = useMemo(() => {
    if (!catalog) return [];
    if (!truck) return catalog.products.filter((p) => p.isActive && p.stockUnits > 0);
    const loaded = new Set(truck.stock.map((s) => s.productId));
    return catalog.products.filter((p) => loaded.has(p.id));
  }, [catalog, truck]);
  const clientOptions = useMemo(() => {
    const term = clientSearch.trim().toLowerCase();
    const list = catalog?.clients ?? [];
    return term ? list.filter((c) => `${c.code} ${c.name} ${c.rif} ${c.nickname ?? ''}`.toLowerCase().includes(term)) : list;
  }, [catalog, clientSearch]);

  const total = estimateTotal(mergeLines(lines), catalog?.products ?? []);

  function addLine() {
    const product = productOptions.find((p) => p.id === productId);
    const b = Number(boxes);
    const u = Number(looseUnits);
    if (!product) return setFormError('Elige un producto.');
    if (!Number.isInteger(b) || !Number.isInteger(u) || b < 0 || u < 0 || b + u === 0) {
      return setFormError('Indica cajas o unidades enteras mayores que 0.');
    }
    setFormError(undefined);
    setLines((current) => mergeLines([...current, { productId: product.id, productName: product.name, boxes: b, looseUnits: u }]));
    setProductId('');
    setBoxes('1');
    setLooseUnits('0');
  }

  async function saveOrder() {
    const client = catalog?.clients.find((c) => c.id === clientId);
    const amount = Number(payment);
    if (!client) return setFormError('Elige el cliente.');
    if (lines.length === 0) return setFormError('Agrega al menos un producto.');
    if (!Number.isFinite(amount) || amount < 0) return setFormError('El abono no puede ser negativo.');
    setFormError(undefined);

    const order: PendingOrder = {
      id: newOrderId(),
      createdAt: new Date().toISOString(),
      clientName: client.name,
      originName: truck ? `Camión ${truck.name}` : 'Galpón',
      estimatedTotal: total,
      request: buildSaleRequest({ clientId, truckId: truckId || null, lines, payment: amount, observation }),
      status: 'pending',
      attempts: 0,
    };
    await store.put(order);
    await refreshOrders();
    setLines([]);
    setPayment('0');
    setObservation('');
    notify(online ? 'Pedido guardado; enviándolo…' : 'Pedido guardado en el dispositivo. Se enviará al volver la conexión.');
    if (online) void sync();
  }

  async function removeOrder(id: string) {
    await store.remove(id);
    await refreshOrders();
  }

  const pendingCount = orders.filter((o) => o.status === 'pending').length;

  return (
    <>
      <PageHeader
        title="Preventa"
        description="Prepara pedidos aunque no haya señal: se guardan en el dispositivo y se facturan solos al volver la conexión, sin duplicarse."
        actions={
          <Button variant="secondary" onClick={() => void sync(true)} isLoading={syncing} disabled={pendingCount === 0}>
            <RefreshCw className="h-4 w-4" aria-hidden />
            Enviar pendientes ({pendingCount})
          </Button>
        }
      />

      <p
        role="status"
        className={cn(
          'mb-4 flex items-center gap-2 rounded-md border px-4 py-2 text-sm',
          online ? 'border-status-ok/40 bg-status-ok/5' : 'border-status-critical/50 bg-status-critical/10',
        )}
      >
        {online ? <Wifi className="h-4 w-4 text-status-ok" aria-hidden /> : <CloudOff className="h-4 w-4 text-status-critical" aria-hidden />}
        {online ? 'En línea: los pedidos se facturan al guardarlos.' : 'Sin conexión: los pedidos se guardan en el dispositivo.'}
        {catalog?.savedAt && <span className="text-muted">Catálogo guardado: {formatDateTime(catalog.savedAt)}</span>}
      </p>

      {loadError ? (
        <ErrorState error={loadError} onRetry={() => void loadCatalog()} />
      ) : !catalog ? (
        <LoadingState />
      ) : (
        <div className="grid grid-cols-1 gap-4 xl:grid-cols-[minmax(0,3fr)_minmax(0,2fr)]">
          <section className="panel flex flex-col gap-4 p-4" aria-labelledby="presale-form">
            <h2 id="presale-form" className="font-display text-xl font-semibold">Nuevo pedido</h2>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <TextField label="Buscar cliente" value={clientSearch} onChange={(e) => setClientSearch(e.target.value)} placeholder="Nombre, RIF o código" />
              <SelectField label="Cliente" value={clientId} onChange={(e) => setClientId(e.target.value)}>
                <option value="">Elige un cliente…</option>
                {clientOptions.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.code} · {c.name}
                  </option>
                ))}
              </SelectField>
              <SelectField
                label="Despacho"
                value={truckId}
                onChange={(e) => {
                  setTruckId(e.target.value);
                  setLines([]);
                }}
                hint="Al cambiar el origen se vacían las líneas."
              >
                <option value="">Galpón</option>
                {catalog.trucks.map((t) => (
                  <option key={t.id} value={t.id}>
                    Camión {t.name} ({t.plate})
                  </option>
                ))}
              </SelectField>
            </div>

            <div className="grid grid-cols-2 gap-3 sm:grid-cols-[minmax(0,1fr)_6rem_6rem_auto] sm:items-end">
              <SelectField label="Producto" value={productId} onChange={(e) => setProductId(e.target.value)} wrapperClassName="col-span-2 sm:col-span-1">
                <option value="">Elige un producto…</option>
                {productOptions.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name}
                  </option>
                ))}
              </SelectField>
              <TextField label="Cajas" type="number" min={0} inputMode="numeric" value={boxes} onChange={(e) => setBoxes(e.target.value)} />
              <TextField label="Unidades" type="number" min={0} inputMode="numeric" value={looseUnits} onChange={(e) => setLooseUnits(e.target.value)} />
              <Button variant="secondary" onClick={addLine} className="col-span-2 sm:col-span-1">
                <Plus className="h-4 w-4" aria-hidden />
                Agregar
              </Button>
            </div>

            {lines.length > 0 && (
              <ul className="flex flex-col divide-y divide-line rounded-md border border-line" aria-label="Productos del pedido">
                {lines.map((line) => (
                  <li key={line.productId} className="flex items-center justify-between gap-3 px-3 py-2 text-sm">
                    <span className="min-w-0 truncate">{line.productName}</span>
                    <span className="flex shrink-0 items-center gap-2 text-muted">
                      {line.boxes} cj + {line.looseUnits} und
                      <Button
                        variant="ghost"
                        size="sm"
                        aria-label={`Quitar ${line.productName}`}
                        onClick={() => setLines((current) => current.filter((l) => l.productId !== line.productId))}
                      >
                        <Trash2 className="h-4 w-4" aria-hidden />
                      </Button>
                    </span>
                  </li>
                ))}
              </ul>
            )}

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <TextField label="Abono (USD)" type="number" min={0} step="0.01" inputMode="decimal" value={payment} onChange={(e) => setPayment(e.target.value)} />
              <TextField label="Observación del pago" value={observation} onChange={(e) => setObservation(e.target.value)} placeholder="Ej.: pagó en efectivo" />
            </div>

            {formError && (
              <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
                {formError}
              </p>
            )}

            <div className="flex flex-wrap items-center justify-between gap-3 border-t border-line pt-4">
              <p className="text-sm text-muted">
                Total estimado <span className="font-display text-2xl font-semibold text-ink">{formatMoney(total)}</span>
              </p>
              <Button onClick={() => void saveOrder()}>
                <Send className="h-4 w-4" aria-hidden />
                Guardar pedido
              </Button>
            </div>
          </section>

          <section className="panel flex flex-col gap-3 p-4" aria-labelledby="presale-queue">
            <h2 id="presale-queue" className="font-display text-xl font-semibold">Pedidos del dispositivo</h2>
            {orders.length === 0 ? (
              <p className="text-sm text-muted">Todavía no hay pedidos guardados.</p>
            ) : (
              <ul className="flex flex-col gap-3">
                {[...orders].reverse().map((order) => (
                  <li key={order.id} className="rounded-md border border-line p-3 text-sm">
                    <div className="flex items-start justify-between gap-2">
                      <div className="min-w-0">
                        <p className="truncate font-medium">{order.clientName}</p>
                        <p className="text-muted">
                          {order.originName} · {formatDateTime(order.createdAt)} · {formatMoney(order.estimatedTotal)}
                        </p>
                      </div>
                      <span className={cn('shrink-0 rounded px-2 py-0.5 text-xs font-semibold', STATUS_LABEL[order.status].tone)}>
                        {STATUS_LABEL[order.status].label}
                      </span>
                    </div>
                    {order.invoiceNumber && <p className="mt-1">{order.invoiceNumber}</p>}
                    {order.error && (
                      <p className={cn('mt-1', order.status === 'rejected' ? 'text-status-out' : 'text-muted')} role={order.status === 'rejected' ? 'alert' : undefined}>
                        {order.error}
                      </p>
                    )}
                    {order.status !== 'pending' && (
                      <Button variant="ghost" size="sm" className="mt-1" onClick={() => void removeOrder(order.id)}>
                        Quitar de la lista
                      </Button>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </section>
        </div>
      )}
    </>
  );
}
