import api from './client';

export const contractsApi = {
  list: (params?: any) => api.get('/contracts', { params }),
  getById: (id: string) => api.get(`/contracts/${id}`),
  create: (data: any) => api.post('/contracts', data),
  update: (id: string, data: any) => api.put(`/contracts/${id}`, { ...data, id }),
  terminate: (id: string, data: any) => api.put(`/contracts/${id}/terminate`, data),
  sign: (id: string) => api.post(`/contracts/${id}/sign`),
  renew: (id: string, data: any) => api.post(`/contracts/${id}/renew`, data),
  expiring: () => api.get('/contracts/expiring'),
  getInvoices: (id: string) => api.get(`/contracts/${id}/invoices`),
  getPdf: (id: string) => api.get(`/contracts/${id}/pdf`),
  addCoTenant: (id: string, data: any) => api.post(`/contracts/${id}/cotenants`, { ...data, contractId: id }),
  removeCoTenant: (id: string, customerId: string) => api.delete(`/contracts/${id}/cotenants/${customerId}`),
};
