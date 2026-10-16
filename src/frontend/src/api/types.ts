// Contratos JSON de la API .NET (Core.Application/DTOs). Los enums viajan como texto.

export type Role = 'Admin' | 'Employee';

export interface AuthResponse {
  token: string;
  tokenType: 'Bearer';
  expiresAt: string;
  username: string;
  email: string;
  fullName: string;
  role: Role;
}

export interface Category {
  id: string;
  name: string;
  description: string | null;
  deposit: string;
  createdAt: string;
  lastModifiedAt: string | null;
}

export interface CategoryRequest {
  name: string;
  description: string | null;
  deposit: string;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  brand: string;
  categoryId: string;
  categoryName: string | null;
  deposit: string | null;
  priceBox: number;
  priceUnit: number;
  costPrice: number;
  unitsPerBox: number;
  stockUnits: number;
  stockBoxes: number;
  stockLooseUnits: number;
  stockDisplay: string;
  minStock: number;
  maxStock: number;
  inventoryValue: number;
  isReturnable: boolean;
  emptyGroupKey: string | null;
  emptyGroupName: string | null;
  isGiftEligible: boolean;
  isActive: boolean;
  createdAt: string;
  lastModifiedAt: string | null;
}

export interface ProductFields {
  name: string;
  brand: string;
  categoryId: string;
  priceBox: number;
  priceUnit: number;
  costPrice: number;
  unitsPerBox: number;
  minStock: number;
  maxStock: number;
  isReturnable: boolean;
  emptyGroupKey: string | null;
  isGiftEligible: boolean;
}

export interface CreateProductRequest extends ProductFields {
  initialBoxes: number;
  initialLooseUnits: number;
}

export type UpdateProductRequest = ProductFields;

export type StockHealthStatus = 'OutOfStock' | 'CriticalRisk' | 'Understocked' | 'Optimal' | 'Overstocked' | 'Excessive';

export interface TruckStockLine {
  productId: string;
  sku: string;
  productName: string;
  categoryName: string | null;
  unitsPerBox: number;
  stockUnits: number;
  stockDisplay: string;
  priceUnit: number;
  value: number;
}

export interface Truck {
  id: string;
  name: string;
  plate: string;
  status: string;
  totalUnits: number;
  inventoryValue: number;
  stock: TruckStockLine[];
  capacityBoxes: number;
  loadedBoxes: number;
  occupancyPercent: number;
  dispatchedAt: string | null;
}

/** Estados de la ruta del camión (Truck.Status en la API). */
export type TruckStatus = 'En galpón' | 'Cargando' | 'En ruta' | 'Liquidado';

export interface DashboardResponse {
  from: string;
  to: string;
  categoryId: string | null;
  inventory: {
    activeProducts: number;
    stockUnits: number;
    costValue: number;
    saleValue: number;
    grossMargin: number;
    grossMarginPercent: number;
  };
  sales: {
    salesTotal: number;
    invoicesCount: number;
    giftsCount: number;
    averageTicket: number;
    unitsSold: number;
    collectedOnInvoices: number;
    creditGenerated: number;
    rotationIndex: number;
  };
  stockAlerts: { status: StockHealthStatus; count: number }[];
  criticalProducts: {
    id: string;
    sku: string;
    name: string;
    categoryName: string | null;
    stockUnits: number;
    stockDisplay: string;
    minStock: number;
    maxStock: number;
    status: StockHealthStatus;
    recommendedPurchaseUnits: number;
  }[];
  salesByDay: { date: string; total: number; invoices: number; units: number }[];
  salesByCategory: { categoryId: string; categoryName: string; total: number; units: number }[];
  topProducts: {
    productId: string;
    sku: string;
    name: string;
    categoryName: string | null;
    unitsSold: number;
    revenue: number;
    stockUnits: number;
    rotationIndex: number;
  }[];
  fleet: {
    capacityBoxes: number;
    loadedBoxes: number;
    occupancyPercent: number;
    trucks: TruckOccupancy[];
  };
  debtAging: DebtAging;
}

export interface DebtAging {
  totalDebt: number;
  debtors: number;
  maxOverdueDays: number;
  overdueDebt: number;
  buckets: { label: string; fromDays: number; toDays: number | null; amount: number; clients: number }[];
  topDebtors: { clientId: string; code: string; name: string; debt: number; overdueDebt: number; oldestDays: number; creditLimit: number | null }[];
}

export interface TruckOccupancy {
  truckId: string;
  name: string;
  plate: string;
  capacityBoxes: number;
  loadedBoxes: number;
  occupancyPercent: number;
  stockUnits: number;
  inventoryValue: number;
  salesTotal: number;
  invoices: number;
}

export type InvoiceType = 'Sale' | 'Gift';
export type StockOrigin = 'Warehouse' | 'Truck';

export interface InvoiceSummary {
  id: string;
  number: string;
  type: InvoiceType;
  clientId: string;
  clientName: string;
  issuedAt: string;
  dispatchOrigin: StockOrigin;
  attendantName: string;
  total: number;
  payment: number;
  pending: number;
  isCancelled: boolean;
}

