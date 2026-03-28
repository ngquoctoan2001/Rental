import api from './client';

export const invoicesApi = {
  list: (params?: any) => {
    const nextParams = { ...params };
    if (nextParams?.month && !nextParams.billingMonth) {
      nextParams.billingMonth = nextParams.month;
      delete nextParams.month;
    }
    return api.get('/invoices', { params: nextParams });
  },
  getById: (id: string) => api.get(`/invoices/${id}`),
  pendingMeter: (params?: any) => api.get('/invoices/pending-meter', { params }),
  create: (data: any) => api.post('/invoices', data),
  bulkGenerate: (data: any) => api.post('/invoices/bulk-generate', data),
  updateMeter: (id: string, data: { invoiceId: string; electricityNew: number; waterNew: number; meterImageElectricity?: string; meterImageWater?: string }) => api.put(`/invoices/${id}/meter`, data),
  send: (id: string, data?: { channel?: string }) => api.post(`/invoices/${id}/send`, data ?? { channel: 'zalo' }),
  cancel: (id: string, data?: { invoiceId: string; reason?: string }) => api.post(`/invoices/${id}/cancel`, data ?? { invoiceId: id }),
  getPublicByToken: (token: string) => api.get(`/public/invoice/${token}`),
};
