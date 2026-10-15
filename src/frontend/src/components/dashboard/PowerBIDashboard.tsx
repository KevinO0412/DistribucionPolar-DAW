import { useMemo } from 'react';
import type { Category, DashboardResponse } from '../../api/types';
import { formatDate, formatDecimal, formatInt, formatMoney, formatMoneyShort, formatPercent } from '../../lib/format';
import { EmptyState } from '../ui/States';
import { StatusBadge } from '../ui/StatusBadge';
import { TruckCard } from '../trucks/TruckCard';
import { CategorySalesChart } from './CategorySalesChart';
import { DashboardFilters, type DashboardFilterValue } from './DashboardFilters';
import { DataTable } from './DataTable';
import { KpiTile } from './KpiTile';
import { SalesTrendChart } from './SalesTrendChart';
import { Section } from './Section';
import { StockHealthBar } from './StockHealthBar';
import { ValuationBar } from './ValuationBar';

interface Props {
  data: DashboardResponse;
  categories: Category[];
  filters: DashboardFilterValue;
  onFiltersChange: (value: DashboardFilterValue) => void;
  isRefreshing?: boolean;
}

/**
 * Dashboard KPI de analítica operativa (Fase 4). Todo sale de GET /api/reports/dashboard:
 * valorización costo frente a venta, rotación, alertas MinStock/MaxStock, ventas por día y categoría y ocupación de la flota.
 */
export function PowerBIDashboard({ data, categories, filters, onFiltersChange, isRefreshing }: Props) {
  const { sales, inventory, fleet } = data;
  const criticalCount = useMemo(
    () => data.stockAlerts.filter((a) => a.status === 'OutOfStock' || a.status === 'CriticalRisk').reduce((s, a) => s + a.count, 0),
    [data.stockAlerts],
  );
  const hasSales = sales.invoicesCount > 0 || sales.giftsCount > 0;
  const categoryName = categories.find((c) => c.id === filters.categoryId)?.name;
  const rangeText =
    data.from === data.to
      ? formatDate(data.from, { day: 'numeric', month: 'long', year: 'numeric' })
      : `${formatDate(data.from, { day: 'numeric', month: 'short' })} al ${formatDate(data.to, { day: 'numeric', month: 'short', year: 'numeric' })}`;

  return (
    <div className="flex flex-col gap-5" aria-busy={isRefreshing}>
      <DashboardFilters value={filters} categories={categories} onChange={onFiltersChange} />

      <p className="text-sm text-muted" aria-live="polite">
        {isRefreshing ? 'Actualizando…' : `Mostrando ${rangeText}${categoryName ? `, solo ${categoryName}` : ', todas las categorías'}.`}
      </p>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KpiTile
          label="Ventas del período"
          value={formatMoney(sales.salesTotal)}
          detail={`${formatInt(sales.invoicesCount)} facturas, ticket promedio ${formatMoney(sales.averageTicket)}`}
        />
        <KpiTile
          label="Rotación del inventario"
          value={`${formatDecimal(sales.rotationIndex)}×`}
          detail={`${formatInt(sales.unitsSold)} und vendidas sobre ${formatInt(inventory.stockUnits)} en existencia`}
        />
        <KpiTile
          label="Margen bruto proyectado"
          value={formatPercent(inventory.grossMarginPercent)}
          detail={`${formatMoney(inventory.grossMargin)} sobre ${formatMoneyShort(inventory.saleValue)} a precio de venta`}
        />
        <KpiTile
          label="Productos bajo el mínimo"
          value={formatInt(criticalCount)}
          tone={criticalCount > 0 ? 'alert' : 'default'}
          detail={`de ${formatInt(inventory.activeProducts)} productos activos`}
        />
      </div>

      <div className="grid grid-cols-1 gap-5 xl:grid-cols-3">
        <Section title="Ventas por día" description="Facturas de venta no anuladas, por día local." className="xl:col-span-2">
          {hasSales ? (
            <SalesTrendChart data={data.salesByDay} />
          ) : (
            <EmptyState title="Sin ventas en este período">Amplía el rango de fechas o quita el filtro de categoría.</EmptyState>
          )}
        </Section>
        <Section title="Ventas por categoría" description="Pulsa una barra para filtrar el dashboard.">
          {data.salesByCategory.length > 0 ? (
            <CategorySalesChart
              data={data.salesByCategory}
              selectedId={filters.categoryId}
              onSelect={(categoryId) => onFiltersChange({ ...filters, categoryId })}
            />
          ) : (
            <EmptyState title="Sin ventas para comparar" />
          )}
        </Section>
      </div>

      <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
        <Section title="Valorización del inventario" description="Galpón y camiones, al costo y a precio de venta.">
          <ValuationBar inventory={inventory} />
        </Section>
        <Section title="Salud del stock en galpón" description="Según el stock mínimo y máximo de cada producto.">
          <StockHealthBar alerts={data.stockAlerts} />
        </Section>
      </div>

      <Section
        title="Ocupación de la flota"
        description={`${formatDecimal(fleet.loadedBoxes)} de ${formatInt(fleet.capacityBoxes)} cajas cargadas (${formatPercent(fleet.occupancyPercent)}).`}
      >
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {fleet.trucks.map((truck) => (
            <TruckCard key={truck.truckId} {...truck}>
              <p className="text-sm text-muted">
                Vendió <span className="font-semibold text-ink">{formatMoney(truck.salesTotal)}</span> en {formatInt(truck.invoices)} facturas
              </p>
            </TruckCard>
          ))}
        </div>
      </Section>

      <div className="grid grid-cols-1 gap-5 xl:grid-cols-2">
        <Section title="Stock crítico" description="Agotados, bajo el mínimo o sobre el máximo, los más urgentes primero.">
          {data.criticalProducts.length > 0 ? (
            <DataTable
              caption="Productos con stock crítico"
              rows={data.criticalProducts}
              rowKey={(p) => p.id}
              columns={[
                { header: 'Producto', cell: (p) => <><p className="font-medium">{p.name}</p><p className="text-muted">{p.sku}</p></> },
                { header: 'Estado', cell: (p) => <StatusBadge status={p.status} /> },
                { header: 'Galpón', align: 'right', cell: (p) => <><p>{p.stockDisplay}</p><p className="text-muted">mín. {formatInt(p.minStock)} und</p></> },
                { header: 'Reponer', align: 'right', cell: (p) => (p.recommendedPurchaseUnits > 0 ? `${formatInt(p.recommendedPurchaseUnits)} und` : 'No') },
              ]}
            />
          ) : (
            <EmptyState title="Ningún producto en riesgo">Todo el catálogo filtrado está entre su mínimo y su máximo.</EmptyState>
          )}
        </Section>
        <Section title="Rotación por producto" description="Más unidades vendidas en el período frente a su existencia actual.">
          {data.topProducts.length > 0 ? (
            <DataTable
              caption="Productos con más rotación"
              rows={data.topProducts}
              rowKey={(p) => p.productId}
              columns={[
                { header: 'Producto', cell: (p) => <><p className="font-medium">{p.name}</p><p className="text-muted">{p.categoryName}</p></> },
                { header: 'Vendidas', align: 'right', cell: (p) => `${formatInt(p.unitsSold)} und` },
                { header: 'Ingresos', align: 'right', cell: (p) => formatMoney(p.revenue) },
                { header: 'Rotación', align: 'right', cell: (p) => (p.stockUnits > 0 ? `${formatDecimal(p.rotationIndex)}×` : 'Agotado') },
              ]}
            />
          ) : (
            <EmptyState title="Sin ventas en este período" />
          )}
        </Section>
      </div>
    </div>
  );
}
