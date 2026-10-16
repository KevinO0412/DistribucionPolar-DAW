import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, vi } from 'vitest';
import { ApiError } from '../../api/client';
import { clientsApi } from '../../api/endpoints';
import type { Client, ClientMovement, ClientStatement } from '../../api/types';
import { debtAging, statementRows } from '../../lib/statement';
import { ClientStatementDialog } from './ClientStatementDialog';

vi.mock('../../api/endpoints', () => ({ clientsApi: { statement: vi.fn() } }));

const client: Client = {
  id: 'c1', code: '001', rif: 'J-1', name: 'Bodega La Esquina', address: 'Av. Principal', nickname: null, type: 'Ocasional',
  moneyBalance: -35, moneyStatus: 'Debe: $35.00', emptyBoxesBalance: -1, emptyUnitsBalance: -5, emptiesStatus: 'Debe vacíos',
  emptyBalances: [{ groupKey: 'RET_222ML_X36', groupName: '222ML RET x36', boxes: -1, units: -5 }],
  isReceivable: true, lastPurchaseAt: '2026-10-10T15:00:00Z', createdAt: '2026-10-01T00:00:00Z', creditLimit: 500,
};

const move = (id: string, type: ClientMovement['type'], amount: number, date: string, invoiceId: string | null = null): ClientMovement => ({
  id, type, title: id, description: `detalle ${id}`, amount, invoiceId, date,
});

// Factura de 100 con abono de 40 (saldo −60), vacíos, abono suelto de 25 (saldo −35), regalía y una factura anulada.
const statement: ClientStatement = {
  client,
  invoices: [move('F2', 'Invoice', 50, '2026-10-12T15:00:00Z', 'i2'), move('F1', 'Invoice', 100, '2026-10-10T15:00:00Z', 'i1')],
  gifts: [move('R1', 'Gift', 0, '2026-10-13T15:00:00Z', 'g1')],
  payments: [move('A2', 'Payment', 25, '2026-10-11T15:00:00Z'), move('A1', 'Payment', 40, '2026-10-10T15:00:00Z', 'i1')],
  empties: [
    move('X2', 'InvoiceCancelled', 50, '2026-10-12T16:00:00Z', 'i2'),
    move('V1', 'EmptiesGenerated', 0, '2026-10-10T15:00:00Z', 'i1'),
  ],
  adjustments: [],
};

describe('estado de cuenta: cálculos', () => {
  it('ordena del más reciente al más antiguo y calcula el saldo resultante de cada movimiento', () => {
    const rows = statementRows(statement);
    expect(rows.map((r) => r.id)).toEqual(['R1', 'X2', 'F2', 'A2', 'V1', 'A1', 'F1']);
    expect(Object.fromEntries(rows.map((r) => [r.id, r.balanceAfter]))).toEqual({
      R1: -35, X2: -35, F2: -85, A2: -35, V1: -60, A1: -60, F1: -100,
    });
  });

  it('calcula la antigüedad con los pendientes vigentes más recientes, sin la factura anulada', () => {
    const aging = debtAging(statement, new Date('2026-10-20T15:00:00Z'));
    expect(aging.debt).toBe(35);
    expect(aging.oldestDays).toBe(10);
    expect(aging.buckets.find((b) => b.label === '8–15 días')?.amount).toBe(35);
  });

  it('la parte de la deuda que ninguna factura explica cuenta como la más antigua', () => {
    const aging = debtAging({ ...statement, client: { ...client, moneyBalance: -80 } }, new Date('2026-10-20T15:00:00Z'));
    expect(aging.oldestDays).toBe(999);
    expect(aging.buckets.find((b) => b.label === 'Más de 30 días')?.amount).toBe(20);
  });
});

describe('ClientStatementDialog', () => {
  beforeEach(() => {
    vi.mocked(clientsApi.statement).mockReset();
  });

  it('muestra datos, saldo, límite, vacíos y los movimientos con su tipo', async () => {
    vi.mocked(clientsApi.statement).mockResolvedValue(statement);
    render(<ClientStatementDialog client={client} onClose={() => {}} />);

    const table = await screen.findByRole('table');
    expect(screen.getByText('Historial de Bodega La Esquina')).toBeInTheDocument();
    const details = screen.getByLabelText('Datos del cliente');
    expect(within(details).getByText(/^Debe .*35,00$/)).toBeInTheDocument();
    expect(within(details).getByText(/500,00/)).toBeInTheDocument();
    expect(screen.getByText(/222ML RET x36: debe 1 cj \+ 5 und/)).toBeInTheDocument();
    const rows = within(table).getAllByRole('row').slice(1);
    expect(rows).toHaveLength(7);
    expect(within(rows[0]).getByText('Regalía')).toBeInTheDocument();
    expect(within(rows[1]).getByText('Anulación')).toBeInTheDocument();
    expect(clientsApi.statement).toHaveBeenCalledWith('c1', expect.any(AbortSignal));
  });

  it('filtra por tipo y por rango de fechas', async () => {
    vi.mocked(clientsApi.statement).mockResolvedValue(statement);
    const user = userEvent.setup();
    render(<ClientStatementDialog client={client} onClose={() => {}} />);
    const table = await screen.findByRole('table');

    await user.selectOptions(screen.getByLabelText('Tipo'), 'Payment');
    expect(within(table).getAllByRole('row')).toHaveLength(3);

    await user.selectOptions(screen.getByLabelText('Tipo'), '');
    await user.type(screen.getByLabelText('Desde'), '2026-10-12');
    expect(within(screen.getByRole('table')).getAllByRole('row').slice(1).map((r) => r.querySelector('p')?.textContent)).toEqual(['R1', 'X2', 'F2']);
  });

  it('muestra el error RFC 7807 si la API rechaza la consulta', async () => {
    vi.mocked(clientsApi.statement).mockImplementation(() => Promise.reject(new ApiError(404, 'Recurso no encontrado', 'No existe el cliente c1.')));
    render(<ClientStatementDialog client={client} onClose={() => {}} />);
    expect(await screen.findByText(/No existe el cliente c1/)).toBeInTheDocument();
  });
});
