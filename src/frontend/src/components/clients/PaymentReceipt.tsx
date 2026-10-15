import { cn } from '../../lib/cn';
import { formatDateTime, formatMoney } from '../../lib/format';
import { paperClass, type PaperWidth } from '../../lib/print';

export interface ReceiptData {
  clientCode: string;
  clientName: string;
  clientRif: string;
  amount: number;
  observation: string | null;
  previousBalance: number;
  newBalance: number;
  receivedAt: string;
  receivedBy: string;
}

const balanceText = (balance: number) => (balance < 0 ? `Debe ${formatMoney(-balance)}` : balance > 0 ? `A favor ${formatMoney(balance)}` : 'Sin saldo');

/** Recibo de cobro (abono fuera de factura) para impresora térmica de 80 o 58 mm. */
export function PaymentReceipt({ receipt, width = 80 }: { receipt: ReceiptData; width?: PaperWidth }) {
  const rule = <hr className="my-2 border-t border-dashed border-black/60" />;
  return (
    <div id="ticket-print" data-paper={width} className={cn('mx-auto max-w-full bg-white p-4 font-mono leading-snug text-black shadow-sm', paperClass[width])}>
      <div className="text-center">
        <p className={cn('font-bold', width === 58 ? 'text-[11px]' : 'text-[13px]')}>DISTRIBUIDORA VICROS C.A.</p>
        <p>RIF J-301798740</p>
        <p className="mt-1 font-bold">RECIBO DE COBRO</p>
        <p>{formatDateTime(receipt.receivedAt)}</p>
      </div>
      {rule}
      <p className="font-bold">Cliente</p>
      <p>
        {receipt.clientCode} · {receipt.clientName}
      </p>
      <p>RIF/CI {receipt.clientRif}</p>
      {rule}
      <p className="flex justify-between text-[1.15em] font-bold">
        <span>Monto recibido</span>
        <span>{formatMoney(receipt.amount)}</span>
      </p>
      {receipt.observation && <p className="whitespace-pre-line">{receipt.observation}</p>}
      {rule}
      <p className="flex justify-between">
        <span>Saldo anterior</span>
        <span>{balanceText(receipt.previousBalance)}</span>
      </p>
      <p className="flex justify-between font-bold">
        <span>Saldo actual</span>
        <span>{balanceText(receipt.newBalance)}</span>
      </p>
      <p className="mt-1">Recibió: {receipt.receivedBy}</p>
      <div className="mt-6 flex flex-col gap-6">
        <p className="border-t border-black/70 pt-1 text-center">Firma del cliente</p>
      </div>
      <p className="mt-3 text-center">Gracias por su pago</p>
    </div>
  );
}
