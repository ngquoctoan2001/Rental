import api from './client';

export const notificationsApi = {
  list: () => api.get('/notifications'),
  markRead: (id: string) => api.post(`/notifications/${id}/mark-read`),
  markAllRead: () => api.post('/notifications/mark-all-read'),
  getLogs: (params?: any) => api.get('/notifications/logs', { params }),
};

/** @deprecated use notificationsApi */
export const notificationApi = notificationsApi;
