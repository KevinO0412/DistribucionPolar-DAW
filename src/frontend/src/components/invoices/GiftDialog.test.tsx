import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, vi } from 'vitest';
import { ApiError } from '../../api/client';
import { clientsApi, invoicesApi, trucksApi } from '../../api/endpoints';
import type { Client, Invoice, InvoiceSummary, Product, Truck } from '../../api/types';
import { InvoicesPage } from '../../pages/InvoicesPage';
import { ToastProvider } from '../ui/Toast';
import { GiftDialog } from './GiftDialog';

vi.mock('../../api/endpoints', () => ({
  clientsApi: { list: vi.fn() },
  trucksApi: { list: vi.fn() },
  invoicesApi: { giftProducts: vi.fn(), createGift: vi.fn(), list: vi.fn(), get: vi.fn() },
}));

const product = (id: string, name: string, emptyGroupKey: string | null) =>
  ({ id, name, stockDisplay: '10 cajas', emptyGroupKey, isGiftEligible: true }) as Product;

const pilsen = product('p1', 'POLAR PILSEN RET 222MLx36UN', 'RET_222ML_X36');
const solera = product('p2', 'SOLERA RET 222MLx36UN', 'RET_222ML_X36');
const client = { id: 'c1', code: '001', name: 'Bodega La Esquina', rif: 'J-1', nickname: null } as Client;
const truck = { id: 't1', name: 'FVR', status: 'En ruta', stock: [{ productId: 'p2', stockDisplay: '3 cajas' }] } as Truck;
const gift = { id: 'g1', number: 'Regalía Octubre 2026 #1', type: 'Gift', clientName: 'Bodega La Esquina' } as Invoice;

beforeEach(() => {
  vi.mocked(invoicesApi.giftProducts).mockResolvedValue([pilsen, solera]);
  vi.mocked(clientsApi.list).mockResolvedValue([client]);
  vi.mocked(trucksApi.list).mockResolvedValue([truck]);
  vi.mocked(invoicesApi.createGift).mockReset();
});

const productOptions = () => within(screen.getByLabelText('Producto')).getAllByRole('option').slice(1).map((o) => o.textContent);

describe('GiftDialog', () => {
  it('solo ofrece productos autorizados y, desde un camión, solo los que lleva', async () => {
    const user = userEvent.setup();
    render(<GiftDialog onClose={() => {}} onCreated={() => {}} />);

    await screen.findByLabelText('Producto');
    expect(productOptions()).toEqual(['POLAR PILSEN RET 222MLx36UN (10 cajas), retornable', 'SOLERA RET 222MLx36UN (10 cajas), retornable']);

    await user.selectOptions(screen.getByLabelText('Despacho'), 't1');
    expect(productOptions()).toEqual(['SOLERA RET 222MLx36UN (3 cajas), retornable']);
  });

  it('envía la regalía sin cobro con el origen y las líneas elegidas', async () => {
    vi.mocked(invoicesApi.createGift).mockResolvedValue(gift);
    const onCreated = vi.fn();
    const user = userEvent.setup();
    render(<GiftDialog onClose={() => {}} onCreated={onCreated} />);

    await user.selectOptions(await screen.findByLabelText('Cliente'), 'c1');
    await user.selectOptions(screen.getByLabelText('Producto'), 'p1');
    await user.clear(screen.getByLabelText('Cajas'));
    await user.type(screen.getByLabelText('Cajas'), '2');
    await user.click(screen.getByRole('button', { name: /Registrar regalía/ }));

    expect(invoicesApi.createGift).toHaveBeenCalledWith({
      clientId: 'c1', dispatchOrigin: 'Warehouse', truckId: null, items: [{ productId: 'p1', boxes: 2, looseUnits: 0 }],
    });
    expect(onCreated).toHaveBeenCalledWith(gift);
  });

  it('valida antes de enviar: cliente y producto obligatorios', async () => {
    const user = userEvent.setup();
    render(<GiftDialog onClose={() => {}} onCreated={() => {}} />);
    await screen.findByLabelText('Cliente');

    await user.click(screen.getByRole('button', { name: /Registrar regalía/ }));

    expect(screen.getByText('Elige el cliente.')).toBeInTheDocument();
    expect(screen.getByText('Elige un producto autorizado.')).toBeInTheDocument();
    expect(invoicesApi.createGift).not.toHaveBeenCalled();
  });

  it('muestra el error RFC 7807 de la API junto al campo o arriba si es una regla de negocio', async () => {
    const user = userEvent.setup();
    render(<GiftDialog onClose={() => {}} onCreated={() => {}} />);
    await user.selectOptions(await screen.findByLabelText('Cliente'), 'c1');
    await user.selectOptions(screen.getByLabelText('Producto'), 'p1');

    vi.mocked(invoicesApi.createGift).mockRejectedValueOnce(
      new ApiError(400, 'Datos de entrada inválidos', 'Uno o más campos no cumplen las reglas.', { 'items[0].Boxes': ['Las cajas no pueden ser negativas.'] }),
    );
    await user.click(screen.getByRole('button', { name: /Registrar regalía/ }));
    expect(await screen.findByText('Las cajas no pueden ser negativas.')).toBeInTheDocument();

    vi.mocked(invoicesApi.createGift).mockRejectedValueOnce(new ApiError(400, 'Solicitud inválida', 'Inventario insuficiente en galpón: POLAR PILSEN.'));
    await user.click(screen.getByRole('button', { name: /Registrar regalía/ }));
    expect(await screen.findByText('Inventario insuficiente en galpón: POLAR PILSEN.')).toBeInTheDocument();
  });
});

describe('Facturas', () => {
  it('muestra el tipo "Regalía" y abre el ticket de la regalía recién registrada', async () => {
    const summary = { id: 'g1', number: gift.number, type: 'Gift', clientName: 'Bodega La Esquina', issuedAt: '2026-10-15T15:00:00Z',
      dispatchOrigin: 'Warehouse', attendantName: 'Carlos', total: 0, payment: 0, pending: 0, isCancelled: false } as InvoiceSummary;
    vi.mocked(invoicesApi.list).mockResolvedValue({ items: [summary], page: 1, pageSize: 200, totalCount: 1 });
    vi.mocked(invoicesApi.get).mockResolvedValue({ ...gift, clientRif: 'J-1', issuedAt: summary.issuedAt, dispatchOrigin: 'Warehouse',
      attendantName: 'Carlos', total: 0, payment: 0, pending: 0, paymentObservation: 'Regalía a cliente - productos autorizados',
      items: [], emptyGroups: [], isCancelled: false } as unknown as Invoice);
    vi.mocked(invoicesApi.createGift).mockResolvedValue(gift);
    const user = userEvent.setup();
    render(<ToastProvider><InvoicesPage /></ToastProvider>);

    const table = await screen.findByRole('table');
    expect(within(table).getByText('Regalía')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /Nueva regalía/ }));
    await user.selectOptions(await screen.findByLabelText('Cliente'), 'c1');
    await user.selectOptions(screen.getByLabelText('Producto'), 'p1');
    await user.click(screen.getByRole('button', { name: /Registrar regalía/ }));

    expect(await screen.findByText('Regalía Octubre 2026 #1 registrada para Bodega La Esquina.')).toBeInTheDocument();
    expect(await screen.findByText('Ticket de entrega')).toBeInTheDocument();
    expect(invoicesApi.get).toHaveBeenCalledWith('g1');
  });
});
