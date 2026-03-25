import api from './client';

export const paymentApi = {
  createMomo: (data: any) => api.post('/payments/momo', data),
  createVNPay: (data: any) => api.post('/payments/vnpay', data),
  getPublicInvoice: (token: string) => api.get(`/payments/invoice/${token}`),
  getMethods: () => api.get('/payments/methods'),
};

export const transactionApi = {
  list: (params?: any) => api.get('/transactions', { params }),
  summary: (params?: any) => api.get('/transactions/summary', { params }),
  recordCash: (data: any) => api.post('/transactions/cash', data),
  recordDepositRefund: (data: any) => api.post('/transactions/deposit-refund', data),
  exportExcel: (params?: any) => api.get('/transactions/export', { params, responseType: 'blob' }),
};
 Eskom Payments & Transactions API complete.
