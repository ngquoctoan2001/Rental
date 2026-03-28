import api from './client';
import { Property, Room } from '@/types';

export const propertiesApi = {
  list: () => api.get<Property[]>('/properties'),
  getById: (id: string) => api.get<Property>(`/properties/${id}`),
  getStats: (id: string) => api.get(`/properties/${id}/stats`),
  create: (data: any) => api.post('/properties', data),
  update: (id: string, data: any) => api.put(`/properties/${id}`, data),
  remove: (id: string) => api.delete(`/properties/${id}`),
  uploadImage: (id: string, file: File, isCover = false) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/properties/${id}/images`, fd, { params: { isCover }, headers: { 'Content-Type': 'multipart/form-data' } });
  },
};
