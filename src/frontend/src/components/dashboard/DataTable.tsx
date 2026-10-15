import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

export interface Column<T> {
  header: string;
  cell: (row: T) => ReactNode;
  align?: 'left' | 'right';
  className?: string;
}

/** Tabla con desplazamiento horizontal propio (overflow-x-auto): nunca desborda el diseño en móviles. */
export function DataTable<T>({ columns, rows, rowKey, caption }: {
  columns: Column<T>[];
  rows: T[];
  rowKey: (row: T) => string;
  caption: string;
}) {
  return (
    <div className="-mx-4 overflow-x-auto sm:mx-0">
      <table className="w-full min-w-[36rem] border-collapse text-sm">
        <caption className="sr-only">{caption}</caption>
        <thead>
          <tr className="border-b border-line text-left text-muted">
            {columns.map((col) => (
              <th key={col.header} scope="col" className={cn('px-4 py-2 font-medium sm:px-3', col.align === 'right' && 'text-right', col.className)}>
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={rowKey(row)} className="border-b border-line/70 last:border-0">
              {columns.map((col) => (
                <td key={col.header} className={cn('px-4 py-2.5 align-top sm:px-3', col.align === 'right' && 'text-right tabular-nums', col.className)}>
                  {col.cell(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
