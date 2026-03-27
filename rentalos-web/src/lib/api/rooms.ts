import api from './client';
import { Room } from '@/types';

export const roomsApi = {
  list: (propertyId?: string, params?: any) => api.get<Room[]>('/rooms', { params: { propertyId, ...params } }),
  getById: (id: string) => api.get<Room>(`/rooms/${id}`),
  available: (propertyId?: string) => api.get<Room[]>('/rooms/available', { params: { propertyId } }),
  getHistory: (id: string) => api.get(`/rooms/${id}/history`),
  getMeterReadings: (id: string, month?: string) => api.get(`/rooms/${id}/meter-readings`, { params: { month } }),
  getQrCode: (id: string) => api.get(`/rooms/${id}/qrcode`, { responseType: 'blob' }),
  create: (data: any) => api.post('/rooms', data),
  update: (id: string, data: any) => api.put(`/rooms/${id}`, data),
  changeStatus: (id: string, data: { id: string; newStatus: string; maintenanceNote?: string }) => api.patch(`/rooms/${id}/status`, data),
  remove: (id: string) => api.delete(`/rooms/${id}`),
  bulkCreate: (data: any) => api.post('/rooms/bulk-create', data),
  uploadImage: (id: string, file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/rooms/${id}/images`, fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
  importCsv: (propertyId: string, file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/rooms/import-csv', fd, { params: { propertyId }, headers: { 'Content-Type': 'multipart/form-data' } });
  },
};
