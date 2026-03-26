'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Mail, ArrowLeft, CheckCircle2 } from 'lucide-react';
import { forgotPasswordSchema, type ForgotPasswordInput } from '@/lib/schemas/authSchema';

export default function ForgotPasswordPage() {
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordInput>({
    resolver: zodResolver(forgotPasswordSchema),
  });

  const onSubmit = async (data: ForgotPasswordInput) => {
    setLoading(true);
    setError(null);
    try {
      console.log('Requesting password reset for...', data.email);
      await new Promise(resolve => setTimeout(resolve, 1500));
      setSuccess(true);
    } catch (err: any) {
      setError('Đã có lỗi xảy ra. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6">
        <div className="max-w-md w-full bg-white p-10 rounded-[2.5rem] shadow-xl shadow-slate-200 text-center animate-scale-in">
          <div className="w-20 h-20 bg-emerald-50 rounded-full flex items-center justify-center mx-auto mb-6">
            <CheckCircle2 className="w-10 h-10 text-emerald-500" />
          </div>
          <h2 className="text-2xl font-black text-slate-900 mb-4">Kiểm tra email của bạn</h2>
          <p className="text-slate-500 font-medium leading-relaxed mb-8">
            Chúng tôi đã gửi hướng dẫn đặt lại mật khẩu đến email của bạn. Vui lòng kiểm tra cả hòm thư rác nếu không thấy.
          </p>
          <a href="/login" className="inline-flex items-center gap-2 font-bold text-indigo-600 hover:text-indigo-700 transition-colors">
            <ArrowLeft className="w-4 h-4" />
            Quay lại đăng nhập
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6">
      <div className="max-w-md w-full bg-white p-10 rounded-[2.5rem] shadow-xl shadow-slate-200 animate-slide-up">
        <div className="text-center mb-10">
          <h2 className="text-3xl font-black text-slate-900 tracking-tight">Quên mật khẩu?</h2>
          <p className="text-slate-500 mt-2 font-medium leading-relaxed">
            Nhập email của bạn và chúng tôi sẽ gửi liên kết đặt lại mật khẩu.
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {error && (
            <div className="p-4 bg-rose-50 border border-rose-100 text-rose-600 rounded-xl text-sm font-bold">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-bold text-slate-700 mb-2">Địa chỉ Email</label>
            <div className="relative group">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                <Mail className="w-5 h-5" />
              </div>
              <input
                {...register('email')}
                type="email"
                className={`block w-full pl-11 pr-4 py-3.5 bg-slate-50 border ${errors.email ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                placeholder="name@email.com"
              />
            </div>
            {errors.email && <p className="mt-2 text-xs font-bold text-rose-500">{errors.email.message}</p>}
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full py-4 px-6 bg-slate-900 text-white rounded-2xl font-bold text-lg hover:bg-slate-800 transition-all shadow-lg shadow-slate-200 disabled:opacity-50"
          >
            {loading ? 'Đang gửi yêu cầu...' : 'Gửi yêu cầu'}
          </button>

          <div className="text-center pt-2">
            <a href="/login" className="inline-flex items-center gap-2 font-bold text-slate-500 hover:text-slate-900 transition-colors">
              <ArrowLeft className="w-4 h-4" />
              Quay lại đăng nhập
            </a>
          </div>
        </form>
      </div>
    </div>
  );
}
