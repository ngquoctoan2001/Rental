import api from './client';

export const transactionsApi = {
  list: (params?: any) => api.get('/transactions', { params }),
  getById: (id: string) => api.get(`/transactions/${id}`),
  summary: (params?: any) => api.get('/transactions/summary', { params }),
  recordCash: (data: any) => api.post('/transactions/cash-payment', data),
  recordDepositRefund: (data: any) => api.post('/transactions/deposit-refund', data),
  exportExcel: (params?: any) => {
    const nextParams = { ...params };
    if (nextParams?.start && !nextParams.dateFrom) {
      nextParams.dateFrom = nextParams.start;
      delete nextParams.start;
    }
    if (nextParams?.end && !nextParams.dateTo) {
      nextParams.dateTo = nextParams.end;
      delete nextParams.end;
    }
    return api.get('/transactions/export', { params: nextParams, responseType: 'blob' });
  },
};
