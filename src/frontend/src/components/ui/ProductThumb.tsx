import { useState } from 'react';
import { Package } from 'lucide-react';
import { productImageUrl } from '../../lib/productImage';
import { cn } from '../../lib/cn';

interface ProductThumbProps {
  productId: string;
  name: string;
  className?: string;
}

/** Foto del producto; si no tiene o no carga, muestra un ícono genérico. */
export function ProductThumb({ productId, name, className }: ProductThumbProps) {
  const [failed, setFailed] = useState(false);
  const src = productImageUrl(productId);
  const box = cn('flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-md border border-line bg-white', className);

  if (!src || failed) {
    return (
      <div className={cn(box, 'bg-surface text-muted')} aria-hidden>
        <Package className="h-1/2 w-1/2" />
      </div>
    );
  }
  return (
    <div className={box}>
      <img src={src} alt={name} loading="lazy" decoding="async" onError={() => setFailed(true)} className="h-full w-full object-contain" />
    </div>
  );
}
