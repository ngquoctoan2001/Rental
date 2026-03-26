import api from './client';

export const contractsApi = {
  list: (params?: any) => api.get('/contracts', { params }),
  getById: (id: string) => api.get(`/contracts/${id}`),
  create: (data: any) => api.post('/contracts', data),
  update: (id: string, data: any) => api.put(`/contracts/${id}`, data),
  terminate: (id: string) => api.post(`/contracts/${id}/terminate`),
};
