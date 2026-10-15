import { useState, type FormEvent } from 'react';
import { toApiError, type ApiError } from '../api/client';
import type { Category, CreateProductRequest, Product, ProductFields } from '../api/types';
import { Button } from '../components/ui/Button';
import { CheckboxField, SelectField, TextField } from '../components/ui/Field';
import { Modal } from '../components/ui/Modal';
import { ProductThumb } from '../components/ui/ProductThumb';

export const EMPTY_GROUPS = [
  { key: 'RET_222ML_X36', name: '222ML RET x36' },
  { key: 'RET_POLAR_PILSEN_330ML_X24', name: 'Polar Pilsen 330ML RET x24' },
  { key: 'PEPSI_125L_X6', name: 'Pepsi/Sabores 1.25L x6' },
  { key: 'PEPSI_350ML_X24', name: 'Pepsi/Sabores 350ML x24' },
];

type FormState = Record<
  'name' | 'brand' | 'categoryId' | 'priceBox' | 'priceUnit' | 'costPrice' | 'unitsPerBox' | 'initialBoxes' | 'initialLooseUnits' | 'minStock' | 'maxStock' | 'emptyGroupKey',
  string
> & { isReturnable: boolean; isGiftEligible: boolean };

type Errors = Partial<Record<keyof FormState, string>>;

function toForm(product: Product | null, categories: Category[]): FormState {
  return {
    name: product?.name ?? '',
    brand: product?.brand ?? '',
    categoryId: product?.categoryId ?? categories[0]?.id ?? '',
    priceBox: product ? String(product.priceBox) : '',
    priceUnit: product ? String(product.priceUnit) : '',
    costPrice: product ? String(product.costPrice) : '',
    unitsPerBox: product ? String(product.unitsPerBox) : '',
    initialBoxes: '0',
    initialLooseUnits: '0',
    minStock: product ? String(product.minStock) : '0',
    maxStock: product ? String(product.maxStock) : '',
    emptyGroupKey: product?.emptyGroupKey ?? '',
    isReturnable: product?.isReturnable ?? false,
    isGiftEligible: product?.isGiftEligible ?? false,
  };
}

const num = (value: string) => Number(value.replace(',', '.'));

/** Mismas reglas que los validadores FluentValidation del backend, para avisar antes de enviar. */
export function validateProduct(form: FormState, isNew: boolean): Errors {
  const errors: Errors = {};
  if (!form.name.trim()) errors.name = 'Escribe el nombre del producto.';
  else if (form.name.trim().length > 150) errors.name = 'Máximo 150 caracteres.';
  if (!form.brand.trim()) errors.brand = 'Escribe la marca.';
  if (!form.categoryId) errors.categoryId = 'Elige una categoría.';
  if (!(num(form.priceBox) > 0)) errors.priceBox = 'El precio por caja debe ser mayor que 0.';
  if (!(num(form.priceUnit) > 0)) errors.priceUnit = 'El precio por unidad debe ser mayor que 0.';
  if (form.costPrice === '' || num(form.costPrice) < 0) errors.costPrice = 'El costo no puede ser negativo.';
  if (!Number.isInteger(num(form.unitsPerBox)) || num(form.unitsPerBox) <= 0) errors.unitsPerBox = 'Debe ser un entero mayor que 0.';
  if (!Number.isInteger(num(form.minStock)) || num(form.minStock) < 0) errors.minStock = 'Debe ser un entero mayor o igual que 0.';
  if (!Number.isInteger(num(form.maxStock)) || num(form.maxStock) <= num(form.minStock)) errors.maxStock = 'Debe ser mayor que el stock mínimo.';
  if (isNew) {
    if (!Number.isInteger(num(form.initialBoxes)) || num(form.initialBoxes) < 0) errors.initialBoxes = 'Entero mayor o igual que 0.';
    if (!Number.isInteger(num(form.initialLooseUnits)) || num(form.initialLooseUnits) < 0) errors.initialLooseUnits = 'Entero mayor o igual que 0.';
  }
  return errors;
}

function serverErrors(error: ApiError): Errors {
  const keys: (keyof FormState)[] = ['name', 'brand', 'categoryId', 'priceBox', 'priceUnit', 'costPrice', 'unitsPerBox', 'initialBoxes', 'initialLooseUnits', 'minStock', 'maxStock', 'emptyGroupKey'];
  return Object.fromEntries(keys.map((k) => [k, error.fieldError(k)]).filter(([, v]) => v)) as Errors;
}

interface Props {
  open: boolean;
  product: Product | null;
  categories: Category[];
  onClose: () => void;
  onSubmit: (body: CreateProductRequest | ProductFields) => Promise<void>;
}

