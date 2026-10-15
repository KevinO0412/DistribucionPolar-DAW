import { stockStatus } from '../components/ui/StatusBadge';
import { validateProduct } from './ProductForm';

const valid = {
  name: 'HARINA PAN 1KGx20UN', brand: 'P.A.N.', categoryId: 'c1', priceBox: '18.50', priceUnit: '0,95', costPrice: '14.80',
  unitsPerBox: '20', initialBoxes: '2', initialLooseUnits: '5', minStock: '100', maxStock: '800', emptyGroupKey: '',
  isReturnable: false, isGiftEligible: false,
};

describe('validateProduct', () => {
  it('acepta un producto válido (con coma decimal)', () => {
    expect(validateProduct(valid, true)).toEqual({});
  });

  it('aplica las mismas reglas que FluentValidation', () => {
    const errors = validateProduct({ ...valid, priceBox: '-1', maxStock: '50', name: ' ' }, true);
    expect(Object.keys(errors).sort()).toEqual(['maxStock', 'name', 'priceBox']);
  });
});

describe('stockStatus', () => {
  it.each([
    [0, 'OutOfStock'],
    [45, 'CriticalRisk'],
    [300, 'Understocked'],
    [600, 'Optimal'],
    [800, 'Overstocked'],
    [900, 'Excessive'],
  ])('%i und con mínimo 100 y máximo 800 → %s', (stock, status) => {
    expect(stockStatus(stock, 100, 800)).toBe(status);
  });
});
