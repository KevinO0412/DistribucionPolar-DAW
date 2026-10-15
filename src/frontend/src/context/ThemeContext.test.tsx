import { act, renderHook } from '@testing-library/react';
import type { ReactNode } from 'react';
import { useTheme } from '../hooks/useTheme';
import { ThemeProvider } from './ThemeContext';

const wrapper = ({ children }: { children: ReactNode }) => <ThemeProvider>{children}</ThemeProvider>;

describe('ThemeProvider', () => {
  it('alterna el modo oscuro con la clase "dark" en <html> y lo guarda en localStorage', () => {
    localStorage.setItem('dp.theme', 'light');
    const { result } = renderHook(() => useTheme(), { wrapper });
    expect(result.current.isDarkMode).toBe(false);

    act(() => result.current.toggleTheme());

    expect(document.documentElement).toHaveClass('dark');
    expect(localStorage.getItem('dp.theme')).toBe('dark');
  });

  it('respeta el tema guardado al recargar', () => {
    localStorage.setItem('dp.theme', 'dark');
    const { result } = renderHook(() => useTheme(), { wrapper });
    expect(result.current.theme).toBe('dark');
    expect(document.documentElement).toHaveClass('dark');
  });
});
