import api from './client';

export const subscriptionApi = {
  getCurrent: () => api.get('/subscriptions/current'),
  getPlans: () => api.get('/subscriptions/plans'),
  upgrade: (planId: string) => api.post('/subscriptions/upgrade', { planId }),
  cancel: () => api.post('/subscriptions/cancel'),
};
