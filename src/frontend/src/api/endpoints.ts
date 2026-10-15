import { api } from './client';
import type {
  AuthResponse,
  Category,
  CategoryRequest,
  Client,
  CreateSaleRequest,
  LineRequest,
  RouteSettlement,
  CreateProductRequest,
  DashboardResponse,
  Invoice,
  InvoiceSummary,
  PagedResponse,
  Product,
  Truck,
  UpdateProductRequest,
} from './types';

export const authApi = {
  login: (username: string, password: string) =>
    api.post<AuthResponse>('/auth/login', { username, password }).then((r) => r.data),
};

export const categoriesApi = {
  list: () => api.get<Category[]>('/categories').then((r) => r.data),
  create: (body: CategoryRequest) => api.post<Category>('/categories', body).then((r) => r.data),
  update: (id: string, body: CategoryRequest) => api.put<Category>(`/categories/${id}`, body).then((r) => r.data),
  remove: (id: string) => api.delete(`/categories/${id}`),
};

export const productsApi = {
  list: (params: { categoryId?: string; search?: string } = {}) =>
    api.get<Product[]>('/products', { params: { categoryId: params.categoryId || undefined, search: params.search || undefined } }).then((r) => r.data),
  create: (body: CreateProductRequest) => api.post<Product>('/products', body).then((r) => r.data),
  update: (id: string, body: UpdateProductRequest) => api.put<Product>(`/products/${id}`, body).then((r) => r.data),
  remove: (id: string) => api.delete(`/products/${id}`),
};

export const reportsApi = {
  dashboard: (params: { from?: string; to?: string; categoryId?: string }, signal?: AbortSignal) =>
    api
      .get<DashboardResponse>('/reports/dashboard', { params: { ...params, categoryId: params.categoryId || undefined }, signal })
      .then((r) => r.data),
};

export const trucksApi = {
  list: () => api.get<Truck[]>('/trucks').then((r) => r.data),
  dispatch: (id: string) => api.post<Truck>(`/trucks/${id}/dispatch`).then((r) => r.data),
  /** Liquidación ciega: solo se envía lo declarado; la planilla con lo esperado llega en la respuesta. */
  settle: (id: string, body: { countedCash: number; returned: LineRequest[]; observation: string | null }) =>
    api.post<RouteSettlement>(`/trucks/${id}/settlement`, body).then((r) => r.data),
  settlements: (truckId?: string) =>
    api.get<RouteSettlement[]>('/trucks/settlements', { params: { truckId: truckId || undefined } }).then((r) => r.data),
};

export const clientsApi = {
  list: (search?: string) => api.get<Client[]>('/clients', { params: { search: search || undefined } }).then((r) => r.data),
  registerPayment: (id: string, amount: number, observation: string | null) =>
    api.post<Client>(`/clients/${id}/payments`, { amount, observation }).then((r) => r.data),
  returnEmpties: (id: string, groups: { groupKey: string; boxes: number; units: number }[], observation: string | null) =>
    api.post<Client>(`/clients/${id}/empties-returns`, { groups, observation }).then((r) => r.data),
  setCreditLimit: (id: string, creditLimit: number | null) =>
    api.put<Client>(`/clients/${id}/credit-limit`, { creditLimit }).then((r) => r.data),
};

export const invoicesApi = {
  list: (params: { from?: string; to?: string; type?: string; includeCancelled?: boolean; page?: number; pageSize?: number }) =>
    api.get<PagedResponse<InvoiceSummary>>('/invoices', { params }).then((r) => r.data),
  get: (id: string) => api.get<Invoice>(`/invoices/${id}`).then((r) => r.data),
  /** Venta idempotente: reenviar con la misma clave devuelve la factura ya emitida en lugar de duplicarla. */
  createSale: (body: CreateSaleRequest, idempotencyKey: string) =>
    api.post<Invoice>('/invoices/sales', body, { headers: { 'Idempotency-Key': idempotencyKey } }).then((r) => r.data),
};

/** Respaldo (solo Admin): descarga el archivo que genera la API. */
export async function downloadBackup(kind: 'json' | 'products' | 'clients' | 'invoices' | 'movements') {
  const url = kind === 'json' ? '/reports/backup/json' : `/reports/backup/csv/${kind}`;
  const response = await api.get<Blob>(url, { responseType: 'blob' });
  const disposition = String(response.headers['content-disposition'] ?? '');
  const name = /filename="?([^";]+)"?/.exec(disposition)?.[1] ?? `respaldo-${kind}.${kind === 'json' ? 'json' : 'csv'}`;
  const link = document.createElement('a');
  link.href = URL.createObjectURL(response.data);
  link.download = name;
  link.click();
  URL.revokeObjectURL(link.href);
}
