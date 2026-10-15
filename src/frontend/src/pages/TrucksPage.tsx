import { useState, type FormEvent } from 'react';
import { ClipboardCheck, Printer, Route } from 'lucide-react';
import { toApiError } from '../api/client';
import { trucksApi } from '../api/endpoints';
import type { RouteSettlement, Truck } from '../api/types';
import { PageHeader } from '../components/layout/PageHeader';
import { PaperWidthToggle, usePaperWidth } from '../components/print/ThermalPaper';
import { SettlementSheet } from '../components/trucks/SettlementSheet';
import { TruckCard } from '../components/trucks/TruckCard';
import { Button } from '../components/ui/Button';
import { TextField } from '../components/ui/Field';
import { Modal } from '../components/ui/Modal';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { useAuth } from '../hooks/useAuth';
import { formatDateTime, formatInt, formatMoney } from '../lib/format';

const num = (value: string) => Number(value.replace(',', '.'));

/**
 * Liquidación ciega: el chofer declara el dinero y lo que trae de vuelta SIN ver lo esperado. Solo se listan los
 * nombres de los productos que salieron en el camión, para facilitar el conteo, nunca sus cantidades.
 */
function SettleDialog({ truck, onClose, onSettled }: { truck: Truck; onClose: () => void; onSettled: (s: RouteSettlement) => void }) {
  const [cash, setCash] = useState('');
  const [counts, setCounts] = useState<Record<string, { boxes: string; units: string }>>({});
  const [observation, setObservation] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);
  const products = [...truck.stock].sort((a, b) => a.productName.localeCompare(b.productName));

  async function submit(event: FormEvent) {
    event.preventDefault();
    const returned = products.map((p) => ({ productId: p.productId, boxes: num(counts[p.productId]?.boxes || '0'), looseUnits: num(counts[p.productId]?.units || '0') }));
    if (cash.trim() === '' || !(num(cash) >= 0)) {
      setError('Declara el dinero recaudado (0 si no cobró nada).');
      return;
    }
    if (returned.some((r) => !Number.isInteger(r.boxes) || !Number.isInteger(r.looseUnits) || r.boxes < 0 || r.looseUnits < 0)) {
      setError('Las cantidades deben ser enteros mayores o iguales que 0.');
      return;
    }
    setIsSaving(true);
    setError(null);
    try {
      onSettled(await trucksApi.settle(truck.id, { countedCash: num(cash), returned: returned.filter((r) => r.boxes > 0 || r.looseUnits > 0), observation: observation.trim() || null }));
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
      size="lg"
      title={`Liquidar la ruta de ${truck.name}`}
      description="Cuenta el dinero y la mercancía que traes. No se muestra lo esperado: las diferencias aparecen en la planilla al confirmar."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="settle-form" isLoading={isSaving}>
            Confirmar liquidación
          </Button>
        </>
      }
    >
      <form id="settle-form" onSubmit={submit} noValidate className="flex flex-col gap-4">
        {error && (
          <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
            {error}
          </p>
        )}
        <TextField label="Dinero recaudado (USD)" inputMode="decimal" value={cash} onChange={(e) => setCash(e.target.value)} autoFocus />
        <fieldset className="flex flex-col gap-2">
          <legend className="mb-1 text-sm font-medium">Mercancía que traes de vuelta</legend>
          {products.length === 0 && <p className="text-sm text-muted">El camión salió sin mercancía registrada.</p>}
          {products.map((p) => (
            <div key={p.productId} className="grid grid-cols-[minmax(0,1fr)_4.5rem_4.5rem] items-end gap-2">
              <p className="pb-3 text-sm">{p.productName} <span className="text-muted">x{p.unitsPerBox}</span></p>
              <TextField label="Cajas" inputMode="numeric" value={counts[p.productId]?.boxes ?? ''} onChange={(e) => setCounts((c) => ({ ...c, [p.productId]: { boxes: e.target.value, units: c[p.productId]?.units ?? '' } }))} />
              <TextField label="Und" inputMode="numeric" value={counts[p.productId]?.units ?? ''} onChange={(e) => setCounts((c) => ({ ...c, [p.productId]: { boxes: c[p.productId]?.boxes ?? '', units: e.target.value } }))} />
            </div>
          ))}
        </fieldset>
        <TextField label="Observación (opcional)" value={observation} onChange={(e) => setObservation(e.target.value)} maxLength={500} />
      </form>
    </Modal>
  );
}

function SheetDialog({ settlement, onClose }: { settlement: RouteSettlement; onClose: () => void }) {
  const { width, setWidth } = usePaperWidth();
  return (
    <Modal
      open
      onClose={onClose}
      title="Planilla de cuadre"
      description={settlement.isBalanced ? 'La ruta cuadra: sin diferencias de dinero ni de mercancía.' : 'La ruta tiene diferencias; quedan registradas en la planilla.'}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cerrar
          </Button>
          <Button onClick={() => window.print()}>
            <Printer className="h-4 w-4" aria-hidden />
            Imprimir planilla
          </Button>
        </>
      }
    >
      <div className="mb-3 flex justify-end">
        <PaperWidthToggle width={width} onChange={setWidth} />
      </div>
      <div className="rounded-md bg-sunken p-3">
        <SettlementSheet settlement={settlement} width={width} />
      </div>
    </Modal>
  );
}

