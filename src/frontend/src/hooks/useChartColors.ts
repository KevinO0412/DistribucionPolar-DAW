import { useTheme } from './useTheme';

// Copia de los tokens de src/index.css. Recharts necesita colores concretos, y leer las variables CSS
// durante el render daría los del tema anterior (la clase "dark" cambia después, en un efecto).
const PALETTES = {
  light: {
    brand: 'rgb(0,51,102)',
    crate: 'rgb(242,169,0)',
    ink: 'rgb(15,34,54)',
    muted: 'rgb(84,102,122)',
    line: 'rgb(213,220,227)',
    surface: 'rgb(255,255,255)',
  },
  dark: {
    brand: 'rgb(141,184,232)',
    crate: 'rgb(245,185,51)',
    ink: 'rgb(227,234,242)',
    muted: 'rgb(150,170,194)',
    line: 'rgb(34,60,94)',
    surface: 'rgb(16,36,63)',
  },
} as const;

/** Colores de los gráficos según el tema activo. */
export function useChartColors() {
  const { theme } = useTheme();
  return PALETTES[theme];
}
