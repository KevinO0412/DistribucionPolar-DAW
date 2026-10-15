import { useState, type FormEvent } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { toApiError } from '../api/client';
import { Button } from '../components/ui/Button';
import { TextField } from '../components/ui/Field';
import { homeFor } from '../components/layout/navigation';
import { useAuth } from '../hooks/useAuth';

export function LoginPage() {
  const { user, login, endReason } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<{ username?: string; password?: string }>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (user) return <Navigate to={homeFor(user.role)} replace />;

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    const errors = {
      username: username.trim() ? undefined : 'Escribe tu usuario o correo.',
      password: password.length >= 6 ? undefined : 'La clave tiene al menos 6 caracteres.',
    };
    setFieldErrors(errors);
    if (errors.username || errors.password) return;

    setIsSubmitting(true);
    setError(null);
    try {
      const session = await login(username, password);
      const from = (location.state as { from?: string } | null)?.from;
      navigate(from && from !== '/login' ? from : homeFor(session.role), { replace: true });
    } catch (err) {
      const apiError = toApiError(err);
      setError(apiError.message);
      setFieldErrors({ username: apiError.fieldError('username'), password: apiError.fieldError('password') });
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="grid min-h-screen lg:grid-cols-[1.1fr_1fr]">
      <div className="relative hidden overflow-hidden bg-unet-blue p-12 text-white lg:flex lg:flex-col lg:justify-between">
        <p className="font-display text-2xl font-semibold">Distribución Polar</p>
        <div className="max-w-md">
          <h1 className="font-display text-5xl font-semibold leading-[1.05]">Galpón, camiones y ventas en un solo lugar.</h1>
          <p className="mt-4 text-lg text-white/80">
            Revisa qué hay en el galpón, qué lleva cada camión y qué se vendió hoy, sin esperar el cierre de caja.
          </p>
        </div>
        {/* Fila de gaveras: el mismo motivo de la carga de los camiones */}
        <svg viewBox="0 0 480 60" className="w-full max-w-lg" aria-hidden>
          {Array.from({ length: 12 }, (_, i) => (
            <g key={i} transform={`translate(${i * 40} ${i % 3 === 0 ? 0 : 14})`}>
              <rect width="36" height={i % 3 === 0 ? 46 : 32} rx="3" fill="#F2A900" opacity={0.25 + (i % 4) * 0.18} />
            </g>
          ))}
        </svg>
      </div>

      <div className="flex items-center justify-center px-4 py-10 sm:px-8">
        <form onSubmit={onSubmit} noValidate className="w-full max-w-sm">
          <h2 className="font-display text-3xl font-semibold">Iniciar sesión</h2>
          <p className="mt-1 text-muted">Usa el usuario que te asignó el administrador.</p>

          {(error ?? endReason) && (
            <p className="mt-5 rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
              {error ?? endReason}
            </p>
          )}

          <div className="mt-6 flex flex-col gap-4">
            <TextField
              label="Usuario o correo"
              name="username"
              autoComplete="username"
              autoFocus
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              error={fieldErrors.username}
            />
            <TextField
              label="Clave"
              name="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={fieldErrors.password}
            />
            <Button type="submit" isLoading={isSubmitting} className="mt-2 w-full">
              {isSubmitting ? 'Entrando…' : 'Entrar'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
}
