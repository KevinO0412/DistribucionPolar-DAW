/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Paleta institucional exigida por el curso.
        unet: {
          blue: '#003366',
          dark: '#0a192f',
        },
        // Tokens semánticos: cambian entre modo claro y oscuro con variables CSS (src/index.css).
        paper: 'rgb(var(--paper) / <alpha-value>)',
        surface: 'rgb(var(--surface) / <alpha-value>)',
        sunken: 'rgb(var(--sunken) / <alpha-value>)',
        line: 'rgb(var(--line) / <alpha-value>)',
        ink: 'rgb(var(--ink) / <alpha-value>)',
        muted: 'rgb(var(--muted) / <alpha-value>)',
        brand: 'rgb(var(--brand) / <alpha-value>)',
        'on-brand': 'rgb(var(--on-brand) / <alpha-value>)',
        crate: 'rgb(var(--crate) / <alpha-value>)',
        status: {
          out: 'rgb(var(--status-out) / <alpha-value>)',
          critical: 'rgb(var(--status-critical) / <alpha-value>)',
          low: 'rgb(var(--status-low) / <alpha-value>)',
          ok: 'rgb(var(--status-ok) / <alpha-value>)',
          full: 'rgb(var(--status-full) / <alpha-value>)',
          excess: 'rgb(var(--status-excess) / <alpha-value>)',
        },
      },
      fontFamily: {
        sans: ['Barlow', 'system-ui', 'sans-serif'],
        display: ['"Barlow Condensed"', 'Barlow', 'system-ui', 'sans-serif'],
      },
      fontSize: {
        // Escala modular 1.25 sobre 16 px.
        '2xs': ['0.75rem', { lineHeight: '1rem' }],
        figure: ['2.441rem', { lineHeight: '1', letterSpacing: '-0.01em' }],
      },
    },
  },
  plugins: [],
};
