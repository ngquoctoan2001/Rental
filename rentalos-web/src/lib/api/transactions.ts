import api from './client';

export const transactionApi = {
  list: (params?: any) => api.get('/transactions', { params }),
  summary: (params?: any) => api.get('/transactions/summary', { params }),
  recordCash: (data: any) => api.post('/transactions/cash', data),
  recordDepositRefund: (data: any) => api.post('/transactions/deposit-refund', data),
  exportExcel: (params?: any) => api.get('/transactions/export', { params, responseType: 'blob' }),
};
 Eskom Transactions API complete.
