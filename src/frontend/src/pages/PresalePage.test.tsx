import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ToastProvider } from '../components/ui/Toast';
import { memoryOrderStore } from '../offline/store';
import { PresalePage } from './PresalePage';

vi.mock('../api/endpoints', () => ({
  clientsApi: { list: vi.fn().mockResolvedValue([{ id: 'c1', code: '001', name: 'Bodega La Esquina', rif: 'V-1', nickname: null }]) },
  productsApi: {
    list: vi.fn().mockResolvedValue([{ id: 'p1', name: 'POLAR PILSEN RET 222ML', priceBox: 23, priceUnit: 0.75, isActive: true, stockUnits: 360 }]),
  },
  trucksApi: { list: vi.fn().mockResolvedValue([]) },
  invoicesApi: { createSale: vi.fn() },
}));

describe('PresalePage', () => {
  it('sin conexión guarda el pedido en el dispositivo como pendiente', async () => {
    Object.defineProperty(navigator, 'onLine', { configurable: true, value: false });
    const store = memoryOrderStore();
    render(
      <ToastProvider>
        <PresalePage store={store} />
      </ToastProvider>,
    );

    expect(await screen.findByText(/Sin conexión: los pedidos se guardan/)).toBeInTheDocument();
    await userEvent.selectOptions(screen.getByLabelText('Cliente'), 'c1');
    await userEvent.selectOptions(screen.getByLabelText('Producto'), 'p1');
    fireEvent.change(screen.getByLabelText('Cajas'), { target: { value: '2' } });
    await userEvent.click(screen.getByRole('button', { name: /Agregar/ }));
    await userEvent.click(screen.getByRole('button', { name: /Guardar pedido/ }));

    await waitFor(async () => expect(await store.all()).toHaveLength(1));
    const [order] = await store.all();
    expect(order).toMatchObject({ status: 'pending', clientName: 'Bodega La Esquina', estimatedTotal: 46 });
    expect(order.request.items).toEqual([{ productId: 'p1', boxes: 2, looseUnits: 0 }]);
    expect(screen.getByText('Pendiente de envío')).toBeInTheDocument();
  });
});
