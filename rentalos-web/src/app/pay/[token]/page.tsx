'use client';

import { useMemo, useState } from 'react';
import { 
  CheckCircle2,
  Clock,
  CreditCard,
  Info,
  Landmark,
  Loader2,
  ShieldCheck,
  Smartphone,
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useParams } from 'next/navigation';
import { useMutation, useQuery } from '@tanstack/react-query';
import { paymentApi, paymentUrls, publicApi, APP_BASE_URL } from '@/lib/api';
import { Invoice } from '@/types';

function formatCurrency(value?: number) {
  const numberValue = Number(value || 0);
  return `${numberValue.toLocaleString('vi-VN')}đ`;
}

function formatDate(value?: string) {
  if (!value) return '--';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleDateString('vi-VN');
}

function formatBillingMonth(value?: string) {
  if (!value) return '--';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return `Tháng ${String(date.getMonth() + 1).padStart(2, '0')}/${date.getFullYear()}`;
}

function buildInvoiceItems(invoice: Invoice) {
  const electricUnits = Math.max((invoice.electricityNew || 0) - (invoice.electricityOld || 0), 0);
  const waterUnits = Math.max((invoice.waterNew || 0) - (invoice.waterOld || 0), 0);

  return [
    { label: 'Tiền phòng', value: invoice.roomRent || 0 },
    { label: `Tiền điện (${electricUnits} kWh)`, value: invoice.electricityAmount || 0 },
    { label: `Tiền nước (${waterUnits} m3)`, value: invoice.waterAmount || 0 },
    { label: 'Phí dịch vụ', value: invoice.serviceFee || 0 },
    { label: 'Phí internet', value: invoice.internetFee || 0 },
    { label: 'Phí rác', value: invoice.garbageFee || 0 },
    { label: 'Phụ phí khác', value: invoice.otherFees || 0 },
    { label: 'Giảm giá', value: -(invoice.discount || 0) },
  ].filter((item) => item.value !== 0);
}

const PAYMENT_METHODS = [
  { id: 'bank', name: 'Chuyển khoản ngân hàng', icon: Landmark, color: 'text-indigo-600', bg: 'bg-indigo-50' },
  { id: 'momo', name: 'Ví MoMo', icon: Smartphone, color: 'text-pink-600', bg: 'bg-pink-50' },
  { id: 'vnpay', name: 'Cổng VNPay', icon: CreditCard, color: 'text-blue-600', bg: 'bg-blue-50' },
] as const;

type PaymentMethod = (typeof PAYMENT_METHODS)[number]['id'];

