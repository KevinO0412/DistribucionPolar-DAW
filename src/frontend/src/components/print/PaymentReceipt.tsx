import type { Client, PaperWidth } from '../../api/types';
import { formatDateTime, formatMoney } from '../../lib/format';
import { ThermalPaper, TicketHeader, TicketRule } from './ThermalPaper';

export interface PaymentReceiptData {
  client: Client;
  amount: number;
  balanceBefore: number;
  observation: string | null;
  receivedAt: string;
  receivedBy: string;
}

const balanceText = (balance: number) =>
  balance < 0 ? `Debe ${formatMoney(-balance)}` : balance > 0 ? `A favor ${formatMoney(balance)}` : 'Sin saldo';

/** Recibo de cobro de un abono, para impresora térmica de 80 o 58 mm. */
export function PaymentReceipt({ data, width }: { data: PaymentReceiptData; width: PaperWidth }) {
  return (
    <ThermalPaper width={width}>
      <TicketHeader title="RECIBO DE COBRO" subtitle={formatDateTime(data.receivedAt)} />
      <TicketRule />
      <p className="font-bold">Cliente</p>
      <p>
        {data.client.code} {data.client.name}
      </p>
      <p>RIF/CI {data.client.rif}</p>
      <TicketRule />
      <p className="flex justify-between font-bold" style={{ fontSize: '1.15em' }}>
        <span>Abono recibido</span>
        <span>{formatMoney(data.amount)}</span>
      </p>
      <p className="flex justify-between">
        <span>Saldo anterior</span>
        <span>{balanceText(data.balanceBefore)}</span>
      </p>
      <p className="flex justify-between">
        <span>Saldo actual</span>
        <span>{balanceText(data.client.moneyBalance)}</span>
      </p>
      {data.observation && (
        <>
          <TicketRule />
          <p className="font-bold">Observación</p>
          <p className="whitespace-pre-line">{data.observation}</p>
        </>
      )}
      <TicketRule />
      <p>Recibió: {data.receivedBy}</p>
      <div className="mt-6 flex flex-col gap-6">
        <p className="border-t border-black/70 pt-1 text-center">Firma del cliente</p>
      </div>
    </ThermalPaper>
  );
}
