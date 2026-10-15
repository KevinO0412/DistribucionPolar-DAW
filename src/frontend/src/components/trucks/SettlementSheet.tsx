import type { TruckSettlement } from '../../api/types';
import { cn } from '../../lib/cn';
import { formatDateTime, formatMoney } from '../../lib/format';
import { paperClass, type PaperWidth } from '../../lib/print';

/** Diferencia con signo delante del monto: +USD 23,34 / -USD 5,00. */
const signed = (value: number) => (value > 0 ? `+${formatMoney(value)}` : value < 0 ? `-${formatMoney(-value)}` : formatMoney(0));
const signedUnits = (value: number) => (value > 0 ? `+${value} und` : `${value} und`);

/** Planilla de cuadre de la liquidación ciega, para imprimir en papel térmico y firmar. */
export function SettlementSheet({ settlement, width = 80 }: { settlement: TruckSettlement; width?: PaperWidth }) {
  const rule = <hr className="my-2 border-t border-dashed border-black/60" />;
  const differences = settlement.lines.filter((l) => l.differenceUnits !== 0);
  return (
    <div id="ticket-print" data-paper={width} className={cn('mx-auto max-w-full bg-white p-4 font-mono leading-snug text-black shadow-sm', paperClass[width])}>
      <div className="text-center">
        <p className={cn('font-bold', width === 58 ? 'text-[11px]' : 'text-[13px]')}>DISTRIBUIDORA VICROS C.A.</p>
        <p className="mt-1 font-bold">PLANILLA DE CUADRE</p>
        <p>
          Camión {settlement.truckName} ({settlement.truckPlate})
        </p>
        <p>Salida: {formatDateTime(settlement.tripStartedAt)}</p>
        <p>Liquidación: {formatDateTime(settlement.settledAt)}</p>
        <p className="mt-1 font-bold">{settlement.isBalanced ? 'RUTA CUADRADA' : 'RUTA CON DIFERENCIAS'}</p>
      </div>
      {rule}
      <p className="font-bold">Efectivo</p>
      <p className="flex justify-between"><span>Esperado</span><span>{formatMoney(settlement.expectedCash)}</span></p>
      <p className="flex justify-between"><span>Entregado</span><span>{formatMoney(settlement.countedCash)}</span></p>
      <p className="flex justify-between font-bold"><span>Diferencia</span><span>{signed(settlement.cashDifference)}</span></p>
      {rule}
      <p className="font-bold">Mercancía devuelta</p>
      <p className="flex justify-between"><span>Esperada</span><span>{formatMoney(settlement.expectedMerchandiseValue)}</span></p>
      <p className="flex justify-between"><span>Contada</span><span>{formatMoney(settlement.countedMerchandiseValue)}</span></p>
      <p className="flex justify-between font-bold"><span>Diferencia</span><span>{signed(settlement.merchandiseDifferenceValue)}</span></p>
      {differences.length > 0 && (
        <>
          {rule}
          <p className="font-bold">Productos con diferencia</p>
          {differences.map((l) => (
            <div key={l.productId} className="mt-1">
              <p>{l.productName}</p>
              <p className="flex justify-between">
                <span>esp. {l.expectedDisplay} / cont. {l.countedDisplay}</span>
              </p>
              <p className="flex justify-between">
                <span>{signedUnits(l.differenceUnits)}</span>
                <span>{signed(l.differenceValue)}</span>
              </p>
            </div>
          ))}
        </>
      )}
      {settlement.observation && (
        <>
          {rule}
          <p className="font-bold">Observación</p>
          <p className="whitespace-pre-line">{settlement.observation}</p>
        </>
      )}
      <p className="mt-2">Liquidó: {settlement.settledBy ?? '—'}</p>
      <div className="mt-6 flex flex-col gap-6">
        <p className="border-t border-black/70 pt-1 text-center">Chofer / vendedor</p>
        <p className="border-t border-black/70 pt-1 text-center">Recibe en galpón</p>
      </div>
    </div>
  );
}
