import api from './client';

export const meterReadingsApi = {
  list: (params?: { roomId?: string; propertyId?: string; month?: string; page?: number; pageSize?: number }) =>
    api.get('/meter-readings', { params }),
  getById: (id: string) => api.get(`/meter-readings/${id}`),
  create: (data: {
    roomId: string;
    readingDate: string;
    electricityReading: number;
    waterReading: number;
    electricityImage?: string;
    waterImage?: string;
    note?: string;
  }) => api.post('/meter-readings', data),
  update: (id: string, data: {
    id: string;
    readingDate: string;
    electricityReading: number;
    waterReading: number;
    electricityImage?: string;
    waterImage?: string;
    note?: string;
  }) => api.put(`/meter-readings/${id}`, { ...data, id }),
  remove: (id: string) => api.delete(`/meter-readings/${id}`),
};
