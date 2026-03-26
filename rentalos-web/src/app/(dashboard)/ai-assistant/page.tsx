'use client';

import { useState, useEffect, useRef } from 'react';
import { 
  Send, Bot, User, Trash2, Plus, Sparkles, 
  Terminal, Check, X, Loader2, MessageSquare,
  ChevronRight, Lightbulb
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import ReactMarkdown from 'react-markdown';

type Message = {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  tools?: {
    name: string;
    input: any;
    output?: any;
    status: 'pending' | 'completed' | 'error';
  }[];
  isConfirmationRequired?: boolean;
};

const SUGGESTED_PROMPTS = [
  "Kiểm tra doanh thu tháng này của nhà trọ A",
  "Ai là khách thuê nợ tiền phòng lâu nhất?",
  "Tạo hóa đơn cho phòng 302 ngay",
  "Gửi nhắc nợ cho tất cả khách thuê quá hạn"
];

export default function AIAssistantPage() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [input, setInput] = useState('');
  const [isStreaming, setIsStreaming] = useState(false);
  const [conversations, setConversations] = useState([
    { id: '1', title: 'Hỗ trợ thu phí tháng 3', date: 'Vừa xong' },
    { id: '2', title: 'Báo cáo doanh thu Q1', date: '2 giờ trước' }
  ]);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, isStreaming]);

  const handleSendMessage = async (content: string) => {
    if (!content.trim() || isStreaming) return;

    const userMsg: Message = {
      id: Date.now().toString(),
      role: 'user',
      content
    };

    setMessages(prev => [...prev, userMsg]);
    setInput('');
    setIsStreaming(true);

    // Simulation of SSE Streaming
    // In real app: const eventSource = new EventSource(`/api/ai/chat?q=${content}`);
    
    let currentAssistantMsg: Message = {
      id: (Date.now() + 1).toString(),
      role: 'assistant',
      content: ''
    };

    setMessages(prev => [...prev, currentAssistantMsg]);

    // Mock Streaming
    const mockFullText = "Tôi đã kiểm tra dữ liệu. Hiện tại có **3 phòng** chưa đóng tiền tháng 3. Tôi có nên tạo lệnh nhắc nợ cho họ không?";
    let charIndex = 0;
    
    const interval = setInterval(() => {
      if (charIndex < mockFullText.length) {
        currentAssistantMsg.content += mockFullText[charIndex];
        setMessages(prev => prev.map(m => m.id === currentAssistantMsg.id ? { ...currentAssistantMsg } : m));
        charIndex++;
      } else {
        clearInterval(interval);
        // Add a mock tool call after text
        setTimeout(() => {
          currentAssistantMsg.tools = [{
            name: 'get_unpaid_invoices',
            input: { month: '2026-03' },
            output: [{ room: '101', amount: 3500000 }, { room: '204', amount: 4200000 }],
            status: 'completed'
          }];
          currentAssistantMsg.isConfirmationRequired = true;
          setMessages(prev => prev.map(m => m.id === currentAssistantMsg.id ? { ...currentAssistantMsg } : m));
          setIsStreaming(false);
        }, 500);
      }
    }, 30);
  };

  return (
    <div className="flex h-[calc(100vh-120px)] gap-6 bg-slate-50/50 -m-8 p-8 overflow-hidden">
      {/* Sidebar: Conversation List */}
      <aside className="w-[280px] flex flex-col bg-white rounded-[2rem] border border-slate-100 shadow-sm overflow-hidden">
        <div className="p-6 border-b border-slate-50">
          <button className="w-full flex items-center justify-center gap-2 py-3 bg-indigo-600 text-white rounded-xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100">
            <Plus className="w-4 h-4" />
            Hội thoại mới
          </button>
        </div>
        <div className="flex-1 overflow-y-auto p-4 space-y-2 custom-scrollbar">
          <p className="px-3 text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-2">Gần đây</p>
          {conversations.map(conv => (
            <button key={conv.id} className="w-full group flex items-center gap-3 p-3 rounded-xl hover:bg-slate-50 transition-all text-left">
              <div className="w-10 h-10 rounded-lg bg-indigo-50 flex items-center justify-center text-indigo-600 group-hover:bg-indigo-100 transition-colors">
                <MessageSquare className="w-5 h-5" />
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-bold text-slate-700 truncate">{conv.title}</p>
                <p className="text-[10px] text-slate-400">{conv.date}</p>
              </div>
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
            {messages.map((msg) => (
              <motion.div 
                key={msg.id}
                initial={{ opacity: 0, y: 10 }}
                animate={{ opacity: 1, y: 0 }}
                className={`flex gap-4 ${msg.role === 'assistant' ? 'justify-start' : 'justify-end'}`}
              >
                {msg.role === 'assistant' && (
                  <div className="w-10 h-10 rounded-xl bg-indigo-600 flex-shrink-0 flex items-center justify-center text-white">
                    <Bot className="w-6 h-6" />
                  </div>
                )}
                
                <div className={`max-w-[80%] space-y-3 ${msg.role === 'user' ? 'order-first' : ''}`}>
                  <div className={`p-4 rounded-[1.5rem] shadow-sm ${
                    msg.role === 'assistant' 
                    ? 'bg-slate-50 border border-slate-100 text-slate-800' 
                    : 'bg-indigo-600 text-white font-medium'
                  }`}>
                    <div className={`prose prose-sm max-w-none ${msg.role === 'user' ? 'prose-invert' : 'text-slate-700 font-medium'}`}>
                      <ReactMarkdown>
                        {msg.content}
                      </ReactMarkdown>
                    </div>
                  </div>

                  {/* Tool Call Rendering */}
                  {msg.tools?.map((tool, idx) => (
                    <div key={idx} className="bg-purple-50 border border-purple-100 rounded-2xl overflow-hidden">
                      <div className="px-4 py-2 bg-purple-600 flex items-center justify-between">
                         <div className="flex items-center gap-2">
                            <Terminal className="w-3.5 h-3.5 text-purple-200" />
                            <span className="text-[10px] font-black text-white uppercase tracking-widest">{tool.name}</span>
                         </div>
                         {tool.status === 'completed' && <Check className="w-3 h-3 text-purple-200" />}
                      </div>
                      <div className="p-4 font-mono text-[11px] space-y-2">
                        <div>
                          <p className="text-purple-400 font-bold mb-1 uppercase tracking-tight">Input:</p>
                          <pre className="bg-white/50 p-2 rounded-lg text-purple-900 border border-purple-100">{JSON.stringify(tool.input, null, 2)}</pre>
                        </div>
                        {tool.output && (
                          <div>
                            <p className="text-purple-400 font-bold mb-1 uppercase tracking-tight">Output:</p>
                            <pre className="bg-white/50 p-2 rounded-lg text-emerald-900 border border-purple-100">{JSON.stringify(tool.output, null, 2)}</pre>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}

                  {/* Confirmation Box */}
                  {msg.isConfirmationRequired && (
                    <div className="bg-amber-50 border border-amber-100 p-4 rounded-2xl flex items-center justify-between gap-4">
                      <div className="flex items-center gap-3">
                        <div className="p-2 bg-amber-200 rounded-lg text-amber-700">
                          <Check className="w-5 h-5" />
                        </div>
                        <p className="text-xs font-bold text-amber-900">Xác nhận thực hiện hành động?</p>
                      </div>
                      <div className="flex gap-2">
                        <button className="px-4 py-2 bg-white border border-amber-200 text-amber-700 rounded-lg text-xs font-bold hover:bg-amber-100 transition-colors">Hủy</button>
                        <button className="px-4 py-2 bg-amber-600 text-white rounded-lg text-xs font-bold hover:bg-amber-700 transition-colors">Xác nhận</button>
                      </div>
                    </div>
                  )}
                </div>

                {msg.role === 'user' && (
                  <div className="w-10 h-10 rounded-xl bg-slate-900 flex-shrink-0 flex items-center justify-center text-white">
                    <User className="w-6 h-6" />
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
