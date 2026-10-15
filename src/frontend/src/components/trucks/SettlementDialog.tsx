import { useState } from 'react';
import { Printer } from 'lucide-react';
import type { TruckSettlement } from '../../api/types';
import { printThermal, type PaperWidth } from '../../lib/print';
import { Button } from '../ui/Button';
import { Modal } from '../ui/Modal';
import { PaperWidthSelector } from './PaperWidthSelector';
import { SettlementSheet } from './SettlementSheet';

export function SettlementDialog({ settlement, onClose }: { settlement: TruckSettlement; onClose: () => void }) {
  const [width, setWidth] = useState<PaperWidth>(80);
  return (
    <Modal
      open
      onClose={onClose}
      title="Planilla de cuadre"
      description={`Camión ${settlement.truckName}: esperado frente a lo contado.`}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>Cerrar</Button>
          <Button onClick={() => printThermal(width)}>
            <Printer className="h-4 w-4" aria-hidden />
            Imprimir planilla
          </Button>
        </>
      }
    >
      <div className="mb-3 flex justify-center">
        <PaperWidthSelector value={width} onChange={setWidth} />
      </div>
      <div className="rounded-md bg-sunken p-3">
        <SettlementSheet settlement={settlement} width={width} />
      </div>
    </Modal>
  );
}
