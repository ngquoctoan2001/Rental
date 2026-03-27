'use client';

import { useState, useEffect, useRef } from 'react';
import {
  Send, Bot, Trash2, Plus, Sparkles,
  Loader2, MessageSquare,
  ChevronRight, Lightbulb
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import ReactMarkdown from 'react-markdown';
import { useAiChat } from '@/hooks/use-ai-chat';
import { aiApi } from '@/lib/api';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

type Message = {
  id: string;
  role: 'user' | 'assistant';
  content: string;
};

const SUGGESTED_PROMPTS = [
  "Kiểm tra doanh thu tháng này của nhà trọ A",
  "Ai là khách thuê nợ tiền phòng lâu nhất?",
  "Tạo hóa đơn cho phòng 302 ngay",
  "Gửi nhắc nợ cho tất cả khách thuê quá hạn"
];

export default function AIAssistantPage() {
  const { messages, sendMessage, isLoading: isStreaming, clearMessages } = useAiChat();
  const [input, setInput] = useState('');
  const [conversationId, setConversationId] = useState<string | undefined>(undefined);
  const queryClient = useQueryClient();
  const scrollRef = useRef<HTMLDivElement>(null);

  const { data: conversations = [] } = useQuery({
    queryKey: ['ai-conversations'],
    queryFn: async () => {
      const resp = await aiApi.getConversations();
      return Array.isArray(resp.data) ? resp.data : [];
    },
  });

  const deleteConvMutation = useMutation({
    mutationFn: (id: string) => aiApi.deleteConversation(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ai-conversations'] }),
  });

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, isStreaming]);

  const handleSendMessage = async (content: string) => {
    if (!content.trim() || isStreaming) return;
    setInput('');
    await sendMessage(content, conversationId);
  };

  return (
    <div className="flex h-[calc(100vh-120px)] gap-6 bg-slate-50/50 -m-8 p-8 overflow-hidden">
      {/* Sidebar: Conversation List */}
      <aside className="w-[280px] flex flex-col bg-white rounded-[2rem] border border-slate-100 shadow-sm overflow-hidden">
        <div className="p-6 border-b border-slate-50">
          <button
            onClick={() => { clearMessages(); setConversationId(undefined); }}
            className="w-full flex items-center justify-center gap-2 py-3 bg-indigo-600 text-white rounded-xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100">
            <Plus className="w-4 h-4" />
            Hội thoại mới
          </button>
        </div>
        <div className="flex-1 overflow-y-auto p-4 space-y-2 custom-scrollbar">
          <p className="px-3 text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-2">Gần đây</p>
          {conversations.map(conv => (
            <button
              key={conv.id}
              onClick={() => { clearMessages(); setConversationId(conv.id); }}
              className={`w-full group flex items-center gap-3 p-3 rounded-xl hover:bg-slate-50 transition-all text-left ${conversationId === conv.id ? 'bg-indigo-50' : ''}`}
            >
              <div className="w-10 h-10 rounded-lg bg-indigo-50 flex items-center justify-center text-indigo-600 group-hover:bg-indigo-100 transition-colors">
                <MessageSquare className="w-5 h-5" />
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-bold text-slate-700 truncate">{conv.title}</p>
                <p className="text-[10px] text-slate-400">{conv.date}</p>
              </div>
              <button
                onClick={(e) => { e.stopPropagation(); deleteConvMutation.mutate(conv.id); }}
                className="opacity-0 group-hover:opacity-100 p-1 rounded text-slate-400 hover:text-rose-500 transition-all"
              >
                <Trash2 className="w-3.5 h-3.5" />
              </button>
            </button>
          ))}
        </div>
        <div className="p-4 bg-slate-50 border-t border-slate-100">
          <button className="w-full flex items-center gap-3 p-3 rounded-xl text-slate-500 hover:text-rose-600 hover:bg-rose-50 transition-all">
            <Trash2 className="w-4 h-4" />
            <span className="text-xs font-bold">Xóa lịch sử</span>
          </button>
        </div>
      </aside>

      {/* Main Chat Area */}
      <main className="flex-1 flex flex-col bg-white rounded-[2rem] border border-slate-100 shadow-sm relative overflow-hidden">
        {/* Chat Header */}
        <header className="p-6 border-b border-slate-50 flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center text-white shadow-lg shadow-indigo-100">
              <Bot className="w-7 h-7" />
            </div>
            <div>
              <h2 className="text-lg font-black text-slate-900 leading-tight">RentalOS AI Assistant</h2>
              <div className="flex items-center gap-1.5 mt-0.5">
                <div className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />
                <span className="text-[10px] font-bold text-slate-400 uppercase tracking-tight">Trực tuyến</span>
              </div>
            </div>
          </div>
          <button className="p-2.5 rounded-xl border border-slate-100 text-slate-400 hover:bg-slate-50 transition-colors">
            <Sparkles className="w-5 h-5" />
          </button>
        </header>

        {/* Messages */}
        <div ref={scrollRef} className="flex-1 overflow-y-auto p-8 space-y-8 custom-scrollbar">
          {messages.length === 0 && (
            <div className="h-full flex flex-col items-center justify-center space-y-8">
              <div className="text-center space-y-3">
                <div className="w-20 h-20 bg-indigo-50 rounded-[2rem] flex items-center justify-center mx-auto text-indigo-500 mb-4">
                  <Lightbulb className="w-10 h-10" />
                </div>
                <h3 className="text-2xl font-black text-slate-900">Chào bạn! Tôi có thể giúp gì cho bạn?</h3>
                <p className="text-slate-500 max-w-sm mx-auto">Bạn có thể yêu cầu tôi báo cáo doanh thu, quản lý khách thuê hoặc tự động hóa các tác vụ.</p>
              </div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-3 max-w-2xl px-4">
                {SUGGESTED_PROMPTS.map((prompt, idx) => (
                  <button 
                    key={idx}
                    onClick={() => handleSendMessage(prompt)}
                    className="flex items-center gap-3 p-4 bg-white border border-slate-100 rounded-2xl text-left text-sm font-bold text-slate-600 hover:border-indigo-200 hover:bg-indigo-50/30 hover:text-indigo-600 transition-all group"
                  >
                    <ChevronRight className="w-4 h-4 text-slate-300 group-hover:text-indigo-400" />
                    {prompt}
                  </button>
                ))}
              </div>
            </div>
          )}

          <AnimatePresence>
            {messages.map((msg, idx) => (
              <motion.div
                key={idx}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                className={`flex gap-4 ${msg.role === 'assistant' ? 'justify-start' : 'justify-end'}`}
              >
                {msg.role === 'assistant' && (
                  <div className="w-10 h-10 rounded-xl bg-indigo-600 flex-shrink-0 flex items-center justify-center text-white">
                    <Bot className="w-6 h-6" />
                  </div>
                )}

                <div className={`max-w-[80%] ${msg.role === 'user' ? 'order-first' : ''}`}>
                  <div className={`p-4 rounded-[1.5rem] shadow-sm ${
                    msg.role === 'assistant'
                    ? 'bg-slate-50 border border-slate-100 text-slate-800'
                    : 'bg-indigo-600 text-white font-medium'
                  }`}>
                    <div className={`prose prose-sm max-w-none ${msg.role === 'user' ? 'prose-invert' : 'text-slate-700 font-medium'}`}>
                      <ReactMarkdown>{msg.content || '...'}</ReactMarkdown>
                    </div>
                  </div>
                </div>

                {msg.role === 'user' && (
                  <div className="w-10 h-10 rounded-xl bg-slate-900 flex-shrink-0 flex items-center justify-center text-white">
                    <span className="font-bold text-sm">U</span>
                  </div>
                )}
              </motion.div>
            ))}

            {isStreaming && (
              <motion.div initial={{ opacity: 0 }} animate={{ opacity: 1 }} className="flex gap-4">
                <div className="w-10 h-10 rounded-xl bg-indigo-600 flex-shrink-0 flex items-center justify-center text-white">
                  <Bot className="w-6 h-6" />
                </div>
                <div className="bg-slate-50 border border-slate-100 p-4 rounded-2xl flex items-center gap-1.5">
                  <div className="w-1.5 h-1.5 bg-slate-400 rounded-full animate-bounce [animation-delay:-0.3s]" />
                  <div className="w-1.5 h-1.5 bg-slate-400 rounded-full animate-bounce [animation-delay:-0.15s]" />
                  <div className="w-1.5 h-1.5 bg-slate-400 rounded-full animate-bounce" />
                </div>
              </motion.div>
            )}
          </AnimatePresence>
        </div>

        {/* Input Area */}
        <footer className="p-6 border-t border-slate-50 bg-white">
          <form 
            onSubmit={(e) => { e.preventDefault(); handleSendMessage(input); }}
            className="relative"
          >
            <input 
              type="text" 
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Nhập yêu cầu quản lý nhà trọ..."
              className="w-full pl-6 pr-16 py-5 bg-slate-50 border border-slate-100 rounded-[2rem] text-sm font-medium focus:ring-4 focus:ring-indigo-500/10 focus:bg-white transition-all outline-none text-slate-900"
            />
            <button 
              disabled={isStreaming || !input.trim()}
              className="absolute right-3 top-1/2 -translate-y-1/2 w-12 h-12 bg-indigo-600 text-white rounded-full flex items-center justify-center hover:bg-indigo-700 transition-all disabled:opacity-50 disabled:bg-slate-300 shadow-lg shadow-indigo-100"
            >
              {isStreaming ? <Loader2 className="w-5 h-5 animate-spin" /> : <Send className="w-5 h-5" />}
            </button>
          </form>
          <p className="text-center text-[10px] text-slate-400 mt-3 font-bold uppercase tracking-widest opacity-60">AI có thể đưa ra câu trả lời không chính xác. Hãy kiểm tra lại thông tin.</p>
        </footer>
      </main>
    </div>
  );
}
