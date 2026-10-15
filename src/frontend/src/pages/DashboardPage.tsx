import { useCallback, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { categoriesApi, reportsApi } from '../api/endpoints';
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
      <PageHeader title="Dashboard" description="Ventas, rotación, stock crítico y carga de los camiones." />
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
