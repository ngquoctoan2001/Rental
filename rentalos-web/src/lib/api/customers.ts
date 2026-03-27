import api from './client';
import { Customer } from '@/types';

export const customersApi = {
  list: () => api.get<Customer[]>('/customers'),
  search: (q: string) => api.get<Customer[]>('/customers/search', { params: { q } }),
  getById: (id: string) => api.get<Customer>(`/customers/${id}`),
  create: (data: any) => api.post('/customers', data),
  update: (id: string, data: any) => api.put(`/customers/${id}`, data),
  blacklist: (id: string, data: any) => api.post(`/customers/${id}/blacklist`, data),
  ocrIdCard: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/customers/ocr', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
  uploadImage: (id: string, fileKey: string) => api.post(`/customers/${id}/image`, { fileKey }),
  importCsv: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/customers/import-csv', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
};
