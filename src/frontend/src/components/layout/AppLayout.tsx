import { useEffect, useState } from 'react';
import { NavLink, Outlet, useLocation } from 'react-router-dom';
import { LogOut, Menu, Moon, PanelLeftClose, PanelLeftOpen, Sun, X } from 'lucide-react';
import { useAuth } from '../../hooks/useAuth';
import { useTheme } from '../../hooks/useTheme';
import { cn } from '../../lib/cn';
import { visibleNavItems } from './navigation';
import { ConnectionIndicator } from '../../offline/ConnectionIndicator';
import { clearCatalogCache } from '../../offline/catalogCache';

const COLLAPSE_KEY = 'dp.sidebar.collapsed';

function readCollapsed() {
  try {
    return localStorage.getItem(COLLAPSE_KEY) === '1';
  } catch {
    return false;
  }
}

function Brand({ compact }: { compact?: boolean }) {
  return (
    <div className="flex items-center gap-2.5">
      <svg viewBox="0 0 32 32" className="h-8 w-8 shrink-0" aria-hidden>
        <rect width="32" height="32" rx="6" className="fill-brand" />
        <path d="M6 20h13V10H6zm13-6h4l3 3v3h-7z" fill="none" strokeWidth="2" strokeLinejoin="round" className="stroke-on-brand" />
        <circle cx="10" cy="22" r="2" className="fill-crate" />
        <circle cx="22" cy="22" r="2" className="fill-crate" />
      </svg>
      {!compact && (
        <span className="font-display text-lg font-semibold leading-tight">
          Distribución
          <br />
          Polar
        </span>
      )}
    </div>
  );
}

function NavList({ collapsed, onNavigate }: { collapsed?: boolean; onNavigate?: () => void }) {
  const { user } = useAuth();
  return (
    <ul className="flex flex-col gap-1">
      {visibleNavItems(user?.role).map(({ to, label, icon: Icon }) => (
        <li key={to}>
          <NavLink
            to={to}
            onClick={onNavigate}
            title={collapsed ? label : undefined}
            className={({ isActive }) =>
              cn(
                'flex h-11 items-center gap-3 rounded-md px-3 font-medium transition-colors duration-150',
                isActive ? 'bg-brand text-on-brand' : 'text-muted hover:bg-sunken hover:text-ink',
                collapsed && 'justify-center px-0',
              )
            }
          >
            <Icon className="h-5 w-5 shrink-0" aria-hidden />
            <span className={cn(collapsed && 'sr-only')}>{label}</span>
          </NavLink>
        </li>
      ))}
    </ul>
  );
}

function ThemeToggle() {
  const { isDarkMode, toggleTheme } = useTheme();
  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-pressed={isDarkMode}
      className="grid h-10 w-10 cursor-pointer place-items-center rounded-md text-muted hover:bg-sunken hover:text-ink"
      aria-label={isDarkMode ? 'Usar modo claro' : 'Usar modo oscuro'}
      title={isDarkMode ? 'Modo claro' : 'Modo oscuro'}
    >
      {isDarkMode ? <Sun className="h-5 w-5" aria-hidden /> : <Moon className="h-5 w-5" aria-hidden />}
    </button>
  );
}

export function AppLayout() {
  const { user, logout } = useAuth();
  const [collapsed, setCollapsed] = useState(readCollapsed);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const location = useLocation();

  useEffect(() => {
    try {
      localStorage.setItem(COLLAPSE_KEY, collapsed ? '1' : '0');
    } catch {
      /* sin almacenamiento */
    }
  }, [collapsed]);

  useEffect(() => setDrawerOpen(false), [location.pathname]);

  const roleLabel = user?.role === 'Admin' ? 'Administrador' : 'Empleado';

  return (
    <div className="flex min-h-screen">
      {/* Barra lateral de escritorio, colapsable */}
      <aside
        className={cn(
          'sticky top-0 hidden h-screen shrink-0 flex-col border-r border-line bg-surface px-3 py-4 transition-[width] duration-200 lg:flex',
          collapsed ? 'w-[4.5rem]' : 'w-60',
        )}
      >
        <div className={cn('mb-6 px-1', collapsed && 'flex justify-center px-0')}>
          <Brand compact={collapsed} />
        </div>
        <nav aria-label="Principal" className="flex-1">
          <NavList collapsed={collapsed} />
        </nav>
        <button
          type="button"
          onClick={() => setCollapsed((c) => !c)}
          className={cn(
            'flex h-10 cursor-pointer items-center gap-3 rounded-md px-3 text-sm text-muted hover:bg-sunken hover:text-ink',
            collapsed && 'justify-center px-0',
          )}
          aria-expanded={!collapsed}
          aria-label={collapsed ? 'Expandir menú' : 'Colapsar menú'}
        >
          {collapsed ? <PanelLeftOpen className="h-5 w-5" aria-hidden /> : <PanelLeftClose className="h-5 w-5" aria-hidden />}
          {!collapsed && <span>Colapsar menú</span>}
        </button>
      </aside>

      {/* Menú lateral móvil */}
      {drawerOpen && (
        <div className="fixed inset-0 z-40 lg:hidden">
          <button type="button" className="absolute inset-0 bg-unet-dark/60" aria-label="Cerrar menú" onClick={() => setDrawerOpen(false)} />
          <div className="absolute inset-y-0 left-0 flex w-72 max-w-[85vw] flex-col bg-surface px-3 py-4 shadow-xl">
            <div className="mb-6 flex items-center justify-between px-1">
              <Brand />
              <button
                type="button"
                onClick={() => setDrawerOpen(false)}
                className="grid h-10 w-10 cursor-pointer place-items-center rounded-md text-muted hover:bg-sunken"
                aria-label="Cerrar menú"
              >
                <X className="h-5 w-5" aria-hidden />
              </button>
            </div>
            <nav aria-label="Principal">
              <NavList onNavigate={() => setDrawerOpen(false)} />
            </nav>
          </div>
        </div>
      )}

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-16 items-center gap-2 border-b border-line bg-paper px-4 sm:px-6">
          <button
            type="button"
            onClick={() => setDrawerOpen(true)}
            className="-ml-2 grid h-10 w-10 cursor-pointer place-items-center rounded-md text-muted hover:bg-sunken lg:hidden"
            aria-label="Abrir menú"
          >
            <Menu className="h-5 w-5" aria-hidden />
          </button>
          <div className="lg:hidden">
            <Brand compact />
          </div>
          <div className="ml-auto flex items-center gap-1 sm:gap-3">
            <ConnectionIndicator />
            <ThemeToggle />
            <div className="hidden text-right sm:block">
              <p className="text-sm font-semibold leading-tight">{user?.fullName}</p>
              <p className="text-2xs text-muted">{roleLabel}</p>
            </div>
            <button
              type="button"
              onClick={() => void clearCatalogCache().finally(() => logout())}
              className="flex h-10 cursor-pointer items-center gap-2 rounded-md px-3 text-sm font-medium text-muted hover:bg-sunken hover:text-ink"
            >
              <LogOut className="h-4 w-4" aria-hidden />
              <span className="hidden sm:inline">Cerrar sesión</span>
              <span className="sr-only sm:hidden">Cerrar sesión</span>
            </button>
          </div>
        </header>
        <main id="contenido" className="mx-auto w-full max-w-[90rem] flex-1 px-4 py-6 sm:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
