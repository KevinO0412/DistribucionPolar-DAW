import { useId } from 'react';
import type { Category } from '../../api/types';
import { addDays, toIsoDate } from '../../lib/format';
import { cn } from '../../lib/cn';

export interface DashboardFilterValue {
  from: string;
  to: string;
  categoryId: string;
}

const PRESETS = [
  { key: 'today', label: 'Hoy', days: 1 },
  { key: '7', label: '7 días', days: 7 },
  { key: '30', label: '30 días', days: 30 },
  { key: '90', label: '90 días', days: 90 },
] as const;

export function presetRange(days: number, today = new Date()) {
  return { from: toIsoDate(addDays(today, -(days - 1))), to: toIsoDate(today) };
}

interface Props {
  value: DashboardFilterValue;
  categories: Category[];
  onChange: (value: DashboardFilterValue) => void;
}

/** Rango de fechas (atajos o personalizado) y categoría. Cada cambio vuelve a consultar la API. */
export function DashboardFilters({ value, categories, onChange }: Props) {
  const id = useId();
  const today = toIsoDate(new Date());
  const activePreset = PRESETS.find((p) => {
    const range = presetRange(p.days);
    return range.from === value.from && range.to === value.to;
  })?.key;

  return (
    <div className="panel flex flex-wrap items-end gap-x-6 gap-y-4 p-4">
      <fieldset>
        <legend className="mb-1.5 text-sm font-medium">Período</legend>
        <div className="flex flex-wrap rounded-md border border-line bg-sunken p-0.5" role="group">
          {PRESETS.map((preset) => (
            <button
              key={preset.key}
              type="button"
              aria-pressed={activePreset === preset.key}
              onClick={() => onChange({ ...value, ...presetRange(preset.days) })}
              className={cn(
                'h-9 cursor-pointer rounded px-3 text-sm font-medium transition-colors duration-150',
                activePreset === preset.key ? 'bg-surface text-ink shadow-sm' : 'text-muted hover:text-ink',
              )}
            >
              {preset.label}
            </button>
          ))}
        </div>
      </fieldset>

      <div className="flex flex-wrap items-end gap-3">
        <div className="flex flex-col gap-1.5">
          <label htmlFor={`${id}-from`} className="text-sm font-medium">
            Desde
          </label>
          <input
            id={`${id}-from`}
            type="date"
            value={value.from}
            max={value.to}
            onChange={(e) => e.target.value && onChange({ ...value, from: e.target.value })}
            className="h-10 rounded-md border border-line bg-surface px-2 text-sm"
          />
        </div>
        <div className="flex flex-col gap-1.5">
          <label htmlFor={`${id}-to`} className="text-sm font-medium">
            Hasta
          </label>
          <input
            id={`${id}-to`}
            type="date"
            value={value.to}
            min={value.from}
            max={today}
            onChange={(e) => e.target.value && onChange({ ...value, to: e.target.value })}
            className="h-10 rounded-md border border-line bg-surface px-2 text-sm"
          />
        </div>
      </div>

      <div className="flex min-w-[12rem] flex-1 flex-col gap-1.5 sm:flex-none">
        <label htmlFor={`${id}-category`} className="text-sm font-medium">
          Categoría
        </label>
        <select
          id={`${id}-category`}
          value={value.categoryId}
          onChange={(e) => onChange({ ...value, categoryId: e.target.value })}
          className="h-10 rounded-md border border-line bg-surface px-2 text-sm"
        >
          <option value="">Todas las categorías</option>
          {categories.map((c) => (
            <option key={c.id} value={c.id}>
              {c.name}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
}