export interface InvoiceItem {
  productId: string;
  sku: string;
  productName: string;
  priceBox: number;
  priceUnit: number;
  unitsPerBox: number;
  boxes: number;
  looseUnits: number;
  totalUnits: number;
  quantityDisplay: string;
  subtotal: number;
}

export interface Invoice {
  id: string;
  number: string;
  type: InvoiceType;
  clientId: string;
  clientName: string;
  clientRif: string;
  issuedAt: string;
  dispatchOrigin: StockOrigin;
  truckId: string | null;
  truckName: string | null;
  truckPlate: string | null;
  attendantName: string;
  total: number;
  payment: number;
  pending: number;
  paymentObservation: string | null;
  generatedEmptyBoxes: number;
  generatedEmptyUnits: number;
  returnedEmptyBoxes: number;
  returnedEmptyUnits: number;
  items: InvoiceItem[];
  emptyGroups: {
    groupKey: string;
    groupName: string;
    generatedBoxes: number;
    generatedUnits: number;
    returnedBoxes: number;
    returnedUnits: number;
    pendingBoxes: number;
    pendingUnits: number;
  }[];
  isCancelled: boolean;
  cancellationReason: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

/** Cuerpo RFC 7807 (application/problem+json); "errors" llega en los 400 de validación. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

export interface LineRequest {
  productId: string;
  boxes: number;
  looseUnits: number;
}

export interface CreateSaleRequest {
  clientId: string;
  dispatchOrigin: StockOrigin;
  truckId: string | null;
  employeeId: string | null;
  items: LineRequest[];
  payment: number;
  paymentObservation: string | null;
  returnedEmpties: { groupKey: string; boxes: number; units: number }[] | null;
}

/** Regalía a cliente (POST /api/invoices/gifts): solo productos autorizados, sin cobro. */
export interface CreateGiftRequest {
  clientId: string;
  dispatchOrigin: StockOrigin;
  truckId: string | null;
  items: LineRequest[];
}

export interface EmptyGroupBalance {
  groupKey: string;
  groupName: string;
  boxes: number;
  units: number;
}

/** Saldos con signo: negativo = el cliente debe; positivo = a favor. */
export interface Client {
  id: string;
  code: string;
  rif: string;
  name: string;
  address: string;
  nickname: string | null;
  type: string;
  moneyBalance: number;
  moneyStatus: string;
  emptyBoxesBalance: number;
  emptyUnitsBalance: number;
  emptiesStatus: string;
  emptyBalances: EmptyGroupBalance[];
  isReceivable: boolean;
  lastPurchaseAt: string | null;
  createdAt: string;
  creditLimit: number | null;
}

/** Tipo de movimiento del estado de cuenta (ClientMovementType en la API). */
export type ClientMovementType =
  | 'Invoice'
  | 'Gift'
  | 'Payment'
  | 'EmptiesGenerated'
  | 'EmptiesReturned'
  | 'InvoiceCancelled'
  | 'BalanceAdjustment';

export interface ClientMovement {
  id: string;
  type: ClientMovementType;
  title: string;
  description: string;
  amount: number;
  invoiceId: string | null;
  date: string;
}

/** GET /api/clients/{id}/statement: movimientos por pestaña, de más reciente a más antiguo. */
export interface ClientStatement {
  client: Client;
  invoices: ClientMovement[];
  gifts: ClientMovement[];
  payments: ClientMovement[];
  empties: ClientMovement[];
  adjustments: ClientMovement[];
}

export interface RouteSettlementLine {
  productId: string;
  productName: string;
  unitsPerBox: number;
  expectedUnits: number;
  expectedDisplay: string;
  countedUnits: number;
  countedDisplay: string;
  differenceUnits: number;
  differenceValue: number;
}

export interface RouteSettlement {
  id: string;
  truckId: string;
  truckName: string;
  truckPlate: string;
  dispatchedAt: string;
  settledAt: string;
  settledBy: string | null;
  invoicesCount: number;
  salesTotal: number;
  expectedCash: number;
  countedCash: number;
  cashDifference: number;
  missingGoodsValue: number;
  isBalanced: boolean;
  observation: string | null;
  lines: RouteSettlementLine[];
}

export interface Employee {
  id: string;
  name: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export type InternalConsumptionType = 'InternalConsumption' | 'Gift';

/** POST /api/internal-consumptions (solo Admin): salida de inventario sin cliente ni cobro. */
export interface CreateInternalConsumptionRequest {
  productId: string;
  boxes: number;
  looseUnits: number;
  origin: StockOrigin;
  truckId: string | null;
  employeeId: string;
  type: InternalConsumptionType;
  observation: string | null;
}

export interface InternalConsumption {
  id: string;
  occurredAt: string;
  type: InternalConsumptionType;
  employeeName: string;
  origin: StockOrigin;
  truckName: string | null;
  productId: string;
  productName: string;
  boxes: number;
  looseUnits: number;
  totalUnits: number;
  /** Valor de referencia a precio de venta (no es un cobro). */
  estimatedValue: number;
  observation: string | null;
}

export interface WithdrawalList<T> {
  items: T[];
  count: number;
  totalValue: number;
}

/** Ancho del papel de la impresora térmica. */
export type PaperWidth = 80 | 58;
