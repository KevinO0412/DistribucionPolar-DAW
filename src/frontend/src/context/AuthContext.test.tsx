import { act, render, renderHook, screen, waitFor } from '@testing-library/react';
import type { ReactNode } from 'react';
import { vi } from 'vitest';
import { authApi } from '../api/endpoints';
import { useAuth } from '../hooks/useAuth';
import { adminToken, employeeToken, fakeJwt } from '../test/jwt';
import { AuthProvider, decodeSession } from './AuthContext';

const wrapper = ({ children }: { children: ReactNode }) => <AuthProvider>{children}</AuthProvider>;

describe('decodeSession', () => {
  it('lee los claims del JWT emitido por la API', () => {
    const session = decodeSession(adminToken());
    expect(session).toMatchObject({ id: '1', username: 'admin', fullName: 'Administrador del Sistema', role: 'Admin' });
  });

  it('rechaza un token sin un rol conocido', () => {
    expect(decodeSession(fakeJwt({ sub: '1', role: 'Root', exp: 9999999999 }))).toBeNull();
    expect(decodeSession('no-es-un-jwt')).toBeNull();
  });
});

describe('AuthProvider', () => {
  afterEach(() => vi.restoreAllMocks());

  it('guarda el token en localStorage al iniciar sesión y lo limpia al cerrarla', async () => {
    const token = employeeToken();
    vi.spyOn(authApi, 'login').mockResolvedValue({
      token, tokenType: 'Bearer', expiresAt: '', username: 'empleado', email: '', fullName: 'Empleado de Ventas', role: 'Employee',
    });
    const { result } = renderHook(() => useAuth(), { wrapper });

    await act(() => result.current.login('empleado', 'Empleado2026!'));
    expect(localStorage.getItem('dp.session')).toBe(token);
    expect(result.current.user?.role).toBe('Employee');
    expect(result.current.isAdmin).toBe(false);

    act(() => result.current.logout());
    expect(localStorage.getItem('dp.session')).toBeNull();
    expect(result.current.user).toBeNull();
  });

  it('recupera la sesión guardada al recargar la página', () => {
    localStorage.setItem('dp.session', adminToken());
    const { result } = renderHook(() => useAuth(), { wrapper });
    expect(result.current.user?.username).toBe('admin');
    expect(result.current.isAdmin).toBe(true);
  });

  it('descarta un token vencido al recargar', () => {
    localStorage.setItem('dp.session', fakeJwt({ sub: '1', role: 'Admin', exp: Math.floor(Date.now() / 1000) - 10 }));
    const { result } = renderHook(() => useAuth(), { wrapper });
    expect(result.current.user).toBeNull();
    expect(localStorage.getItem('dp.session')).toBeNull();
  });

  it('cierra la sesión sola cuando vence el token', async () => {
    vi.useFakeTimers();
    localStorage.setItem('dp.session', fakeJwt({ sub: '1', unique_name: 'admin', role: 'Admin', exp: Math.floor(Date.now() / 1000) + 2 }));
    function Probe() {
      const { user, endReason } = useAuth();
      return <p>{user ? 'activa' : endReason}</p>;
    }
    render(<Probe />, { wrapper });
    expect(screen.getByText('activa')).toBeInTheDocument();

    await act(() => vi.advanceTimersByTimeAsync(2500));
    vi.useRealTimers();
    await waitFor(() => expect(screen.getByText('Tu sesión venció. Inicia sesión de nuevo.')).toBeInTheDocument());
  });
});
