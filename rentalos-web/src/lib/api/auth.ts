import api from './client';
import { AuthResult } from '@/types';

export const authApi = {
  login: (data: any) => api.post<AuthResult>('/auth/login', data),
  register: (data: any) => api.post<AuthResult>('/auth/register', data),
  refresh: (refreshToken: string) => api.post<AuthResult>('/auth/refresh', { refreshToken }),
  forgotPassword: (email: string) => api.post('/auth/forgot-password', { email }),
  resetPassword: (data: any) => api.post('/auth/reset-password', data),
  changePassword: (data: any) => api.put('/auth/change-password', data),
  getProfile: () => api.get('/auth/profile'),
  updateProfile: (data: any) => api.put('/auth/profile', data),
};
