import { printThermal } from './print';

describe('printThermal', () => {
  it('imprime una copia del ticket fuera del diálogo, con @page del ancho elegido, y la quita al terminar', () => {
    document.body.innerHTML = '<dialog open><div id="ticket-print" class="w-[80mm]"><p>RECIBO DE COBRO</p></div></dialog>';
    let pageRule = '';
    let copyText = '';
    let copyWidth = '';

    printThermal(58, () => {
      pageRule = document.head.querySelector('style[data-thermal]')?.textContent ?? '';
      const copy = document.querySelector('#print-root > div') as HTMLElement;
      copyText = copy.textContent ?? '';
      copyWidth = copy.style.width;
    });

    expect(pageRule).toMatch(/@page \{ size: 58mm \d+mm; margin: 0; \}/);
    expect(copyText).toBe('RECIBO DE COBRO');
    expect(copyWidth).toBe('58mm');
    expect(document.querySelectorAll('#ticket-print')).toHaveLength(1);
    window.dispatchEvent(new Event('afterprint'));
    expect(document.getElementById('print-root')).toBeNull();
    expect(document.head.querySelector('style[data-thermal]')).toBeNull();
  });

  it('sin ticket en pantalla no imprime', () => {
    document.body.innerHTML = '';
    const print = vi.fn();
    printThermal(80, print);
    expect(print).not.toHaveBeenCalled();
  });
});
