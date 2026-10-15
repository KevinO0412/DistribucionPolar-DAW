import type { Invoice } from '../../api/types';
import { cn } from '../../lib/cn';
import { formatDateTime, formatMoney } from '../../lib/format';
import { paperClass, type PaperWidth } from '../../lib/print';

/**
 * Ticket térmico de 80 mm o 58 mm (lógica de negocio §8): empresa, cliente, despacho, productos, totales,
 * observación de pago, vacíos por grupo y líneas de firma. Se imprime con CSS @media print (index.css y lib/print).
 */
export function Ticket({ invoice, width = 80 }: { invoice: Invoice; width?: PaperWidth }) {
  const isGift = invoice.type === 'Gift';
  const rule = <hr className="my-2 border-t border-dashed border-black/60" />;

  return (
    <div id="ticket-print" data-paper={width} className={cn('mx-auto max-w-full bg-white p-4 font-mono leading-snug text-black shadow-sm', paperClass[width])}>
      <div className="text-center">
        <p className={cn('font-bold', width === 58 ? 'text-[11px]' : 'text-[13px]')}>DISTRIBUIDORA VICROS C.A.</p>
        <p>RIF J-301798740</p>
        <p className="mt-1 font-bold">{invoice.number}</p>
        <p>{formatDateTime(invoice.issuedAt)}</p>
        {invoice.isCancelled && <p className="mt-1 font-bold">ANULADA: {invoice.cancellationReason}</p>}
      </div>
      {rule}
      <p className="font-bold">Cliente</p>
      <p>{invoice.clientName}</p>
      <p>RIF/CI {invoice.clientRif}</p>
      {rule}
      <p className="font-bold">Despacho</p>
      <p>{invoice.dispatchOrigin === 'Truck' ? `Camión ${invoice.truckName ?? ''} (${invoice.truckPlate ?? ''})` : 'Galpón'}</p>
      <p>Atendió: {invoice.attendantName}</p>
      {rule}
      <p className="font-bold">Productos</p>
      {invoice.items.map((item) => (
        <div key={item.productId} className="mt-1">
          <p>{item.productName}</p>
          <p className="flex justify-between">
            <span>{item.quantityDisplay}</span>
            <span>{formatMoney(item.subtotal)}</span>
          </p>
        </div>
      ))}
      {rule}
      <p className="flex justify-between font-bold">
        <span>Total</span>
        <span>{formatMoney(invoice.total)}</span>
      </p>
      {!isGift && (
        <>
          <p className="flex justify-between">
            <span>Abono</span>
            <span>{formatMoney(invoice.payment)}</span>
          </p>
          <p className="flex justify-between">
            <span>{invoice.pending < 0 ? 'Saldo a favor' : 'Pendiente'}</span>
            <span>{formatMoney(Math.abs(invoice.pending))}</span>
          </p>
        </>
      )}
      {invoice.paymentObservation && (
        <>
          {rule}
          <p className="font-bold">Observación de pago</p>
          <p className="whitespace-pre-line">{invoice.paymentObservation}</p>
        </>
      )}
      {invoice.emptyGroups.length > 0 && (
        <>
          {rule}
          <p className="font-bold">Vacíos retornables</p>
          {invoice.emptyGroups.map((g) => (
            <p key={g.groupKey}>
              {g.groupName}:{width === 58 ? <br /> : ' '}gen. {g.generatedBoxes} cj + {g.generatedUnits} und, dev. {g.returnedBoxes} cj + {g.returnedUnits} und
            </p>
          ))}
        </>
      )}
      <div className="mt-6 flex flex-col gap-6">
        <p className="border-t border-black/70 pt-1 text-center">Recibido por</p>
        <p className="border-t border-black/70 pt-1 text-center">Firma</p>
      </div>
      <p className="mt-3 text-center">Gracias por su compra</p>
    </div>
  );
}
