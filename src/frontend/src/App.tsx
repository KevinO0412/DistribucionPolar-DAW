import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AppLayout } from './components/layout/AppLayout';
import { homeFor } from './components/layout/navigation';
import { useAuth } from './hooks/useAuth';
import { CategoriesPage } from './pages/CategoriesPage';
import { InvoicesPage } from './pages/InvoicesPage';
import { LoginPage } from './pages/LoginPage';
import { ProductsPage } from './pages/ProductsPage';
import { ForbiddenPage, NotFoundPage } from './pages/StatusPages';
import { TrucksPage } from './pages/TrucksPage';
import { ClientsPage } from './pages/ClientsPage';
import { PreventaPage } from './pages/PreventaPage';
import { LoadingState } from './components/ui/States';

// Recharts solo se descarga al entrar al dashboard (solo Admin).
const DashboardPage = lazy(() => import('./pages/DashboardPage').then((m) => ({ default: m.DashboardPage })));

function Home() {
  const { user } = useAuth();
  return <Navigate to={homeFor(user?.role)} replace />;
}

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Home />} />
        <Route
          path="dashboard"
          element={
            <ProtectedRoute role="Admin">
              <Suspense fallback={<LoadingState label="Cargando el dashboard…" />}>
                <DashboardPage />
              </Suspense>
            </ProtectedRoute>
          }
        />
        <Route path="productos" element={<ProductsPage />} />
        <Route path="categorias" element={<CategoriesPage />} />
        <Route path="preventa" element={<PreventaPage />} />
        <Route path="clientes" element={<ClientsPage />} />
        <Route path="camiones" element={<TrucksPage />} />
        <Route path="facturas" element={<InvoicesPage />} />
        <Route path="sin-permiso" element={<ForbiddenPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
