import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { trucksApi } from '../api/endpoints';
import { SettlementPage } from './SettlementPage';

vi.mock('../api/endpoints', () => ({
  trucksApi: {
    settlementForm: vi.fn().mockResolvedValue({
      truckId: 't1', truckName: 'NPR', truckPlate: 'AB123CD', status: 'En ruta', dispatchedAt: '2026-10-15T12:00:00Z',
      products: [{ productId: 'p1', sku: 'BEB-POL-0001', name: 'POLAR PILSEN RET 222ML', unitsPerBox: 36 }],
    }),
    settle: vi.fn().mockResolvedValue({
      id: 's1', truckId: 't1', truckName: 'NPR', truckPlate: 'AB123CD', tripStartedAt: '2026-10-15T12:00:00Z', settledAt: '2026-10-15T22:00:00Z',
      settledBy: 'empleado', observation: null, expectedCash: 300, countedCash: 280, cashDifference: -20,
      expectedMerchandiseValue: 46.08, countedMerchandiseValue: 42.24, merchandiseDifferenceValue: -3.84, isBalanced: false,
      lines: [{ productId: 'p1', sku: 'BEB-POL-0001', productName: 'POLAR PILSEN RET 222ML', unitsPerBox: 36, expectedUnits: 72,
        expectedDisplay: '2 cajas', countedUnits: 66, countedDisplay: '1 caja + 30 und', differenceUnits: -6, differenceValue: -3.84 }],
    }),
  },
}));

describe('SettlementPage', () => {
  it('pide el conteo sin mostrar lo esperado y, al confirmar, muestra la planilla con las diferencias', async () => {
    render(
      <MemoryRouter initialEntries={['/camiones/t1/liquidacion']}>
        <Routes>
          <Route path="/camiones/:id/liquidacion" element={<SettlementPage />} />
        </Routes>
      </MemoryRouter>,
    );

    expect(await screen.findByText('POLAR PILSEN RET 222ML')).toBeInTheDocument();
    expect(screen.queryByText('2 cajas')).toBeNull();
    expect(screen.queryByText(/300/)).toBeNull();

    await userEvent.type(screen.getByLabelText('Efectivo entregado (USD)'), '280');
    await userEvent.type(screen.getByLabelText('Cajas de POLAR PILSEN RET 222ML'), '1');
    await userEvent.type(screen.getByLabelText('Unidades de POLAR PILSEN RET 222ML'), '30');
    await userEvent.click(screen.getByRole('button', { name: /Enviar conteo/ }));
    await userEvent.click(screen.getByRole('button', { name: 'Confirmar liquidación' }));

    expect(trucksApi.settle).toHaveBeenCalledWith('t1', { cashCounted: 280, items: [{ productId: 'p1', boxes: 1, looseUnits: 30 }], observation: null });
    expect(await screen.findByText('PLANILLA DE CUADRE')).toBeInTheDocument();
    expect(screen.getByText('RUTA CON DIFERENCIAS')).toBeInTheDocument();
    expect(screen.getByText('-6 und')).toBeInTheDocument();
  });
});