export default function PublicPaymentPage() {
  const params = useParams<{ token: string }>();
  const token = params?.token;
  const [selectedMethod, setSelectedMethod] = useState<PaymentMethod>('bank');
  const [actionError, setActionError] = useState<string | null>(null);

  const invoiceQuery = useQuery({
    queryKey: ['public-invoice', token],
    queryFn: async () => (await publicApi.getInvoiceByToken(token)).data as Invoice,
    enabled: Boolean(token),
    refetchOnWindowFocus: false,
  });

  const paymentMutation = useMutation({
    mutationFn: async (method: Exclude<PaymentMethod, 'bank'>) => {
      if (!invoiceQuery.data?.id) throw new Error('Không tìm thấy hóa đơn.');

      if (method === 'momo') {
        const response = await paymentApi.createMomo({
          invoiceId: invoiceQuery.data.id,
          returnUrl: `${APP_BASE_URL}/payment-status`,
          notifyUrl: paymentUrls.momoWebhook,
        });

        return response.data?.payUrl as string;
      }

      const response = await paymentApi.createVNPay({
        invoiceId: invoiceQuery.data.id,
        returnUrl: paymentUrls.vnpayReturn,
      });

      return response.data?.paymentUrl as string;
    },
    onSuccess: (url) => {
      if (!url) {
        setActionError('Không nhận được đường dẫn thanh toán từ hệ thống.');
        return;
      }
      window.location.href = url;
    },
    onError: (err: any) => {
      setActionError(err?.message || 'Không tạo được thanh toán. Vui lòng thử lại.');
    },
  });

  const invoiceItems = useMemo(() => {
    if (!invoiceQuery.data) return [];
    return buildInvoiceItems(invoiceQuery.data);
  }, [invoiceQuery.data]);

  if (invoiceQuery.isLoading) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-indigo-600" />
      </div>
    );
  }

  if (invoiceQuery.isError || !invoiceQuery.data) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-6">
        <div className="bg-white rounded-3xl p-8 max-w-md w-full text-center shadow-sm border border-slate-100 space-y-3">
          <h1 className="text-2xl font-black text-slate-900">Liên kết không hợp lệ</h1>
          <p className="text-sm font-medium text-slate-500">
            Mã thanh toán không hợp lệ hoặc đã hết hạn. Vui lòng liên hệ quản lý nhà trọ để nhận liên kết mới.
          </p>
        </div>
      </div>
    );
  }

  const invoice = invoiceQuery.data;
  const isPaid = String(invoice.status).toLowerCase() === 'paid';
  const isProcessing = paymentMutation.isPending;

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
            <h1 className="text-2xl font-black text-slate-900">Hóa đơn đã thanh toán</h1>
            <p className="text-sm font-medium text-slate-500">Hệ thống đã ghi nhận thanh toán cho hóa đơn {invoice.invoiceCode}.</p>
          </div>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 font-sans pb-10">
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
        <section className="bg-white rounded-[2.5rem] shadow-sm border border-slate-100 p-8 space-y-6">
           <div className="text-center space-y-1">
          <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">{formatBillingMonth(invoice.billingMonth)}</p>
          <h1 className="text-4xl font-black text-slate-900 tracking-tight">{formatCurrency(invoice.totalAmount)}</h1>
              <div className="pt-2">
                 <span className="px-3 py-1 bg-amber-100 text-amber-700 rounded-full text-[10px] font-black uppercase tracking-widest flex items-center gap-1.5 justify-center w-fit mx-auto">
              <Clock className="w-3 h-3" /> Hạn: {formatDate(invoice.dueDate)}
                 </span>
              </div>
           </div>

           <div className="space-y-4 pt-6 border-t border-slate-50">
              <div className="flex justify-between items-center text-sm">
                 <span className="font-bold text-slate-400 italic">Cơ sở / Phòng</span>
            <span className="font-black text-slate-800">{invoice.propertyName || '--'} ({invoice.roomNumber || '--'})</span>
              </div>
              <div className="flex justify-between items-center text-sm">
                 <span className="font-bold text-slate-400 italic">Khách thuê</span>
            <span className="font-black text-slate-800">{invoice.customerName || '--'}</span>
              </div>
          <div className="flex justify-between items-center text-sm">
            <span className="font-bold text-slate-400 italic">Mã hóa đơn</span>
            <span className="font-black text-slate-800">{invoice.invoiceCode || '--'}</span>
          </div>
           </div>

           <details className="group cursor-pointer">
              <summary className="list-none flex items-center justify-center gap-2 py-2 text-[10px] font-black text-indigo-600 uppercase tracking-widest hover:bg-slate-50 rounded-xl transition-all">
                 <Info className="w-3.5 h-3.5" /> Chi tiết hóa đơn
              </summary>
              <div className="pt-4 space-y-3">
                  {invoiceItems.map((item, idx) => (
                   <div key={idx} className="flex justify-between text-xs font-bold italic text-slate-500">
                      <span>{item.label}</span>
                     <span>{formatCurrency(item.value)}</span>
                   </div>
                 ))}
              </div>
           </details>
        </section>

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
                    onChange={() => setSelectedMethod(method.id as PaymentMethod)} 
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

        <AnimatePresence mode="wait">
          {selectedMethod === 'bank' && (
            <motion.div
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -10 }}
              className="bg-slate-900 rounded-[2.5rem] p-8 text-center space-y-6 text-white"
            >
               <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Chuyển khoản ngân hàng</p>
               <div className="w-48 h-48 bg-white rounded-[2rem] mx-auto p-4 flex items-center justify-center relative group">
                  <div className="text-slate-200">
                    <CreditCard className="w-20 h-20 animate-pulse" />
                  </div>
               </div>
               <div className="space-y-1">
                  <p className="text-xl font-black italic">Vui lòng liên hệ quản lý để nhận thông tin tài khoản</p>
               </div>
               <div className="p-4 bg-white/5 rounded-2xl border border-white/10 space-y-1">
                  <p className="text-[10px] font-black text-slate-500 uppercase tracking-widest">Nội dung bắt buộc</p>
                  <p className="text-sm font-black text-indigo-400">PAY {invoice.invoiceCode || invoice.id}</p>
               </div>
            </motion.div>
          )}
        </AnimatePresence>

        {selectedMethod !== 'bank' && (
          <button 
            onClick={() => {
              setActionError(null);
              paymentMutation.mutate(selectedMethod);
            }}
            disabled={isProcessing}
            className="w-full py-5 bg-indigo-600 text-white rounded-[1.5rem] font-black text-lg hover:bg-indigo-700 shadow-xl shadow-indigo-100 transition-all active:scale-95 disabled:opacity-70"
          >
            {isProcessing ? 'Đang chuyển đến cổng thanh toán...' : `Thanh toán qua ${selectedMethod === 'momo' ? 'MoMo' : 'VNPay'}`}
          </button>
        )}

        {actionError && (
          <div className="px-4 py-3 rounded-2xl bg-rose-50 text-rose-700 text-sm font-bold">
            {actionError}
          </div>
        )}

        <footer className="pt-8 text-center space-y-4 pb-12">
           <div className="flex flex-col items-center gap-2">
              <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Hỗ trợ khách hàng</p>
              <div className="flex gap-4">
                  <div className="p-3 bg-white border border-slate-100 rounded-xl text-slate-500 text-xs font-semibold">
                    Liên hệ quản lý từ thông báo hóa đơn
                  </div>
              </div>
           </div>
           <p className="text-[10px] font-bold text-slate-400 italic">RentalOS v3.0 - Hệ thống quản lý chỗ ở thông minh</p>
        </footer>
      </main>
    </div>
  );
}
