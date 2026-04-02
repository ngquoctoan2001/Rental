import api from './client';

export const tenantsApi = {
  getCurrent: () => api.get('/tenants/current'),
  update: (data: any) => api.put('/tenants/current', data),
  getSettings: () => api.get('/tenants/settings'),
  updateSettings: (data: any) => api.put('/tenants/settings', data),
  // Admin only
  listAllLandlords: () => api.get('/admin/landlords'),
  toggleLandlordActive: (id: string) => api.patch(`/admin/landlords/${id}/toggle-active`, {}),
};
