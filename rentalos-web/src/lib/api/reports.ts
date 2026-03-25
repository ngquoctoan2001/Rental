import api from './client';

export const reportApi = {
  dashboard: () => api.get('/reports/dashboard'),
  revenue: (params?: any) => api.get('/reports/revenue', { params }),
  occupancy: (params?: any) => api.get('/reports/occupancy', { params }),
  collectionRate: (params?: any) => api.get('/reports/collection-rate', { params }),
  export: (type: string, params?: any) => api.get(`/reports/export/${type}`, { params, responseType: 'blob' }),
};

export const aiApi = {
  getConversations: () => api.get('/ai/conversations'),
  deleteConversation: (id: string) => api.delete(`/ai/conversations/${id}`),
  // Chat sử dụng EventSource truyền thống hoặc fetch cho streaming (trả về EventSource URL hoặc dùng stream trực tiếp)
  chatUrl: `${process.env.NEXT_PUBLIC_API_URL}/ai/chat`,
};

export const subscriptionApi = {
  getCurrent: () => api.get('/subscriptions/current'),
  getPlans: () => api.get('/subscriptions/plans'),
  upgrade: (planId: string) => api.post('/subscriptions/upgrade', { planId }),
  cancel: () => api.post('/subscriptions/cancel'),
};
 Eskom Reports, AI & Subscriptions API complete.
