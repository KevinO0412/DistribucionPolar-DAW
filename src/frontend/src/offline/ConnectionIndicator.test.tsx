import 'fake-indexeddb/auto';
import { act, render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { AuthProvider } from '../context/AuthContext';
import { adminToken } from '../test/jwt';
import { ConnectionIndicator } from './ConnectionIndicator';
import { resetDbForTests } from './db';
import { OfflineProvider } from './OfflineContext';
import { enqueueOrder } from './queue';

function setOnline(value: boolean) {
  Object.defineProperty(window.navigator, 'onLine', { configurable: true, get: () => value });
}

describe('indicador de conexión', () => {
  beforeEach(async () => {
    await resetDbForTests();
    indexedDB.deleteDatabase('dp-preventa');
    localStorage.setItem('dp.session', adminToken());
  });
  afterEach(() => setOnline(true));

  it('sin conexión muestra "Sin conexión" y los pedidos pendientes', async () => {
    setOnline(false);
    await enqueueOrder({
      id: 'p1', createdAt: '2026-10-15T10:00:00Z',
      request: { clientId: 'c', dispatchOrigin: 'Warehouse', truckId: null, employeeId: null, items: [], payment: 0, paymentObservation: null, returnedEmpties: null },
      summary: { clientName: 'Bodega', origin: 'Galpón', total: 10, lines: 1 },
    });
    const send = vi.fn();

    render(
      <AuthProvider>
        <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
          <OfflineProvider send={send}>
            <ConnectionIndicator />
          </OfflineProvider>
        </MemoryRouter>
      </AuthProvider>,
    );
    await act(async () => window.dispatchEvent(new Event('offline')));

    expect(await screen.findByText('Sin conexión: 1 pedido pendiente')).toBeInTheDocument();
  });

  it('al volver la conexión envía los pedidos y el indicador desaparece', async () => {
    setOnline(false);
    await enqueueOrder({
      id: 'p2', createdAt: '2026-10-15T10:00:00Z',
      request: { clientId: 'c', dispatchOrigin: 'Warehouse', truckId: null, employeeId: null, items: [], payment: 0, paymentObservation: null, returnedEmpties: null },
      summary: { clientName: 'Bodega', origin: 'Galpón', total: 10, lines: 1 },
    });
    const send = vi.fn().mockRejectedValueOnce(new (await import('axios')).AxiosError('Network Error', 'ERR_NETWORK', { headers: new (await import('axios')).AxiosHeaders() }))
      .mockResolvedValue({ id: 'f', number: 'Factura Octubre 2026 #9' });

    render(
      <AuthProvider>
        <MemoryRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
          <OfflineProvider send={send}>
            <ConnectionIndicator />
          </OfflineProvider>
        </MemoryRouter>
      </AuthProvider>,
    );
    expect(await screen.findByText(/1 pedido pendiente/)).toBeInTheDocument();

    setOnline(true);
    await act(async () => window.dispatchEvent(new Event('online')));

    await vi.waitFor(() => expect(screen.queryByText(/pendiente/)).not.toBeInTheDocument());
    expect(send).toHaveBeenLastCalledWith(expect.anything(), 'p2');
  });
});
