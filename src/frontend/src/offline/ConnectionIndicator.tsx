import { Link } from 'react-router-dom';
import { CloudOff, Loader2, RefreshCw, TriangleAlert } from 'lucide-react';
import { cn } from '../lib/cn';
import { useOffline } from './OfflineContext';

/** Indicador de la cabecera: sin conexión, pedidos pendientes o rechazados, y sincronización en curso. */
export function ConnectionIndicator() {
  const { isOnline, pendingCount, rejectedCount, isSyncing, sync } = useOffline();
  if (isOnline && pendingCount === 0 && rejectedCount === 0 && !isSyncing) return null;

  const label = isSyncing
    ? 'Sincronizando pedidos…'
    : !isOnline
      ? `Sin conexión${pendingCount > 0 ? `: ${pendingCount} ${pendingCount === 1 ? 'pedido pendiente' : 'pedidos pendientes'}` : ''}`
      : pendingCount > 0
        ? `${pendingCount} ${pendingCount === 1 ? 'pedido pendiente' : 'pedidos pendientes'}`
        : `${rejectedCount} ${rejectedCount === 1 ? 'pedido rechazado' : 'pedidos rechazados'}`;

  return (
    <div className="flex items-center gap-1" role="status" aria-live="polite">
      <Link
        to="/preventa"
        className={cn(
          'flex h-9 items-center gap-2 rounded-md px-3 text-sm font-semibold',
          !isOnline ? 'bg-crate/20 text-ink' : rejectedCount > 0 && pendingCount === 0 ? 'bg-status-out/10 text-status-out' : 'bg-brand/10 text-brand',
        )}
        title="Ver pedidos de preventa"
      >
        {isSyncing ? (
          <Loader2 className="h-4 w-4 animate-spin" aria-hidden />
        ) : !isOnline ? (
          <CloudOff className="h-4 w-4" aria-hidden />
        ) : rejectedCount > 0 && pendingCount === 0 ? (
          <TriangleAlert className="h-4 w-4" aria-hidden />
        ) : (
          <RefreshCw className="h-4 w-4" aria-hidden />
        )}
        <span className="hidden sm:inline">{label}</span>
        <span className="sm:hidden">{pendingCount || rejectedCount || ''}</span>
      </Link>
      {isOnline && pendingCount > 0 && !isSyncing && (
        <button type="button" onClick={() => void sync()} className="h-9 cursor-pointer rounded-md px-2 text-sm font-medium text-brand hover:bg-sunken">
          Enviar ahora
        </button>
      )}
    </div>
  );
}
