import { useState } from 'react';
import { toApiError } from '../api/client';
import { Button } from '../components/ui/Button';
import { Modal } from '../components/ui/Modal';

interface Props {
  open: boolean;
  title: string;
  message: string;
  confirmLabel: string;
  onClose: () => void;
  onConfirm: () => Promise<void>;
}

/** Confirmación de una acción destructiva; muestra el error de la API sin cerrar el diálogo. */
export function ConfirmDialog({ open, title, message, confirmLabel, onClose, onConfirm }: Props) {
  const [isWorking, setIsWorking] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function confirm() {
    setIsWorking(true);
    setError(null);
    try {
      await onConfirm();
    } catch (err) {
      setError(toApiError(err).message);
    } finally {
      setIsWorking(false);
    }
  }

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={title}
      footer={
        <>
          <Button variant="secondary" onClick={onClose}>
            Cancelar
          </Button>
          <Button variant="danger" onClick={confirm} isLoading={isWorking}>
            {confirmLabel}
          </Button>
        </>
      }
    >
      <p>{message}</p>
      {error && (
        <p className="mt-3 rounded-md border border-status-out/40 bg-status-out/5 px-3 py-2 text-sm" role="alert">
          {error}
        </p>
      )}
    </Modal>
  );
}
