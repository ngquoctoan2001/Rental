'use client';

import { 
  Check, X, Zap, Crown, Star, Shield, 
  Users, Building2, LayoutGrid, Sparkles,
  ArrowRight, Heart, Loader2
} from 'lucide-react';
import { motion } from 'framer-motion';
import { useQuery, useMutation } from '@tanstack/react-query';
import { subscriptionApi } from '@/lib/api/subscriptions';

const PLANS = [
  {
    id: 'free',
    name: 'Gói Cơ bản (Free)',
    price: '0đ',
    desc: 'Dành cho chủ trọ mới bắt đầu.',
    icon: Star,
    color: 'slate',
    features: [
      { text: 'Quản lý 1 nhà trọ', status: true },
      { text: 'Tối đa 15 phòng', status: true },
      { text: 'Tự động tính hóa đơn', status: true },
      { text: 'AI Assistant cơ bản', status: true },
      { text: 'In hóa đơn PDF', status: true },
      { text: 'Thanh toán VietQR', status: true },
      { text: 'Nhắc nợ tự động (MoMo/Zalo)', status: false },
      { text: 'Quản lý nhân viên', status: false },
      { text: 'Báo cáo doanh thu chuyên sâu', status: false },
    ],
    button: 'Gói hiện tại',
    current: true
  },
  {
    id: 'pro',
    name: 'Gói Chuyên nghiệp (Pro)',
    price: '199.000đ',
    period: '/tháng',
    desc: 'Dành cho chủ trọ quy mô vừa và lớn.',
    icon: Crown,
    color: 'indigo',
    features: [
      { text: 'Không giới hạn nhà trọ', status: true },
      { text: 'Không giới hạn số phòng', status: true },
      { text: 'Tự động tính hóa đơn', status: true },
      { text: 'AI Assistant Pro (Vắt kiệt dữ liệu)', status: true },
      { text: 'In hóa đơn PDF custom logo', status: true },
      { text: 'Full cổng: MoMo, VNPay, Chuyển khoản', status: true },
      { text: 'Nhắc nợ tự động & Gạch nợ AI', status: true },
      { text: 'Quản lý & Giám sát nhân viên', status: true },
      { text: 'Báo cáo doanh thu chuyên sâu', status: true },
    ],
    button: 'Nâng cấp ngay',
    popular: true
  }
];

