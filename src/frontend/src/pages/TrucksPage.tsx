import { trucksApi } from '../api/endpoints';
import { PageHeader } from '../components/layout/PageHeader';
import { TruckCard } from '../components/trucks/TruckCard';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useApiQuery } from '../hooks/useApiQuery';
import { formatInt, formatMoney } from '../lib/format';

/** Tablero de flota: cada camión con su carga, su valor y el detalle de lo que lleva. */
export function TrucksPage() {
  const trucks = useApiQuery(() => trucksApi.list(), []);

  return (
    <>
      <PageHeader title="Camiones" description="Qué lleva cada camión y cuánto de su capacidad ocupa." />
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
            </TruckCard>
          ))}
        </div>
      )}
    </>
  );
}
