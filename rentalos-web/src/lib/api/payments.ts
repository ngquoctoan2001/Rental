import api, { API_BASE_URL, API_ORIGIN } from './client';

export const paymentApi = {
  createMomo: (data: any) => api.post('/payments/momo', data),
  createVNPay: (data: any) => api.post('/payments/vnpay', data),
  getSettings: () => api.get('/payments/settings'),
  updateSettings: (data: any) => api.put('/payments/settings', data),
};

export const publicApi = {
  getInvoiceByToken: (token: string) => api.get(`/public/invoice/${token}`),
};

export const paymentUrls = {
  momoWebhook: `${API_BASE_URL}/payments/momo/webhook`,
  vnpayWebhook: `${API_BASE_URL}/payments/vnpay/webhook`,
  vnpayReturn: `${API_BASE_URL}/payments/vnpay/return`,
  apiOrigin: API_ORIGIN,
};
