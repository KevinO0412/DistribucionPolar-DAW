import images from '../data/productImages.json';

// Los productos sembrados tienen Id 00000000-0000-4000-8000-{legacyId con 12 dígitos} (CatalogSeedData.cs).
const SEEDED_ID = /^00000000-0000-4000-8000-(\d{12})$/i;
const byLegacyId: Record<string, string> = images;

/** URL de la foto del producto, o null si no tiene (productos creados desde la aplicación). */
export function productImageUrl(productId: string): string | null {
  const match = SEEDED_ID.exec(productId);
  const path = match ? byLegacyId[String(Number(match[1]))] : undefined;
  return path ? `/img/productos/${path}` : null;
}
