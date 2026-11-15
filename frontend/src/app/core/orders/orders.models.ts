import { CustomerAddress } from '../account/account.models';

export interface OrderSummary {
  orderNumber: string;
  createdAt: string;
  itemsCount: number;
  total: number;
  status: string;
}

export interface OrderItem {
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderStatusHistoryEntry {
  status: string;
  changedAt: string;
}

export interface OrderDetails {
  orderNumber: string;
  createdAt: string;
  status: string;
  paymentMethod: string;
  paymentStatus: string;
  subtotal: number;
  shippingFee: number;
  total: number;
  items: OrderItem[];
  shippingAddress: CustomerAddress;
  statusHistory: OrderStatusHistoryEntry[];
}

export interface CreateOrderRequest {
  clientRequestId: string;
  expectedItems: {
    productId: number;
    quantity: number;
    unitPrice: number;
  }[];
  expectedShippingFee: number;
}

export interface OrderItemError {
  productId: number;
  reason: string;
}
