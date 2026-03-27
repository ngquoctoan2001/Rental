import api from './client';

export const transactionsApi = {
  list: (params?: any) => api.get('/transactions', { params }),
  getById: (id: string) => api.get(`/transactions/${id}`),
  summary: (params?: any) => api.get('/transactions/summary', { params }),
  recordCash: (data: any) => api.post('/transactions/cash-payment', data),
  recordDepositRefund: (data: any) => api.post('/transactions/deposit-refund', data),
  exportExcel: (params?: any) => api.get('/transactions/export', { params, responseType: 'blob' }),
};
