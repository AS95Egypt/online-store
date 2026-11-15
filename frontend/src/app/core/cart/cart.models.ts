export interface CartItem {
  productId: number;
  itemNameEn: string;
  primaryImageUrl: string | null;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Cart {
  cartId: number;
  items: CartItem[];
  subtotal: number;
}

export interface AddCartItemRequest {
  productId: number;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

export interface CheckoutLineError {
  productId: number;
  reason: string;
}

export interface CheckoutCalculationResult {
  isValid: boolean;
  items: CartItem[] | null;
  subtotal: number | null;
  shippingFee: number | null;
  total: number | null;
  governorateError: string | null;
  errors: CheckoutLineError[] | null;
}
