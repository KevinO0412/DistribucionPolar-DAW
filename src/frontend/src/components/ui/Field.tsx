import { forwardRef, useId, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes } from 'react';
import { cn } from '../../lib/cn';

const control =
  'h-11 w-full rounded-md border border-line bg-surface px-3 text-[0.9375rem] text-ink placeholder:text-muted/80 ' +
  'aria-[invalid=true]:border-status-out disabled:opacity-60';

interface FieldShellProps {
  label: string;
  hint?: string;
  error?: string;
  children: (props: { id: string; describedBy?: string; invalid: boolean }) => ReactNode;
  className?: string;
}

/** Etiqueta visible + control + ayuda o error debajo del campo. */
export function FieldShell({ label, hint, error, children, className }: FieldShellProps) {
  const id = useId();
  const describedBy = error ? `${id}-error` : hint ? `${id}-hint` : undefined;
  return (
    <div className={cn('flex flex-col gap-1.5', className)}>
      <label htmlFor={id} className="text-sm font-medium text-ink">
        {label}
      </label>
      {children({ id, describedBy, invalid: Boolean(error) })}
      {error ? (
        <p id={`${id}-error`} className="text-sm text-status-out" role="alert">
          {error}
        </p>
      ) : hint ? (
        <p id={`${id}-hint`} className="text-sm text-muted">
          {hint}
        </p>
      ) : null}
    </div>
  );
}

interface TextFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  hint?: string;
  error?: string;
  wrapperClassName?: string;
}

export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(function TextField(
  { label, hint, error, wrapperClassName, className, ...props },
  ref,
) {
  return (
    <FieldShell label={label} hint={hint} error={error} className={wrapperClassName}>
      {({ id, describedBy, invalid }) => (
        <input ref={ref} id={id} aria-describedby={describedBy} aria-invalid={invalid} className={cn(control, className)} {...props} />
      )}
    </FieldShell>
  );
});

interface SelectFieldProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  hint?: string;
  error?: string;
  wrapperClassName?: string;
}

export const SelectField = forwardRef<HTMLSelectElement, SelectFieldProps>(function SelectField(
  { label, hint, error, wrapperClassName, className, children, ...props },
  ref,
) {
  return (
    <FieldShell label={label} hint={hint} error={error} className={wrapperClassName}>
      {({ id, describedBy, invalid }) => (
        <select ref={ref} id={id} aria-describedby={describedBy} aria-invalid={invalid} className={cn(control, 'pr-8', className)} {...props}>
          {children}
        </select>
      )}
    </FieldShell>
  );
});

export function CheckboxField({ label, hint, ...props }: InputHTMLAttributes<HTMLInputElement> & { label: string; hint?: string }) {
  const id = useId();
  return (
    <div className="flex items-start gap-3">
      <input id={id} type="checkbox" className="mt-1 h-4 w-4 cursor-pointer rounded border-line accent-[rgb(var(--brand))]" {...props} />
      <label htmlFor={id} className="cursor-pointer text-sm">
        <span className="font-medium text-ink">{label}</span>
        {hint && <span className="block text-muted">{hint}</span>}
      </label>
    </div>
  );
}
