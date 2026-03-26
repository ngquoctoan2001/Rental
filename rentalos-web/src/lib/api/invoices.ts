import api from './client';

export const invoicesApi = {
  list: (params?: any) => api.get('/invoices', { params }),
  getById: (id: string) => api.get(`/invoices/${id}`),
  create: (data: any) => api.post('/invoices', data),
  pay: (id: string) => api.post(`/invoices/${id}/pay`),
  void: (id: string) => api.post(`/invoices/${id}/void`),
};
