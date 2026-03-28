import axios, { AxiosError } from 'axios';
import { useAuthStore } from '@/lib/stores/authStore';

function stripTrailingSlash(value: string) {
  return value.replace(/\/+$/, '');
}

function normalizeApiBaseUrl(value: string) {
  const normalized = stripTrailingSlash(value.trim());
  if (normalized.endsWith('/api/v1')) return normalized;
  if (normalized.endsWith('/api')) return `${normalized}/v1`;
  return `${normalized}/api/v1`;
}

const configuredApiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api/v1';

export const API_BASE_URL = normalizeApiBaseUrl(configuredApiUrl);
export const API_ORIGIN = API_BASE_URL.replace(/\/api\/v1$/, '');
export const APP_BASE_URL = stripTrailingSlash(process.env.NEXT_PUBLIC_APP_URL || 'http://localhost:3000');
export const SIGNALR_URL = stripTrailingSlash(
  process.env.NEXT_PUBLIC_SIGNALR_URL || `${API_ORIGIN}/hubs/notifications`
);

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

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

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401 && useAuthStore.getState().accessToken) {
      useAuthStore.getState().logout();
    }

    return Promise.reject(formatError(error));
  }
);

export type ApiError = {
  code: string;
  message: string;
  details: unknown;
  status?: number;
};

export const formatError = (error: AxiosError): ApiError => {
  const data = error.response?.data as any;
  const nestedError = data?.error;

  return {
    code: nestedError?.code || data?.code || error.code || 'UNKNOWN_ERROR',
    message:
      nestedError?.message ||
      data?.message ||
      data?.title ||
      (typeof data === 'string' ? data : null) ||
      error.message ||
      'Đã có lỗi xảy ra, vui lòng thử lại.',
    details: nestedError?.details || data?.details || data?.errors || null,
    status: error.response?.status,
  };
};

export default apiClient;
