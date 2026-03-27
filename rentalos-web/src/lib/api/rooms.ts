import api from './client';
import { Room } from '@/types';

export const roomsApi = {
  list: (propertyId?: string) => api.get<Room[]>('/rooms', { params: { propertyId } }),
  getById: (id: string) => api.get<Room>(`/rooms/${id}`),
  available: (propertyId?: string) => api.get<Room[]>('/rooms/available', { params: { propertyId } }),
  getHistory: (id: string) => api.get(`/rooms/${id}/history`),
  getMeterReadings: (id: string) => api.get(`/rooms/${id}/meter-readings`),
  getQrCode: (id: string) => api.get(`/rooms/${id}/qrcode`),
  create: (data: any) => api.post('/rooms', data),
  update: (id: string, data: any) => api.put(`/rooms/${id}`, data),
  changeStatus: (id: string, status: string) => api.patch(`/rooms/${id}/status`, { status }),
  remove: (id: string) => api.delete(`/rooms/${id}`),
  bulkCreate: (data: any) => api.post('/rooms/bulk', data),
  importCsv: (propertyId: string, file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/rooms/import-csv', fd, { params: { propertyId }, headers: { 'Content-Type': 'multipart/form-data' } });
  },
};
