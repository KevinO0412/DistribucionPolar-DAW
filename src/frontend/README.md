# Distribución Polar: SPA

Cliente React 18 + Vite + TypeScript + Tailwind CSS que consume la API REST .NET 10 (`/api`).

## Ejecutar en desarrollo

```bash
cp .env.example .env          # VITE_PROXY_TARGET: URL de la API (por defecto http://localhost:5000)
npm ci
npm run dev                   # http://localhost:5173
```

Vite reenvía `/api` a la API, así que no hace falta configurar CORS en desarrollo. En Docker, Nginx hace el mismo reenvío.

| Script | Qué hace |
| --- | --- |
| `npm run dev` | Servidor de desarrollo con recarga en caliente |
| `npm run build` | Verificación de tipos (`tsc -b`) y build de producción en `dist/` |
| `npm test` | Pruebas con Vitest y Testing Library |
| `npm run test:coverage` | Pruebas con reporte de cobertura (`coverage/index.html`) |

## Estructura

```
src/
├── api/            client.ts (Axios + interceptores Bearer y RFC 7807), endpoints.ts, types.ts
├── context/        AuthContext.tsx (JWT), ThemeContext.tsx (Azul UNET / modo oscuro)
├── hooks/          useAuth, useTheme, useApiQuery, useChartColors, useDebounced
├── components/
│   ├── ui/         Button, Field, Modal, ProductThumb, States, StatusBadge, Toast (átomos)
│   ├── clients/    DebtAgingBar (tramos de antigüedad), PaymentReceipt (recibo de cobro)
│   ├── layout/     AppLayout (menú colapsable), PageHeader, navigation (menú por rol)
│   ├── dashboard/  PowerBIDashboard y sus gráficos, tarjetas KPI, filtros y tablas
│   ├── trucks/     TruckCard y TruckStatusBadge, Ticket y SettlementSheet (80 o 58 mm), PaperWidthSelector
│   └── ProtectedRoute.tsx
├── data/           productImages.json (legacyId del catálogo → foto)
├── lib/            cn, format, productImage, print (impresión térmica de 80 y 58 mm)
├── offline/        store (IndexedDB), sync (cola idempotente), presale (armado del pedido)
└── pages/          Login, Dashboard, Productos, Categorías, Camiones, Liquidación, Facturas, Clientes, Preventa
```

## Fotos de los productos

Las fotos están en `public/img/productos/{alimentos,bebidas,jabones}/` (webp) y Nginx las sirve como archivos estáticos.
No van en `public/productos/` porque esa carpeta taparía la ruta `/productos` de la SPA al recargar.

`src/data/productImages.json` asocia cada producto del catálogo sembrado (por su `legacyId`, que forma el `Id` en `CatalogSeedData.cs`) con su foto.
`ProductThumb` la muestra en la tabla y en el formulario de edición; los productos creados desde la aplicación, o cuya foto no carga, muestran un ícono genérico.

## Preventa sin conexión (PWA)

- `public/sw.js` (service worker) y `public/manifest.webmanifest`: la aplicación abre sin conexión. El service worker se registra solo en el build de producción.
- La página Preventa guarda cada pedido primero en IndexedDB y lo envía con su id como `Idempotency-Key`; sin señal queda en cola y se envía al reconectarse sin duplicar la factura. Detalle en [Observaciones del profesor](../../docs/09-OBSERVACIONES-PROFESOR.md).

## Sesión y roles

- `AuthContext` inicia sesión contra `POST /api/auth/login`, guarda el JWT en `localStorage` (`dp.session`) y lo decodifica con `jwt-decode` para leer el usuario y el rol. La sesión se conserva al recargar la página y se cierra sola cuando vence el token o cuando la API responde 401. La API no emite refresh tokens.
- `ProtectedRoute` envía al login si no hay sesión y a `/sin-permiso` si el rol no alcanza.
- El rol Employee no ve el dashboard en el menú (ni puede abrirlo por URL) y no tiene los botones de editar o eliminar productos y categorías. Son las mismas reglas que aplica la API con `[Authorize(Roles = ...)]`.

## Tema

`ThemeContext` alterna el modo claro en Azul UNET (`#003366`) y el modo oscuro (`unet.dark #0a192f`) con la clase `dark` en `<html>` (`darkMode: 'class'` en `tailwind.config.js`) y guarda la preferencia en `localStorage` (`dp.theme`).
