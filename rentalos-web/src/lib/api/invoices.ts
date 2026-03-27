import api from './client';

export const invoicesApi = {
  list: (params?: any) => api.get('/invoices', { params }),
  getById: (id: string) => api.get(`/invoices/${id}`),
  create: (data: any) => api.post('/invoices', data),
  bulkGenerate: (data: { month: string; propertyId?: string }) => api.post('/invoices/bulk-generate', data),
  updateMeter: (id: string, data: { electricityNew: number; waterNew: number }) => api.patch(`/invoices/${id}/meter`, data),
  send: (id: string) => api.post(`/invoices/${id}/send`),
  getPdf: (id: string) => api.get(`/invoices/${id}/pdf`, { responseType: 'blob' }),
  pay: (id: string) => api.post(`/invoices/${id}/pay`),
  cancel: (id: string) => api.post(`/invoices/${id}/cancel`),
  getPaymentLink: (token: string) => api.get(`/invoices/pay/${token}`),
};