export function ProductForm({ open, product, categories, onClose, onSubmit }: Props) {
  const isNew = product === null;
  const [form, setForm] = useState<FormState>(() => toForm(product, categories));
  const [errors, setErrors] = useState<Errors>({});
  const [formError, setFormError] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const set = <K extends keyof FormState>(key: K, value: FormState[K]) => setForm((f) => ({ ...f, [key]: value }));

  async function submit(event: FormEvent) {
    event.preventDefault();
    const found = validateProduct(form, isNew);
    setErrors(found);
    setFormError(null);
    if (Object.keys(found).length > 0) return;

    const fields: ProductFields = {
      name: form.name.trim(),
      brand: form.brand.trim(),
      categoryId: form.categoryId,
      priceBox: num(form.priceBox),
      priceUnit: num(form.priceUnit),
      costPrice: num(form.costPrice),
      unitsPerBox: num(form.unitsPerBox),
      minStock: num(form.minStock),
      maxStock: num(form.maxStock),
      isReturnable: form.isReturnable,
      emptyGroupKey: form.emptyGroupKey || null,
      isGiftEligible: form.isGiftEligible,
    };
    setIsSaving(true);
    try {
      await onSubmit(isNew ? { ...fields, initialBoxes: num(form.initialBoxes), initialLooseUnits: num(form.initialLooseUnits) } : fields);
    } catch (err) {
      const apiError = toApiError(err);
      setErrors(serverErrors(apiError));
      setFormError(apiError.message);
    } finally {
      setIsSaving(false);
    }
  }

  const numeric = { inputMode: 'decimal' as const };

  return (
    <Modal
      open={open}
      onClose={onClose}
      size="lg"
      title={isNew ? 'Nuevo producto' : `Editar ${product.name}`}
      description={isNew ? 'El SKU se genera solo a partir de la categoría y el nombre.' : `SKU ${product.sku}. El stock cambia solo con operaciones de inventario.`}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="product-form" isLoading={isSaving}>
            {isNew ? 'Crear producto' : 'Guardar cambios'}
          </Button>
        </>
      }
    >
      <form id="product-form" onSubmit={submit} noValidate className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        {!isNew && <ProductThumb productId={product.id} name={product.name} className="h-28 w-28 sm:col-span-2" />}
        {formError && (
          <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm sm:col-span-2" role="alert">
            {formError}
          </p>
        )}
        <TextField label="Nombre" value={form.name} onChange={(e) => set('name', e.target.value)} error={errors.name} wrapperClassName="sm:col-span-2" maxLength={150} />
        <TextField label="Marca" value={form.brand} onChange={(e) => set('brand', e.target.value)} error={errors.brand} maxLength={80} />
        <SelectField label="Categoría" value={form.categoryId} onChange={(e) => set('categoryId', e.target.value)} error={errors.categoryId}>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </SelectField>
        <TextField label="Precio por caja (USD)" {...numeric} value={form.priceBox} onChange={(e) => set('priceBox', e.target.value)} error={errors.priceBox} />
        <TextField label="Precio por unidad (USD)" {...numeric} value={form.priceUnit} onChange={(e) => set('priceUnit', e.target.value)} error={errors.priceUnit} hint="Independiente del precio por caja." />
        <TextField label="Costo por caja (USD)" {...numeric} value={form.costPrice} onChange={(e) => set('costPrice', e.target.value)} error={errors.costPrice} />
        <TextField label="Unidades por caja" inputMode="numeric" value={form.unitsPerBox} onChange={(e) => set('unitsPerBox', e.target.value)} error={errors.unitsPerBox} />
        {isNew && (
          <>
            <TextField label="Stock inicial: cajas" inputMode="numeric" value={form.initialBoxes} onChange={(e) => set('initialBoxes', e.target.value)} error={errors.initialBoxes} />
            <TextField label="Stock inicial: unidades sueltas" inputMode="numeric" value={form.initialLooseUnits} onChange={(e) => set('initialLooseUnits', e.target.value)} error={errors.initialLooseUnits} />
          </>
        )}
        <TextField label="Stock mínimo (und)" inputMode="numeric" value={form.minStock} onChange={(e) => set('minStock', e.target.value)} error={errors.minStock} hint="Por debajo se marca como crítico." />
        <TextField label="Stock máximo (und)" inputMode="numeric" value={form.maxStock} onChange={(e) => set('maxStock', e.target.value)} error={errors.maxStock} hint="Capacidad del depósito." />
        <SelectField label="Grupo de vacíos" value={form.emptyGroupKey} onChange={(e) => set('emptyGroupKey', e.target.value)} error={errors.emptyGroupKey} wrapperClassName="sm:col-span-2" hint="Solo los productos de un grupo generan vacíos al venderse.">
          <option value="">No genera vacíos</option>
          {EMPTY_GROUPS.map((g) => (
            <option key={g.key} value={g.key}>
              {g.name}
            </option>
          ))}
        </SelectField>
        <CheckboxField label="Envase retornable" checked={form.isReturnable} onChange={(e) => set('isReturnable', e.target.checked)} />
        <CheckboxField label="Autorizado para regalías" checked={form.isGiftEligible} onChange={(e) => set('isGiftEligible', e.target.checked)} />
      </form>
    </Modal>
  );
}
