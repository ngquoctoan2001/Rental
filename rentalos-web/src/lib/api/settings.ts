import api from './client';

export const settingsApi = {
  getAll: () => api.get('/settings'),
  updateMomo: (data: any) => api.put('/settings/momo', data),
  updateVNPay: (data: any) => api.put('/settings/vnpay', data),
  updateBank: (data: any) => api.put('/settings/bank', data),
  updateBilling: (data: any) => api.put('/settings/billing', data),
  updateCompany: (data: any) => api.put('/settings/company', data),
  testMomo: () => api.post('/settings/momo/test'),
  testVNPay: () => api.post('/settings/vnpay/test'),
  uploadLogo: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/settings/logo', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
};


