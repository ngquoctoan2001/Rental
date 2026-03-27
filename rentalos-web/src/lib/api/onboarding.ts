import api from './client';

export const onboardingApi = {
  getStatus: () => api.get('/onboarding/status'),
  getSteps: () => api.get('/onboarding/steps'),
  complete: () => api.post('/onboarding/complete'),
};
