import { createContext, useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';
import { configureAuth } from '../api/client';
import { authApi } from '../api/endpoints';
import type { Role } from '../api/types';

const STORAGE_KEY = 'dp.session';
const ROLE_CLAIM = 'role';
const MS_CLAIM_ROLE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

/** Claims emitidos por JwtTokenService (sub, unique_name, email, name, role, exp, iss, aud, jti). */
export interface JwtClaims {
  sub: string;
  unique_name?: string;
  email?: string;
  name?: string;
  role?: string | string[];
  exp: number;
  [MS_CLAIM_ROLE]?: string;
}

export interface SessionUser {
  id: string;
  username: string;
  email: string;
  fullName: string;
  role: Role;
  expiresAt: number;
}

interface AuthContextValue {
  user: SessionUser | null;
  token: string | null;
  isAdmin: boolean;
  login: (username: string, password: string) => Promise<SessionUser>;
  logout: (reason?: string) => void;
  /** Mensaje para la pantalla de login cuando la sesión terminó sola (vencida o rechazada por la API). */
  endReason: string | null;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

/** Decodifica el JWT (sin verificar la firma: eso lo hace la API) y arma el usuario de la sesión. */
export function decodeSession(token: string): SessionUser | null {
  try {
    const claims = jwtDecode<JwtClaims>(token);
    const rawRole = claims[ROLE_CLAIM] ?? claims[MS_CLAIM_ROLE];
    const role = (Array.isArray(rawRole) ? rawRole[0] : rawRole) as Role | undefined;
    if (!claims.exp || (role !== 'Admin' && role !== 'Employee')) return null;
    return {
      id: claims.sub,
      username: claims.unique_name ?? '',
      email: claims.email ?? '',
      fullName: claims.name ?? claims.unique_name ?? '',
      role,
      expiresAt: claims.exp * 1000,
    };
  } catch {
    return null;
  }
}

function readStoredToken(): string | null {
  try {
    const token = localStorage.getItem(STORAGE_KEY);
    const session = token ? decodeSession(token) : null;
    if (token && session && session.expiresAt > Date.now()) return token;
    localStorage.removeItem(STORAGE_KEY);
  } catch {
    // localStorage bloqueado (modo privado estricto): la sesión vive solo en memoria.
  }
  return null;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(readStoredToken);
  const [endReason, setEndReason] = useState<string | null>(null);
  const user = useMemo(() => (token ? decodeSession(token) : null), [token]);

  const logout = useCallback((reason?: string) => {
    try {
      localStorage.removeItem(STORAGE_KEY);
    } catch {
      /* sin almacenamiento */
    }
    setEndReason(reason ?? null);
    setToken(null);
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    const response = await authApi.login(username, password);
    const session = decodeSession(response.token);
    if (!session) throw new Error('El servidor devolvió un token sin rol válido.');
    try {
      localStorage.setItem(STORAGE_KEY, response.token);
    } catch {
      /* sin almacenamiento */
    }
    setEndReason(null);
    setToken(response.token);
    return session;
  }, []);

  // El cliente HTTP lee el token actual y cierra la sesión si la API responde 401.
  useEffect(() => {
    configureAuth(() => token, () => logout('Tu sesión terminó. Inicia sesión de nuevo.'));
  }, [token, logout]);

  // La API no emite refresh tokens: al vencer el JWT se cierra la sesión.
  useEffect(() => {
    if (!user) return;
    const remaining = user.expiresAt - Date.now();
    const timer = window.setTimeout(() => logout('Tu sesión venció. Inicia sesión de nuevo.'), Math.max(remaining, 0));
    return () => window.clearTimeout(timer);
  }, [user, logout]);

  // Mantiene sincronizadas varias pestañas: cerrar sesión en una la cierra en todas.
  useEffect(() => {
    const onStorage = (event: StorageEvent) => {
      if (event.key === STORAGE_KEY) setToken(readStoredToken());
    };
    window.addEventListener('storage', onStorage);
    return () => window.removeEventListener('storage', onStorage);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({ user, token, isAdmin: user?.role === 'Admin', login, logout, endReason }),
    [user, token, login, logout, endReason],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
