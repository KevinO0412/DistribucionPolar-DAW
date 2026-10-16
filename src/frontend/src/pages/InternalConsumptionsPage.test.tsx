import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, vi } from 'vitest';
import { ApiError } from '../api/client';
import { employeesApi, internalConsumptionsApi, productsApi, trucksApi } from '../api/endpoints';
import type { Employee, InternalConsumption, Product, Truck } from '../api/types';
import { visibleNavItems } from '../components/layout/navigation';
import { ToastProvider } from '../components/ui/Toast';
import { InternalConsumptionsPage } from './InternalConsumptionsPage';

vi.mock('../api/endpoints', () => ({
  productsApi: { list: vi.fn() },
  trucksApi: { list: vi.fn() },
  employeesApi: { list: vi.fn() },
  internalConsumptionsApi: { list: vi.fn(), create: vi.fn() },
}));

const harina = { id: 'p1', name: 'HARINA PAN 1KGx20UN', stockUnits: 45, stockDisplay: '2 cajas + 5 und' } as Product;
const agotado = { id: 'p2', name: 'ARROZ AGOTADO', stockUnits: 0, stockDisplay: '0 und' } as Product;
const truck = { id: 't1', name: 'FVR', stock: [{ productId: 'p9', productName: 'POLAR PILSEN', stockDisplay: '3 cajas' }] } as Truck;
const employees: Employee[] = [
  { id: 'e1', name: 'Carlos Pérez', role: 'Vendedor', isActive: true, createdAt: '' },
  { id: 'e2', name: 'Inactivo', role: 'Chofer', isActive: false, createdAt: '' },
];
const consumption: InternalConsumption = {
  id: 'ic1', occurredAt: '2026-10-15T15:00:00Z', type: 'InternalConsumption', employeeName: 'Carlos Pérez', origin: 'Warehouse',
  truckName: null, productId: 'p1', productName: 'HARINA PAN 1KGx20UN', boxes: 0, looseUnits: 2, totalUnits: 2, estimatedValue: 1.9,
  observation: 'Refrigerio del personal',
};

beforeEach(() => {
  vi.clearAllMocks();
  vi.mocked(productsApi.list).mockResolvedValue([harina, agotado]);
  vi.mocked(trucksApi.list).mockResolvedValue([truck]);
  vi.mocked(employeesApi.list).mockResolvedValue(employees);
  vi.mocked(internalConsumptionsApi.list).mockResolvedValue({ items: [consumption], count: 1, totalValue: 1.9 });
  vi.mocked(internalConsumptionsApi.create).mockReset();
});

const renderPage = () => render(<ToastProvider><InternalConsumptionsPage /></ToastProvider>);

describe('Consumos internos', () => {
  it('lista fecha, producto, cantidad, empleado y motivo', async () => {
    renderPage();
    const row = within(await screen.findByRole('table')).getAllByRole('row')[1];
    expect(row).toHaveTextContent('HARINA PAN 1KGx20UN');
    expect(row).toHaveTextContent('0 cj + 2 und');
    expect(row).toHaveTextContent('Carlos Pérez');
    expect(row).toHaveTextContent('Consumo interno');
    expect(row).toHaveTextContent('Refrigerio del personal');
    expect(screen.getByText(/cobrado \$0/)).toBeInTheDocument();
  });

  it('registra un consumo del galpón con empleado activo y avisa con un Toast', async () => {
    vi.mocked(internalConsumptionsApi.create).mockResolvedValue(consumption);
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole('button', { name: /Registrar consumo/ }));
    const product = await screen.findByLabelText('Producto');
    expect(within(product).getAllByRole('option').map((o) => o.textContent)).toEqual(['Elige un producto (1)', 'HARINA PAN 1KGx20UN (2 cajas + 5 und)']);
    expect(within(screen.getByLabelText('Empleado')).queryByText(/Inactivo/)).not.toBeInTheDocument();

    await user.selectOptions(product, 'p1');
    await user.clear(screen.getByLabelText('Unidades'));
    await user.type(screen.getByLabelText('Unidades'), '2');
    await user.selectOptions(screen.getByLabelText('Empleado'), 'e1');
    await user.type(screen.getByLabelText('Motivo (opcional)'), 'Refrigerio del personal');
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Registrar consumo' }));

    expect(internalConsumptionsApi.create).toHaveBeenCalledWith({
      productId: 'p1', boxes: 0, looseUnits: 2, origin: 'Warehouse', truckId: null, employeeId: 'e1', type: 'InternalConsumption',
      observation: 'Refrigerio del personal',
    });
    expect(await screen.findByText(/Consumo registrado: HARINA PAN 1KGx20UN, 2 und a nombre de Carlos Pérez/)).toBeInTheDocument();
    expect(internalConsumptionsApi.list).toHaveBeenCalledTimes(2);
  });

  it('desde un camión solo ofrece lo que lleva y muestra el error RFC 7807 de la API', async () => {
    vi.mocked(internalConsumptionsApi.create).mockRejectedValueOnce(new ApiError(400, 'Solicitud inválida', 'Inventario insuficiente en el camión FVR.'));
    const user = userEvent.setup();
    renderPage();

    await user.click(await screen.findByRole('button', { name: /Registrar consumo/ }));
    await user.selectOptions(await screen.findByLabelText('Sale de'), 't1');
    expect(within(screen.getByLabelText('Producto')).getAllByRole('option').map((o) => o.textContent)).toEqual(['Elige un producto (1)', 'POLAR PILSEN (3 cajas)']);

    await user.selectOptions(screen.getByLabelText('Producto'), 'p9');
    await user.selectOptions(screen.getByLabelText('Empleado'), 'e1');
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Registrar consumo' }));

    expect(await screen.findByText('Inventario insuficiente en el camión FVR.')).toBeInTheDocument();
    expect(internalConsumptionsApi.create).toHaveBeenCalledWith(expect.objectContaining({ origin: 'Truck', truckId: 't1', productId: 'p9' }));
  });

  it('valida en el formulario antes de llamar a la API', async () => {
    const user = userEvent.setup();
    renderPage();
    await user.click(await screen.findByRole('button', { name: /Registrar consumo/ }));
    await screen.findByLabelText('Producto');
    await user.clear(screen.getByLabelText('Unidades'));
    await user.click(within(screen.getByRole('dialog')).getByRole('button', { name: 'Registrar consumo' }));

    expect(screen.getByText('Elige el producto.')).toBeInTheDocument();
    expect(screen.getByText('Elige el empleado.')).toBeInTheDocument();
    expect(screen.getByText(/no pueden ser ambas 0/)).toBeInTheDocument();
    expect(internalConsumptionsApi.create).not.toHaveBeenCalled();
  });

  it('solo aparece en el menú del Admin', () => {
    expect(visibleNavItems('Admin').map((i) => i.to)).toContain('/consumos');
    expect(visibleNavItems('Employee').map((i) => i.to)).not.toContain('/consumos');
  });
});
