'use client';

import { useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Lock, ShieldCheck, ArrowRight } from 'lucide-react';
import { resetPasswordSchema, type ResetPasswordInput } from '@/lib/schemas/authSchema';
import { authApi } from '@/lib/api';

function ResetPasswordForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ResetPasswordInput>({
    resolver: zodResolver(resetPasswordSchema),
  });

  const onSubmit = async (data: ResetPasswordInput) => {
    const token = searchParams.get('token');
    const email = searchParams.get('email');
    const tenantSlug = searchParams.get('tenant');
    if (!token || !email || !tenantSlug) {
      setError('Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.');
      return;
    }
    setLoading(true);
    setError(null);
    try {
      await authApi.resetPassword({ email, token, newPassword: data.password, tenantSlug });
      router.push('/login?reset=true');
    } catch (err: any) {
      setError(err.message || 'Token không hợp lệ hoặc đã hết hạn.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6">
      <div className="max-w-md w-full bg-white p-10 rounded-[2.5rem] shadow-xl shadow-slate-200 animate-slide-up">
        <div className="text-center mb-10">
          <div className="w-16 h-16 bg-indigo-50 rounded-2xl flex items-center justify-center mx-auto mb-6 text-indigo-600">
            <ShieldCheck className="w-8 h-8" />
          </div>
          <h2 className="text-3xl font-black text-slate-900 tracking-tight">Đặt lại mật khẩu</h2>
          <p className="text-slate-500 mt-2 font-medium leading-relaxed">
            Vui lòng nhập mật khẩu mới bảo mật nhất cho tài khoản của bạn.
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {error && (
            <div className="p-4 bg-rose-50 border border-rose-100 text-rose-600 rounded-xl text-sm font-bold">
              {error}
            </div>
          )}

          <div className="space-y-5">
            {/* Password */}
            <div>
              <label className="block text-sm font-bold text-slate-700 mb-2">Mật khẩu mới</label>
              <div className="relative group">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                  <Lock className="w-5 h-5" />
                </div>
                <input
                  {...register('password')}
                  type="password"
                  className={`block w-full pl-11 pr-4 py-3.5 bg-slate-50 border ${errors.password ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                  placeholder="••••••••"
                />
              </div>
              {errors.password && <p className="mt-2 text-xs font-bold text-rose-500">{errors.password.message}</p>}
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
                  className={`block w-full pl-11 pr-4 py-3.5 bg-slate-50 border ${errors.confirmPassword ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                  placeholder="••••••••"
                />
              </div>
              {errors.confirmPassword && <p className="mt-2 text-xs font-bold text-rose-500">{errors.confirmPassword.message}</p>}
            </div>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full flex items-center justify-center gap-2 py-4 px-6 bg-slate-900 text-white rounded-2xl font-bold text-lg hover:bg-slate-800 transition-all shadow-lg shadow-slate-200 disabled:opacity-50"
          >
            {loading ? 'Đang cập nhật...' : 'Cập nhật mật khẩu'}
            <ArrowRight className="w-5 h-5" />
          </button>
        </form>
      </div>
    </div>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={null}>
      <ResetPasswordForm />
    </Suspense>
  );
}
