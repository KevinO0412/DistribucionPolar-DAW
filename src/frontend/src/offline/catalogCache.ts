import { isConnectivityError, toApiError } from '../api/client';
import { CATALOG, idb } from './db';

export interface Cached<T> {
  data: T;
  /** Fecha de la copia local si la API no respondió; null si los datos son de ahora. */
  savedAt: string | null;
}

/**
 * Consulta la API y guarda una copia en IndexedDB. Sin conexión devuelve la última copia guardada, para que la
 * preventa funcione en zonas sin cobertura. Complementa la caché del service worker (que no existe en desarrollo).
 */
export async function cachedFetch<T>(key: string, fetcher: () => Promise<T>): Promise<Cached<T>> {
  try {
    const data = await fetcher();
    await idb.put(CATALOG, { data, savedAt: new Date().toISOString() }, key).catch(() => undefined);
    return { data, savedAt: null };
  } catch (error) {
    if (!isConnectivityError(toApiError(error))) throw error;
    const saved = await idb.get<{ data: T; savedAt: string }>(CATALOG, key);
    if (!saved) throw error;
    return { data: saved.data, savedAt: saved.savedAt };
  }
}

/** Al cerrar sesión se borra la copia local del catálogo y de los clientes. */
export async function clearCatalogCache() {
  await idb.clear(CATALOG).catch(() => undefined);
  if ('caches' in window) await caches.delete('dp-catalogo').catch(() => false);
}
