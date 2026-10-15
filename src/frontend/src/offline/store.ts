import type { CreateSaleRequest } from '../api/types';

/** Pedido de preventa guardado en el dispositivo hasta que la API lo confirma. */
export interface PendingOrder {
  /** Identificador del pedido; también es la clave Idempotency-Key al enviarlo. */
  id: string;
  createdAt: string;
  clientName: string;
  originName: string;
  estimatedTotal: number;
  request: CreateSaleRequest;
  status: 'pending' | 'synced' | 'rejected';
  attempts: number;
  invoiceId?: string;
  invoiceNumber?: string;
  error?: string;
  syncedAt?: string;
}

export interface OrderStore {
  all(): Promise<PendingOrder[]>;
  put(order: PendingOrder): Promise<void>;
  remove(id: string): Promise<void>;
}

const DB_NAME = 'distribucion-polar';
const DB_VERSION = 1;
const ORDERS = 'orders';
const CACHE = 'cache';

let dbPromise: Promise<IDBDatabase> | null = null;

function openDb(): Promise<IDBDatabase> {
  dbPromise ??= new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);
    request.onupgradeneeded = () => {
      const db = request.result;
      if (!db.objectStoreNames.contains(ORDERS)) db.createObjectStore(ORDERS, { keyPath: 'id' });
      if (!db.objectStoreNames.contains(CACHE)) db.createObjectStore(CACHE);
    };
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => {
      dbPromise = null;
      reject(request.error);
    };
  });
  return dbPromise;
}

async function run<T>(storeName: string, mode: IDBTransactionMode, action: (store: IDBObjectStore) => IDBRequest<T>): Promise<T> {
  const db = await openDb();
  return new Promise<T>((resolve, reject) => {
    const tx = db.transaction(storeName, mode);
    const request = action(tx.objectStore(storeName));
    tx.oncomplete = () => resolve(request.result);
    tx.onerror = () => reject(tx.error);
    tx.onabort = () => reject(tx.error);
  });
}

/** Pedidos en IndexedDB: sobreviven a recargas y a cerrar el navegador. */
export const idbOrderStore: OrderStore = {
  all: async () => {
    const orders = await run<PendingOrder[]>(ORDERS, 'readonly', (s) => s.getAll() as IDBRequest<PendingOrder[]>);
    return orders.sort((a, b) => a.createdAt.localeCompare(b.createdAt));
  },
  put: async (order) => {
    await run(ORDERS, 'readwrite', (s) => s.put(order));
  },
  remove: async (id) => {
    await run(ORDERS, 'readwrite', (s) => s.delete(id));
  },
};

/** Almacén en memoria con la misma interfaz (pruebas y navegadores sin IndexedDB). */
export function memoryOrderStore(initial: PendingOrder[] = []): OrderStore {
  const data = new Map(initial.map((o) => [o.id, o]));
  return {
    all: async () => [...data.values()].sort((a, b) => a.createdAt.localeCompare(b.createdAt)),
    put: async (order) => {
      data.set(order.id, { ...order });
    },
    remove: async (id) => {
      data.delete(id);
    },
  };
}

/** Copia local del catálogo, clientes y camiones para preparar pedidos sin conexión. */
export async function cacheSet<T>(key: string, value: T): Promise<void> {
  try {
    await run(CACHE, 'readwrite', (s) => s.put({ value, savedAt: new Date().toISOString() }, key));
  } catch {
    // Sin IndexedDB (modo privado estricto): la preventa sigue funcionando en línea.
  }
}

export async function cacheGet<T>(key: string): Promise<{ value: T; savedAt: string } | undefined> {
  try {
    return await run<{ value: T; savedAt: string } | undefined>(CACHE, 'readonly', (s) => s.get(key));
  } catch {
    return undefined;
  }
}

/** UUID v4; usa crypto.randomUUID cuando existe (solo en contextos seguros: https o localhost). */
export function newOrderId(): string {
  if (typeof crypto.randomUUID === 'function') return crypto.randomUUID();
  const bytes = crypto.getRandomValues(new Uint8Array(16));
  bytes[6] = (bytes[6] & 0x0f) | 0x40;
  bytes[8] = (bytes[8] & 0x3f) | 0x80;
  const hex = [...bytes].map((b) => b.toString(16).padStart(2, '0')).join('');
  return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
}
