import { render, screen } from '@testing-library/react';
import type { Client, RouteSettlement } from '../../api/types';
import { SettlementSheet } from '../trucks/SettlementSheet';
import { PaymentReceipt } from './PaymentReceipt';

const client = { id: 'c', code: '007', name: 'Bodega La Esquina', rif: 'J-1', moneyBalance: -35 } as Client;

describe('impresión térmica', () => {
  it('el recibo de cobro se adapta al papel de 58 mm', () => {
    const { container } = render(
      <PaymentReceipt width={58} data={{ client, amount: 25, balanceBefore: -60, observation: 'Efectivo', receivedAt: '2026-10-15T15:00:00Z', receivedBy: 'Empleado' }} />,
    );

    expect(container.querySelector('#ticket-print')).toHaveAttribute('data-paper', '58');
    expect(container.querySelector('style')?.textContent).toContain('size: 58mm auto');
    expect(screen.getByText('RECIBO DE COBRO')).toBeInTheDocument();
    expect(screen.getByText('Saldo anterior').nextSibling).toHaveTextContent(/^Debe .*60,00$/);
    expect(screen.getByText('Saldo actual').nextSibling).toHaveTextContent(/^Debe .*35,00$/);
  });

  it('la planilla de cuadre marca el faltante de dinero y mercancía', () => {
    const settlement: RouteSettlement = {
      id: 's', truckId: 't', truckName: 'FVR', truckPlate: 'AA200AA', dispatchedAt: '2026-10-15T08:00:00Z', settledAt: '2026-10-15T18:00:00Z',
      settledBy: 'empleado', invoicesCount: 3, salesTotal: 230, expectedCash: 40, countedCash: 30, cashDifference: -10,
      missingGoodsValue: 18, isBalanced: false, observation: null,
      lines: [{ productId: 'p', productName: 'POLAR PILSEN', unitsPerBox: 36, expectedUnits: 108, expectedDisplay: '3 cajas', countedUnits: 102, countedDisplay: '2 cajas + 30 und', differenceUnits: -6, differenceValue: -18 }],
    };

    render(<SettlementSheet settlement={settlement} width={80} />);

    expect(screen.getByText('Faltante')).toBeInTheDocument();
    expect(screen.getByText('-6 und')).toBeInTheDocument();
    expect(screen.getByText('RUTA CON DIFERENCIAS')).toBeInTheDocument();
  });
});
