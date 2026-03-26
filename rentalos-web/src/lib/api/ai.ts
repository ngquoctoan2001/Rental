import api from './client';

export const aiApi = {
  getConversations: () => api.get('/ai/conversations'),
  deleteConversation: (id: string) => api.delete(`/ai/conversations/${id}`),
  // URL cho SSE (Server-Sent Events) chat
  getChatUrl: (propertyId?: string) => 
    `${api.defaults.baseURL}/ai/chat${propertyId ? `?propertyId=${propertyId}` : ''}`,
};
