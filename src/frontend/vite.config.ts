/// <reference types="vitest/config" />
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import { VitePWA } from 'vite-plugin-pwa';


// En desarrollo, /api se reenvía a la API .NET (por defecto http://localhost:5000).
// En Docker, Nginx hace el mismo reenvío, así que la SPA siempre llama a rutas relativas /api.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  return {
    plugins: [
      react(),
      // PWA: la app se instala y abre sin conexión (precaché del shell) y guarda el catálogo para la preventa.
      VitePWA({
        registerType: 'autoUpdate',
        injectRegister: 'auto',
        includeAssets: ['favicon.svg', 'icons/apple-touch-icon.png'],
        manifest: {
          name: 'Distribución Polar',
          short_name: 'Polar',
          description: 'Preventa, inventario de galpón y camiones, clientes y facturación.',
          lang: 'es',
          start_url: '/',
          scope: '/',
          display: 'standalone',
          theme_color: '#003366',
          background_color: '#f2f4f6',
          icons: [
            { src: '/icons/icon-192.png', sizes: '192x192', type: 'image/png' },
            { src: '/icons/icon-512.png', sizes: '512x512', type: 'image/png' },
            { src: '/icons/maskable-512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
          ],
        },
        workbox: {
          globPatterns: ['**/*.{js,css,html,svg,woff2,png,ico,webmanifest}'],
          globIgnores: ['img/**'],
          navigateFallback: '/index.html',
          navigateFallbackDenylist: [/^\/api\//],
          cleanupOutdatedCaches: true,
          runtimeCaching: [
            {
              // Catálogo, clientes y camiones: red primero; sin señal, la última respuesta guardada.
              // La lista va dentro de la función: Workbox la copia tal cual en sw.js (sin acceso a este archivo).
              urlPattern: ({ url, request }) =>
                request.method === 'GET' && ['/api/products', '/api/clients', '/api/trucks', '/api/categories'].includes(url.pathname),
              handler: 'NetworkFirst',
              options: {
                cacheName: 'dp-catalogo',
                networkTimeoutSeconds: 5,
                expiration: { maxEntries: 40, maxAgeSeconds: 7 * 24 * 60 * 60 },
                cacheableResponse: { statuses: [200] },
              },
            },
            {
              // Fotos de productos: se guardan a medida que se ven (no entran en la precaché por su peso).
              urlPattern: ({ url }) => url.pathname.startsWith('/img/'),
              handler: 'CacheFirst',
              options: { cacheName: 'dp-fotos', expiration: { maxEntries: 400, maxAgeSeconds: 30 * 24 * 60 * 60 } },
            },
          ],
        },
        devOptions: { enabled: false },
      }),
    ],
    server: {
      port: 5173,
      proxy: {
        '/api': { target: env.VITE_PROXY_TARGET ?? 'http://localhost:5000', changeOrigin: true },
      },
    },
    build: { sourcemap: false, chunkSizeWarningLimit: 700 },
    test: {
      environment: 'jsdom',
      globals: true,
      setupFiles: ['./src/test/setup.ts'],
      css: false,
      coverage: { provider: 'v8', reporter: ['text', 'html'], include: ['src/**/*.{ts,tsx}'], exclude: ['src/test/**', 'src/main.tsx'] },
    },
  };
});
