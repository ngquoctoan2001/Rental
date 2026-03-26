'use client';

import { useState } from 'react';
import { 
  ShieldCheck, CreditCard, Landmark, Wallet, 
  CheckCircle2, Clock, Phone, Mail, ChevronRight,
  Download, ExternalLink, Smartphone, Info
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';

const MOCK_INVOICE = {
  id: 'HD-2026-9912',
  amount: 5240000,
  propertyName: 'Nhà trọ Blue Moon',
  room: '101',
  tenantName: 'Nguyễn Văn A',
  period: 'Tháng 03/2026',
  dueDate: '30/03/2026',
  items: [
    { label: 'Tiền phòng', value: 4500000 },
    { label: 'Tiền điện (120kWh)', value: 420000 },
    { label: 'Tiền nước (12m3)', value: 180000 },
    { label: 'Dịch vụ rác & vệ sinh', value: 140000 },
  ]
};

const PAYMENT_METHODS = [
  { id: 'bank', name: 'Chuyển khoản / VietQR', icon: Landmark, color: 'text-indigo-600', bg: 'bg-indigo-50' },
  { id: 'momo', name: 'Ví MoMo', icon: Smartphone, color: 'text-pink-600', bg: 'bg-pink-50' },
  { id: 'vnpay', name: 'Cổng VNPay', icon: CreditCard, color: 'text-blue-600', bg: 'bg-blue-50' },
];

export default function PublicPaymentPage() {
  const [selectedMethod, setSelectedMethod] = useState('bank');
  const [isPaid, setIsPaid] = useState(false);

  if (isPaid) {
    return (
      <div className="min-h-screen bg-emerald-50 flex items-center justify-center p-6">
        <motion.div 
          initial={{ scale: 0.9, opacity: 0 }}
          animate={{ scale: 1, opacity: 1 }}
          className="bg-white rounded-[3rem] p-10 text-center shadow-xl space-y-6 max-w-sm"
        >
          <div className="w-20 h-20 bg-emerald-100 rounded-full flex items-center justify-center mx-auto">
             <CheckCircle2 className="w-10 h-10 text-emerald-600" />
          </div>
          <div className="space-y-2">
            <h1 className="text-2xl font-black text-slate-900">Thanh toán thành công!</h1>
            <p className="text-sm font-medium text-slate-500">Hệ thống đã ghi nhận thanh toán của bạn cho hóa đơn {MOCK_INVOICE.id}.</p>
          </div>
          <button className="w-full py-4 bg-slate-900 text-white rounded-2xl font-black text-sm">
            Tải biên lai (PDF)
          </button>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 font-sans pb-10">
      {/* Top Header */}
      <header className="bg-white border-b border-slate-100 p-4 sticky top-0 z-30">
        <div className="max-w-md mx-auto flex items-center justify-between">
           <div className="flex items-center gap-2">
              <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center text-white font-black text-xs">R</div>
              <span className="font-black text-slate-900 tracking-tighter">RentalOS Pay</span>
           </div>
           <div className="flex items-center gap-1.5 px-3 py-1.5 bg-indigo-50 rounded-full">
              <ShieldCheck className="w-3.5 h-3.5 text-indigo-600" />
              <span className="text-[10px] font-black text-indigo-600 uppercase tracking-widest">Bảo mật</span>
           </div>
        </div>
      </header>

      <main className="max-w-md mx-auto p-4 space-y-6">
        {/* Invoice Summary Card */}
        <section className="bg-white rounded-[2.5rem] shadow-sm border border-slate-100 p-8 space-y-6">
           <div className="text-center space-y-1">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">{MOCK_INVOICE.period}</p>
              <h1 className="text-4xl font-black text-slate-900 tracking-tight">{MOCK_INVOICE.amount.toLocaleString()}đ</h1>
              <div className="pt-2">
                 <span className="px-3 py-1 bg-amber-100 text-amber-700 rounded-full text-[10px] font-black uppercase tracking-widest flex items-center gap-1.5 justify-center w-fit mx-auto">
                    <Clock className="w-3 h-3" /> Hạn: {MOCK_INVOICE.dueDate}
                 </span>
              </div>
           </div>

           <div className="space-y-4 pt-6 border-t border-slate-50">
              <div className="flex justify-between items-center text-sm">
                 <span className="font-bold text-slate-400 italic">Cơ sở / Phòng</span>
                 <span className="font-black text-slate-800">{MOCK_INVOICE.propertyName} ({MOCK_INVOICE.room})</span>
              </div>
              <div className="flex justify-between items-center text-sm">
                 <span className="font-bold text-slate-400 italic">Khách thuê</span>
                 <span className="font-black text-slate-800">{MOCK_INVOICE.tenantName}</span>
              </div>
           </div>

           <details className="group cursor-pointer">
              <summary className="list-none flex items-center justify-center gap-2 py-2 text-[10px] font-black text-indigo-600 uppercase tracking-widest hover:bg-slate-50 rounded-xl transition-all">
                 <Info className="w-3.5 h-3.5" /> Chi tiết hóa đơn
              </summary>
              <div className="pt-4 space-y-3">
                 {MOCK_INVOICE.items.map((item, idx) => (
                   <div key={idx} className="flex justify-between text-xs font-bold italic text-slate-500">
                      <span>{item.label}</span>
                      <span>{item.value.toLocaleString()}đ</span>
                   </div>
                 ))}
              </div>
           </details>
        </section>

        {/* Payment Methods */}
        <section className="space-y-4">
           <h3 className="text-xs font-black text-slate-400 uppercase tracking-[0.2em] pl-2">Chọn phương thức thanh toán</h3>
           <div className="space-y-2">
              {PAYMENT_METHODS.map(method => (
                <label 
                  key={method.id} 
                  className={`flex items-center gap-4 p-5 rounded-3xl border cursor-pointer transition-all ${
                    selectedMethod === method.id 
                    ? 'bg-white border-indigo-600 shadow-lg shadow-indigo-100/50' 
                    : 'bg-white/60 border-slate-200 hover:bg-white'
                  }`}
                >
                  <input 
                    type="radio" 
                    name="payment" 
                    value={method.id} 
                    className="sr-only"
                    onChange={() => setSelectedMethod(method.id)} 
                  />
                  <div className={`w-12 h-12 rounded-2xl ${method.bg} flex items-center justify-center ${method.color}`}>
                     <method.icon className="w-6 h-6" />
                  </div>
                  <div className="flex-1">
                    <p className="font-black text-slate-800 text-sm">{method.name}</p>
                    <p className="text-[10px] font-bold text-slate-400 italic uppercase">Xử lý ngay lập tức</p>
                  </div>
                  <div className={`w-6 h-6 rounded-full border-2 flex items-center justify-center transition-all ${
                    selectedMethod === method.id ? 'border-indigo-600' : 'border-slate-200'
                  }`}>
                    {selectedMethod === method.id && <div className="w-3 h-3 bg-indigo-600 rounded-full" />}
                  </div>
                </label>
              ))}
           </div>
        </section>

        {/* Dynamic Payment Content */}
        <AnimatePresence mode="wait">
          {selectedMethod === 'bank' && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="bg-slate-900 rounded-[2.5rem] p-8 text-center space-y-6 text-white"
            >
               <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Quét mã VietQR để thanh toán</p>
               <div className="w-48 h-48 bg-white rounded-[2rem] mx-auto p-4 flex items-center justify-center relative group">
                  <div className="text-slate-200">
                    {/* Placeholder for QR actual gen */}
                    <CreditCard className="w-20 h-20 animate-pulse" />
                  </div>
                  <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity bg-white/80 rounded-[2rem]">
                     <button className="text-[10px] font-black text-slate-900 uppercase">Tải xuống mã QR</button>
                  </div>
               </div>
               <div className="space-y-1">
                  <p className="text-xl font-black italic">MB BANK</p>
                  <p className="text-2xl font-black tracking-widest">0987654321</p>
                  <p className="text-[10px] font-bold text-indigo-400 uppercase">NGUYEN VAN A</p>
               </div>
               <div className="p-4 bg-white/5 rounded-2xl border border-white/10 space-y-1">
                  <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Nội dung bắt buộc</p>
                  <p className="text-sm font-black text-indigo-400">PAY {MOCK_INVOICE.id}</p>
               </div>
            </motion.div>
          )}
        </AnimatePresence>

        {selectedMethod !== 'bank' && (
          <button 
            onClick={() => setIsPaid(true)}
            className="w-full py-5 bg-indigo-600 text-white rounded-[1.5rem] font-black text-lg hover:bg-indigo-700 shadow-xl shadow-indigo-100 transition-all active:scale-95"
          >
            Thanh toán qua {selectedMethod === 'momo' ? 'MoMo' : 'VNPay'}
          </button>
        )}

        <footer className="pt-8 text-center space-y-4 pb-12">
           <div className="flex flex-col items-center gap-2">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Hỗ trợ khách hàng</p>
              <div className="flex gap-4">
                 <button className="p-3 bg-white border border-slate-100 rounded-xl text-slate-500">
                    <Phone className="w-4 h-4" />
                 </button>
                 <button className="p-3 bg-white border border-slate-100 rounded-xl text-slate-500">
                    <Mail className="w-4 h-4" />
                 </button>
              </div>
           </div>
           <p className="text-[10px] font-bold text-slate-400 italic">RentalOS v3.0 - Hệ thống quản lý chỗ ở thông minh</p>
        </footer>
      </main>
    </div>
  );
}
