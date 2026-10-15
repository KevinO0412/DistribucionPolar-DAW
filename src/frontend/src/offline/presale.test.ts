import { buildSaleRequest, estimateTotal, mergeLines } from './presale';

const products = [
  { id: 'cerveza', priceBox: 23, priceUnit: 0.75 },
  { id: 'harina', priceBox: 30, priceUnit: 1.6 },
];

describe('preventa', () => {
  it('estima el total con precio por caja y por unidad', () => {
    expect(estimateTotal([{ productId: 'cerveza', boxes: 2, looseUnits: 4 }, { productId: 'harina', boxes: 1, looseUnits: 0 }], products)).toBe(79);
  });

  it('une líneas repetidas y descarta las vacías', () => {
    const merged = mergeLines([
      { productId: 'cerveza', productName: 'Polar', boxes: 1, looseUnits: 2 },
      { productId: 'harina', productName: 'Harina', boxes: 0, looseUnits: 0 },
      { productId: 'cerveza', productName: 'Polar', boxes: 2, looseUnits: 1 },
    ]);
    expect(merged).toEqual([{ productId: 'cerveza', productName: 'Polar', boxes: 3, looseUnits: 3 }]);
  });

  it('arma la venta desde camión o desde galpón', () => {
    const lines = [{ productId: 'cerveza', productName: 'Polar', boxes: 1, looseUnits: 0 }];
    expect(buildSaleRequest({ clientId: 'c1', truckId: 't1', lines, payment: 10, observation: ' efectivo ' })).toMatchObject({
      dispatchOrigin: 'Truck',
      truckId: 't1',
      items: [{ productId: 'cerveza', boxes: 1, looseUnits: 0 }],
      paymentObservation: 'efectivo',
    });
    expect(buildSaleRequest({ clientId: 'c1', truckId: null, lines, payment: 0, observation: '' })).toMatchObject({
      dispatchOrigin: 'Warehouse',
      truckId: null,
      paymentObservation: null,
    });
  });
});
