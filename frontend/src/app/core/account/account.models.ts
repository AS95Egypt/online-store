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
