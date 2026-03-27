import api from './client';

export const subscriptionApi = {
  getCurrent: () => api.get('/subscriptions/current'),
  getPlans: () => api.get('/subscriptions/plans'),
  upgrade: (plan: string) => api.post('/subscriptions/upgrade', plan),
  cancel: () => api.post('/subscriptions/cancel'),
  getHistory: () => api.get('/subscriptions/history'),
  checkPayment: (id: string) => api.get(`/subscriptions/check-payment/${id}`),
  applyPromo: (code: string) => api.post('/subscriptions/apply-promo', code),
};
