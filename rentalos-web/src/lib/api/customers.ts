import api from './client';
import { Customer } from '@/types';

export const customersApi = {
  list: (params?: any) => api.get<Customer[]>('/customers', { params }),
  lookup: (q: string) => api.get<Customer[]>('/customers/lookup', { params: { q } }),
  getById: (id: string) => api.get<Customer>(`/customers/${id}`),
  create: (data: any) => api.post('/customers', data),
  update: (id: string, data: any) => api.put(`/customers/${id}`, data),
  blacklist: (id: string, data: any) => api.patch(`/customers/${id}/blacklist`, data),
  getContracts: (id: string) => api.get(`/customers/${id}/contracts`),
  getInvoices: (id: string) => api.get(`/customers/${id}/invoices`),
  ocrIdCard: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/customers/ocr', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
  uploadImage: (id: string, file: File, type = 'portrait') => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post(`/customers/${id}/images`, fd, { params: { type }, headers: { 'Content-Type': 'multipart/form-data' } });
  },
  importCsv: (file: File) => {
    const fd = new FormData();
    fd.append('file', file);
    return api.post('/customers/import-csv', fd, { headers: { 'Content-Type': 'multipart/form-data' } });
  },
};
