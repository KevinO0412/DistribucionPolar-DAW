import { BarChart3, Boxes, FolderTree, ReceiptText, Truck, type LucideIcon } from 'lucide-react';
import type { Role } from '../../api/types';

export interface NavItem {
  to: string;
  label: string;
  icon: LucideIcon;
  /** Solo visible para este rol. */
  role?: Role;
}

export const NAV_ITEMS: NavItem[] = [
  { to: '/dashboard', label: 'Dashboard', icon: BarChart3, role: 'Admin' },
  { to: '/productos', label: 'Productos', icon: Boxes },
  { to: '/categorias', label: 'Categorías', icon: FolderTree },
  { to: '/camiones', label: 'Camiones', icon: Truck },
  { to: '/facturas', label: 'Facturas', icon: ReceiptText },
];

export const visibleNavItems = (role: Role | undefined) => NAV_ITEMS.filter((item) => !item.role || item.role === role);

/** Primera pantalla según el rol: el Admin entra al dashboard y el Employee al catálogo. */
export const homeFor = (role: Role | undefined) => (role === 'Admin' ? '/dashboard' : '/productos');
