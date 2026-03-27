import api from './client';

export const staffApi = {
  list: () => api.get('/staff'),
  getById: (id: string) => api.get(`/staff/${id}`),
  invite: (data: { email: string; role: string; assignedPropertyIds?: string[] }) => api.post('/staff', data),
  acceptInvite: (data: { token: string; password: string; fullName: string; phone: string }) => api.post('/staff/accept-invite', data),
  update: (id: string, data: any) => api.put(`/staff/${id}`, data),
  updatePermissions: (id: string, assignedPropertyIds: string[]) => api.patch(`/staff/${id}/permissions`, assignedPropertyIds),
  deactivate: (id: string) => api.delete(`/staff/${id}`),
  getActivityLog: (id: string, page = 1) => api.get(`/staff/${id}/logs`, { params: { page } }),
};
