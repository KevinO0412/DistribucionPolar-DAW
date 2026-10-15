import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import type { Role } from '../api/types';
import { useAuth } from '../hooks/useAuth';

interface ProtectedRouteProps {
  children: ReactNode;
  /** Rol requerido. Sin rol, basta con haber iniciado sesión. */
  role?: Role;
}

/** Protege rutas privadas: sin sesión → /login; con un rol insuficiente → /sin-permiso. */
export function ProtectedRoute({ children, role }: ProtectedRouteProps) {
  const { user } = useAuth();
  const location = useLocation();

  if (!user) return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  if (role && user.role !== role) return <Navigate to="/sin-permiso" replace />;
  return <>{children}</>;
}
