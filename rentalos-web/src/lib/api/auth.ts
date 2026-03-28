import api from './client';
import { AuthResult } from '@/types';

export interface LoginPayload {
  email: string;
  password: string;
  tenantSlug: string;
}

export interface RegisterPayload {
  ownerName: string;
  ownerEmail: string;
  phone: string;
  tenantName: string;
  password: string;
}

export interface ForgotPasswordPayload {
  email: string;
  tenantSlug: string;
}

export interface ResetPasswordPayload {
  email: string;
  token: string;
  newPassword: string;
  tenantSlug: string;
}

export interface ChangePasswordPayload {
  currentPassword: string;
  newPassword: string;
}

export const authApi = {
  login: (data: LoginPayload) => api.post<AuthResult>('/auth/login', data),
  register: (data: RegisterPayload) => api.post<AuthResult>('/auth/register', data),
  refresh: () => api.post<AuthResult>('/auth/refresh'),
  forgotPassword: (data: ForgotPasswordPayload) => api.post('/auth/forgot-password', data),
  resetPassword: (data: ResetPasswordPayload) => api.post('/auth/reset-password', data),
  changePassword: (data: ChangePasswordPayload) => api.put('/auth/change-password', data),
};
