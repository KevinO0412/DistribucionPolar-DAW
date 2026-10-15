import { Link } from 'react-router-dom';
import { homeFor } from '../components/layout/navigation';
import { useAuth } from '../hooks/useAuth';

function StatusPage({ title, message }: { title: string; message: string }) {
  const { user } = useAuth();
  return (
    <div className="flex flex-col items-start gap-3 py-16">
      <h1 className="font-display text-3xl font-semibold">{title}</h1>
      <p className="max-w-prose text-muted">{message}</p>
      <Link to={homeFor(user?.role)} className="font-semibold text-brand underline-offset-4 hover:underline">
        Ir al inicio
      </Link>
    </div>
  );
}

export const ForbiddenPage = () => (
  <StatusPage title="Esta sección es solo para administradores" message="Tu rol de empleado no puede ver el dashboard. Si lo necesitas, pide acceso al administrador." />
);

export const NotFoundPage = () => <StatusPage title="Esta página no existe" message="Revisa la dirección o vuelve al inicio." />;
