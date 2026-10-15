import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../context/AuthContext';
import { adminToken, employeeToken } from '../test/jwt';
import { ProtectedRoute } from './ProtectedRoute';
import { visibleNavItems } from './layout/navigation';

function renderAt(path: string) {
  render(
    <AuthProvider>
      <MemoryRouter initialEntries={[path]} future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
        <Routes>
          <Route path="/login" element={<p>Pantalla de login</p>} />
          <Route path="/sin-permiso" element={<p>Sin permiso</p>} />
          <Route path="/productos" element={<ProtectedRoute><p>Catálogo</p></ProtectedRoute>} />
          <Route path="/dashboard" element={<ProtectedRoute role="Admin"><p>Dashboard KPI</p></ProtectedRoute>} />
        </Routes>
      </MemoryRouter>
    </AuthProvider>,
  );
}

describe('ProtectedRoute', () => {
  it('envía al login cuando no hay sesión', () => {
    renderAt('/productos');
    expect(screen.getByText('Pantalla de login')).toBeInTheDocument();
  });

  it('deja pasar al Admin al dashboard', () => {
    localStorage.setItem('dp.session', adminToken());
    renderAt('/dashboard');
    expect(screen.getByText('Dashboard KPI')).toBeInTheDocument();
  });

  it('bloquea el dashboard al Employee', () => {
    localStorage.setItem('dp.session', employeeToken());
    renderAt('/dashboard');
    expect(screen.getByText('Sin permiso')).toBeInTheDocument();
  });

  it('deja al Employee consultar el catálogo', () => {
    localStorage.setItem('dp.session', employeeToken());
    renderAt('/productos');
    expect(screen.getByText('Catálogo')).toBeInTheDocument();
  });
});

describe('menú por rol', () => {
  it('oculta el dashboard al Employee y lo muestra al Admin', () => {
    expect(visibleNavItems('Employee').map((i) => i.to)).not.toContain('/dashboard');
    expect(visibleNavItems('Admin').map((i) => i.to)).toContain('/dashboard');
  });
});
