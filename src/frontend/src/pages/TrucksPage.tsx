import { useState } from 'react';
import { Navigation } from 'lucide-react';
import { trucksApi } from '../api/endpoints';
import { toApiError } from '../api/client';
import type { Truck } from '../api/types';
import { PageHeader } from '../components/layout/PageHeader';
import { TruckCard } from '../components/trucks/TruckCard';
import { Button } from '../components/ui/Button';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { formatDateTime, formatInt, formatMoney } from '../lib/format';

/** Ventas de la ruta: desde la salida si está en ruta o liquidado; si no, las de hoy. */
export function tripLabel(truck: Pick<Truck, 'status' | 'dispatchedAt'>) {
  return (truck.status === 'En ruta' || truck.status === 'Liquidado') && truck.dispatchedAt
    ? `Facturado en la ruta (salió ${formatDateTime(truck.dispatchedAt)})`
    : 'Facturado hoy';
}

/** Tablero de flota (observación 4): estado de la ruta, ocupación, lo facturado y el detalle de la carga. */
export function TrucksPage() {
  const notify = useToast();
  const trucks = useApiQuery(() => trucksApi.list(), []);
  const [busyId, setBusyId] = useState<string | null>(null);

  async function dispatch(truck: Truck) {
    setBusyId(truck.id);
    try {
      await trucksApi.dispatch(truck.id);
      notify(`Camión ${truck.name} en ruta.`);
      trucks.reload();
    } catch (err) {
      notify(toApiError(err).message, 'error');
    } finally {
      setBusyId(null);
    }
  }

  return (
    <>
      <PageHeader title="Camiones" description="Estado de cada ruta, cuánto de su capacidad ocupa y cuánto lleva facturado." />
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
              <dl className="grid grid-cols-2 gap-2 border-t border-line pt-3 text-sm">
                <div>
                  <dt className="text-muted">{tripLabel(truck)}</dt>
                  <dd className="font-display text-xl font-semibold">{formatMoney(truck.tripSales)}</dd>
                </div>
                <div>
                  <dt className="text-muted">Cobrado al facturar</dt>
                  <dd className="font-display text-xl font-semibold">{formatMoney(truck.tripCollected)}</dd>
                </div>
                <p className="col-span-2 text-muted">
                  {formatInt(truck.tripInvoices)} facturas · carga de {formatInt(truck.totalUnits)} und, valor {formatMoney(truck.inventoryValue)}
                </p>
              </dl>
              {truck.status === 'Cargando' && (
                <Button variant="secondary" onClick={() => void dispatch(truck)} isLoading={busyId === truck.id}>
                  <Navigation className="h-4 w-4" aria-hidden />
                  Despachar a ruta
                </Button>
              )}
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
            </TruckCard>
          ))}
        </div>
      )}
    </>
  );
}
