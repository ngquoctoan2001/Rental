import api from './client';

export const notificationApi = {
  list: (params?: any) => api.get('/notifications', { params }),
  markRead: (id: string) => api.post(`/notifications/${id}/read`),
  markAllRead: () => api.post('/notifications/read-all'),
  getLogs: (params?: any) => api.get('/notifications/logs', { params }),
};
 Eskom Notifications API complete.