export default function SubscribePage() {
  const { data: subscription } = useQuery({
    queryKey: ['subscription'],
    queryFn: () => subscriptionApi.getCurrent().then((r: any) => r.data ?? r),
  });

  const upgradeMutation = useMutation({
    mutationFn: (planId: string) => subscriptionApi.upgrade(planId),
    onSuccess: (resp: any) => {
      const paymentUrl = resp?.data?.paymentUrl ?? resp?.paymentUrl;
      if (paymentUrl) window.location.href = paymentUrl;
    },
  });

  return (
    <div className="space-y-12 pb-20">
      <div className="text-center space-y-4 max-w-2xl mx-auto pt-8">
        <h1 className="text-5xl font-black text-slate-900 tracking-tighter">Nâng tầm quản lý với <span className="text-indigo-600">Pro</span></h1>
        <p className="text-lg font-medium text-slate-500 italic">Chọn gói dịch vụ phù hợp để tối ưu hóa 100% công sức vận hành nhà trọ của bạn.</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 max-w-5xl mx-auto">
        {PLANS.map(plan => {
          const Icon = plan.icon;
          const isPro = plan.id === 'pro';
          const isCurrent = plan.id === currentPlan;

          return (
            <motion.div
              key={plan.id}
              whileHover={{ y: -10 }}
              className={`bg-white rounded-[3rem] p-10 border-2 relative overflow-hidden flex flex-col ${
                isPro 
                ? 'border-indigo-600 shadow-2xl shadow-indigo-100 ring-8 ring-indigo-50' 
                : 'border-slate-100 shadow-sm'
              }`}
            >
              {plan.popular && (
                <div className="absolute top-8 right-[-35px] bg-indigo-600 text-white px-12 py-1.5 rotate-45 text-[10px] font-black uppercase tracking-[0.2em] shadow-lg">
                  Phổ biến
                </div>
              )}

              <div className="flex items-center gap-4 mb-8">
                 <div className={`w-14 h-14 rounded-2xl flex items-center justify-center ${isPro ? 'bg-indigo-600 text-white' : 'bg-slate-100 text-slate-400'}`}>
                    <Icon className="w-8 h-8" />
                 </div>
                 <div>
                    <h3 className="text-xl font-black text-slate-900">{plan.name}</h3>
                    <p className="text-xs font-bold text-slate-400 italic">{plan.desc}</p>
                 </div>
              </div>

              <div className="mb-10 flex items-baseline gap-1">
                 <span className="text-5xl font-black text-slate-900 tracking-tighter">{plan.price}</span>
                 {plan.period && <span className="text-sm font-bold text-slate-400 italic">{plan.period}</span>}
              </div>

              <div className="space-y-4 mb-12 flex-1">
                 {plan.features.map((feat, idx) => (
                   <div key={idx} className={`flex items-center gap-3 ${feat.status ? 'opacity-100' : 'opacity-30'}`}>
                      <div className={`w-5 h-5 rounded-full flex items-center justify-center flex-shrink-0 ${feat.status ? (isPro ? 'bg-indigo-100 text-indigo-600' : 'bg-slate-100 text-slate-600') : 'bg-slate-50 text-slate-300'}`}>
                        {feat.status ? <Check className="w-3 h-3" /> : <X className="w-3 h-3" />}
                      </div>
                      <span className={`text-sm font-bold ${feat.status ? 'text-slate-700' : 'text-slate-400 line-through'}`}>{feat.text}</span>
                   </div>
                 ))}
              </div>

              <button
                disabled={isCurrent || upgradeMutation.isPending}
                onClick={() => !isCurrent && upgradeMutation.mutate(plan.id)}
                className={`w-full py-5 rounded-[1.5rem] font-black text-lg transition-all active:scale-95 flex items-center justify-center gap-2 ${
                  isCurrent
                  ? 'bg-slate-100 text-slate-400 cursor-not-allowed italic' 
                  : 'bg-indigo-600 text-white shadow-xl shadow-indigo-200 hover:bg-indigo-700 disabled:opacity-70'
                }`}
              >
                {upgradeMutation.isPending && !isCurrent && <Loader2 className="w-5 h-5 animate-spin" />}
                {isCurrent ? 'Gói hiện tại' : plan.button}
              </button>
            </motion.div>
          );
        })}
      </div>

      {/* Trust Badges */}
      <div className="max-w-4xl mx-auto bg-slate-50 border border-slate-100 rounded-[2.5rem] p-10 grid grid-cols-1 md:grid-cols-3 gap-8 text-center">
         <div className="space-y-2">
            <Shield className="w-8 h-8 text-indigo-400 mx-auto" />
            <p className="text-sm font-black text-slate-800">Bảo mật tuyệt đối</p>
            <p className="text-[10px] font-bold text-slate-400 italic uppercase">Chứng chỉ SSL & Backup 24/7</p>
         </div>
         <div className="space-y-2">
            <Zap className="w-8 h-8 text-amber-400 mx-auto" />
            <p className="text-sm font-black text-slate-800">Dùng thử 14 ngày</p>
            <p className="text-[10px] font-bold text-slate-400 italic uppercase">Hoàn trả nếu không hài lòng</p>
         </div>
         <div className="space-y-2">
            <Heart className="w-8 h-8 text-rose-400 mx-auto" />
            <p className="text-sm font-black text-slate-800">Hỗ trợ 24/7</p>
            <p className="text-[10px] font-bold text-slate-400 italic uppercase">Zalo / Hotline cực nhanh</p>
         </div>
      </div>
    </div>
  );
}
