import { useCallback, useMemo, useState } from 'react';
import { Download } from 'lucide-react';
import { toApiError } from '../api/client';
import { useSearchParams } from 'react-router-dom';
import { categoriesApi, downloadBackup, reportsApi } from '../api/endpoints';
import { Button } from '../components/ui/Button';
import { useToast } from '../components/ui/Toast';
import { PowerBIDashboard } from '../components/dashboard/PowerBIDashboard';
import { presetRange, type DashboardFilterValue } from '../components/dashboard/DashboardFilters';
import { PageHeader } from '../components/layout/PageHeader';
import { ErrorState, LoadingState } from '../components/ui/States';
import { useApiQuery } from '../hooks/useApiQuery';

/** Los filtros viven en la URL (?from=&to=&categoryId=) para poder compartir o recargar la misma vista. */
export function DashboardPage() {
  const [params, setParams] = useSearchParams();
  const filters = useMemo<DashboardFilterValue>(() => {
    const fallback = presetRange(30);
    return { from: params.get('from') ?? fallback.from, to: params.get('to') ?? fallback.to, categoryId: params.get('categoryId') ?? '' };
  }, [params]);

  const setFilters = useCallback(
    (next: DashboardFilterValue) => {
      const search = new URLSearchParams({ from: next.from, to: next.to });
      if (next.categoryId) search.set('categoryId', next.categoryId);
      setParams(search, { replace: true });
    },
    [setParams],
  );

  const categories = useApiQuery(() => categoriesApi.list(), []);
  const dashboard = useApiQuery(
    (signal) => reportsApi.dashboard({ from: filters.from, to: filters.to, categoryId: filters.categoryId }, signal),
    [filters.from, filters.to, filters.categoryId],
  );

  return (
    <>
      <PageHeader title="Dashboard" description="Ventas, rotación, stock crítico, deuda y carga de los camiones." actions={<BackupMenu />} />
      {dashboard.error ? (
        <ErrorState error={dashboard.error} onRetry={dashboard.reload} />
      ) : !dashboard.data ? (
        <LoadingState label="Calculando indicadores…" />
      ) : (
        <PowerBIDashboard
          data={dashboard.data}
          categories={categories.data ?? []}
          filters={filters}
          onFiltersChange={setFilters}
          isRefreshing={dashboard.isLoading}
        />
      )}
    </>
  );
}

const BACKUPS = [
  { kind: 'json', label: 'Respaldo completo (JSON)' },
  { kind: 'products', label: 'Productos (CSV)' },
  { kind: 'clients', label: 'Clientes (CSV)' },
  { kind: 'invoices', label: 'Facturas (CSV)' },
  { kind: 'movements', label: 'Movimientos (CSV)' },
] as const;

/** Descarga de los respaldos que genera la API (solo Admin, igual que el dashboard). */
function BackupMenu() {
  const notify = useToast();
  const [busy, setBusy] = useState<string | null>(null);

  async function download(kind: (typeof BACKUPS)[number]['kind']) {
    setBusy(kind);
    try {
      await downloadBackup(kind);
    } catch (err) {
      notify(toApiError(err).message, 'error');
    } finally {
      setBusy(null);
    }
  }

  return (
    <details className="relative">
      <summary className="flex h-11 cursor-pointer list-none items-center gap-2 rounded-md border border-line bg-surface px-4 font-semibold hover:bg-sunken">
        <Download className="h-4 w-4" aria-hidden />
        Descargar respaldo
      </summary>
      <div className="absolute right-0 z-20 mt-1 flex w-64 flex-col rounded-md border border-line bg-surface p-1 shadow-lg">
        {BACKUPS.map((b) => (
          <Button key={b.kind} variant="ghost" size="sm" className="justify-start" isLoading={busy === b.kind} onClick={() => void download(b.kind)}>
            {b.label}
          </Button>
        ))}
      </div>
    </details>
  );
}

