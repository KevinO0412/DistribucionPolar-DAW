import { api } from './client';
import type {
  AuthResponse,
  Category,
  CategoryRequest,
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
};

export const invoicesApi = {
  list: (params: { from?: string; to?: string; type?: string; includeCancelled?: boolean; page?: number; pageSize?: number }) =>
    api.get<PagedResponse<InvoiceSummary>>('/invoices', { params }).then((r) => r.data),
  get: (id: string) => api.get<Invoice>(`/invoices/${id}`).then((r) => r.data),
};
