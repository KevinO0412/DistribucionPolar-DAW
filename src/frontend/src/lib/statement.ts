import type { ClientMovement, ClientMovementType, ClientStatement } from '../api/types';

export const MOVEMENT_LABELS: Record<ClientMovementType, string> = {
  Invoice: 'Factura',
  Gift: 'Regalía',
  Payment: 'Abono',
  EmptiesGenerated: 'Vacíos de factura',
  EmptiesReturned: 'Devolución de vacíos',
  InvoiceCancelled: 'Anulación',
  BalanceAdjustment: 'Ajuste de saldo',
};

/**
 * Efecto de un movimiento en el saldo de dinero (convención de la API: negativo = el cliente debe).
 * Factura: −total (el abono al facturar llega como movimiento "Abono" aparte, así que la suma es −pendiente).
 * Abono, anulación (pendiente revertido) y ajuste (después − antes): +monto. Regalías y vacíos no mueven dinero.
 */
export function moneyEffect(movement: ClientMovement): number {
  switch (movement.type) {
    case 'Invoice':
      return -movement.amount;
    case 'Payment':
    case 'InvoiceCancelled':
    case 'BalanceAdjustment':
      return movement.amount;
    default:
      return 0;
  }
}

export interface StatementRow extends ClientMovement {
  /** Saldo de dinero del cliente justo después del movimiento. */
  balanceAfter: number;
}

/**
 * Une las pestañas del estado de cuenta en una sola lista, de más reciente a más antiguo, y calcula el saldo
 * resultante de cada movimiento recorriendo hacia atrás desde el saldo actual del cliente.
 */
export function statementRows(statement: ClientStatement): StatementRow[] {
  const all = [...statement.invoices, ...statement.gifts, ...statement.payments, ...statement.empties, ...statement.adjustments];
  // Desempate estable: dentro de una misma factura, primero el detalle de vacíos, luego el abono y al final la factura.
  const order: Record<ClientMovementType, number> = {
    EmptiesGenerated: 0, Payment: 1, Invoice: 2, Gift: 2, EmptiesReturned: 3, InvoiceCancelled: 3, BalanceAdjustment: 3,
  };
  all.sort((a, b) => time(b.date) - time(a.date) || order[a.type] - order[b.type]);

  let balance = statement.client.moneyBalance;
  return all.map((movement) => {
    const row = { ...movement, balanceAfter: round2(balance) };
    balance -= moneyEffect(movement);
    return row;
  });
}

export interface DebtAgingSummary {
  debt: number;
  /** Días de la parte más antigua de la deuda; null si no debe; 999 si una parte no la explica ninguna factura. */
  oldestDays: number | null;
  buckets: { label: string; amount: number }[];
}

const BUCKETS: { label: string; from: number; to: number | null }[] = [
  { label: '0–7 días', from: 0, to: 7 },
  { label: '8–15 días', from: 8, to: 15 },
  { label: '16–30 días', from: 16, to: 30 },
  { label: 'Más de 30 días', from: 31, to: null },
];

const UNEXPLAINED_AGE = 999;
const DAY_MS = 86_400_000;

/**
 * Antigüedad de la deuda del cliente con la misma imputación del dominio (Core.Domain/Services/DebtAging.cs):
 * los abonos saldan primero lo más antiguo, así que la deuda actual corresponde a los pendientes más recientes;
 * lo que ninguna factura explica (saldo inicial o ajuste) es lo más antiguo.
 * El pendiente de cada factura vigente = total − abono hecho al facturarla (movimientos con el mismo InvoiceId).
 */
export function debtAging(statement: ClientStatement, now: Date = new Date()): DebtAgingSummary {
  const debt = Math.max(0, -statement.client.moneyBalance);
  const cancelled = new Set(statement.empties.filter((m) => m.type === 'InvoiceCancelled').map((m) => m.invoiceId));
  const sales = statement.invoices
    .filter((m) => m.invoiceId && !cancelled.has(m.invoiceId))
    .map((m) => ({
      date: m.date,
      pending: m.amount - statement.payments.filter((p) => p.invoiceId === m.invoiceId).reduce((s, p) => s + p.amount, 0),
    }))
    .filter((s) => s.pending > 0)
    .sort((a, b) => time(b.date) - time(a.date));

  const parts: { age: number; amount: number }[] = [];
  let remaining = debt;
  for (const sale of sales) {
    if (remaining <= 0) break;
    const take = Math.min(remaining, sale.pending);
    parts.push({ age: Math.max(0, Math.floor((now.getTime() - new Date(sale.date).getTime()) / DAY_MS)), amount: take });
    remaining -= take;
  }
  if (remaining > 0.004) parts.push({ age: UNEXPLAINED_AGE, amount: remaining });

  return {
    debt,
    oldestDays: parts.length === 0 ? null : Math.max(...parts.map((p) => p.age)),
    buckets: BUCKETS.map((b) => ({
      label: b.label,
      amount: round2(parts.filter((p) => p.age >= b.from && (b.to === null || p.age <= b.to)).reduce((s, p) => s + p.amount, 0)),
    })),
  };
}

const time = (iso: string) => new Date(iso).getTime();

function round2(value: number) {
  return Math.round(value * 100) / 100;
}
