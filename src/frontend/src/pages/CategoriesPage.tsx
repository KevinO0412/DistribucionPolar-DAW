import { useState, type FormEvent } from 'react';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import { toApiError } from '../api/client';
import { categoriesApi } from '../api/endpoints';
import type { Category } from '../api/types';
import { PageHeader } from '../components/layout/PageHeader';
import { Button } from '../components/ui/Button';
import { TextField } from '../components/ui/Field';
import { Modal } from '../components/ui/Modal';
import { EmptyState, ErrorState, LoadingState } from '../components/ui/States';
import { useToast } from '../components/ui/Toast';
import { useApiQuery } from '../hooks/useApiQuery';
import { useAuth } from '../hooks/useAuth';
import { ConfirmDialog } from './ConfirmDialog';

function CategoryForm({ category, onClose, onSaved }: { category: Category | null; onClose: () => void; onSaved: (name: string) => void }) {
  const [name, setName] = useState(category?.name ?? '');
  const [description, setDescription] = useState(category?.description ?? '');
  const [deposit, setDeposit] = useState(category?.deposit ?? '');
  const [errors, setErrors] = useState<{ name?: string; deposit?: string; form?: string }>({});
  const [isSaving, setIsSaving] = useState(false);

  async function submit(event: FormEvent) {
    event.preventDefault();
    const found = { name: name.trim() ? undefined : 'Escribe el nombre.', deposit: deposit.trim() ? undefined : 'Indica el depósito del galpón.' };
    setErrors(found);
    if (found.name || found.deposit) return;
    setIsSaving(true);
    try {
      const body = { name: name.trim(), description: description.trim() || null, deposit: deposit.trim() };
      if (category) await categoriesApi.update(category.id, body);
      else await categoriesApi.create(body);
      onSaved(body.name);
    } catch (err) {
      const apiError = toApiError(err);
      setErrors({ name: apiError.fieldError('name'), deposit: apiError.fieldError('deposit'), form: apiError.message });
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <Modal
      open
      onClose={onClose}
      title={category ? `Editar ${category.name}` : 'Nueva categoría'}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button type="submit" form="category-form" isLoading={isSaving}>
            {category ? 'Guardar cambios' : 'Crear categoría'}
          </Button>
        </>
      }
    >
      <form id="category-form" onSubmit={submit} noValidate className="flex flex-col gap-4">
        {errors.form && (
          <p className="rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
            {errors.form}
          </p>
        )}
        <TextField label="Nombre" value={name} onChange={(e) => setName(e.target.value)} error={errors.name} maxLength={50} />
        <TextField label="Depósito del galpón" value={deposit} onChange={(e) => setDeposit(e.target.value)} error={errors.deposit} maxLength={100} hint="Por ejemplo: Depósito #4 - Licores" />
        <TextField label="Descripción (opcional)" value={description} onChange={(e) => setDescription(e.target.value)} maxLength={250} />
      </form>
    </Modal>
  );
}

/** Categorías: todos las consultan; solo Admin crea, edita o elimina (igual que en la API). */
export function CategoriesPage() {
  const { isAdmin } = useAuth();
  const notify = useToast();
  const categories = useApiQuery(() => categoriesApi.list(), []);
  const [editing, setEditing] = useState<Category | 'new' | null>(null);
  const [deleting, setDeleting] = useState<Category | null>(null);

  return (
    <>
      <PageHeader
        title="Categorías"
        description="Cada categoría se guarda en su propio depósito del galpón."
        actions={
          isAdmin && (
            <Button onClick={() => setEditing('new')}>
              <Plus className="h-4 w-4" aria-hidden />
              Nueva categoría
            </Button>
          )
        }
      />
      {categories.error ? (
        <ErrorState error={categories.error} onRetry={categories.reload} />
      ) : !categories.data ? (
        <LoadingState />
      ) : categories.data.length === 0 ? (
        <EmptyState title="Aún no hay categorías" />
      ) : (
        <ul className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {categories.data.map((c) => (
            <li key={c.id} className="panel flex items-start justify-between gap-3 p-4">
              <div className="min-w-0">
                <h2 className="font-display text-xl font-semibold">{c.name}</h2>
                <p className="text-sm text-muted">{c.deposit}</p>
                {c.description && <p className="mt-2 text-sm">{c.description}</p>}
              </div>
              {isAdmin && (
                <div className="flex shrink-0 gap-1">
                  <Button variant="ghost" size="sm" onClick={() => setEditing(c)} aria-label={`Editar ${c.name}`}>
                    <Pencil className="h-4 w-4" aria-hidden />
                  </Button>
                  <Button variant="ghost" size="sm" onClick={() => setDeleting(c)} aria-label={`Eliminar ${c.name}`} className="text-status-out">
                    <Trash2 className="h-4 w-4" aria-hidden />
                  </Button>
                </div>
              )}
            </li>
          ))}
        </ul>
      )}

      {editing && (
        <CategoryForm
          category={editing === 'new' ? null : editing}
          onClose={() => setEditing(null)}
          onSaved={(name) => {
            notify(editing === 'new' ? `Categoría ${name} creada.` : `Categoría ${name} actualizada.`);
            setEditing(null);
            categories.reload();
          }}
        />
      )}
      <ConfirmDialog
        open={deleting !== null}
        title="Eliminar categoría"
        message={`Solo se puede eliminar ${deleting?.name ?? ''} si no tiene productos.`}
        confirmLabel="Eliminar categoría"
        onClose={() => setDeleting(null)}
        onConfirm={async () => {
          if (!deleting) return;
          await categoriesApi.remove(deleting.id);
          notify(`Categoría ${deleting.name} eliminada.`);
          setDeleting(null);
          categories.reload();
        }}
      />
    </>
  );
}
