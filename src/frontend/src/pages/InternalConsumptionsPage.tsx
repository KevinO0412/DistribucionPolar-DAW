import { useMemo, useState, type FormEvent } from 'react';
import { PackageMinus } from 'lucide-react';
import { toApiError } from '../api/client';
import { employeesApi, internalConsumptionsApi, productsApi, trucksApi } from '../api/endpoints';
import type { InternalConsumption, InternalConsumptionType } from '../api/types';
import { DataTable } from '../components/dashboard/DataTable';
import { presetRange } from '../components/dashboard/DashboardFilters';
import { PageHeader } from '../components/layout/PageHeader';
import { Button } from '../components/ui/Button';
import { SelectField, TextField } from '../components/ui/Field';
import { Modal } from '../components/ui/Modal';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { cn } from '../lib/cn';
import { formatDateTime, formatInt, formatMoney } from '../lib/format';

const RANGES = [
  { days: 1, label: 'Hoy' },
  { days: 7, label: '7 días' },
  { days: 30, label: '30 días' },
];

export const CONSUMPTION_TYPES: Record<InternalConsumptionType, string> = {
  InternalConsumption: 'Consumo interno',
  Gift: 'Regalía a empleado',
};

const toInt = (value: string) => (value.trim() === '' ? 0 : Number(value));

type Field = 'productId' | 'truckId' | 'employeeId' | 'boxes' | 'looseUnits' | 'observation';
type Errors = Partial<Record<Field | 'form', string>>;

async function loadOptions() {
  const [products, trucks, employees] = await Promise.all([productsApi.list(), trucksApi.list(), employeesApi.list()]);
  return { products, trucks, employees: employees.filter((e) => e.isActive) };
}

/** Formulario de consumo interno: sale del galpón o de un camión, a nombre de un empleado activo, sin cliente ni cobro. */
function ConsumptionDialog({ onClose, onSaved }: { onClose: () => void; onSaved: (c: InternalConsumption) => void }) {
  const options = useApiQuery(() => loadOptions(), []);
  const [truckId, setTruckId] = useState('');
  const [productSearch, setProductSearch] = useState('');
  const [productId, setProductId] = useState('');
  const [boxes, setBoxes] = useState('0');
  const [looseUnits, setLooseUnits] = useState('1');
  const [employeeId, setEmployeeId] = useState('');
  const [type, setType] = useState<InternalConsumptionType>('InternalConsumption');
  const [observation, setObservation] = useState('');
  const [errors, setErrors] = useState<Errors>({});
  const [isSaving, setIsSaving] = useState(false);

  const truck = options.data?.trucks.find((t) => t.id === truckId);
  const products = useMemo(() => {
    const list = truck
      ? truck.stock.map((s) => ({ id: s.productId, name: s.productName, stockDisplay: s.stockDisplay }))
      : (options.data?.products ?? []).filter((p) => p.stockUnits > 0).map((p) => ({ id: p.id, name: p.name, stockDisplay: p.stockDisplay }));
    const term = productSearch.trim().toLowerCase();
    return term ? list.filter((p) => p.name.toLowerCase().includes(term) || p.id === productId) : list;
  }, [options.data, truck, productSearch, productId]);

  function validate(): Errors {
    const found: Errors = {};
    if (!productId) found.productId = 'Elige el producto.';
    if (!employeeId) found.employeeId = 'Elige el empleado.';
    const b = toInt(boxes);
    const u = toInt(looseUnits);
    if (!Number.isInteger(b) || b < 0) found.boxes = 'Entero mayor o igual que 0.';
    if (!Number.isInteger(u) || u < 0) found.looseUnits = 'Entero mayor o igual que 0.';
    if (!found.boxes && !found.looseUnits && b === 0 && u === 0) found.form = 'Indica cajas o unidades sueltas (no pueden ser ambas 0).';
    return found;
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    const found = validate();
    setErrors(found);
    if (Object.keys(found).length > 0) return;

    setIsSaving(true);
    try {
      onSaved(await internalConsumptionsApi.create({
        productId,
        boxes: toInt(boxes),
        looseUnits: toInt(looseUnits),
        origin: truck ? 'Truck' : 'Warehouse',
        truckId: truck?.id ?? null,
        employeeId,
        type,
        observation: observation.trim() || null,
      }));
    } catch (err) {
      // RFC 7807: errores de validación junto a su campo; reglas de negocio (inventario insuficiente, empleado inactivo) arriba.
      const apiError = toApiError(err);
      const fields: Field[] = ['productId', 'truckId', 'employeeId', 'boxes', 'looseUnits', 'observation'];
      const byField = Object.fromEntries(fields.map((f) => [f, apiError.fieldError(f)]).filter(([, v]) => v)) as Errors;
      setErrors({ ...byField, form: apiError.fieldError('') ?? (Object.keys(byField).length > 0 ? undefined : apiError.message) });
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open
      onClose={onClose}
      size="lg"
      title="Registrar consumo interno"
      description="Sale del inventario sin cliente y sin cobro (valor cobrado $0). Queda registrado a nombre del empleado y auditado."
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="consumption-form" isLoading={isSaving} disabled={!options.data}>
            Registrar consumo
          </Button>
        </>
      }
    >
      {options.error ? (
        <ErrorState error={options.error} onRetry={options.reload} />
      ) : !options.data ? (
        <LoadingState label="Cargando productos y empleados…" />
      ) : (
        <form id="consumption-form" onSubmit={submit} noValidate className="flex flex-col gap-4">
          {errors.form && (
            <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
              {errors.form}
            </p>
          )}
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <SelectField label="Sale de" value={truckId} error={errors.truckId} onChange={(e) => { setTruckId(e.target.value); setProductId(''); }}>
              <option value="">Galpón</option>
              {options.data.trucks.map((t) => (
                <option key={t.id} value={t.id}>
                  Camión {t.name}
                </option>
              ))}
            </SelectField>
            <SelectField label="Tipo" value={type} onChange={(e) => setType(e.target.value as InternalConsumptionType)}>
              {Object.entries(CONSUMPTION_TYPES).map(([value, label]) => (
                <option key={value} value={value}>
                  {label}
                </option>
              ))}
            </SelectField>
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <TextField label="Buscar producto" type="search" value={productSearch} onChange={(e) => setProductSearch(e.target.value)} placeholder="Nombre del producto" />
            <SelectField label="Producto" value={productId} error={errors.productId} onChange={(e) => setProductId(e.target.value)}>
              <option value="">Elige un producto ({products.length})</option>
              {products.slice(0, 300).map((p) => (
                <option key={p.id} value={p.id}>
                  {p.name} ({p.stockDisplay})
                </option>
              ))}
            </SelectField>
          </div>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-[6rem_6rem_minmax(0,1fr)]">
            <TextField label="Cajas" inputMode="numeric" value={boxes} error={errors.boxes} onChange={(e) => setBoxes(e.target.value)} />
            <TextField label="Unidades" inputMode="numeric" value={looseUnits} error={errors.looseUnits} onChange={(e) => setLooseUnits(e.target.value)} />
            <SelectField label="Empleado" value={employeeId} error={errors.employeeId} onChange={(e) => setEmployeeId(e.target.value)} wrapperClassName="col-span-2 sm:col-span-1">
              <option value="">Elige un empleado</option>
              {options.data.employees.map((e) => (
                <option key={e.id} value={e.id}>
                  {e.name} ({e.role})
                </option>
              ))}
            </SelectField>
          </div>
          <TextField label="Motivo (opcional)" value={observation} error={errors.observation} onChange={(e) => setObservation(e.target.value)} maxLength={500} placeholder="Por ejemplo: refrigerio del personal de carga" />
        </form>
      )}
    </Modal>
  );
}

