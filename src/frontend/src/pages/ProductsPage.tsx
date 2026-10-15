import { useState } from 'react';
import { Pencil, Plus, Search, Trash2 } from 'lucide-react';
import { categoriesApi, productsApi } from '../api/endpoints';
import type { CreateProductRequest, Product, ProductFields } from '../api/types';
import { DataTable } from '../components/dashboard/DataTable';
import { PageHeader } from '../components/layout/PageHeader';
import { Button } from '../components/ui/Button';
import { ProductThumb } from '../components/ui/ProductThumb';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { StatusBadge, stockStatus } from '../components/ui/StatusBadge';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { useAuth } from '../hooks/useAuth';
import { useDebounced } from '../hooks/useDebounced';
import { formatInt, formatMoney } from '../lib/format';
import { ConfirmDialog } from './ConfirmDialog';
import { ProductForm } from './ProductForm';

type Editing = { mode: 'create' } | { mode: 'edit'; product: Product } | null;

/**
 * Catálogo. RBAC en la interfaz, igual que en la API:
 * Admin y Employee consultan y registran productos; solo Admin edita y elimina.
 */
export function ProductsPage() {
  const { isAdmin } = useAuth();
  const notify = useToast();
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const debouncedSearch = useDebounced(search.trim());
  const [editing, setEditing] = useState<Editing>(null);
  const [deleting, setDeleting] = useState<Product | null>(null);

  const categories = useApiQuery(() => categoriesApi.list(), []);
  const products = useApiQuery(() => productsApi.list({ categoryId, search: debouncedSearch }), [categoryId, debouncedSearch]);

  async function save(body: CreateProductRequest | ProductFields) {
    if (editing?.mode === 'edit') {
      await productsApi.update(editing.product.id, body);
      notify(`Producto ${body.name} actualizado.`);
    } else {
      const created = await productsApi.create(body as CreateProductRequest);
      notify(`Producto creado con el SKU ${created.sku}.`);
    }
    setEditing(null);
    products.reload();
  }

  async function remove() {
    if (!deleting) return;
    await productsApi.remove(deleting.id);
    notify(`Producto ${deleting.name} eliminado.`);
    setDeleting(null);
    products.reload();
  }

  const list = products.data ?? [];

  return (
    <>
      <PageHeader
        title="Productos"
        description="Existencia en galpón, precios por caja y por unidad, y umbrales de stock."
        actions={
          <Button onClick={() => setEditing({ mode: 'create' })} disabled={!categories.data?.length}>
            <Plus className="h-4 w-4" aria-hidden />
            Nuevo producto
          </Button>
        }
      />

      <div className="panel mb-4 flex flex-wrap items-end gap-3 p-4">
        <div className="flex min-w-[14rem] flex-1 flex-col gap-1.5">
          <label htmlFor="product-search" className="text-sm font-medium">
            Buscar
          </label>
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted" aria-hidden />
            <input
              id="product-search"
              type="search"
              placeholder="Nombre, marca o SKU"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="h-11 w-full rounded-md border border-line bg-surface pl-9 pr-3"
            />
          </div>
        </div>
        <div className="flex min-w-[12rem] flex-col gap-1.5">
          <label htmlFor="product-category" className="text-sm font-medium">
            Categoría
          </label>
          <select id="product-category" value={categoryId} onChange={(e) => setCategoryId(e.target.value)} className="h-11 rounded-md border border-line bg-surface px-3">
            <option value="">Todas</option>
            {categories.data?.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
        </div>
        <p className="ml-auto self-center text-sm text-muted" aria-live="polite">
          {products.isLoading ? 'Buscando…' : `${formatInt(list.length)} productos`}
        </p>
      </div>

      {products.error ? (
        <ErrorState error={products.error} onRetry={products.reload} />
      ) : products.isLoading && !products.data ? (
        <LoadingState />
      ) : list.length === 0 ? (
        <EmptyState title={debouncedSearch || categoryId ? 'Ningún producto coincide' : 'Aún no hay productos'}>
          {debouncedSearch || categoryId ? 'Prueba con otro texto o quita el filtro de categoría.' : 'Crea el primero con el botón Nuevo producto.'}
        </EmptyState>
      ) : (
        <div className="panel p-4 sm:p-2">
          <DataTable
            caption="Catálogo de productos"
            rows={list}
            rowKey={(p) => p.id}
            columns={[
              {
                header: 'Producto',
                cell: (p) => (
                  <div className="flex items-center gap-3">
                    <ProductThumb productId={p.id} name={p.name} />
                    <div>
                      <p className="font-medium">{p.name}</p>
                      <p className="text-muted">{p.sku}, {p.brand}</p>
                    </div>
                  </div>
                ),
              },
              { header: 'Categoría', cell: (p) => <><p>{p.categoryName}</p><p className="text-muted">{p.deposit}</p></> },
              { header: 'Caja', align: 'right', cell: (p) => <><p>{formatMoney(p.priceBox)}</p><p className="text-muted">x{p.unitsPerBox}</p></> },
              { header: 'Unidad', align: 'right', cell: (p) => formatMoney(p.priceUnit) },
              { header: 'Galpón', align: 'right', cell: (p) => <><p>{p.stockDisplay}</p><p className="text-muted">{formatInt(p.stockUnits)} und</p></> },
              { header: 'Estado', cell: (p) => <StatusBadge status={stockStatus(p.stockUnits, p.minStock, p.maxStock)} /> },
              ...(isAdmin
                ? [{
                    header: 'Acciones',
                    align: 'right' as const,
                    cell: (p: Product) => (
                      <div className="flex justify-end gap-1">
                        <Button variant="ghost" size="sm" onClick={() => setEditing({ mode: 'edit', product: p })} aria-label={`Editar ${p.name}`}>
                          <Pencil className="h-4 w-4" aria-hidden />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => setDeleting(p)} aria-label={`Eliminar ${p.name}`} className="text-status-out">
                          <Trash2 className="h-4 w-4" aria-hidden />
                        </Button>
                      </div>
                    ),
                  }]
                : []),
            ]}
          />
        </div>
      )}

      {editing && (
        <ProductForm
          key={editing.mode === 'edit' ? editing.product.id : 'new'}
          open
          product={editing.mode === 'edit' ? editing.product : null}
          categories={categories.data ?? []}
          onClose={() => setEditing(null)}
          onSubmit={save}
        />
      )}
      <ConfirmDialog
        open={deleting !== null}
        title="Eliminar producto"
        message={`${deleting?.name ?? ''} dejará de aparecer en el catálogo. Las facturas anteriores lo conservan.`}
        confirmLabel="Eliminar producto"
        onClose={() => setDeleting(null)}
        onConfirm={remove}
      />
    </>
  );
}
