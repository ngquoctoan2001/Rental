import api from './client';
import { Property, Room } from '@/types';

export const propertiesApi = {
  list: (params?: { page?: number; pageSize?: number; search?: string; isActive?: boolean }) => api.get<Property[]>('/properties', { params }),
  getById: (id: string) => api.get<Property>(`/properties/${id}`),
  getStats: (id: string) => api.get(`/properties/${id}/stats`),
  getRooms: (id: string, params?: { page?: number; pageSize?: number }) => api.get<Room[]>(`/properties/${id}/rooms`, { params }),
  create: (data: any) => api.post('/properties', data),
  update: (id: string, data: any) => api.put(`/properties/${id}`, { ...data, id }),
  remove: (id: string) => api.delete(`/properties/${id}`),
  uploadImage: (id: string, file: File, isCover = false) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/properties/${id}/images`, fd, { params: { isCover }, headers: { 'Content-Type': 'multipart/form-data' } });
  },
};
