// IndexedDB de la preventa sin conexión: pedidos pendientes y una copia del catálogo para trabajar sin red.
// Envoltorio mínimo sobre la API nativa (sin librerías).

const DB_NAME = 'dp-preventa';
const DB_VERSION = 1;
export const ORDERS = 'orders';
export const CATALOG = 'catalog';

let opening: Promise<IDBDatabase> | null = null;

function openDb(): Promise<IDBDatabase> {
  opening ??= new Promise((resolve, reject) => {
    const request = indexedDB.open(DB_NAME, DB_VERSION);
    request.onupgradeneeded = () => {
      const db = request.result;
      if (!db.objectStoreNames.contains(ORDERS)) db.createObjectStore(ORDERS, { keyPath: 'id' });
      if (!db.objectStoreNames.contains(CATALOG)) db.createObjectStore(CATALOG);
    };
    request.onsuccess = () => resolve(request.result);
    request.onerror = () => {
      opening = null;
      reject(request.error);
    };
  });
  return opening;
}

function run<T>(store: string, mode: IDBTransactionMode, action: (s: IDBObjectStore) => IDBRequest<T>): Promise<T> {
  return openDb().then(
    (db) =>
      new Promise<T>((resolve, reject) => {
        const tx = db.transaction(store, mode);
        const request = action(tx.objectStore(store));
        tx.oncomplete = () => resolve(request.result);
        tx.onerror = () => reject(tx.error ?? request.error);
        tx.onabort = () => reject(tx.error ?? request.error);
      }),
  );
}

export const idb = {
  getAll: <T>(store: string) => run<T[]>(store, 'readonly', (s) => s.getAll() as IDBRequest<T[]>),
  get: <T>(store: string, key: IDBValidKey) => run<T | undefined>(store, 'readonly', (s) => s.get(key) as IDBRequest<T | undefined>),
  put: <T>(store: string, value: T, key?: IDBValidKey) => run(store, 'readwrite', (s) => s.put(value, key)),
  delete: (store: string, key: IDBValidKey) => run(store, 'readwrite', (s) => s.delete(key)),
  clear: (store: string) => run(store, 'readwrite', (s) => s.clear()),
};

/** Solo para pruebas: cierra la conexión para poder reiniciar la base entre casos. */
export async function resetDbForTests() {
  if (opening) (await opening).close();
  opening = null;
}
