'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Eye, EyeOff, Lock, Mail, Building2, ArrowRight } from 'lucide-react';
import { loginSchema, type LoginInput } from '@/lib/schemas/authSchema';
import { authApi } from '@/lib/api/auth';
import { useAuthStore } from '@/lib/stores/authStore';

export default function LoginPage() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginInput>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginInput) => {
    setLoading(true);
    setError(null);
    try {
      const response = await authApi.login({
        email: data.email, 
        password: data.password, 
        tenantSlug: data.tenantSlug 
      });
      setAuth(response.data);
      router.push('/');
    } catch (err: any) {
      setError(err.message || 'Email hoặc mật khẩu không chính xác');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex">
      {/* Left side: Form */}
      <div className="w-full lg:w-1/2 flex items-center justify-center p-8 bg-white">
        <div className="max-w-md w-full space-y-8 animate-slide-up">
          <div className="text-center">
            <h2 className="text-4xl font-extrabold text-slate-900 tracking-tight">Chào mừng trở lại</h2>
            <p className="mt-3 text-slate-500 font-medium">Đăng nhập vào hệ thống quản lý của bạn</p>
          </div>

          <form className="mt-10 space-y-6" onSubmit={handleSubmit(onSubmit)}>
            {error && (
              <div className="p-4 bg-rose-50 border border-rose-100 text-rose-600 rounded-xl text-sm font-medium">
                {error}
              </div>
            )}

            <div className="space-y-5">
              {/* Tenant Slug */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Mã định danh (Slug)</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Building2 className="w-5 h-5" />
                  </div>
                  <input
                    {...register('tenantSlug')}
                    className={`block w-full pl-11 pr-4 py-3.5 bg-slate-50 border ${errors.tenantSlug ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="ví dụ: nha-tro-abc"
                  />
                </div>
                {errors.tenantSlug && <p className="mt-2 text-xs font-bold text-rose-500">{errors.tenantSlug.message}</p>}
              </div>

              {/* Email */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Email</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Mail className="w-5 h-5" />
                  </div>
                  <input
                    {...register('email')}
                    type="email"
                    className={`block w-full pl-11 pr-4 py-3.5 bg-slate-50 border ${errors.email ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="name@company.com"
                  />
                </div>
                {errors.email && <p className="mt-2 text-xs font-bold text-rose-500">{errors.email.message}</p>}
              </div>

              {/* Password */}
              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="block text-sm font-bold text-slate-700">Mật khẩu</label>
                  <a href="/forgot-password" className="text-sm font-bold text-indigo-600 hover:text-indigo-700 transition-colors">Quên mật khẩu?</a>
                </div>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Lock className="w-5 h-5" />
                  </div>
                  <input
                    {...register('password')}
                    type={showPassword ? 'text' : 'password'}
                    className={`block w-full pl-11 pr-12 py-3.5 bg-slate-50 border ${errors.password ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="••••••••"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute inset-y-0 right-0 pr-4 flex items-center text-slate-400 hover:text-slate-600 transition-colors"
                  >
                    {showPassword ? <EyeOff className="w-5 h-5" /> : <Eye className="w-5 h-5" />}
                  </button>
                </div>
                {errors.password && <p className="mt-2 text-xs font-bold text-rose-500">{errors.password.message}</p>}
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full flex items-center justify-center gap-2 py-4 px-6 bg-slate-900 text-white rounded-2xl font-bold text-lg hover:bg-slate-800 focus:outline-none focus:ring-4 focus:ring-slate-900/10 transition-all disabled:opacity-50 disabled:cursor-not-allowed shadow-xl shadow-slate-200"
            >
              {loading ? (
                <div className="w-6 h-6 border-4 border-white/20 border-t-white rounded-full animate-spin" />
              ) : (
                <>
                  Đăng nhập
                  <ArrowRight className="w-5 h-5" />
                </>
              )}
            </button>

            <p className="text-center text-slate-500 font-medium pt-4">
              Chưa có tài khoản?{' '}
              <a href="/register" className="text-indigo-600 font-bold hover:underline">Hãy đăng ký ngay</a>
            </p>
          </form>
        </div>
      </div>

      {/* Right side: Visual Content */}
      <div className="hidden lg:flex lg:w-1/2 bg-indigo-600 items-center justify-center p-12 relative overflow-hidden">
        {/* Abstract Background pattern */}
        <div className="absolute inset-0 opacity-10 pointer-events-none">
          <div className="absolute top-0 -left-1/4 w-full h-full bg-[radial-gradient(circle_at_center,_var(--indigo-400)_0%,_transparent_70%)]" />
        </div>
        
        <div className="relative z-10 text-center max-w-lg">
          <div className="bg-white/10 backdrop-blur-xl border border-white/20 p-8 rounded-[3rem] shadow-[0_32px_64px_-16px_rgba(0,0,0,0.2)]">
            <h3 className="text-4xl font-extrabold text-white mb-6 leading-tight">Quản lý nhà trọ <br/> tầm cao mới với AI ⚡</h3>
            <p className="text-indigo-100 text-lg font-medium leading-relaxed">
              Tự động hóa thanh toán, quản lý hợp đồng thông minh và tối ưu hóa doanh thu chỉ với một vài cú click chuột.
            </p>
            <div className="mt-10 flex flex-wrap justify-center gap-4">
              <div className="bg-white/10 px-4 py-2 rounded-xl text-white text-sm font-bold border border-white/10">+5000 Phòng trọ</div>
              <div className="bg-white/10 px-4 py-2 rounded-xl text-white text-sm font-bold border border-white/10">+1200 Chủ trọ</div>
              <div className="bg-white/10 px-4 py-2 rounded-xl text-white text-sm font-bold border border-white/10">99.9% Up-time</div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
