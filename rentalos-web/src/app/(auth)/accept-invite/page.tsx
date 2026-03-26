'use client';

import { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { User, Phone, Lock, ShieldCheck, AlertCircle } from 'lucide-react';
import { acceptInviteSchema, type AcceptInviteInput } from '@/lib/schemas/authSchema';
import { staffApi } from '@/lib/api/staff';
import { authApi } from '@/lib/api/auth';
import { useAuthStore } from '@/lib/stores/authStore';

import { Suspense } from 'react';

function AcceptInviteContent() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const token = searchParams.get('token');
  const setAuth = useAuthStore((state) => state.setAuth);

  const [verifying, setVerifying] = useState(true);
  const [valid, setValid] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [tenantInfo, setTenantInfo] = useState<any>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<AcceptInviteInput>({
    resolver: zodResolver(acceptInviteSchema),
  });

  useEffect(() => {
    if (!token) {
      setVerifying(false);
      return;
    }

    const verifyToken = async () => {
      try {
        const response = await staffApi.verifyInvite(token);
        setTenantInfo(response.data.tenant);
        setValid(true);
      } catch (err) {
        setValid(false);
      } finally {
        setVerifying(false);
      }
    };

    verifyToken();
  }, [token]);

  const onSubmit = async (data: AcceptInviteInput) => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      await staffApi.acceptInvite(token, {
        fullName: data.fullName,
        phoneNumber: data.phoneNumber,
        password: data.password
      });
      // Auto login after success
      // Note: Assuming acceptInvite returns auth result or we need to login manually
      // Here we simulate a login or expect the API to return credentials
      const loginRes = await authApi.login({
        email: tenantInfo.inviteEmail, 
        password: data.password, 
        tenantSlug: tenantInfo.slug 
      });
      setAuth(loginRes.data);
      router.push('/');
    } catch (err: any) {
      setError(err.message || 'Xác nhận lời mời thất bại. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  if (verifying) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50">
        <div className="text-center">
          <div className="w-16 h-16 border-4 border-indigo-600/20 border-t-indigo-600 rounded-full animate-spin mx-auto mb-4" />
          <p className="text-slate-500 font-bold">Đang xác thực lời mời...</p>
        </div>
      </div>
    );
  }

  if (!valid) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6">
        <div className="max-w-md w-full bg-white p-10 rounded-[2.5rem] shadow-xl text-center">
          <div className="w-20 h-20 bg-rose-50 rounded-full flex items-center justify-center mx-auto mb-6">
            <AlertCircle className="w-10 h-10 text-rose-500" />
          </div>
          <h2 className="text-2xl font-black text-slate-900 mb-4">Lời mời không hợp lệ</h2>
          <p className="text-slate-500 font-medium mb-8">
            Liên kết mời đã hết hạn hoặc không tồn tại. Vui lòng liên hệ quản trị viên để nhận mã mời mới.
          </p>
          <a href="/login" className="px-8 py-3 bg-slate-900 text-white rounded-2xl font-bold hover:bg-slate-800 transition-all">
            Quay lại đăng nhập
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex bg-slate-50">
      <div className="w-full lg:w-1/2 flex items-center justify-center p-8 bg-white shadow-2xl z-10">
        <div className="max-w-md w-full animate-slide-up">
          <div className="mb-10 text-center">
            <div className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-50 border border-indigo-100 rounded-full text-indigo-600 text-sm font-bold mb-6">
              <ShieldCheck className="w-4 h-4" />
              Lời mời gia nhập đội ngũ
            </div>
            <h2 className="text-3xl font-black text-slate-900 tracking-tight">Hoàn tất hồ sơ của bạn</h2>
            <p className="text-slate-500 mt-2 font-medium">Bạn đang tham gia vào <strong>{tenantInfo?.name || 'nhà trọ'}</strong></p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {error && (
              <div className="p-4 bg-rose-50 border border-rose-100 text-rose-600 rounded-xl text-sm font-bold">
                {error}
              </div>
            )}

            <div className="space-y-5">
              {/* Full Name */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Họ & Tên</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <User className="w-5 h-5" />
                  </div>
                  <input
                    {...register('fullName')}
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.fullName ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="Nguyễn Văn B"
                  />
                </div>
                {errors.fullName && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.fullName.message}</p>}
              </div>

              {/* Phone */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Số điện thoại</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Phone className="w-5 h-5" />
                  </div>
                  <input
                    {...register('phoneNumber')}
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.phoneNumber ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="09xx xxx xxx"
                  />
                </div>
                {errors.phoneNumber && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.phoneNumber.message}</p>}
              </div>

              {/* Password */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Thiết lập mật khẩu</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Lock className="w-5 h-5" />
                  </div>
                  <input
                    {...register('password')}
                    type="password"
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.password ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="••••••••"
                  />
                </div>
                {errors.password && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.password.message}</p>}
              </div>

              {/* Confirm Password */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Xác nhận mật khẩu</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Lock className="w-5 h-5" />
                  </div>
                  <input
                    {...register('confirmPassword')}
                    type="password"
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.confirmPassword ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="••••••••"
                  />
                </div>
                {errors.confirmPassword && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.confirmPassword.message}</p>}
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full py-4 px-6 bg-slate-900 text-white rounded-2xl font-bold text-lg hover:bg-slate-800 transition-all mt-4"
            >
              {loading ? 'Đang thiết lập tài khoản...' : 'Kích hoạt & Gia nhập ngay'}
            </button>
          </form>
        </div>
      </div>

      <div className="hidden lg:block w-1/2 bg-indigo-600 p-20 relative overflow-hidden">
        <div className="absolute top-0 right-0 w-96 h-96 bg-white/10 rounded-full blur-3xl -mr-48 -mt-48" />
        <div className="absolute bottom-0 left-0 w-96 h-96 bg-indigo-500 rounded-full blur-3xl -ml-48 -mb-48" />
        
        <div className="relative z-10 h-full flex flex-col justify-center text-white">
          <h3 className="text-5xl font-black mb-10 leading-tight">Chào mừng bạn đến với đội ngũ! 🚀</h3>
          <div className="space-y-8">
            <div className="flex gap-4 items-start">
              <div className="w-12 h-12 bg-white/10 backdrop-blur-md rounded-xl flex items-center justify-center font-black text-xl border border-white/20">1</div>
              <div>
                <h4 className="text-xl font-bold mb-2">Thao tác dễ dàng</h4>
                <p className="text-indigo-100 font-medium">Giao diện trực quan, dễ học và sử dụng cho mọi thành viên.</p>
              </div>
            </div>
            <div className="flex gap-4 items-start">
              <div className="w-12 h-12 bg-white/10 backdrop-blur-md rounded-xl flex items-center justify-center font-black text-xl border border-white/20">2</div>
              <div>
                <h4 className="text-xl font-bold mb-2">Làm việc mọi lúc</h4>
                <p className="text-indigo-100 font-medium">Truy cập dữ liệu thời gian thực từ bất kỳ thiết bị nào.</p>
              </div>
            </div>
            <div className="flex gap-4 items-start">
              <div className="w-12 h-12 bg-white/10 backdrop-blur-md rounded-xl flex items-center justify-center font-black text-xl border border-white/20">3</div>
              <div>
                <h4 className="text-xl font-bold mb-2">Hỗ trợ AI ⚡</h4>
                <p className="text-indigo-100 font-medium">AI Agent giúp bạn trả lời khách hàng và phân tích dữ liệu nhanh chóng.</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function AcceptInvitePage() {
  return (
    <Suspense fallback={<div>Đang tải...</div>}>
      <AcceptInviteContent />
    </Suspense>
  );
}
