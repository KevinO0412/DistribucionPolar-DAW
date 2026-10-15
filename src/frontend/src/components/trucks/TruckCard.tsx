import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';
import { formatDecimal, formatInt, formatPercent } from '../../lib/format';
import { TruckStatusBadge } from './TruckStatusBadge';

interface TruckCardProps {
  name: string;
  plate: string;
  capacityBoxes: number;
  loadedBoxes: number;
  occupancyPercent: number;
  /** Estado de la ruta (Cargando, En ruta, Liquidado…); si no se indica no se muestra. */
  status?: string;
  children?: ReactNode;
}

/** Nivel de ocupación con texto: el color nunca es la única señal. */
export function occupancyLevel(percent: number) {
  if (percent > 100) return { label: 'Sobrecargado', tone: 'text-status-out' };
  if (percent >= 85) return { label: 'Casi lleno', tone: 'text-status-critical' };
  if (percent > 0) return { label: 'Con carga', tone: 'text-ink' };
  return { label: 'Vacío', tone: 'text-muted' };
}

/**
 * Camión de perfil: la caja de carga se llena de gaveras según la ocupación (cajas cargadas / capacidad).
 * Es el elemento distintivo del dashboard y de la pantalla de camiones.
 */
export function TruckCard({ name, plate, capacityBoxes, loadedBoxes, occupancyPercent, status, children }: TruckCardProps) {
  const fill = Math.min(occupancyPercent, 100);
  const level = occupancyLevel(occupancyPercent);
  const bedWidth = 196;
  const fillWidth = (bedWidth * fill) / 100;
  const patternId = `crates-${plate.replace(/[^A-Za-z0-9]/g, '')}`;

  return (
    <article className="panel flex flex-col gap-3 p-4">
      <header className="flex items-start justify-between gap-2">
        <div>
          <h3 className="font-display text-xl font-semibold leading-tight">{name}</h3>
          <p className="font-display text-sm tracking-wide text-muted">{plate}</p>
          {status && (
            <div className="mt-1">
              <TruckStatusBadge status={status} />
            </div>
          )}
        </div>
        <div className="text-right">
          <p className={cn('font-display text-3xl font-semibold leading-none', level.tone)}>{formatPercent(occupancyPercent)}</p>
          <p className={cn('text-sm', level.tone)}>{level.label}</p>
        </div>
      </header>

      <svg viewBox="0 0 300 96" className="w-full" role="img" aria-label={`${name}: ${formatDecimal(loadedBoxes)} de ${formatInt(capacityBoxes)} cajas`}>
        <defs>
          <pattern id={patternId} width="14" height="14" patternUnits="userSpaceOnUse">
            <rect width="14" height="14" className="fill-crate" />
            <path d="M0 7h14M7 0v14" className="stroke-unet-dark/25" strokeWidth="1.5" />
          </pattern>
        </defs>
        {/* Caja de carga */}
        <rect x="6" y="10" width={bedWidth} height="58" rx="3" className="fill-sunken stroke-line" strokeWidth="2" />
        {fillWidth > 0 && <rect x="8" y="12" width={Math.max(fillWidth - 4, 0)} height="54" rx="2" fill={`url(#${patternId})`} />}
        {occupancyPercent > 100 && <rect x="6" y="10" width={bedWidth} height="58" rx="3" fill="none" className="stroke-status-out" strokeWidth="3" />}
        {/* Cabina */}
        <path d="M208 30h44l26 22v16h-70z" className="fill-brand" />
        <path d="M218 36h30l16 14h-46z" className="fill-surface opacity-80" />
        {/* Chasis y ruedas */}
        <rect x="4" y="68" width="276" height="6" rx="2" className="fill-ink/70" />
        {[44, 92, 244].map((cx) => (
          <g key={cx}>
            <circle cx={cx} cy="80" r="12" className="fill-ink" />
            <circle cx={cx} cy="80" r="4.5" className="fill-surface" />
          </g>
        ))}
      </svg>

      <p className="text-sm text-muted">
        <span className="font-semibold text-ink">{formatDecimal(loadedBoxes)}</span> de {formatInt(capacityBoxes)} cajas
      </p>
      {children}
    </article>
  );
}
