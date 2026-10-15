import { clsx, type ClassValue } from 'clsx';
import { extendTailwindMerge } from 'tailwind-merge';

// Los tamaños propios (text-figure, text-2xs) se registran como tamaños de fuente; si no, tailwind-merge
// los confunde con colores (text-status-out) y elimina uno de los dos.
const twMerge = extendTailwindMerge({
  extend: { classGroups: { 'font-size': [{ text: ['figure', '2xs'] }] } },
});

/** Combina clases de Tailwind resolviendo conflictos (p-2 + p-4 → p-4). */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}
