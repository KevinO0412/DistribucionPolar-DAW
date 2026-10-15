import { toApiError } from '../api/client';
import type { CreateSaleRequest, Product, SaleLineRequest } from '../api/types';
import { cacheGet, cacheSet } from './store';

export interface DraftLine extends SaleLineRequest {
  productName: string;
}

/** Total estimado del pedido con los precios del catálogo guardado (la API recalcula el definitivo). */
export function estimateTotal(lines: SaleLineRequest[], products: Pick<Product, 'id' | 'priceBox' | 'priceUnit'>[]) {
  const byId = new Map(products.map((p) => [p.id, p]));
  const total = lines.reduce((sum, line) => {
    const product = byId.get(line.productId);
    return product ? sum + line.boxes * product.priceBox + line.looseUnits * product.priceUnit : sum;
  }, 0);
  return Math.round(total * 100) / 100;
}

/** Une líneas repetidas del mismo producto y descarta las vacías. */
export function mergeLines(lines: DraftLine[]): DraftLine[] {
  const merged = new Map<string, DraftLine>();
  for (const line of lines) {
    if (line.boxes <= 0 && line.looseUnits <= 0) continue;
    const current = merged.get(line.productId);
    merged.set(line.productId, current ? { ...current, boxes: current.boxes + line.boxes, looseUnits: current.looseUnits + line.looseUnits } : { ...line });
  }
  return [...merged.values()];
}

export function buildSaleRequest(input: {
  clientId: string;
  truckId: string | null;
  lines: DraftLine[];
  payment: number;
  observation: string;
}): CreateSaleRequest {
  return {
    clientId: input.clientId,
    dispatchOrigin: input.truckId ? 'Truck' : 'Warehouse',
    truckId: input.truckId,
    employeeId: null,
    items: mergeLines(input.lines).map(({ productId, boxes, looseUnits }) => ({ productId, boxes, looseUnits })),
    payment: input.payment,
    paymentObservation: input.observation.trim() || null,
    returnedEmpties: null,
  };
}

/**
 * Consulta la API y guarda una copia local; si no hay conexión, devuelve la última copia guardada.
 * Así el vendedor puede armar pedidos con el catálogo, los clientes y los camiones aunque esté sin señal.
 */
export async function fetchWithCache<T>(key: string, fetcher: () => Promise<T>): Promise<{ data: T; savedAt: string | null }> {
  try {
    const data = await fetcher();
    await cacheSet(key, data);
    return { data, savedAt: null };
  } catch (err) {
    const error = toApiError(err);
    if (error.status !== 0) throw error;
    const cached = await cacheGet<T>(key);
    if (!cached) throw error;
    return { data: cached.value, savedAt: cached.savedAt };
  }
}
