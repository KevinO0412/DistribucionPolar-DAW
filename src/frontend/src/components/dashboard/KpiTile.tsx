import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

interface KpiTileProps {
  label: string;
  value: string;
  detail?: ReactNode;
  tone?: 'default' | 'alert';
}

/** Indicador: cifra grande en Barlow Condensed con su contexto debajo. */
export function KpiTile({ label, value, detail, tone = 'default' }: KpiTileProps) {
  return (
    <div className={cn('panel flex flex-col gap-1 p-4', tone === 'alert' && 'border-status-out/50')}>
      <p className="text-sm font-medium text-muted">{label}</p>
      <p className={cn('font-display text-figure font-semibold', tone === 'alert' && 'text-status-out')}>{value}</p>
      {detail && <div className="text-sm text-muted">{detail}</div>}
    </div>
  );
}
