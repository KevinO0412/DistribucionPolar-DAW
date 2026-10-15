import { fireEvent, render, screen } from '@testing-library/react';
import { ProductThumb } from './ProductThumb';

describe('ProductThumb', () => {
  it('muestra la foto con el nombre como texto alternativo', () => {
    render(<ProductThumb productId="00000000-0000-4000-8000-000000000082" name="BUEN CAFÉ 100X30UN" />);
    expect(screen.getByRole('img', { name: 'BUEN CAFÉ 100X30UN' })).toHaveAttribute('src', '/img/productos/alimentos/buen_cafe_100x30un.webp');
  });

  it('cambia al ícono si la foto no carga', () => {
    render(<ProductThumb productId="00000000-0000-4000-8000-000000000082" name="BUEN CAFÉ 100X30UN" />);
    fireEvent.error(screen.getByRole('img'));
    expect(screen.queryByRole('img')).toBeNull();
  });
});
