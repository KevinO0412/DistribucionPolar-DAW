import { useState, type ReactNode } from 'react';
import type { PaperWidth } from '../../api/types';
import { cn } from '../../lib/cn';

const STORAGE_KEY = 'dp.paper';

function readWidth(): PaperWidth {
  try {
    return localStorage.getItem(STORAGE_KEY) === '58' ? 58 : 80;
  } catch {
    return 80;
  }
}

/** Ancho del papel de la impresora térmica (80 o 58 mm), recordado en el dispositivo. */
export function usePaperWidth() {
  const [width, setWidthState] = useState<PaperWidth>(readWidth);
  const setWidth = (next: PaperWidth) => {
    setWidthState(next);
    try {
      localStorage.setItem(STORAGE_KEY, String(next));
    } catch {
      /* sin almacenamiento */
    }
  };
  return { width, setWidth };
}

export function PaperWidthToggle({ width, onChange }: { width: PaperWidth; onChange: (width: PaperWidth) => void }) {
  return (
    <div className="flex rounded-md border border-line bg-sunken p-0.5" role="group" aria-label="Ancho del papel">
      {([80, 58] as const).map((w) => (
        <button
          key={w}
          type="button"
          aria-pressed={width === w}
          onClick={() => onChange(w)}
          className={cn('h-8 cursor-pointer rounded px-3 text-sm font-medium', width === w ? 'bg-surface text-ink shadow-sm' : 'text-muted hover:text-ink')}
        >
          {w} mm
        </button>
      ))}
    </div>
  );
}

/**
 * Papel térmico para impresoras POS portátiles. Al imprimir solo se ve este bloque (index.css) y la página toma el
 * ancho elegido; en 58 mm la letra es más pequeña para que quepan las columnas.
 */
export function ThermalPaper({ width, children }: { width: PaperWidth; children: ReactNode }) {
  return (
    <>
      <style>{`@media print { @page { size: ${width}mm auto; margin: 0; } #ticket-print { width: ${width}mm !important; } }`}</style>
      <div
        id="ticket-print"
        data-paper={width}
        style={{ width: `${width}mm` }}
        className={cn(
          'mx-auto max-w-full bg-white font-mono leading-snug text-black shadow-sm',
          width === 58 ? 'p-2 text-[9.5px]' : 'p-4 text-[11px]',
        )}
      >
        {children}
      </div>
    </>
  );
}

export const TicketRule = () => <hr className="my-2 border-t border-dashed border-black/60" />;

export function TicketHeader({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="text-center">
      <p className="font-bold" style={{ fontSize: '1.2em' }}>
        DISTRIBUCIÓN POLAR C.A.
      </p>
      <p>RIF J-00000000-0</p>
      <p className="mt-1 font-bold">{title}</p>
      {subtitle && <p>{subtitle}</p>}
    </div>
  );
}
