import { useMemo, useState, type FormEvent } from 'react';
import { Gift, Plus, Trash2 } from 'lucide-react';
import { toApiError } from '../../api/client';
import { clientsApi, invoicesApi, trucksApi } from '../../api/endpoints';
import type { Invoice } from '../../api/types';
import { useApiQuery } from '../../hooks/useApiQuery';
import { Button } from '../ui/Button';
import { SelectField, TextField } from '../ui/Field';
import { Modal } from '../ui/Modal';
import { ErrorState, LoadingState } from '../ui/States';

interface Line {
  productId: string;
  boxes: string;
  looseUnits: string;
}

type Errors = Partial<Record<'clientId' | 'truckId' | 'form', string>> & { lines?: Record<number, string> };

const emptyLine = (): Line => ({ productId: '', boxes: '1', looseUnits: '0' });
const toInt = (value: string) => (value.trim() === '' ? 0 : Number(value));

/** Productos autorizados, clientes y camiones (con su carga) para armar la regalía. */
async function loadOptions() {
  const [products, clients, trucks] = await Promise.all([invoicesApi.giftProducts(), clientsApi.list(), trucksApi.list()]);
  return { products, clients, trucks };
}

/**
 * Regalía a cliente (POST /api/invoices/gifts). Solo ofrece productos con IsGiftEligible = true; desde un camión,
 * solo los autorizados que ese camión lleva. Descuenta stock y genera vacíos, pero no cobra ni cambia el saldo de dinero.
 */
