'use client';

import { useMemo, useState } from 'react';
import { 
  ArrowRight,
  Building2,
  Check,
  Home,
  Loader2,
  MapPin,
  Rocket,
  Sparkles,
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useRouter } from 'next/navigation';
import { useMutation, useQuery } from '@tanstack/react-query';
import { onboardingApi, propertiesApi, roomsApi, settingsApi } from '@/lib/api';

type OnboardingFormData = {
  companyName: string;
  companyAddress: string;
  companyPhone: string;
  companyEmail: string;
  taxCode: string;
  propertyName: string;
  propertyAddress: string;
  province: string;
  district: string;
  ward: string;
  propertyDescription: string;
  totalFloors: number;
  roomNumber: string;
  floor: number;
  areaSqm: number;
  basePrice: number;
  electricityPrice: number;
  waterPrice: number;
  serviceFee: number;
  internetFee: number;
  garbageFee: number;
  amenitiesText: string;
  notes: string;
};

const INITIAL_DATA: OnboardingFormData = {
  companyName: '',
  companyAddress: '',
  companyPhone: '',
  companyEmail: '',
  taxCode: '',
  propertyName: '',
  propertyAddress: '',
  province: '',
  district: '',
  ward: '',
  propertyDescription: '',
  totalFloors: 1,
  roomNumber: '101',
  floor: 1,
  areaSqm: 20,
  basePrice: 3500000,
  electricityPrice: 3500,
  waterPrice: 15000,
  serviceFee: 0,
  internetFee: 0,
  garbageFee: 0,
  amenitiesText: '',
  notes: '',
};

function splitAmenities(value: string) {
  return value
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean);
}

