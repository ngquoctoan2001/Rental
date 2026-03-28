import axios, { AxiosError } from 'axios';
import { useAuthStore } from '@/lib/stores/authStore';

const apiClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api',
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: Attach Token & Tenant
apiClient.interceptors.request.use((config) => {
  const { accessToken, tenant } = useAuthStore.getState();
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }
  if (tenant?.slug) {
    config.headers['X-Tenant-Slug'] = tenant.slug;
  }
  return config;
});

// Response Interceptor: Handle Refresh Token & Global Errors
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config;
    
    // Auto-refresh token on 401
    if (error.response?.status === 401 && originalRequest && !(originalRequest as any)._retry) {
      (originalRequest as any)._retry = true;
      try {
        const { refreshToken, logout, setAuth } = useAuthStore.getState();
        if (!refreshToken) throw new Error('No refresh token');

        // Call refresh endpoint directly to avoid interceptor loop
        const resp = await axios.post(`${process.env.NEXT_PUBLIC_API_URL}/auth/refresh`, { refreshToken });
        setAuth(resp.data);

        originalRequest.headers.Authorization = `Bearer ${resp.data.accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        useAuthStore.getState().logout();
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(formatError(error));
  }
);

export const formatError = (error: AxiosError) => {
  const data = error.response?.data as any;
  return {
    code: data?.code || error.code || 'UNKNOWN_ERROR',
    message: data?.message || error.message || 'Đã có lỗi xảy ra, vui lòng thử lại.',
    details: data?.errors || null,
  };
};

export default apiClient;
