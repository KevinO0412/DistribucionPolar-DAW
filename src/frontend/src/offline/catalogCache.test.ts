import 'fake-indexeddb/auto';
import { AxiosError, AxiosHeaders } from 'axios';
import { resetDbForTests } from './db';
import { cachedFetch } from './catalogCache';

const offline = () => new AxiosError('Network Error', 'ERR_NETWORK', { headers: new AxiosHeaders() });

beforeEach(async () => {
  await resetDbForTests();
  indexedDB.deleteDatabase('dp-preventa');
});

describe('caché del catálogo', () => {
  it('sin conexión devuelve la última copia guardada', async () => {
    await cachedFetch('products', async () => [{ id: 'p1' }]);

    const cached = await cachedFetch('products', async () => Promise.reject(offline()));

    expect(cached.data).toEqual([{ id: 'p1' }]);
    expect(cached.savedAt).not.toBeNull();
  });

  it('con conexión devuelve los datos frescos', async () => {
    const fresh = await cachedFetch('clients', async () => ['a']);
    expect(fresh).toEqual({ data: ['a'], savedAt: null });
  });

  it('sin conexión y sin copia previa propaga el error', async () => {
    await expect(cachedFetch('trucks', async () => Promise.reject(offline()))).rejects.toBeInstanceOf(AxiosError);
  });
});
