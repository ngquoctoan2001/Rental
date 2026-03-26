import api from './client';
import { Property, Room } from '@/types';

export const propertiesApi = {
  list: () => api.get<Property[]>('/properties'),
  getById: (id: string) => api.get<Property>(`/properties/${id}`),
  getStats: (id: string) => api.get(`/properties/${id}/stats`),
  create: (data: any) => api.post('/properties', data),
  update: (id: string, data: any) => api.put(`/properties/${id}`, data),
  remove: (id: string) => api.delete(`/properties/${id}`),
  uploadImage: (id: string, fileKey: string) => api.post(`/properties/${id}/image`, { fileKey }),
};
