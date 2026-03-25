import api from './client';
import { Property, Room } from '@/types';

export const propertyApi = {
  list: () => api.get<Property[]>('/properties'),
  getById: (id: string) => api.get<Property>(`/properties/${id}`),
  getStats: (id: string) => api.get(`/properties/${id}/stats`),
  create: (data: any) => api.post('/properties', data),
  update: (id: string, data: any) => api.put(`/properties/${id}`, data),
  remove: (id: string) => api.delete(`/properties/${id}`),
  uploadImage: (id: string, fileKey: string) => api.post(`/properties/${id}/image`, { fileKey }),
};

export const roomApi = {
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
};
 Eskom Properties & Rooms API complete.
