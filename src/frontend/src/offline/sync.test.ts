import { ApiError } from '../api/client';
import type { CreateSaleRequest, Invoice } from '../api/types';
import { memoryOrderStore, type PendingOrder } from './store';
import { syncOrders } from './sync';

const request: CreateSaleRequest = {
  clientId: 'c1',
  dispatchOrigin: 'Truck',
  truckId: 't1',
  employeeId: null,
  items: [{ productId: 'p1', boxes: 2, looseUnits: 0 }],
  payment: 0,
  paymentObservation: null,
  returnedEmpties: null,
};

const order = (id: string, createdAt: string): PendingOrder => ({
  id,
  createdAt,
  clientName: 'Bodega La Esquina',
  originName: 'Camión NPR',
  estimatedTotal: 46,
  request,
  status: 'pending',
  attempts: 0,
});

const invoice = (number: string) => ({ id: `inv-${number}`, number }) as Invoice;

describe('syncOrders', () => {
  it('envía cada pedido con su id como Idempotency-Key y lo marca facturado', async () => {
    const store = memoryOrderStore([order('a', '2026-10-15T10:00:00Z'), order('b', '2026-10-15T10:05:00Z')]);
    const keys: string[] = [];

    const result = await syncOrders(store, async (o) => {
      keys.push(o.id);
      return invoice(`Factura #${keys.length}`);
    });

    expect(keys).toEqual(['a', 'b']);
    expect(result).toEqual({ synced: 2, rejected: 0, pending: 0, offline: false });
    expect((await store.all()).map((o) => [o.status, o.invoiceNumber])).toEqual([
      ['synced', 'Factura #1'],
      ['synced', 'Factura #2'],
    ]);
  });

  it('sin conexión deja todo pendiente y no sigue intentando', async () => {
    const store = memoryOrderStore([order('a', '2026-10-15T10:00:00Z'), order('b', '2026-10-15T10:05:00Z')]);
    const send = vi.fn().mockRejectedValue(new ApiError(0, 'Sin conexión', 'No hay conexión'));

    const result = await syncOrders(store, send);

    expect(send).toHaveBeenCalledTimes(1);
    expect(result).toEqual({ synced: 0, rejected: 0, pending: 2, offline: true });
    expect((await store.all()).every((o) => o.status === 'pending')).toBe(true);
  });

  it('un faltante de stock (400 RFC 7807) rechaza ese pedido con el detalle y sigue con el resto', async () => {
    const store = memoryOrderStore([order('a', '2026-10-15T10:00:00Z'), order('b', '2026-10-15T10:05:00Z')]);
    const send = vi
      .fn()
      .mockRejectedValueOnce(new ApiError(400, 'Solicitud inválida', 'Inventario insuficiente en el camión NPR: se solicitan 72 und y hay 30 und.'))
      .mockResolvedValueOnce(invoice('Factura #9'));

    const result = await syncOrders(store, send);

    expect(result).toMatchObject({ synced: 1, rejected: 1 });
    const [first, second] = await store.all();
    expect(first.status).toBe('rejected');
    expect(first.error).toBe('Solicitud inválida: Inventario insuficiente en el camión NPR: se solicitan 72 und y hay 30 und.');
    expect(second.invoiceNumber).toBe('Factura #9');
  });

  it('un error del servidor (500) deja el pedido pendiente para reintentarlo', async () => {
    const store = memoryOrderStore([order('a', '2026-10-15T10:00:00Z')]);

    const result = await syncOrders(store, vi.fn().mockRejectedValue(new ApiError(500, 'Error interno del servidor', undefined)));

    expect(result.pending).toBe(1);
    const [saved] = await store.all();
    expect(saved).toMatchObject({ status: 'pending', attempts: 1 });
  });

  it('no reenvía pedidos ya facturados', async () => {
    const store = memoryOrderStore([{ ...order('a', '2026-10-15T10:00:00Z'), status: 'synced', invoiceNumber: 'Factura #1' }]);
    const send = vi.fn();

    await syncOrders(store, send);

    expect(send).not.toHaveBeenCalled();
  });
});
