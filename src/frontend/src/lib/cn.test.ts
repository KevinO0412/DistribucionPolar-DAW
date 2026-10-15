import { cn } from './cn';

describe('cn', () => {
  it('resuelve conflictos de Tailwind', () => {
    expect(cn('p-2', 'p-4')).toBe('p-4');
  });

  it('conserva el tamaño propio junto a un color de texto', () => {
    expect(cn('text-figure', 'text-status-out')).toBe('text-figure text-status-out');
  });
});
