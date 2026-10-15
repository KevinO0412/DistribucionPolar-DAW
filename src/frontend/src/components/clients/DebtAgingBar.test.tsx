import { render, screen } from '@testing-library/react';
import { DebtAgingBar } from './DebtAgingBar';

describe('DebtAgingBar', () => {
  it('muestra cada tramo con su texto y su monto, y la barra proporcional', () => {
    const { container } = render(<DebtAgingBar aging={{ days0To30: 140, days31To60: 100, days61To90: 0, over90: 100 }} />);

    for (const label of ['0-30 días', '31-60 días', '61-90 días', 'Más de 90 días']) expect(screen.getByText(label)).toBeInTheDocument();
    const segments = container.querySelectorAll('[role="img"] > div');
    expect(segments).toHaveLength(3);
    expect((segments[0] as HTMLElement).style.width).toBe(`${(140 / 340) * 100}%`);
  });

  it('sin deuda deja la barra vacía', () => {
    const { container } = render(<DebtAgingBar aging={{ days0To30: 0, days31To60: 0, days61To90: 0, over90: 0 }} />);
    expect(container.querySelectorAll('[role="img"] > div')).toHaveLength(0);
  });
});
