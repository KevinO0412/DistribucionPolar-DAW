import { render, screen } from '@testing-library/react';
import type { Invoice } from '../../api/types';
import { Ticket } from './Ticket';

const invoice = {
  id: 'i1', number: 'Factura Octubre 2026 #3', type: 'Sale', issuedAt: '2026-10-15T15:00:00Z', isCancelled: false, cancellationReason: null,
  clientName: 'Bodega La Esquina', clientRif: 'J-1', dispatchOrigin: 'Truck', truckName: 'NPR', truckPlate: 'AB123CD', attendantName: 'Carlos Pérez',
  items: [{ productId: 'p1', productName: 'POLAR PILSEN RET 222ML', quantityDisplay: '1 caja', subtotal: 23 }],
  total: 23, payment: 10, pending: 13, paymentObservation: null, emptyGroups: [],
} as unknown as Invoice;

describe('Ticket', () => {
  it('se arma en 80 mm por defecto y en 58 mm para impresoras portátiles', () => {
    const { container, rerender } = render(<Ticket invoice={invoice} />);
    expect(container.querySelector('#ticket-print')).toHaveAttribute('data-paper', '80');
    expect(container.querySelector('#ticket-print')).toHaveClass('w-[80mm]');

    rerender(<Ticket invoice={invoice} width={58} />);
    expect(container.querySelector('#ticket-print')).toHaveClass('w-[58mm]');
    expect(screen.getByText('Factura Octubre 2026 #3')).toBeInTheDocument();
    expect(screen.getByText('Pendiente')).toBeInTheDocument();
  });
});