/**
 * Consumos internos (solo Admin, como exige la API): mercancía que sale del galpón o de un camión para uso de la
 * empresa o como regalía a un empleado, sin cliente ni cobro. Lista los registrados en el período.
 */
export function InternalConsumptionsPage() {
  const notify = useToast();
  const [days, setDays] = useState(30);
  const [isOpen, setIsOpen] = useState(false);
  const range = presetRange(days);
  const list = useApiQuery((signal) => internalConsumptionsApi.list(range, signal), [range.from, range.to]);

  return (
    <>
      <PageHeader
        title="Consumos internos"
        description="Mercancía que sale del inventario sin cliente ni cobro: consumo de la empresa o regalía a un empleado."
        actions={
          <Button onClick={() => setIsOpen(true)}>
            <PackageMinus className="h-4 w-4" aria-hidden />
            Registrar consumo
          </Button>
        }
      />
      <div className="panel mb-4 flex flex-wrap items-center gap-4 p-4">
        <div className="flex rounded-md border border-line bg-sunken p-0.5" role="group" aria-label="Período">
          {RANGES.map((r) => (
            <button
              key={r.days}
              type="button"
              aria-pressed={days === r.days}
              onClick={() => setDays(r.days)}
              className={cn('h-9 cursor-pointer rounded px-3 text-sm font-medium', days === r.days ? 'bg-surface text-ink shadow-sm' : 'text-muted hover:text-ink')}
            >
              {r.label}
            </button>
          ))}
        </div>
        {list.data && (
          <p className="ml-auto text-sm text-muted">
            {formatInt(list.data.count)} registros, valor de referencia {formatMoney(list.data.totalValue)} (cobrado $0)
          </p>
        )}
      </div>

      {list.error ? (
        <ErrorState error={list.error} onRetry={list.reload} />
      ) : !list.data ? (
        <LoadingState />
      ) : list.data.items.length === 0 ? (
        <EmptyState title="No hay consumos en este período">Elige un período más largo o registra uno nuevo.</EmptyState>
      ) : (
        <div className="panel p-4 sm:p-2">
          <DataTable
            caption="Consumos internos registrados"
            rows={list.data.items}
            rowKey={(c) => c.id}
            columns={[
              { header: 'Fecha', cell: (c) => <span className="whitespace-nowrap">{formatDateTime(c.occurredAt)}</span> },
              { header: 'Producto', cell: (c) => <><p className="font-medium">{c.productName}</p><p className="text-muted">{c.origin === 'Truck' ? `Camión ${c.truckName ?? ''}` : 'Galpón'}</p></> },
              { header: 'Cantidad', align: 'right', cell: (c) => <><p className="whitespace-nowrap">{formatInt(c.boxes)} cj + {formatInt(c.looseUnits)} und</p><p className="text-muted">{formatInt(c.totalUnits)} und</p></> },
              { header: 'Empleado', cell: (c) => <><p>{c.employeeName}</p><p className="text-muted">{CONSUMPTION_TYPES[c.type]}</p></> },
              { header: 'Motivo', cell: (c) => <span className="break-words">{c.observation ?? '—'}</span> },
              { header: 'Valor ref.', align: 'right', cell: (c) => <span title="Valor a precio de venta; no se cobra">{formatMoney(c.estimatedValue)}</span> },
            ]}
          />
        </div>
      )}

      {isOpen && (
        <ConsumptionDialog
          onClose={() => setIsOpen(false)}
          onSaved={(c) => {
            notify(`Consumo registrado: ${c.productName}, ${formatInt(c.totalUnits)} und a nombre de ${c.employeeName}.`);
            setIsOpen(false);
            list.reload();
          }}
        />
      )}
    </>
  );
}
