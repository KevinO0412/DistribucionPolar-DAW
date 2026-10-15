const money = new Intl.NumberFormat('es-VE', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 });
const integer = new Intl.NumberFormat('es-VE', { maximumFractionDigits: 0 });
const decimal = new Intl.NumberFormat('es-VE', { maximumFractionDigits: 2 });

export const formatMoney = (value: number) => money.format(value);
export const formatInt = (value: number) => integer.format(value);
export const formatDecimal = (value: number) => decimal.format(value);
export const formatPercent = (value: number) => `${decimal.format(value)} %`;

/** Moneda compacta para ejes: $1,2 mil. */
export const formatMoneyShort = (value: number) =>
  new Intl.NumberFormat('es-VE', { style: 'currency', currency: 'USD', notation: 'compact', maximumFractionDigits: 1 }).format(value);

export function formatDate(value: string | Date, options: Intl.DateTimeFormatOptions = { dateStyle: 'medium' }) {
  const date = typeof value === 'string' ? new Date(value.length === 10 ? `${value}T12:00:00` : value) : value;
  return new Intl.DateTimeFormat('es-VE', options).format(date);
}

export const formatDateTime = (value: string) => formatDate(value, { dateStyle: 'short', timeStyle: 'short' });

/** Fecha local en formato ISO (YYYY-MM-DD) para los filtros de la API. */
export function toIsoDate(date: Date) {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
}

export function addDays(date: Date, days: number) {
  const copy = new Date(date);
  copy.setDate(copy.getDate() + days);
  return copy;
}
