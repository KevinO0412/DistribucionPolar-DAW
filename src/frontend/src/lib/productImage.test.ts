import { productImageUrl } from './productImage';

describe('productImageUrl', () => {
  it('devuelve la foto de un producto sembrado', () => {
    expect(productImageUrl('00000000-0000-4000-8000-000000000082')).toBe('/img/productos/alimentos/buen_cafe_100x30un.webp');
  });

  it('devuelve null para un producto creado desde la aplicación', () => {
    expect(productImageUrl('3f2b8c1e-9a4d-4e6f-8b1a-2c3d4e5f6a7b')).toBeNull();
  });
});
