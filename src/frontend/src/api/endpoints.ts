import { api } from './client';
import type {
  AuthResponse,
  Category,
  CategoryRequest,
  Client,
  CreateSaleRequest,
  CreditStatus,
  CreditTermsRequest,
  CreateProductRequest,
  DashboardResponse,
  Invoice,
  InvoiceSummary,
  PagedResponse,
  Product,
  SettlementForm,
  SettlementRequest,
  Truck,
  TruckSettlement,
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
  settlementForm: (id: string) => api.get<SettlementForm>(`/trucks/${id}/settlement-form`).then((r) => r.data),
  settle: (id: string, body: SettlementRequest) => api.post<TruckSettlement>(`/trucks/${id}/settlements`, body).then((r) => r.data),
  settlements: (id: string) => api.get<TruckSettlement[]>(`/trucks/${id}/settlements`).then((r) => r.data),
};

export const invoicesApi = {
  list: (params: { from?: string; to?: string; type?: string; includeCancelled?: boolean; page?: number; pageSize?: number }) =>
    api.get<PagedResponse<InvoiceSummary>>('/invoices', { params }).then((r) => r.data),
  get: (id: string) => api.get<Invoice>(`/invoices/${id}`).then((r) => r.data),
  /** Venta idempotente: la misma clave reenviada devuelve la factura ya emitida en lugar de crear otra. */
  createSale: (body: CreateSaleRequest, idempotencyKey: string) =>
    api.post<Invoice>('/invoices/sales', body, { headers: { 'Idempotency-Key': idempotencyKey } }).then((r) => r.data),
};

export const clientsApi = {
  list: (search?: string) => api.get<Client[]>('/clients', { params: { search: search || undefined } }).then((r) => r.data),
  credit: (id: string, signal?: AbortSignal) => api.get<CreditStatus>(`/clients/${id}/credit`, { signal }).then((r) => r.data),
  setCredit: (id: string, body: CreditTermsRequest) => api.put<CreditStatus>(`/clients/${id}/credit`, body).then((r) => r.data),
  /** Abono fuera de factura; devuelve el cliente con el saldo nuevo. */
  pay: (id: string, body: { amount: number; observation: string | null }) => api.post<Client>(`/clients/${id}/payments`, body).then((r) => r.data),
};
