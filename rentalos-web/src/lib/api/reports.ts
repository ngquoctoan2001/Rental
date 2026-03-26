import api from './client';

export const reportsApi = {
  dashboard: () => api.get('/reports/dashboard'),
  revenue: (params?: any) => api.get('/reports/revenue', { params }),
  occupancy: (params?: any) => api.get('/reports/occupancy', { params }),
  collectionRate: (params?: any) => api.get('/reports/collection-rate', { params }),
  export: (type: string, params?: any) => api.get(`/reports/export/${type}`, { params, responseType: 'blob' }),
};
