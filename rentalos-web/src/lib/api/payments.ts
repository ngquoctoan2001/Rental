import api from './client';

export const paymentApi = {
  createMomo: (data: any) => api.post('/payments/momo', data),
  createVNPay: (data: any) => api.post('/payments/vnpay', data),
  getSettings: () => api.get('/payments/settings'),
  updateSettings: (data: any) => api.put('/payments/settings', data),
};

export const publicApi = {
  getInvoiceByToken: (token: string) => api.get(`/public/invoice/${token}`),
};