export function GiftDialog({ onClose, onCreated }: { onClose: () => void; onCreated: (gift: Invoice) => void }) {
  const options = useApiQuery(() => loadOptions(), []);
  const [truckId, setTruckId] = useState('');
  const [clientSearch, setClientSearch] = useState('');
  const [clientId, setClientId] = useState('');
  const [lines, setLines] = useState<Line[]>([emptyLine()]);
  const [errors, setErrors] = useState<Errors>({});
  const [isSaving, setIsSaving] = useState(false);

  const truck = options.data?.trucks.find((t) => t.id === truckId);
  const available = useMemo(() => {
    const eligible = options.data?.products ?? [];
    if (!truck) return eligible.map((p) => ({ id: p.id, name: p.name, stockDisplay: p.stockDisplay, returnable: Boolean(p.emptyGroupKey) }));
    const loaded = new Map(truck.stock.map((s) => [s.productId, s.stockDisplay]));
    return eligible
      .filter((p) => loaded.has(p.id))
      .map((p) => ({ id: p.id, name: p.name, stockDisplay: loaded.get(p.id) ?? '', returnable: Boolean(p.emptyGroupKey) }));
  }, [options.data, truck]);
  const clients = useMemo(() => {
    const term = clientSearch.trim().toLowerCase();
    const list = options.data?.clients ?? [];
    return term ? list.filter((c) => `${c.code} ${c.name} ${c.rif} ${c.nickname ?? ''}`.toLowerCase().includes(term)) : list;
  }, [options.data, clientSearch]);

  const setLine = (index: number, patch: Partial<Line>) => setLines((list) => list.map((l, i) => (i === index ? { ...l, ...patch } : l)));

  function validate(): Errors {
    const found: Errors = {};
    if (!clientId) found.clientId = 'Elige el cliente.';
    const lineErrors: Record<number, string> = {};
    lines.forEach((line, i) => {
      const boxes = toInt(line.boxes);
      const loose = toInt(line.looseUnits);
      if (!line.productId) lineErrors[i] = 'Elige un producto autorizado.';
      else if (!Number.isInteger(boxes) || !Number.isInteger(loose) || boxes < 0 || loose < 0) lineErrors[i] = 'Cajas y unidades deben ser enteros mayores o iguales que 0.';
      else if (boxes === 0 && loose === 0) lineErrors[i] = 'Indica cajas o unidades.';
      else if (lines.findIndex((l) => l.productId === line.productId) !== i) lineErrors[i] = 'Este producto ya está en otra línea.';
    });
    if (Object.keys(lineErrors).length > 0) found.lines = lineErrors;
    return found;
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const found = validate();
    setErrors(found);
    if (Object.keys(found).length > 0) return;

    setIsSaving(true);
    try {
      const gift = await invoicesApi.createGift({
        clientId,
        dispatchOrigin: truck ? 'Truck' : 'Warehouse',
        truckId: truck?.id ?? null,
        items: lines.map((l) => ({ productId: l.productId, boxes: toInt(l.boxes), looseUnits: toInt(l.looseUnits) })),
      });
      onCreated(gift);
    } catch (err) {
      // Errores RFC 7807: los de validación van junto a su campo; las reglas de negocio (stock, no autorizado) arriba.
      const apiError = toApiError(err);
      const lineErrors: Record<number, string> = {};
      lines.forEach((_, i) => {
        const message = apiError.fieldError(`items[${i}].boxes`) ?? apiError.fieldError(`items[${i}].looseUnits`) ?? apiError.fieldError(`items[${i}].productId`);
        if (message) lineErrors[i] = message;
      });
      const clientError = apiError.fieldError('clientId');
      const truckError = apiError.fieldError('truckId');
      const fieldHandled = Boolean(clientError || truckError || Object.keys(lineErrors).length);
      setErrors({
        clientId: clientError,
        truckId: truckError,
        lines: lineErrors,
        form: apiError.fieldError('items') ?? (fieldHandled ? undefined : apiError.message),
      });
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open
      onClose={onClose}
      size="lg"
      title="Nueva regalía"
      description="Sin cobro: no cambia el saldo de dinero del cliente. Descuenta el inventario y genera vacíos si el producto es retornable."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="gift-form" isLoading={isSaving} disabled={!options.data}>
            <Gift className="h-4 w-4" aria-hidden />
            Registrar regalía
          </Button>
        </>
      }
    >
      {options.error ? (
        <ErrorState error={options.error} onRetry={options.reload} />
      ) : !options.data ? (
        <LoadingState label="Cargando productos autorizados…" />
      ) : (
        <form id="gift-form" onSubmit={submit} noValidate className="flex flex-col gap-4">
          {errors.form && (
            <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
              {errors.form}
            </p>
          )}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <SelectField label="Despacho" value={truckId} error={errors.truckId}
              onChange={(e) => { setTruckId(e.target.value); setLines([emptyLine()]); }}>
              <option value="">Galpón</option>
              {options.data.trucks.map((t) => (
                <option key={t.id} value={t.id}>
                  Camión {t.name} ({t.status})
                </option>
              ))}
            </SelectField>
            <TextField label="Buscar cliente" type="search" value={clientSearch} onChange={(e) => setClientSearch(e.target.value)} placeholder="Nombre, RIF o código" />
          </div>
          <SelectField label="Cliente" value={clientId} onChange={(e) => setClientId(e.target.value)} error={errors.clientId}>
            <option value="">Elige un cliente ({clients.length})</option>
            {clients.slice(0, 200).map((c) => (
              <option key={c.id} value={c.id}>
                {c.code} {c.name}
              </option>
            ))}
          </SelectField>

          <fieldset className="flex flex-col gap-3">
            <legend className="mb-1 text-sm font-medium">Productos autorizados para regalía</legend>
            {available.length === 0 && (
              <p className="text-sm text-muted">{truck ? 'Este camión no lleva productos autorizados para regalía.' : 'No hay productos autorizados para regalía.'}</p>
            )}
            {lines.map((line, index) => (
              <div key={index} className="flex flex-col gap-1">
                <div className="grid grid-cols-[minmax(0,1fr)_4.5rem_4.5rem_auto] items-end gap-2">
                  <SelectField label={index === 0 ? 'Producto' : `Producto ${index + 1}`} value={line.productId}
                    aria-invalid={Boolean(errors.lines?.[index])} onChange={(e) => setLine(index, { productId: e.target.value })}>
                    <option value="">Elige…</option>
                    {available.map((p) => (
                      <option key={p.id} value={p.id}>
                        {p.name} ({p.stockDisplay}){p.returnable ? ', retornable' : ''}
                      </option>
                    ))}
                  </SelectField>
                  <TextField label="Cajas" inputMode="numeric" value={line.boxes} onChange={(e) => setLine(index, { boxes: e.target.value })} />
                  <TextField label="Und" inputMode="numeric" value={line.looseUnits} onChange={(e) => setLine(index, { looseUnits: e.target.value })} />
                  <Button variant="ghost" onClick={() => setLines((l) => l.filter((_, i) => i !== index))} aria-label={`Quitar producto ${index + 1}`} disabled={lines.length === 1}>
                    <Trash2 className="h-4 w-4" aria-hidden />
                  </Button>
                </div>
                {errors.lines?.[index] && (
                  <p className="text-sm text-status-out" role="alert">
                    {errors.lines[index]}
                  </p>
                )}
              </div>
            ))}
            <Button variant="secondary" size="sm" className="self-start" onClick={() => setLines((l) => [...l, emptyLine()])} disabled={lines.length >= available.length}>
              <Plus className="h-4 w-4" aria-hidden />
              Agregar producto
            </Button>
          </fieldset>
        </form>
      )}
    </Modal>
  );
}
