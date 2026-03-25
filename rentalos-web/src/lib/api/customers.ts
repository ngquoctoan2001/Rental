import api from './client';
import { Customer, Contract, Invoice } from '@/types';

export const customerApi = {
  list: () => api.get<Customer[]>('/customers'),
  search: (q: string) => api.get<Customer[]>('/customers/search', { params: { q } }),
  getById: (id: string) => api.get<Customer>(`/customers/${id}`),
  create: (data: any) => api.post('/customers', data),
  update: (id: string, data: any) => api.put(`/customers/${id}`, data),
  blacklist: (id: string, data: any) => api.post(`/customers/${id}/blacklist`, data),
  ocrIdCard: (fileKey: string) => api.post('/customers/ocr', { fileKey }),
  uploadImage: (id: string, fileKey: string) => api.post(`/customers/${id}/image`, { fileKey }),
};

export const contractApi = {
  list: () => api.get<Contract[]>('/contracts'),
  getById: (id: string) => api.get<Contract>(`/contracts/${id}`),
  expiring: () => api.get<Contract[]>('/contracts/expiring'),
  create: (data: any) => api.post('/contracts', data),
  update: (id: string, data: any) => api.put(`/contracts/${id}`, data),
  terminate: (id: string, data: any) => api.post(`/contracts/${id}/terminate`, data),
  renew: (id: string, data: any) => api.post(`/contracts/${id}/renew`, data),
  getPdf: (id: string) => api.get(`/contracts/${id}/pdf`, { responseType: 'blob' }),
};

export const invoiceApi = {
  list: () => api.get<Invoice[]>('/invoices'),
  overdue: () => api.get<Invoice[]>('/invoices/overdue'),
  pendingMeter: () => api.get<Invoice[]>('/invoices/pending-meter'),
  getById: (id: string) => api.get<Invoice>(`/invoices/${id}`),
  create: (data: any) => api.post('/invoices', data),
  bulkGenerate: (data: any) => api.post('/invoices/bulk', data),
  update: (id: string, data: any) => api.put(`/invoices/${id}`, data),
  cancel: (id: string) => api.post(`/invoices/${id}/cancel`),
  send: (id: string) => api.post(`/invoices/${id}/send`),
  getPdf: (id: string) => api.get(`/invoices/${id}/pdf`, { responseType: 'blob' }),
  getByToken: (token: string) => api.get(`/invoices/public/${token}`),
};
 Eskom CRM, Billing & Contracts API complete.
