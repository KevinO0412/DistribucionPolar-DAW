import 'fake-indexeddb/auto';
import { AxiosError, AxiosHeaders } from 'axios';
import type { CreateSaleRequest, Invoice } from '../api/types';
import { resetDbForTests } from './db';
import { discardOrder, enqueueOrder, listOrders, retryOrder, syncOrders } from './queue';

const request: CreateSaleRequest = {
  clientId: 'c1', dispatchOrigin: 'Truck', truckId: 't1', employeeId: null,
  items: [{ productId: 'p1', boxes: 1, looseUnits: 0 }], payment: 0, paymentObservation: null, returnedEmpties: null,
};

const order = (id: string, createdAt = '2026-10-15T10:00:00Z') =>
  enqueueOrder({ id, createdAt, request, summary: { clientName: 'Bodega', origin: 'Camión FVR', total: 23, lines: 1 } });

function httpError(status: number | null, data?: unknown) {
  const config = { headers: new AxiosHeaders(), url: '/invoices/sales' };
  return new AxiosError('fallo', 'ERR', config, null, status === null ? undefined : { status, data, statusText: '', headers: {}, config });
}

const invoice = (number: string) => ({ id: number, number }) as Invoice;

beforeEach(async () => {
  await resetDbForTests();
  indexedDB.deleteDatabase('dp-preventa');
});

describe('cola de preventa', () => {
  it('guarda los pedidos en IndexedDB en el orden en que se registraron', async () => {
    await order('b', '2026-10-15T11:00:00Z');
    await order('a', '2026-10-15T09:00:00Z');

    expect((await listOrders()).map((o) => o.id)).toEqual(['a', 'b']);
  });

  it('envía cada pedido con su id como Idempotency-Key y lo borra al confirmarse', async () => {
    await order('pedido-1');
    const send = vi.fn().mockResolvedValue(invoice('Factura Octubre 2026 #4'));

    const result = await syncOrders(send);

    expect(send).toHaveBeenCalledWith(request, 'pedido-1');
    expect(result.sent.map((s) => s.invoice.number)).toEqual(['Factura Octubre 2026 #4']);
    expect(await listOrders()).toEqual([]);
  });

  it('sin conexión se detiene y deja los pedidos pendientes para reintentar', async () => {
    await order('pedido-1');
    await order('pedido-2', '2026-10-15T11:00:00Z');
    const send = vi.fn().mockRejectedValue(httpError(null));

    const result = await syncOrders(send);

    expect(result.offline).toBe(true);
    expect(send).toHaveBeenCalledTimes(1);
    const pending = await listOrders();
    expect(pending.map((o) => [o.status, o.attempts])).toEqual([['pending', 1], ['pending', 0]]);
  });

  it('marca como rechazado el pedido que la API rechaza (RFC 7807) y sigue con los demás', async () => {
    await order('sin-stock');
    await order('bueno', '2026-10-15T11:00:00Z');
    const send = vi.fn()
      .mockRejectedValueOnce(httpError(400, { title: 'Solicitud inválida', detail: 'Inventario insuficiente en el camión FVR.' }))
      .mockResolvedValueOnce(invoice('Factura Octubre 2026 #5'));

    const result = await syncOrders(send);

    expect(result.rejected[0].error).toBe('Inventario insuficiente en el camión FVR.');
    expect(result.sent).toHaveLength(1);
    expect((await listOrders()).map((o) => [o.id, o.status])).toEqual([['sin-stock', 'rejected']]);
  });

  it('un pedido rechazado se puede reintentar o descartar', async () => {
    await order('x');
    await syncOrders(vi.fn().mockRejectedValue(httpError(400, { detail: 'Crédito bloqueado' })));

    await retryOrder('x');
    expect((await listOrders())[0].status).toBe('pending');

    await discardOrder('x');
    expect(await listOrders()).toEqual([]);
  });

  it('un error del servidor no descarta el pedido; si el proxy no alcanza la API cuenta como sin conexión', async () => {
    await order('x');

    const down = await syncOrders(vi.fn().mockRejectedValue(httpError(502, '')));
    const failing = await syncOrders(vi.fn().mockRejectedValue(httpError(500, { title: 'Error interno' })));

    expect([down.offline, failing.offline]).toEqual([true, false]);
    expect(failing.rejected).toEqual([]);
    expect((await listOrders())[0].status).toBe('pending');
  });
});
