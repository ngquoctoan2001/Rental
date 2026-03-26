import api from './client';

export const settingsApi = {
  getAll: () => api.get('/settings'),
  updateMomo: (data: any) => api.put('/settings/momo', data),
  updateVNPay: (data: any) => api.put('/settings/vnpay', data),
  updateBank: (data: any) => api.put('/settings/bank', data),
  updateZalo: (data: any) => api.put('/settings/zalo', data),
  updateSms: (data: any) => api.put('/settings/sms', data),
  updateEmail: (data: any) => api.put('/settings/email', data),
  updateBilling: (data: any) => api.put('/settings/billing', data),
  updateCompany: (data: any) => api.put('/settings/company', data),
  testMomo: () => api.post('/settings/momo/test'),
  testVNPay: () => api.post('/settings/vnpay/test'),
  uploadLogo: (fileKey: string) => api.post('/settings/logo', { fileKey }),
};

export const notificationApi = {
  list: (params?: any) => api.get('/notifications', { params }),
  markRead: (id: string) => api.post(`/notifications/${id}/read`),
  markAllRead: () => api.post('/notifications/read-all'),
  getLogs: (params?: any) => api.get('/notifications/logs', { params }),
};


