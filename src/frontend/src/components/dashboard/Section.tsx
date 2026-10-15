import type { ReactNode } from 'react';
import { cn } from '../../lib/cn';

export function Section({ title, description, children, className, actions }: {
  title: string;
  description?: string;
  children: ReactNode;
  className?: string;
  actions?: ReactNode;
}) {
  return (
    <section className={cn('panel flex min-w-0 flex-col p-4 sm:p-5', className)} aria-label={title}>
      <div className="mb-4 flex flex-wrap items-start justify-between gap-2">
        <div>
          <h2 className="font-display text-xl font-semibold leading-tight">{title}</h2>
          {description && <p className="mt-0.5 text-sm text-muted">{description}</p>}
        </div>
        {actions}
      </div>
      {children}
    </section>
  );
}
