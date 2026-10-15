import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ToastProvider } from '../components/ui/Toast';
import { clientsApi } from '../api/endpoints';
import { ClientsPage, creditLabel } from './ClientsPage';

let isAdmin = true;
vi.mock('../hooks/useAuth', () => ({ useAuth: () => ({ isAdmin }) }));
vi.mock('../api/endpoints', () => ({
  clientsApi: {
    list: vi.fn().mockResolvedValue([
      { id: 'c1', code: '001', name: 'Bodega La Esquina', rif: 'J-1', moneyBalance: -250, moneyStatus: 'Debe: $250.00', creditLimit: 300, creditDays: 30 },
    ]),
    credit: vi.fn().mockResolvedValue({
      debt: 250, creditLimit: 300, creditDays: 30, overdueAmount: 150, oldestDebtDays: 100, available: 50, isBlocked: true,
      blockReason: 'Tiene $150.00 de deuda vencida.', days0To30: 100, days31To60: 100, days61To90: 0, over90: 50,
    }),
    setCredit: vi.fn().mockResolvedValue({}),
  },
}));

const renderPage = () => render(<ToastProvider><ClientsPage /></ToastProvider>);

describe('ClientsPage', () => {
  it('describe las condiciones de crédito', () => {
    expect(creditLabel({ creditLimit: null, creditDays: 30 })).toBe('Sin límite · 30 días');
  });

  it('muestra el bloqueo y los tramos de antigüedad, y el Admin guarda nuevas condiciones', async () => {
    isAdmin = true;
    renderPage();
    await userEvent.click(await screen.findByRole('button', { name: 'Crédito de Bodega La Esquina' }));

    expect(await screen.findByText('Crédito bloqueado.')).toBeInTheDocument();
    expect(screen.getByText('61-90 días')).toBeInTheDocument();
    await userEvent.clear(screen.getByLabelText('Días de crédito'));
    await userEvent.type(screen.getByLabelText('Días de crédito'), '45');
    await userEvent.click(screen.getByRole('button', { name: 'Guardar', hidden: true }));

    expect(clientsApi.setCredit).toHaveBeenCalledWith('c1', { creditLimit: 300, creditDays: 45 });
  });

  it('el Employee ve el crédito pero no lo puede cambiar', async () => {
    isAdmin = false;
    renderPage();
    await userEvent.click(await screen.findByRole('button', { name: 'Crédito de Bodega La Esquina' }));

    expect(await screen.findByText(/Solo un administrador cambia estas condiciones/)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Guardar', hidden: true })).toBeNull();
  });
});
