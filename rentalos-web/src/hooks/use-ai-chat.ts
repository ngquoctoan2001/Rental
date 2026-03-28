'use client';

import { useState, useCallback } from 'react';
import { useAuthStore } from '@/lib/stores/authStore';
import { API_BASE_URL } from '@/lib/api/client';

export type Message = {
  role: 'user' | 'assistant';
  content: string;
};

export function useAiChat() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const { accessToken } = useAuthStore();

  const sendMessage = useCallback(async (content: string, conversationId?: string) => {
    const userMessage: Message = { role: 'user', content };
    setMessages((prev) => [...prev, userMessage]);
    setIsLoading(true);

    try {
      const response = await fetch(`${API_BASE_URL}/ai/chat`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`,
        },
        body: JSON.stringify({ message: content, conversationId: conversationId ?? null }),
      });

      if (!response.ok) throw new Error('Failed to start AI chat');

      const reader = response.body?.getReader();
      if (!reader) throw new Error('No reader available');

      let assistantContent = '';
      setMessages((prev) => [...prev, { role: 'assistant', content: '' }]);

      const decoder = new TextDecoder();
      let buffer = '';

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split('\n');
        buffer = lines.pop() ?? '';

        for (const line of lines) {
          if (!line.startsWith('data: ')) continue;
          const jsonStr = line.slice(6).trim();
          if (!jsonStr) continue;
          try {
            const chunk = JSON.parse(jsonStr);
            if (chunk.type === 'Text' && chunk.content) {
              assistantContent += chunk.content;
              setMessages((prev) => {
                const updated = [...prev];
                const last = updated[updated.length - 1];
                if (last?.role === 'assistant') {
                  updated[updated.length - 1] = { ...last, content: assistantContent };
                }
                return updated;
              });
            } else if (chunk.type === 'Done') {
              break;
            }
          } catch {}
        }
      }
    } catch (error) {
      console.error('AI Chat Error:', error);
      setMessages((prev) => [...prev, { role: 'assistant', content: 'Đã có lỗi xảy ra khi kết nối với AI. Vui lòng thử lại.' }]);
    } finally {
      setIsLoading(false);
    }
  }, [accessToken]);

  const clearMessages = useCallback(() => setMessages([]), []);

  return { messages, sendMessage, isLoading, clearMessages };
}
