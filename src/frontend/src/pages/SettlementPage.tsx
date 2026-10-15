import { useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, ClipboardCheck } from 'lucide-react';
import { trucksApi } from '../api/endpoints';
import { toApiError, type ApiError } from '../api/client';
import type { TruckSettlement } from '../api/types';
import { PageHeader } from '../components/layout/PageHeader';
import { SettlementDialog } from '../components/trucks/SettlementDialog';
import { Button } from '../components/ui/Button';
import { TextField } from '../components/ui/Field';
import { ErrorState, LoadingState } from '../components/ui/States';
import { useApiQuery } from '../hooks/useApiQuery';
import { formatDateTime } from '../lib/format';

type Counts = Record<string, { boxes: string; looseUnits: string }>;

/**
 * Liquidación ciega de la ruta (observación 1): el chofer cuenta el efectivo y la mercancía que devuelve sin ver
 * lo esperado. La planilla con las diferencias aparece recién después de enviar el conteo.
 */
export function SettlementPage() {
  const { id = '' } = useParams();
  const form = useApiQuery(() => trucksApi.settlementForm(id), [id]);
  const [counts, setCounts] = useState<Counts>({});
  const [cash, setCash] = useState('');
  const [observation, setObservation] = useState('');
  const [error, setError] = useState<ApiError | null>(null);
  const [confirming, setConfirming] = useState(false);
  const [saving, setSaving] = useState(false);
  const [sheet, setSheet] = useState<TruckSettlement | null>(null);

  const setCount = (productId: string, field: 'boxes' | 'looseUnits', value: string) =>
    setCounts((current) => ({ ...current, [productId]: { ...(current[productId] ?? { boxes: '', looseUnits: '' }), [field]: value } }));

  async function submit() {
    setSaving(true);
    setError(null);
    try {
      const items = Object.entries(counts)
        .map(([productId, c]) => ({ productId, boxes: Number(c.boxes || 0), looseUnits: Number(c.looseUnits || 0) }))
        .filter((c) => c.boxes > 0 || c.looseUnits > 0);
      setSheet(await trucksApi.settle(id, { cashCounted: Number(cash || 0), items, observation: observation.trim() || null }));
    } catch (err) {
      setError(toApiError(err));
      setConfirming(false);
    } finally {
      setSaving(false);
    }
  }

  const back = (
    <Link to="/camiones" className="inline-flex items-center gap-1 text-sm font-medium text-brand">
      <ArrowLeft className="h-4 w-4" aria-hidden />
      Volver a camiones
    </Link>
  );

  if (sheet) {
    return (
      <>
        <PageHeader title="Ruta liquidada" description="El conteo quedó registrado. La planilla compara lo esperado con lo que se contó." actions={back} />
        <SettlementDialog settlement={sheet} onClose={() => setSheet(sheet)} />
      </>
    );
  }

  return (
    <>
      <PageHeader
        title={form.data ? `Liquidar ${form.data.truckName}` : 'Liquidación de ruta'}
        description="Cuenta el efectivo y la mercancía que vuelve. Por ser ciega, aquí no se muestra lo esperado: las diferencias aparecen al enviar."
        actions={back}
      />
      {form.error ? (
        <ErrorState error={form.error} onRetry={form.reload} />
      ) : !form.data ? (
        <LoadingState />
      ) : (
        <form
          className="panel flex flex-col gap-5 p-4 sm:p-5"
          onSubmit={(e) => {
            e.preventDefault();
            setConfirming(true);
          }}
        >
          <p className="text-sm text-muted">
            Camión {form.data.truckName} ({form.data.truckPlate}){form.data.dispatchedAt ? `, salió ${formatDateTime(form.data.dispatchedAt)}` : ''}.
            Lo que no cuentes se toma como 0.
          </p>
          <div className="grid grid-cols-1 gap-4 sm:max-w-md">
            <TextField label="Efectivo entregado (USD)" type="number" min={0} step="0.01" inputMode="decimal" required value={cash}
              onChange={(e) => setCash(e.target.value)} error={error?.fieldError('cashCounted')} />
          </div>
          <fieldset className="flex flex-col gap-2">
            <legend className="mb-2 text-sm font-medium">Mercancía devuelta</legend>
            <ul className="flex flex-col divide-y divide-line rounded-md border border-line">
              {form.data.products.map((p) => (
                <li key={p.productId} className="grid grid-cols-2 items-end gap-3 px-3 py-3 sm:grid-cols-[minmax(0,1fr)_7rem_7rem]">
                  <div className="col-span-2 min-w-0 sm:col-span-1">
                    <p className="truncate font-medium">{p.name}</p>
                    <p className="text-sm text-muted">{p.sku} · caja de {p.unitsPerBox}</p>
                  </div>
                  <TextField label="Cajas" aria-label={`Cajas de ${p.name}`} type="number" min={0} inputMode="numeric"
                    value={counts[p.productId]?.boxes ?? ''} onChange={(e) => setCount(p.productId, 'boxes', e.target.value)} />
                  <TextField label="Unidades" aria-label={`Unidades de ${p.name}`} type="number" min={0} inputMode="numeric"
                    value={counts[p.productId]?.looseUnits ?? ''} onChange={(e) => setCount(p.productId, 'looseUnits', e.target.value)} />
                </li>
              ))}
            </ul>
          </fieldset>
          <TextField label="Observación" value={observation} onChange={(e) => setObservation(e.target.value)} placeholder="Ej.: una caja rota" />
          {error && !error.fieldError('cashCounted') && (
            <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">{error.message}</p>
          )}
          {confirming ? (
            <div className="flex flex-wrap items-center gap-3 rounded-md border border-status-critical/50 bg-status-critical/10 p-3" role="alert">
              <p className="text-sm">Al enviar, el conteo queda registrado y el camión pasa a Liquidado. No se puede corregir después.</p>
              <div className="ml-auto flex gap-2">
                <Button variant="secondary" onClick={() => setConfirming(false)}>Revisar</Button>
                <Button onClick={() => void submit()} isLoading={saving}>Confirmar liquidación</Button>
              </div>
            </div>
          ) : (
            <Button type="submit" className="self-end">
              <ClipboardCheck className="h-4 w-4" aria-hidden />
              Enviar conteo
            </Button>
          )}
        </form>
      )}
    </>
  );
}
