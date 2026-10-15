import type { PaperWidth, RouteSettlement } from '../../api/types';
import { formatDateTime, formatMoney } from '../../lib/format';
import { ThermalPaper, TicketHeader, TicketRule } from '../print/ThermalPaper';

const signed = (value: number) => `${value > 0 ? '+' : ''}${formatMoney(value)}`;

/** Planilla de cuadre de la liquidación ciega, imprimible en papel térmico de 80 o 58 mm. */
export function SettlementSheet({ settlement, width }: { settlement: RouteSettlement; width: PaperWidth }) {
  return (
    <ThermalPaper width={width}>
      <TicketHeader title="PLANILLA DE CUADRE" subtitle={`Camión ${settlement.truckName} (${settlement.truckPlate})`} />
      <p className="text-center">Salida {formatDateTime(settlement.dispatchedAt)}</p>
      <p className="text-center">Liquidación {formatDateTime(settlement.settledAt)}</p>
      <TicketRule />
      <p className="font-bold">Dinero</p>
      <p className="flex justify-between"><span>Ventas ({settlement.invoicesCount} fact.)</span><span>{formatMoney(settlement.salesTotal)}</span></p>
      <p className="flex justify-between"><span>Esperado (abonos)</span><span>{formatMoney(settlement.expectedCash)}</span></p>
      <p className="flex justify-between"><span>Declarado</span><span>{formatMoney(settlement.countedCash)}</span></p>
      <p className="flex justify-between font-bold">
        <span>{settlement.cashDifference < 0 ? 'Faltante' : settlement.cashDifference > 0 ? 'Sobrante' : 'Diferencia'}</span>
        <span>{signed(settlement.cashDifference)}</span>
      </p>
      <TicketRule />
      <p className="font-bold">Mercancía (esperado / contado)</p>
      {settlement.lines.length === 0 && <p>Sin mercancía de vuelta.</p>}
      {settlement.lines.map((line) => (
        <div key={line.productId} className="mt-1">
          <p>{line.productName}</p>
          <p className="flex justify-between">
            <span>{line.expectedDisplay} / {line.countedDisplay}</span>
            <span>{line.differenceUnits === 0 ? 'OK' : `${line.differenceUnits > 0 ? '+' : ''}${line.differenceUnits} und`}</span>
          </p>
        </div>
      ))}
      <p className="mt-1 flex justify-between font-bold"><span>Faltante de mercancía</span><span>{formatMoney(settlement.missingGoodsValue)}</span></p>
      <TicketRule />
      <p className="text-center font-bold">{settlement.isBalanced ? 'RUTA CUADRADA' : 'RUTA CON DIFERENCIAS'}</p>
      {settlement.observation && <p className="mt-1 whitespace-pre-line">Obs.: {settlement.observation}</p>}
      <p className="mt-1">Liquidó: {settlement.settledBy ?? ''}</p>
      <div className="mt-6 flex flex-col gap-6">
        <p className="border-t border-black/70 pt-1 text-center">Chofer / vendedor</p>
        <p className="border-t border-black/70 pt-1 text-center">Supervisor</p>
      </div>
    </ThermalPaper>
  );
}
