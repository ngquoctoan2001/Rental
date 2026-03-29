import api from './client';

export const reportsApi = {
  dashboard: (params?: any) => api.get('/reports/dashboard', { params }),
  revenue: (params?: any) => api.get('/reports/revenue', { params }),
  occupancy: (params?: any) => api.get('/reports/occupancy', { params }),
  collectionRate: (params?: any) => api.get('/reports/collection-rate', { params }),
  export: (type: string, params?: any) => api.get(`/reports/export/${type}`, { params, responseType: 'blob' }),
  monthlySummary: (month: string) => api.get('/reports/monthly', { params: { month } }),
  overdueTrend: (months?: number) => api.get('/reports/overdue-trend', { params: { months } }),
  topRooms: (top?: number) => api.get('/reports/top-rooms', { params: { top } }),
};
