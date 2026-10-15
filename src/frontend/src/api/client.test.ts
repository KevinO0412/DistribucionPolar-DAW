import { AxiosError, AxiosHeaders } from 'axios';
import { ApiError, toApiError } from './client';

function axiosError(status: number | null, data?: unknown) {
  const config = { headers: new AxiosHeaders(), url: '/products' };
  const response = status === null ? undefined : { status, data, statusText: '', headers: {}, config };
  return new AxiosError('fallo', 'ERR', config, null, response);
}

describe('toApiError (RFC 7807)', () => {
  it('lee title y detail del Problem Details', () => {
    const error = toApiError(axiosError(404, { title: 'Recurso no encontrado', status: 404, detail: 'No existe el producto 1.' }));
    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(404);
    expect(error.title).toBe('Recurso no encontrado');
    expect(error.message).toBe('No existe el producto 1.');
  });

  it('expone los errores por campo de un 400 de validación', () => {
    const error = toApiError(axiosError(400, { title: 'Uno o más datos no son válidos.', errors: { PriceBox: ['El precio por caja debe ser mayor que 0.'] } }));
    expect(error.fieldError('priceBox')).toBe('El precio por caja debe ser mayor que 0.');
  });

  it('explica el 403 aunque la API no envíe cuerpo', () => {
    expect(toApiError(axiosError(403, '')).message).toBe('Tu rol no tiene permiso para esta acción.');
  });

  it('distingue la falta de conexión', () => {
    expect(toApiError(axiosError(null)).status).toBe(0);
  });
});
