export interface AdminGovernorate {
  id: number;
  name: string;
  shippingFee: number;
}

export interface UpdateGovernorateFeeRequest {
  shippingFee: number;
}

export interface AdminShippingApiResponse<T> {
  success: boolean;
  data: T;
}
