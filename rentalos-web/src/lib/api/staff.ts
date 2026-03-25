import api from './client';

export const staffApi = {
  list: () => api.get('/staff'),
  getById: (id: string) => api.get(`/staff/${id}`),
  invite: (data: any) => api.post('/staff/invite', data),
  acceptInvite: (token: string, data: any) => api.post(`/staff/accept-invite/${token}`, data),
  update: (id: string, data: any) => api.put(`/staff/${id}`, data),
  updatePermissions: (id: string, permissions: string[]) => api.patch(`/staff/${id}/permissions`, { permissions }),
  deactivate: (id: string) => api.post(`/staff/${id}/deactivate`),
};
 Eskom Staff API complete.
