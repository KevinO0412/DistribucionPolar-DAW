import type { ApiError } from '../api/client';
import type { Invoice } from '../api/types';
import type { OrderStore, PendingOrder } from './store';

export interface SyncSummary {
  synced: number;
  rejected: number;
  pending: number;
  offline: boolean;
}

/** Errores que no se arreglan reintentando: el pedido queda rechazado con el mensaje de la API (RFC 7807). */
function isPermanent(error: ApiError) {
  return error.status >= 400 && error.status < 500 && ![401, 408, 409, 429].includes(error.status);
}

function describe(error: ApiError) {
  const fields = Object.values(error.fieldErrors).flat();
  const detail = fields.length > 0 ? fields.join(' ') : error.message;
  return detail && detail !== error.title ? `${error.title}: ${detail}` : error.title;
}

/**
 * Envía los pedidos pendientes en orden. Cada pedido viaja con su propio id como Idempotency-Key, así que
 * si la respuesta se pierde y se reenvía, la API devuelve la misma factura en lugar de emitir otra.
 * - Sin conexión (status 0): se detiene y deja el resto pendiente.
 * - 400/404/422: el pedido queda rechazado con el detalle (por ejemplo, faltante de stock en el camión).
 * - 401/409/429/5xx: se reintenta en la próxima sincronización.
 */
export async function syncOrders(
  store: OrderStore,
  send: (order: PendingOrder) => Promise<Invoice>,
  now: () => Date = () => new Date(),
): Promise<SyncSummary> {
  const summary: SyncSummary = { synced: 0, rejected: 0, pending: 0, offline: false };
  const orders = (await store.all()).filter((o) => o.status === 'pending');

  for (const order of orders) {
    if (summary.offline) {
      summary.pending += 1;
      continue;
    }
    try {
      const invoice = await send(order);
      await store.put({
        ...order,
        status: 'synced',
        attempts: order.attempts + 1,
        invoiceId: invoice.id,
        invoiceNumber: invoice.number,
        error: undefined,
        syncedAt: now().toISOString(),
      });
      summary.synced += 1;
    } catch (err) {
      const error = err as ApiError;
      if (error.status === 0) {
        summary.offline = true;
        summary.pending += 1;
        continue;
      }
      if (isPermanent(error)) {
        await store.put({ ...order, status: 'rejected', attempts: order.attempts + 1, error: describe(error) });
        summary.rejected += 1;
      } else {
        await store.put({ ...order, attempts: order.attempts + 1, error: describe(error) });
        summary.pending += 1;
      }
    }
  }
  return summary;
}
