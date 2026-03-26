'use client';

import { useState, useCallback, useRef } from 'react';

export type Message = {
  role: 'user' | 'assistant';
  content: string;
};

export function useAiChat() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const eventSourceRef = useRef<EventSource | null>(null);

  const sendMessage = useCallback(async (content: string) => {
    const userMessage: Message = { role: 'user', content };
    setMessages((prev) => [...prev, userMessage]);
    setIsLoading(true);

    try {
      // In a real app, this would be a POST to create a session if needed,
      // then an SSE connection to stream. We'll simplify for now.
      const response = await fetch('/api/ai/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: content }),
      });

      if (!response.ok) throw new Error('Failed to start chat');

      const reader = response.body?.getReader();
      if (!reader) throw new Error('No reader available');

      let assistantMessage = '';
      setMessages((prev) => [...prev, { role: 'assistant', content: '' }]);

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const text = new TextDecoder().decode(value);
        // Basic parsing of SSE data format "data: content\n\n"
        const lines = text.split('\n');
        for (const line of lines) {
          if (line.startsWith('data: ')) {
            const data = line.slice(6);
            if (data === '[DONE]') break;
            assistantMessage += data;
            setMessages((prev) => {
              const last = prev[prev.length - 1];
              if (last.role === 'assistant') {
                return [...prev.slice(0, -1), { ...last, content: assistantMessage }];
              }
              return prev;
            });
          }
        }
      }
    } catch (error) {
      console.error('AI Chat Error:', error);
    } finally {
      setIsLoading(false);
    }
  }, []);

  return { messages, sendMessage, isLoading };
}
