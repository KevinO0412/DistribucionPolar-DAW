import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ToastProvider } from '../components/ui/Toast';
import { trucksApi } from '../api/endpoints';
import { TrucksPage, tripLabel } from './TrucksPage';

const truck = (status: string, extra: object = {}) => ({
  id: status, name: `Camión ${status}`, plate: 'AB123CD', status, totalUnits: 72, inventoryValue: 54, stock: [],
  capacityBoxes: 100, loadedBoxes: 2, occupancyPercent: 2, dispatchedAt: null, tripFrom: null, tripInvoices: 3, tripSales: 420, tripCollected: 300,
  ...extra,
});

vi.mock('../api/endpoints', () => ({
  trucksApi: {
    list: vi.fn(),
    dispatch: vi.fn().mockResolvedValue({}),
  },
}));

describe('TrucksPage', () => {
  it('muestra el estado, lo facturado y solo deja despachar al camión que está cargando', async () => {
    vi.mocked(trucksApi.list).mockResolvedValue([truck('Cargando'), truck('En ruta', { dispatchedAt: '2026-10-15T12:00:00Z' })] as never);
    render(<ToastProvider><TrucksPage /></ToastProvider>);

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
});
