import { useCallback, useEffect, useRef, useState } from 'react';
import { toApiError, type ApiError } from '../api/client';

interface QueryState<T> {
  data: T | undefined;
  error: ApiError | null;
  isLoading: boolean;
  reload: () => void;
}

/**
 * Ejecuta una consulta a la API y la repite cuando cambian sus dependencias.
 * Cancela la petición anterior (AbortController) para que una respuesta lenta no pise a una más nueva.
 */
export function useApiQuery<T>(fetcher: (signal: AbortSignal) => Promise<T>, deps: unknown[]): QueryState<T> {
  const [data, setData] = useState<T>();
  const [error, setError] = useState<ApiError | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [version, setVersion] = useState(0);
  const fetcherRef = useRef(fetcher);
  fetcherRef.current = fetcher;

  useEffect(() => {
    const controller = new AbortController();
    setIsLoading(true);
    setError(null);
    fetcherRef
      .current(controller.signal)
      .then((result) => {
        if (!controller.signal.aborted) setData(result);
      })
      .catch((err: unknown) => {
        if (!controller.signal.aborted) setError(toApiError(err));
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false);
      });
    return () => controller.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [...deps, version]);

  const reload = useCallback(() => setVersion((v) => v + 1), []);
  return { data, error, isLoading, reload };
}
