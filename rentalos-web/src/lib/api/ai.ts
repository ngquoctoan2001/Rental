import api, { API_BASE_URL } from './client';

export const aiApi = {
  getConversations: () => api.get('/ai/conversations'),
  deleteConversation: (id: string) => api.delete(`/ai/conversations/${id}`),
  getChatUrl: () => `${API_BASE_URL}/ai/chat`,
};
