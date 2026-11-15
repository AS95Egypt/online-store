export type AdminOrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Processing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled';

export interface AdminOrderListItem {
  id: number;
  orderNumber: string;
  createdAt: string;
  customerName: string;
  itemsCount: number;
  total: number;
  status: AdminOrderStatus;
  paymentStatus: string;
}

export interface OrderStatusCounts {
  all: number;
  pending: number;
  confirmed: number;
  processing: number;
  shipped: number;
  delivered: number;
  cancelled: number;
}

export interface AdminOrderListResult {
  items: AdminOrderListItem[];
  totalCount: number;
  counts: OrderStatusCounts;
}

export interface AdminOrderStatusHistory {
  status: AdminOrderStatus;
  changedAt: string;
  changedBy: string | null;
}

export interface AdminOrderAvailableActions {
  nextStatus: AdminOrderStatus | null;
  canCancel: boolean;
  canMarkPaid: boolean;
}

export interface AdminOrderDetails {
  id: number;
  orderNumber: string;
  createdAt: string;
  status: AdminOrderStatus;
  paymentMethod: string;
  paymentStatus: string;
  paidAt: string | null;
  customerName: string;
  customerPhone: string;
  subtotal: number;
  shippingFee: number;
  total: number;
  shippingFullName: string;
  shippingPhone: string;
  shippingGovernorateName: string;
  shippingCity: string;
  shippingDistrict: string;
  shippingStreet: string;
  shippingBuildingNumber: string;
  shippingApartment: string | null;
  shippingInstructions: string | null;
  items: AdminOrderItem[];
  statusHistory: AdminOrderStatusHistory[];
  availableActions: AdminOrderAvailableActions;
}

export interface AdminOrderItem {
  productNameSnapshot: string;
  skuSnapshot: string;
  unitPriceSnapshot: number;
  quantity: number;
  lineTotal: number;
}

export interface AdminApiResponse<T> {
  success: boolean;
  data: T;
}

export const ADMIN_ORDER_STATUSES: readonly AdminOrderStatus[] = [
  'Pending',
  'Confirmed',
  'Processing',
  'Shipped',
  'Delivered',
];

export const ADMIN_ORDER_FILTERS: readonly AdminOrderStatus[] = [
  ...ADMIN_ORDER_STATUSES,
  'Cancelled',
];
