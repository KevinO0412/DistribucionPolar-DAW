import { isConnectivityError, toApiError } from '../api/client';
import type { CreateSaleRequest, Invoice } from '../api/types';
import { ORDERS, idb } from './db';

/** Pedido de preventa guardado en el dispositivo hasta que la API lo acepta. Su id es la clave de idempotencia. */
export interface PendingOrder {
  id: string;
  createdAt: string;
  request: CreateSaleRequest;
  summary: { clientName: string; origin: string; total: number; lines: number };
  status: 'pending' | 'rejected';
  error: string | null;
  attempts: number;
}

export type SendSale = (request: CreateSaleRequest, idempotencyKey: string) => Promise<Invoice>;

export interface SyncResult {
  sent: { order: PendingOrder; invoice: Invoice }[];
  rejected: PendingOrder[];
  /** Se detuvo porque no hay conexión con la API (los pedidos siguen pendientes). */
  offline: boolean;
}

export function newOrderId(): string {
  return crypto.randomUUID();
}

export async function enqueueOrder(order: Omit<PendingOrder, 'status' | 'error' | 'attempts'>): Promise<PendingOrder> {
  const pending: PendingOrder = { ...order, status: 'pending', error: null, attempts: 0 };
  await idb.put(ORDERS, pending);
  return pending;
}

export async function listOrders(): Promise<PendingOrder[]> {
  const orders = await idb.getAll<PendingOrder>(ORDERS);
  return orders.sort((a, b) => a.createdAt.localeCompare(b.createdAt));
}

export const discardOrder = (id: string) => idb.delete(ORDERS, id);

/** Vuelve a dejar pendiente un pedido rechazado (por ejemplo, después de cargar más mercancía al camión). */
export async function retryOrder(id: string) {
  const order = await idb.get<PendingOrder>(ORDERS, id);
  if (order) await idb.put(ORDERS, { ...order, status: 'pending', error: null });
}

let syncing: Promise<SyncResult> | null = null;

/**
 * Envía los pedidos pendientes en orden, uno a uno, con su id como Idempotency-Key: si un envío llegó a la API pero
 * la respuesta se perdió, el reintento devuelve la misma factura en lugar de duplicarla.
 * - Sin conexión (status 0), 5xx o 409: se detiene y el pedido sigue pendiente.
 * - 4xx (stock insuficiente en el camión, crédito bloqueado, datos inválidos): queda "rechazado" con el mensaje
 *   RFC 7807 de la API para que el vendedor lo corrija o lo descarte.
 * Solo corre una sincronización a la vez.
 */
export function syncOrders(send: SendSale): Promise<SyncResult> {
  syncing ??= (async () => {
    const result: SyncResult = { sent: [], rejected: [], offline: false };
    try {
      for (const order of (await listOrders()).filter((o) => o.status === 'pending')) {
        try {
          const invoice = await send(order.request, order.id);
          await idb.delete(ORDERS, order.id);
          result.sent.push({ order, invoice });
        } catch (error) {
          const apiError = toApiError(error);
          const retryLater = apiError.status === 0 || apiError.status >= 500 || apiError.status === 409 || apiError.status === 401;
          if (retryLater) {
            await idb.put(ORDERS, { ...order, attempts: order.attempts + 1 });
            result.offline = isConnectivityError(apiError);
            break;
          }

          const rejected: PendingOrder = { ...order, status: 'rejected', error: apiError.message, attempts: order.attempts + 1 };
          await idb.put(ORDERS, rejected);
          result.rejected.push(rejected);
        }
      }
    } finally {
      syncing = null;
    }
    return result;
  })();
  return syncing;
}