export default function OnboardingPage() {
  const [step, setStep] = useState(1);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState<OnboardingFormData>(INITIAL_DATA);
  const router = useRouter();

  const statusQuery = useQuery({
    queryKey: ['onboarding-status'],
    queryFn: async () => (await onboardingApi.getStatus()).data,
    refetchOnWindowFocus: false,
  });

  const canContinueStep2 = useMemo(() => {
    return (
      formData.companyName.trim().length >= 2 &&
      formData.companyAddress.trim().length >= 2 &&
      formData.companyPhone.trim().length >= 9 &&
      /.+@.+\..+/.test(formData.companyEmail.trim())
    );
  }, [formData.companyName, formData.companyAddress, formData.companyPhone, formData.companyEmail]);

  const canContinueStep3 = useMemo(() => {
    return formData.propertyName.trim().length >= 2 && formData.propertyAddress.trim().length >= 2;
  }, [formData.propertyName, formData.propertyAddress]);

  const canFinish = useMemo(() => {
    return formData.roomNumber.trim().length > 0 && formData.basePrice >= 100000;
  }, [formData.roomNumber, formData.basePrice]);

  const setField = <K extends keyof OnboardingFormData>(key: K, value: OnboardingFormData[K]) => {
    setFormData((prev) => ({ ...prev, [key]: value }));
  };

  const completeMutation = useMutation({
    mutationFn: async () => {
      await settingsApi.updateCompany({
        companyName: formData.companyName,
        address: formData.companyAddress,
        phone: formData.companyPhone,
        email: formData.companyEmail,
        taxCode: formData.taxCode || null,
        logoUrl: null,
      });

      const createPropertyResponse = await propertiesApi.create({
        name: formData.propertyName,
        address: formData.propertyAddress,
        province: formData.province || null,
        district: formData.district || null,
        ward: formData.ward || null,
        description: formData.propertyDescription || null,
        totalFloors: Number(formData.totalFloors || 1),
      });

      const propertyId = createPropertyResponse.data;

      await roomsApi.create({
        propertyId,
        roomNumber: formData.roomNumber,
        floor: Number(formData.floor || 1),
        areaSqm: Number(formData.areaSqm || 0),
        basePrice: Number(formData.basePrice),
        electricityPrice: Number(formData.electricityPrice),
        waterPrice: Number(formData.waterPrice),
        serviceFee: Number(formData.serviceFee || 0),
        internetFee: Number(formData.internetFee || 0),
        garbageFee: Number(formData.garbageFee || 0),
        amenities: splitAmenities(formData.amenitiesText),
        notes: formData.notes || null,
      });

      await onboardingApi.complete();
    },
    onSuccess: () => router.push('/'),
    onError: (err: any) => setError(err?.message || 'Thiết lập thất bại, vui lòng thử lại.'),
  });

  const nextStep = () => {
    setError(null);
    setStep((s) => s + 1);
  };

  const finish = () => {
    setError(null);
    completeMutation.mutate();
  };

  if (statusQuery.isLoading) {
    return (
      <div className="min-h-screen bg-slate-900 flex items-center justify-center text-white">
        <Loader2 className="w-8 h-8 animate-spin" />
      </div>
    );
  }

  if (statusQuery.data?.isDone) {
    router.push('/');
    return null;
  }

  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-6 relative overflow-hidden">
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
                <p className="text-lg font-medium text-slate-500 italic">Thiết lập nhanh hồ sơ, cơ sở và phòng đầu tiên theo đúng API backend.</p>
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
                  <h2 className="text-2xl font-black text-slate-900">Thông tin công ty/nhà trọ</h2>
                  <p className="text-sm font-bold text-slate-400 italic">Dữ liệu này sẽ được gửi vào endpoint Settings Company.</p>
               </div>
               <div className="space-y-4">
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tên công ty/nhà trọ</label>
                    <div className="relative">
                       <Building2 className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                       <input value={formData.companyName} onChange={(e) => setField('companyName', e.target.value)} type="text" placeholder="VD: Blue Moon Apartment" className="w-full pl-12 pr-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Địa chỉ</label>
                    <div className="relative">
                       <MapPin className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-300" />
                       <input value={formData.companyAddress} onChange={(e) => setField('companyAddress', e.target.value)} type="text" placeholder="Số nhà, tên đường, quận..." className="w-full pl-12 pr-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                    </div>
                  </div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <input value={formData.companyPhone} onChange={(e) => setField('companyPhone', e.target.value)} type="text" placeholder="Số điện thoại" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                    <input value={formData.companyEmail} onChange={(e) => setField('companyEmail', e.target.value)} type="email" placeholder="Email" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                  </div>
                  <input value={formData.taxCode} onChange={(e) => setField('taxCode', e.target.value)} type="text" placeholder="Mã số thuế (không bắt buộc)" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
               </div>
               <button 
                onClick={nextStep}
                disabled={!canContinueStep2}
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
                  <h2 className="text-2xl font-black text-slate-900">Tạo cơ sở đầu tiên</h2>
                  <p className="text-sm font-bold text-slate-400 italic">Dữ liệu sẽ gọi trực tiếp endpoint Create Property.</p>
               </div>
               <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2 col-span-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tên cơ sở</label>
                    <input value={formData.propertyName} onChange={(e) => setField('propertyName', e.target.value)} type="text" placeholder="VD: Khu trọ Bình An" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all" />
                  </div>
                  <div className="space-y-2 col-span-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Địa chỉ</label>
                    <input value={formData.propertyAddress} onChange={(e) => setField('propertyAddress', e.target.value)} type="text" placeholder="Số nhà, tên đường, quận..." className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tỉnh/TP</label>
                    <input value={formData.province} onChange={(e) => setField('province', e.target.value)} type="text" placeholder="TP.HCM" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Quận/Huyện</label>
                    <input value={formData.district} onChange={(e) => setField('district', e.target.value)} type="text" placeholder="Bình Thạnh" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Phường/Xã</label>
                    <input value={formData.ward} onChange={(e) => setField('ward', e.target.value)} type="text" placeholder="Phường 25" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
                  <div className="space-y-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Tổng số tầng</label>
                    <input value={formData.totalFloors} onChange={(e) => setField('totalFloors', Number(e.target.value || 1))} type="number" min={1} className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all" />
                  </div>
                  <div className="space-y-2 col-span-2">
                    <label className="text-[10px] font-black text-slate-400 uppercase tracking-widest pl-1">Mô tả</label>
                    <textarea value={formData.propertyDescription} onChange={(e) => setField('propertyDescription', e.target.value)} rows={3} placeholder="Mô tả cơ sở" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white transition-all resize-none" />
                  </div>
               </div>
               <button 
                onClick={nextStep}
                disabled={!canContinueStep3}
                className="w-full py-5 bg-slate-900 text-white rounded-2xl font-black text-lg flex items-center justify-center gap-3"
               >
                 Sang tạo phòng đầu tiên <ArrowRight className="w-6 h-6 text-indigo-400" />
               </button>
            </motion.div>
          )}

          {step === 4 && (
            <motion.div
              key="step4"
              initial={{ scale: 0.5, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              className="bg-white rounded-[3rem] p-12 space-y-8"
            >
               <div className="flex items-center justify-center w-20 h-20 bg-emerald-50 rounded-full mx-auto text-emerald-600">
                 <Check className="w-10 h-10" />
               </div>
               <div className="space-y-2 text-center">
                  <h1 className="text-3xl font-black text-slate-900 tracking-tight">Tạo phòng đầu tiên</h1>
                  <p className="text-sm font-bold text-slate-400 italic">Bước cuối: tạo một phòng mẫu và đánh dấu onboarding hoàn tất.</p>
               </div>
               <div className="grid grid-cols-2 gap-4">
                 <input value={formData.roomNumber} onChange={(e) => setField('roomNumber', e.target.value)} type="text" placeholder="Số phòng (VD: 101)" className="col-span-2 w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.floor} onChange={(e) => setField('floor', Number(e.target.value || 1))} type="number" min={1} placeholder="Tầng" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.areaSqm} onChange={(e) => setField('areaSqm', Number(e.target.value || 0))} type="number" min={0} step="0.1" placeholder="Diện tích" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.basePrice} onChange={(e) => setField('basePrice', Number(e.target.value || 0))} type="number" min={100000} placeholder="Giá thuê" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.electricityPrice} onChange={(e) => setField('electricityPrice', Number(e.target.value || 0))} type="number" min={1000} placeholder="Giá điện" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.waterPrice} onChange={(e) => setField('waterPrice', Number(e.target.value || 0))} type="number" min={1000} placeholder="Giá nước" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.serviceFee} onChange={(e) => setField('serviceFee', Number(e.target.value || 0))} type="number" min={0} placeholder="Phí dịch vụ" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.internetFee} onChange={(e) => setField('internetFee', Number(e.target.value || 0))} type="number" min={0} placeholder="Phí internet" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.garbageFee} onChange={(e) => setField('garbageFee', Number(e.target.value || 0))} type="number" min={0} placeholder="Phí rác" className="w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <input value={formData.amenitiesText} onChange={(e) => setField('amenitiesText', e.target.value)} type="text" placeholder="Tiện ích (cách nhau bởi dấu phẩy)" className="col-span-2 w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white" />
                 <textarea value={formData.notes} onChange={(e) => setField('notes', e.target.value)} rows={2} placeholder="Ghi chú phòng" className="col-span-2 w-full px-4 py-4 bg-slate-50 border border-slate-100 rounded-2xl font-bold outline-none focus:bg-white resize-none" />
               </div>
               {error && <p className="text-sm font-bold text-rose-600 text-center">{error}</p>}
               <button 
                onClick={finish}
                disabled={completeMutation.isPending || !canFinish}
                className="w-full py-5 bg-indigo-600 text-white rounded-2xl font-black text-lg shadow-xl shadow-indigo-100 flex items-center justify-center gap-3 disabled:opacity-70"
               >
                {completeMutation.isPending && <Loader2 className="w-5 h-5 animate-spin" />}
                 Hoàn tất thiết lập <Rocket className="w-5 h-5" />
               </button>
            </motion.div>
          )}
        </AnimatePresence>
      </div>
    </div>
  );
}
