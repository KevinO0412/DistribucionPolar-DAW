import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import userEvent from '@testing-library/user-event';
import { ToastProvider } from '../components/ui/Toast';
import { trucksApi } from '../api/endpoints';
import { TrucksPage, tripLabel } from './TrucksPage';

const truck = (status: string, extra: object = {}) => ({
  id: status, name: `Camión ${status}`, plate: 'AB123CD', status, totalUnits: 72, inventoryValue: 54, stock: [],
  capacityBoxes: 100, loadedBoxes: 2, occupancyPercent: 2, dispatchedAt: null, tripFrom: null, tripInvoices: 3, tripSales: 420, tripCollected: 300,
  ...extra,
});

let isAdmin = true;
vi.mock('../hooks/useAuth', () => ({ useAuth: () => ({ isAdmin }) }));

vi.mock('../api/endpoints', () => ({
  trucksApi: {
    list: vi.fn(),
    dispatch: vi.fn().mockResolvedValue({}),
    settlements: vi.fn().mockResolvedValue([]),
  },
}));

describe('TrucksPage', () => {
  it('muestra el estado, lo facturado y solo deja despachar al camión que está cargando', async () => {
    isAdmin = true;
    vi.mocked(trucksApi.list).mockResolvedValue([truck('Cargando'), truck('En ruta', { dispatchedAt: '2026-10-15T12:00:00Z' })] as never);
    render(<MemoryRouter><ToastProvider><TrucksPage /></ToastProvider></MemoryRouter>);

    expect(await screen.findByText('Cargando')).toBeInTheDocument();
    expect(screen.getByText('En ruta')).toBeInTheDocument();
    expect(screen.getAllByText('USD 420,00')).toHaveLength(2);
    const buttons = screen.getAllByRole('button', { name: /Despachar a ruta/ });
    expect(buttons).toHaveLength(1);

    await userEvent.click(buttons[0]);
    expect(trucksApi.dispatch).toHaveBeenCalledWith('Cargando');
  });

  it('cuenta las ventas desde la salida solo si el camión salió a ruta', () => {
    expect(tripLabel({ status: 'Cargando', dispatchedAt: null })).toBe('Facturado hoy');
    expect(tripLabel({ status: 'En ruta', dispatchedAt: '2026-10-15T12:00:00Z' })).toMatch(/^Facturado en la ruta/);
  });

  it('en ruta, el Employee no ve lo cobrado ni la carga (liquidación ciega) y puede liquidar', async () => {
    isAdmin = false;
    const stock = [{ productId: 'p1', productName: 'POLAR PILSEN', stockDisplay: '2 cajas' }];
    vi.mocked(trucksApi.list).mockResolvedValue([truck('En ruta', { dispatchedAt: '2026-10-15T12:00:00Z', stock })] as never);
    render(<MemoryRouter><ToastProvider><TrucksPage /></ToastProvider></MemoryRouter>);

    expect(await screen.findByText('Oculto')).toBeInTheDocument();
    expect(screen.getByText(/carga oculta hasta la liquidación/)).toBeInTheDocument();
    expect(screen.queryByText('POLAR PILSEN')).toBeNull();
    expect(screen.getByRole('link', { name: /Liquidar ruta/ })).toHaveAttribute('href', '/camiones/En ruta/liquidacion');
  });
});
