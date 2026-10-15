import { cn } from '../../lib/cn';
import type { PaperWidth } from '../../lib/print';

/** Selector del ancho del papel térmico (80 mm o 58 mm). */
export function PaperWidthSelector({ value, onChange }: { value: PaperWidth; onChange: (width: PaperWidth) => void }) {
  return (
    <div className="flex rounded-md border border-line bg-sunken p-0.5" role="group" aria-label="Ancho del papel">
      {([80, 58] as const).map((width) => (
        <button
          key={width}
          type="button"
          aria-pressed={value === width}
          onClick={() => onChange(width)}
          className={cn(
            'h-9 cursor-pointer rounded px-3 text-sm font-medium',
            value === width ? 'bg-surface text-ink shadow-sm' : 'text-muted hover:text-ink',
          )}
        >
          {width} mm
        </button>
      ))}
    </div>
  );
}
