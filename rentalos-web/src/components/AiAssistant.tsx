'use client';

import { useState, useRef, useEffect } from 'react';
import { 
  Sparkles, X, Send, 
  MessageSquare, User, Bot,
  Loader2, Maximize2, Minimize2
} from 'lucide-react';
import { useAiChat, Message } from '@/hooks/use-ai-chat';

export function AiAssistant() {
  const [isOpen, setIsOpen] = useState(false);
  const [input, setInput] = useState('');
  const { messages, sendMessage, isLoading } = useAiChat();
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim() || isLoading) return;
    sendMessage(input);
    setInput('');
  };

  return (
    <>
      {/* Floating Toggle Button */}
      <button 
        onClick={() => setIsOpen(true)}
        className={`fixed bottom-8 right-8 w-16 h-16 bg-slate-900 text-white rounded-[1.5rem] shadow-2xl flex items-center justify-center hover:scale-110 transition-all z-40 group ${isOpen ? 'opacity-0 scale-0 pointer-events-none' : 'opacity-100 scale-100'}`}
      >
        <Sparkles className="w-8 h-8 text-indigo-400 group-hover:rotate-12 transition-transform" />
        <div className="absolute -top-1 -right-1 w-4 h-4 bg-indigo-500 rounded-full border-2 border-white" />
      </button>

      {/* Chat Drawer */}
      <div className={`fixed bottom-8 right-8 w-[450px] max-w-[calc(100vw-4rem)] bg-white rounded-[2.5rem] shadow-[0_32px_128px_-16px_rgba(0,0,0,0.15)] border border-slate-100 flex flex-col z-50 transition-all duration-500 origin-bottom-right ${isOpen ? 'scale-100 opacity-100' : 'scale-0 opacity-0 pointer-events-none'}`}>
        {/* Header */}
        <div className="p-6 border-b border-slate-50 flex items-center justify-between bg-slate-900 rounded-t-[2.5rem]">
          <div className="flex items-center gap-3">
             <div className="w-10 h-10 rounded-xl bg-indigo-500 flex items-center justify-center">
               <Sparkles className="w-6 h-6 text-white" />
             </div>
             <div>
               <h3 className="text-white font-black text-base">Trợ lý RentalOS</h3>
               <p className="text-indigo-400 text-[10px] font-black uppercase tracking-widest">Sẵn sàng hỗ trợ bạn</p>
             </div>
          </div>
          <button onClick={() => setIsOpen(false)} className="p-2 hover:bg-white/10 rounded-xl transition-colors text-white/50 hover:text-white">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Messages Area */}
        <div 
          ref={scrollRef}
          className="flex-1 overflow-y-auto p-6 space-y-6 min-h-[400px] max-h-[500px] bg-slate-50/30"
        >
          {messages.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-center">
              <div className="w-20 h-20 rounded-[2rem] bg-indigo-50 flex items-center justify-center mb-4">
                <Bot className="w-10 h-10 text-indigo-400" />
              </div>
              <h4 className="text-slate-900 font-black text-lg">Chào bạn! Tôi có thể giúp gì?</h4>
              <p className="text-slate-500 text-sm max-w-[280px] mt-2 font-medium">Bạn có thể hỏi về doanh thu, tình trạng phòng, hoặc nhờ tôi tạo hóa đơn giúp.</p>
              
              <div className="grid grid-cols-1 gap-2 mt-8 w-full max-w-[320px]">
                {['Doanh thu tháng này thế nào?', 'Có phòng nào sắp trống không?', 'Tạo hóa đơn cho phòng 101'].map((suggestion) => (
                  <button 
                    key={suggestion}
                    onClick={() => sendMessage(suggestion)}
                    className="px-4 py-3 bg-white border border-slate-100 rounded-2xl text-xs font-bold text-slate-600 hover:border-indigo-200 hover:bg-indigo-50 transition-all text-left shadow-sm"
                  >
                    {suggestion}
                  </button>
                ))}
              </div>
            </div>
          ) : (
            messages.map((m, idx) => (
              <div key={idx} className={`flex ${m.role === 'user' ? 'justify-end' : 'justify-start shadow-sm'}`}>
                <div className={`max-w-[85%] rounded-[2rem] p-4 text-sm font-bold leading-relaxed ${
                  m.role === 'user' 
                    ? 'bg-slate-900 text-white rounded-br-none' 
                    : 'bg-white border border-slate-100 text-slate-700 rounded-bl-none'
                }`}>
                  {m.content || (
                    <div className="flex gap-1 py-1">
                      <div className="w-1.5 h-1.5 bg-slate-300 rounded-full animate-bounce [animation-delay:-0.3s]" />
                      <div className="w-1.5 h-1.5 bg-slate-300 rounded-full animate-bounce [animation-delay:-0.15s]" />
                      <div className="w-1.5 h-1.5 bg-slate-300 rounded-full animate-bounce" />
                    </div>
                  )}
                </div>
              </div>
            ))
          )}
        </div>

        {/* Input Area */}
        <div className="p-6 bg-white border-t border-slate-50 rounded-b-[2.5rem]">
          <form onSubmit={handleSubmit} className="relative">
            <input 
              type="text"
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Nhập câu hỏi tại đây..."
              className="w-full pl-6 pr-14 py-4 bg-slate-50 border-none rounded-2xl font-bold text-slate-900 focus:ring-2 focus:ring-indigo-500/20 outline-none"
            />
            <button 
              type="submit"
              disabled={isLoading || !input.trim()}
              className="absolute right-2 top-2 w-12 h-12 bg-indigo-600 text-white rounded-xl flex items-center justify-center hover:bg-indigo-700 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-lg shadow-indigo-100"
            >
              {isLoading ? <Loader2 className="w-5 h-5 animate-spin" /> : <Send className="w-5 h-5" />}
            </button>
          </form>
        </div>
      </div>
    </>
  );
}
