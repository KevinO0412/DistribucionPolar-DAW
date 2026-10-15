import axios, { AxiosError, type AxiosInstance } from 'axios';
import type { ProblemDetails } from './types';

/** Error de la API normalizado desde un cuerpo RFC 7807 (Problem Details). */
export class ApiError extends Error {
  readonly status: number;
  readonly title: string;
  readonly fieldErrors: Record<string, string[]>;

  constructor(status: number, title: string, detail: string | undefined, fieldErrors: Record<string, string[]> = {}) {
    super(detail || title);
    this.name = 'ApiError';
    this.status = status;
    this.title = title;
    this.fieldErrors = fieldErrors;
  }

  /** Errores por campo con la clave en camelCase ("PriceBox" → "priceBox"). */
  fieldError(field: string): string | undefined {
    const entry = Object.entries(this.fieldErrors).find(([key]) => key.toLowerCase() === field.toLowerCase());
    return entry?.[1][0];
  }
}

/**
 * Sin conexión con la API: no hay red (status 0) o el proxy no la alcanza (502, 503, 504). La preventa guarda los
 * pedidos y usa la copia local del catálogo en estos casos.
 */
export const isConnectivityError = (error: ApiError) => error.status === 0 || [502, 503, 504].includes(error.status);

const NETWORK_MESSAGE = 'No hay conexión con el servidor. Revisa tu red o que la API esté en marcha.';

/** Convierte cualquier error de Axios en ApiError, leyendo el Problem Details si viene en la respuesta. */
export function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error;
  if (error instanceof AxiosError) {
    if (!error.response) return new ApiError(0, 'Sin conexión', NETWORK_MESSAGE);
    const { status, data } = error.response;
    const problem = (typeof data === 'object' && data !== null ? data : {}) as ProblemDetails;
    const fallback =
      status === 401 ? 'Tu sesión terminó. Inicia sesión de nuevo.'
      : status === 403 ? 'Tu rol no tiene permiso para esta acción.'
      : status === 429 ? 'Demasiados intentos. Espera un momento y vuelve a intentarlo.'
      : 'El servidor no pudo completar la solicitud.';
    return new ApiError(status, problem.title ?? fallback, problem.detail ?? (problem.errors ? undefined : fallback), problem.errors ?? {});
  }
  return new ApiError(0, 'Error inesperado', error instanceof Error ? error.message : String(error));
}

type TokenProvider = () => string | null;
type UnauthorizedHandler = () => void;

let getToken: TokenProvider = () => null;
let onUnauthorized: UnauthorizedHandler = () => {};

/** AuthContext registra aquí cómo leer el token y qué hacer ante un 401. */
export function configureAuth(provider: TokenProvider, handler: UnauthorizedHandler) {
  getToken = provider;
  onUnauthorized = handler;
}

export const api: AxiosInstance = axios.create({
  baseURL: '/api',
  timeout: 20_000,
  headers: { Accept: 'application/json' },
});

// Interceptor de petición: adjunta "Authorization: Bearer <JWT>" en cada llamada.
api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// Interceptor de respuesta: normaliza los errores RFC 7807 y cierra la sesión ante un 401 (token vencido o inválido).
api.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    const apiError = toApiError(error);
    const isLogin = error instanceof AxiosError && error.config?.url?.includes('/auth/login');
    if (apiError.status === 401 && !isLogin) onUnauthorized();
    return Promise.reject(apiError);
  },
);
