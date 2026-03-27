'use client';

import { useState } from 'react';
import { 
  Building2, Home, Sparkles, ArrowRight, Check, 
  MapPin, User, LayoutGrid, Rocket, Info, Loader2
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useRouter } from 'next/navigation';
import { useMutation } from '@tanstack/react-query';
import { onboardingApi } from '@/lib/api';

export default function OnboardingPage() {
  const [step, setStep] = useState(1);
  const router = useRouter();

  const completeMutation = useMutation({
    mutationFn: () => onboardingApi.complete(),
    onSuccess: () => router.push('/'),
  });

  const nextStep = () => setStep(s => s + 1);
  const finish = () => completeMutation.mutate();

  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-6 relative overflow-hidden">
      {/* Background Orbs */}
      <div className="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-indigo-500/10 blur-[120px] rounded-full" />
      <div className="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-purple-500/10 blur-[120px] rounded-full" />

      <div className="w-full max-w-xl relative z-10">
        <AnimatePresence mode="wait">
          {step === 1 && (
            <motion.div
              key="step1"
              initial={{ scale: 0.9, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 1.1, opacity: 0 }}
              className="bg-white rounded-[3rem] p-12 text-center space-y-8"
            >
               <div className="w-24 h-24 bg-indigo-50 rounded-[2rem] flex items-center justify-center mx-auto text-indigo-600">
                  <Sparkles className="w-12 h-12" />
               </div>
               <div className="space-y-3">
                  <h1 className="text-4xl font-black text-slate-900 tracking-tight">Chào mừng chủ trọ mới!</h1>
                  <p className="text-lg font-medium text-slate-500 italic">RentalOS sẽ giúp bạn tự động hóa việc quản lý nhà trọ chỉ trong vài giây.</p>
               </div>
               <button 
                onClick={nextStep}
                className="w-full py-5 bg-indigo-600 text-white rounded-2xl font-black text-lg flex items-center justify-center gap-3 hover:bg-indigo-700 transition-all active:scale-95 shadow-xl shadow-indigo-100"
               >
                 Bắt đầu ngay <ArrowRight className="w-6 h-6" />
               </button>
            </motion.div>
          )}

          {step === 2 && (
            <motion.div
              key="step2"
              initial={{ x: 50, opacity: 0 }}
              animate={{ x: 0, opacity: 1 }}
              exit={{ x: -50, opacity: 0 }}
              className="bg-white rounded-[3rem] p-12 space-y-8"
            >
               <div className="flex items-center gap-2">
                 <div className="w-2 h-2 rounded-full bg-slate-200" />
                 <div className="w-8 h-2 rounded-full bg-indigo-600" />
                 <div className="w-2 h-2 rounded-full bg-slate-200" />
               </div>
               <div className="space-y-2">
                  <h2 className="text-2xl font-black text-slate-900">Thiết lập cơ sở đầu tiên</h2>
                  <p className="text-sm font-bold text-slate-400 italic">Nhập thông tin tòa nhà/nhà trọ bạn đang quản lý.</p>
               </div>
               <div className="space-y-4">
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tên cơ sở</label>
                    <div className="relative">
                       <Building2 className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                       <input type="text" placeholder="vd: Blue Moon Apartment" className="w-full pl-12 pr-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Địa chỉ</label>
                    <div className="relative">
                       <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                       <input type="text" placeholder="Số nhà, Tên đường, Quận..." className="w-full pl-12 pr-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                    </div>
                  </div>
               </div>
               <button 
                onClick={nextStep}
                className="w-full py-5 bg-indigo-600 text-white rounded-2xl font-black text-lg flex items-center justify-center gap-3"
               >
                 Tiếp tục <ArrowRight className="w-6 h-6" />
               </button>
            </motion.div>
          )}

          {step === 3 && (
            <motion.div
              key="step3"
              initial={{ x: 50, opacity: 0 }}
              animate={{ x: 0, opacity: 1 }}
              exit={{ scale: 0.9, opacity: 0 }}
              className="bg-white rounded-[3rem] p-12 space-y-8"
            >
               <div className="flex items-center gap-2">
                 <div className="w-2 h-2 rounded-full bg-slate-200" />
                 <div className="w-2 h-2 rounded-full bg-slate-200" />
                 <div className="w-8 h-2 rounded-full bg-indigo-600" />
               </div>
               <div className="space-y-2">
                  <h2 className="text-2xl font-black text-slate-900">Thêm phòng mẫu</h2>
                  <p className="text-sm font-bold text-slate-400 italic">Bạn có thể thêm hàng loạt phòng sau trong trang quản lý.</p>
               </div>
               <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2 col-span-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tên phòng</label>
                    <input type="text" placeholder="vd: 101" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Giá thuê / tháng</label>
                    <input type="number" placeholder="4500000" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tiền cọc</label>
                    <input type="number" placeholder="4500000" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
               </div>
               <button 
                onClick={nextStep}
                className="w-full py-5 bg-slate-900 text-white rounded-2xl font-black text-lg flex items-center justify-center gap-3"
               >
                 Hoàn tất thiết lập <Rocket className="w-6 h-6 text-indigo-400" />
               </button>
            </motion.div>
          )}

          {step === 4 && (
            <motion.div
              key="step4"
              initial={{ scale: 0.5, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              className="bg-white rounded-[3rem] p-12 text-center space-y-8"
            >
               <div className="w-24 h-24 bg-emerald-50 rounded-full flex items-center justify-center mx-auto text-emerald-600">
                  <motion.div
                    initial={{ scale: 0 }}
                    animate={{ scale: 1 }}
                    transition={{ delay: 0.2, type: 'spring' }}
                  >
                    <Check className="w-12 h-12" />
                  </motion.div>
               </div>
               <div className="space-y-3">
                  <h1 className="text-4xl font-black text-slate-900 tracking-tight">Tuyệt vời!</h1>
                  <p className="text-lg font-medium text-slate-500 italic">Mọi thứ đã sẵn sàng. Chào mừng bạn gia nhập cộng đồng RentalOS.</p>
               </div>
               <button 
                onClick={finish}
                disabled={completeMutation.isPending}
                className="w-full py-5 bg-indigo-600 text-white rounded-2xl font-black text-lg shadow-xl shadow-indigo-100 flex items-center justify-center gap-3 disabled:opacity-70"
               >
                {completeMutation.isPending && <Loader2 className="w-5 h-5 animate-spin" />}
                 Vào Dashboard ngay
               </button>
               <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest animate-pulse">Nhấn để vào ngay...</p>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}