/** Tablero de flota: carga y valor de cada camión, salida a ruta y liquidación ciega al volver. */
export function TrucksPage() {
  const { isAdmin } = useAuth();
  const notify = useToast();
  const trucks = useApiQuery(() => trucksApi.list(), []);
  const history = useApiQuery(() => (isAdmin ? trucksApi.settlements() : Promise.resolve([] as RouteSettlement[])), [isAdmin]);
  const [settling, setSettling] = useState<Truck | null>(null);
  const [sheet, setSheet] = useState<RouteSettlement | null>(null);
  const [busy, setBusy] = useState<string | null>(null);

  async function dispatch(truck: Truck) {
    setBusy(truck.id);
    try {
      await trucksApi.dispatch(truck.id);
      notify(`${truck.name} salió a ruta.`);
      trucks.reload();
    } catch (err) {
      notify(toApiError(err).message, 'error');
    } finally {
      setBusy(null);
    }
  }

  return (
    <>
      <PageHeader title="Camiones" description="Qué lleva cada camión, en qué parte de la ruta está y su liquidación al volver." />
      {trucks.error ? (
        <ErrorState error={trucks.error} onRetry={trucks.reload} />
      ) : !trucks.data ? (
        <LoadingState />
      ) : trucks.data.length === 0 ? (
        <EmptyState title="No hay camiones registrados" />
      ) : (
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {trucks.data.map((truck) => (
            <TruckCard key={truck.id} {...truck}>
              <p className="text-sm text-muted">
                {formatInt(truck.totalUnits)} und, valor <span className="font-semibold text-ink">{formatMoney(truck.inventoryValue)}</span>
                {truck.dispatchedAt && truck.status === 'En ruta' ? `, salió ${formatDateTime(truck.dispatchedAt)}` : ''}
              </p>
              {truck.stock.length > 0 ? (
                <details className="group border-t border-line pt-3">
                  <summary className="cursor-pointer text-sm font-medium text-brand">Ver {truck.stock.length} productos cargados</summary>
                  <ul className="mt-2 flex flex-col gap-1.5 text-sm">
                    {truck.stock.map((line) => (
                      <li key={line.productId} className="flex justify-between gap-3">
                        <span className="min-w-0 truncate">{line.productName}</span>
                        <span className="shrink-0 text-muted">{line.stockDisplay}</span>
                      </li>
                    ))}
                  </ul>
                </details>
              ) : (
                <p className="border-t border-line pt-3 text-sm text-muted">Sin carga: está en el galpón.</p>
              )}
              <div className="flex flex-wrap gap-2">
                {truck.status === 'En ruta' ? (
                  <Button size="sm" onClick={() => setSettling(truck)}>
                    <ClipboardCheck className="h-4 w-4" aria-hidden />
                    Liquidar ruta
                  </Button>
                ) : (
                  <Button size="sm" variant="secondary" onClick={() => void dispatch(truck)} disabled={truck.totalUnits === 0} isLoading={busy === truck.id}>
                    <Route className="h-4 w-4" aria-hidden />
                    Salir a ruta
                  </Button>
                )}
              </div>
            </TruckCard>
          ))}
        </div>
      )}

      {isAdmin && (history.data?.length ?? 0) > 0 && (
        <section className="panel mt-6 p-4 sm:p-5" aria-label="Liquidaciones recientes">
          <h2 className="mb-3 font-display text-xl font-semibold">Liquidaciones recientes</h2>
          <ul className="flex flex-col divide-y divide-line">
            {history.data!.slice(0, 10).map((s) => (
              <li key={s.id} className="flex flex-wrap items-center justify-between gap-3 py-2 text-sm">
                <span>
                  <span className="font-semibold">{s.truckName}</span>, {formatDateTime(s.settledAt)}
                </span>
                <span className={s.isBalanced ? 'text-status-ok' : 'text-status-out'}>
                  {s.isBalanced ? 'Cuadrada' : `Dinero ${s.cashDifference > 0 ? '+' : ''}${formatMoney(s.cashDifference)}, faltante ${formatMoney(s.missingGoodsValue)}`}
                </span>
                <Button size="sm" variant="ghost" onClick={() => setSheet(s)}>
                  Ver planilla
                </Button>
              </li>
            ))}
          </ul>
        </section>
      )}

      {settling && (
        <SettleDialog
          truck={settling}
          onClose={() => setSettling(null)}
          onSettled={(s) => {
            setSettling(null);
            setSheet(s);
            notify(s.isBalanced ? `Ruta de ${s.truckName} liquidada sin diferencias.` : `Ruta de ${s.truckName} liquidada con diferencias.`, s.isBalanced ? 'success' : 'error');
            trucks.reload();
            history.reload();
          }}
        />
      )}
      {sheet && <SheetDialog settlement={sheet} onClose={() => setSheet(null)} />}
    </>
  );
}
