import { UserRole } from '../auth/auth.models';

export interface CustomerProfile {
  id: string;
  name: string;
  email: string;
  role: UserRole;
}

export interface UpdateCustomerProfileRequest {
  name: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface CurrentUser {
  name: string;
  email: string;
  role: UserRole;
}

export interface CustomerAddress {
  fullName: string | null;
  phone: string | null;
  governorateId: number | null;
  governorateName: string | null;
  city: string | null;
  district: string | null;
  street: string | null;
  buildingNumber: string | null;
  apartment: string | null;
  instructions: string | null;
}

export interface UpdateCustomerAddressRequest {
  fullName: string;
  phone: string;
  governorateId: number;
  city: string;
  district: string;
  street: string;
  buildingNumber: string;
  apartment: string | null;
  instructions: string | null;
}
