/** Anchos de papel térmico soportados: 80 mm (estándar) y 58 mm (impresoras POS portátiles). */
export type PaperWidth = 80 | 58;

const PX_PER_MM = 96 / 25.4;

/**
 * Imprime el ticket (#ticket-print) en papel térmico del ancho indicado.
 * - Copia el ticket a #print-root, fuera del diálogo: dentro del <dialog> modal la impresión lo centraba y lo recortaba.
 * - Fija @page con el ancho del papel y el alto real del ticket (Chrome ignora "size: 80mm auto" e imprimía en carta).
 * La hoja de estilos y la copia son temporales y se quitan al terminar.
 */
export function printThermal(width: PaperWidth, print: () => void = () => window.print()) {
  const source = document.getElementById('ticket-print');
  if (!source) return;

  const root = document.createElement('div');
  root.id = 'print-root';
  const copy = source.cloneNode(true) as HTMLElement;
  copy.removeAttribute('id');
  copy.className = '';
  copy.style.cssText = `width:${width}mm;padding:2mm ${width === 58 ? 2 : 4}mm;font-size:${width === 58 ? 10 : 11}px;` +
    'font-family:ui-monospace,Menlo,Consolas,monospace;line-height:1.35;color:#000;background:#fff;box-sizing:border-box;';
  root.appendChild(copy);
  document.body.appendChild(root);

  // Alto del ticket ya maquetado con el ancho del papel (mínimo 40 mm) + 4 mm de margen de corte.
  const heightMm = Math.max(40, Math.ceil((copy.getBoundingClientRect().height || 0) / PX_PER_MM) + 4);
  const style = document.createElement('style');
  style.setAttribute('data-thermal', String(width));
  style.textContent = `@media print { @page { size: ${width}mm ${heightMm}mm; margin: 0; } }`;
  document.head.appendChild(style);

  let done = false;
  const cleanup = () => {
    if (done) return;
    done = true;
    style.remove();
    root.remove();
    window.removeEventListener('afterprint', cleanup);
  };
  window.addEventListener('afterprint', cleanup);
  print();
  // Algunos navegadores no emiten afterprint; print() bloquea hasta cerrar el diálogo de impresión.
  window.setTimeout(cleanup, 1000);
}

/** Clases de vista previa en pantalla para cada ancho. */
export const paperClass: Record<PaperWidth, string> = {
  80: 'w-[80mm] text-[11px]',
  58: 'w-[58mm] text-[10px]',
};
