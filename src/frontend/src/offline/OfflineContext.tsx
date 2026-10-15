import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState, type ReactNode } from 'react';
import { invoicesApi } from '../api/endpoints';
import { useAuth } from '../hooks/useAuth';
import { enqueueOrder, listOrders, syncOrders, type PendingOrder, type SendSale, type SyncResult } from './queue';

const RETRY_MS = 30_000;

interface OfflineContextValue {
  /** El navegador tiene red y la última llamada a la API no falló por conexión. */
  isOnline: boolean;
  orders: PendingOrder[];
  pendingCount: number;
  rejectedCount: number;
  isSyncing: boolean;
  lastResult: SyncResult | null;
  enqueue: (order: Omit<PendingOrder, 'status' | 'error' | 'attempts'>) => Promise<SyncResult | null>;
  sync: () => Promise<SyncResult | null>;
  refresh: () => Promise<void>;
}

const OfflineContext = createContext<OfflineContextValue | null>(null);

/**
 * Preventa sin conexión: guarda los pedidos en IndexedDB y los envía a la API en cuanto hay conexión
 * (evento "online", al iniciar sesión y cada 30 s mientras queden pendientes).
 */
export function OfflineProvider({ children, send = invoicesApi.createSale }: { children: ReactNode; send?: SendSale }) {
  const { user } = useAuth();
  const [browserOnline, setBrowserOnline] = useState(() => navigator.onLine);
  const [apiReachable, setApiReachable] = useState(true);
  const [orders, setOrders] = useState<PendingOrder[]>([]);
  const [isSyncing, setIsSyncing] = useState(false);
  const [lastResult, setLastResult] = useState<SyncResult | null>(null);
  const sendRef = useRef(send);
  sendRef.current = send;

  const refresh = useCallback(async () => {
    setOrders(await listOrders().catch(() => []));
  }, []);

  const sync = useCallback(async () => {
    if (!user) return null;
    setIsSyncing(true);
    try {
      const result = await syncOrders(sendRef.current);
      setApiReachable(!result.offline);
      setLastResult(result);
      return result;
    } finally {
      setIsSyncing(false);
      await refresh();
    }
  }, [user, refresh]);

  const enqueue = useCallback(
    async (order: Omit<PendingOrder, 'status' | 'error' | 'attempts'>) => {
      await enqueueOrder(order);
      await refresh();
      return navigator.onLine ? sync() : null;
    },
    [refresh, sync],
  );

  useEffect(() => {
    const goOnline = () => {
      setBrowserOnline(true);
      setApiReachable(true);
      void sync();
    };
    const goOffline = () => setBrowserOnline(false);
    window.addEventListener('online', goOnline);
    window.addEventListener('offline', goOffline);
    return () => {
      window.removeEventListener('online', goOnline);
      window.removeEventListener('offline', goOffline);
    };
  }, [sync]);

  // Al iniciar sesión se envía lo que haya quedado pendiente.
  useEffect(() => {
    void refresh().then(() => (user ? sync() : null));
  }, [user, refresh, sync]);

  const pendingCount = orders.filter((o) => o.status === 'pending').length;
  const rejectedCount = orders.length - pendingCount;

  useEffect(() => {
    if (!user || pendingCount === 0) return;
    const timer = window.setInterval(() => void sync(), RETRY_MS);
    return () => window.clearInterval(timer);
  }, [user, pendingCount, sync]);

  const value = useMemo<OfflineContextValue>(
    () => ({
      isOnline: browserOnline && apiReachable,
      orders,
      pendingCount,
      rejectedCount,
      isSyncing,
      lastResult,
      enqueue,
      sync,
      refresh,
    }),
    [browserOnline, apiReachable, orders, pendingCount, rejectedCount, isSyncing, lastResult, enqueue, sync, refresh],
  );

  return <OfflineContext.Provider value={value}>{children}</OfflineContext.Provider>;
}

export function useOffline() {
  const ctx = useContext(OfflineContext);
  if (!ctx) throw new Error('useOffline debe usarse dentro de <OfflineProvider>.');
  return ctx;
}
