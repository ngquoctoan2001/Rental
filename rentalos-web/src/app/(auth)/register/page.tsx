'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { 
  User, Mail, Phone, 
  Building2, Lock, ArrowRight,
  ShieldCheck 
} from 'lucide-react';
import { registerSchema, type RegisterInput } from '@/lib/schemas/authSchema';
import { authApi } from '@/lib/api/auth';

export default function RegisterPage() {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterInput>({
    resolver: zodResolver(registerSchema),
  });

  const onSubmit = async (data: RegisterInput) => {
    setLoading(true);
    setError(null);
    try {
      await authApi.register({
        ownerName: data.fullName,
        ownerEmail: data.email,
        phone: data.phoneNumber,
        tenantName: data.tenantName,
        password: data.password,
      });
      router.push('/login?registered=true');
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data?.title || err.message || 'Đăng ký không thành công. Vui lòng thử lại sau.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex bg-slate-50">
      <div className="w-full lg:w-1/2 flex flex-col justify-center py-12 px-6 lg:px-24 bg-white shadow-2xl z-10">
        <div className="max-w-xl w-full mx-auto animate-fade-in">
          <div className="flex items-center gap-3 mb-10">
            <div className="w-12 h-12 bg-indigo-600 rounded-2xl flex items-center justify-center text-white shadow-lg shadow-indigo-100">
              <ShieldCheck className="w-7 h-7" />
            </div>
            <div>
              <h1 className="text-2xl font-black text-slate-900 tracking-tight">RentalOS</h1>
              <p className="text-xs font-bold text-indigo-600 uppercase tracking-widest leading-none">Smart Management</p>
            </div>
          </div>

          <div className="mb-10">
            <h2 className="text-3xl font-extrabold text-slate-900 tracking-tight">Tạo tài khoản mới</h2>
            <p className="text-slate-500 mt-2 font-medium">Bắt đầu hành trình chuyển đổi số cho nhà trọ của bạn chỉ trong 2 phút.</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {error && (
              <div className="p-4 bg-rose-50 border border-rose-100 text-rose-600 rounded-xl text-sm font-bold animate-shake">
                {error}
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Full Name */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Họ tên chủ trọ</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <User className="w-5 h-5" />
                  </div>
                  <input
                    {...register('fullName')}
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.fullName ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="Nguyễn Văn A"
                  />
                </div>
                {errors.fullName && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.fullName.message}</p>}
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
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.email ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="name@email.com"
                  />
                </div>
                {errors.email && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.email.message}</p>}
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
                    placeholder="0912345678"
                  />
                </div>
                {errors.phoneNumber && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.phoneNumber.message}</p>}
              </div>

              {/* Tenant Name */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Tên cơ sở kinh doanh</label>
                <div className="relative group">
                  <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-indigo-600 transition-colors">
                    <Building2 className="w-5 h-5" />
                  </div>
                  <input
                    {...register('tenantName')}
                    className={`block w-full pl-11 pr-4 py-3 bg-slate-50 border ${errors.tenantName ? 'border-rose-300' : 'border-slate-200'} rounded-2xl text-slate-900 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all font-medium`}
                    placeholder="VD: Nhà trọ ABC"
                  />
                </div>
                {errors.tenantName && <p className="mt-1.5 text-xs font-bold text-rose-500">{errors.tenantName.message}</p>}
              </div>

              {/* Password */}
              <div>
                <label className="block text-sm font-bold text-slate-700 mb-2">Mật khẩu</label>
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

            <div className="pt-4 border-t border-slate-100">
              <p className="text-xs text-slate-500 text-center mb-6 leading-relaxed">
                Bằng cách đăng ký, bạn đồng ý với <a href="#" className="text-indigo-600 font-bold hover:underline">Điều khoản dịch vụ</a> và <a href="#" className="text-indigo-600 font-bold hover:underline">Chính sách bảo mật</a> của chúng tôi.
              </p>
              
              <button
                type="submit"
                disabled={loading}
                className="w-full flex items-center justify-center gap-2 py-4 px-6 bg-slate-900 text-white rounded-2xl font-bold text-lg hover:bg-slate-800 transition-all shadow-xl shadow-slate-100 disabled:opacity-50"
              >
                {loading ? 'Đang khởi tạo hệ thống...' : 'Tạo tài khoản & Bắt đầu'}
                <ArrowRight className="w-5 h-5" />
              </button>
            </div>

            <p className="text-center text-slate-600 font-bold mt-4">
              Đã có tài khoản?{' '}
              <a href="/login" className="text-indigo-600 hover:underline">Đăng nhập ngay</a>
            </p>
          </form>
        </div>
      </div>

      <div className="hidden lg:block w-1/2 bg-slate-50 relative overflow-hidden bg-[url('https://images.unsplash.com/photo-1560518883-ce09059eeffa?ixlib=rb-4.0.3&auto=format&fit=crop&w=1200&q=80')] bg-cover bg-center">
        <div className="absolute inset-0 bg-slate-900/40 backdrop-blur-[2px]" />
        <div className="absolute inset-0 flex items-center justify-center p-20">
          <div className="text-white">
            <h3 className="text-5xl font-black mb-6 leading-tight">Mọi phòng trọ,<br/> Một hệ thống.</h3>
            <p className="text-xl font-medium text-slate-100 leading-relaxed max-w-lg">
              Giảm 70% thời gian quản lý thủ công. Tập trung vào việc phát triển chuỗi nhà trọ của bạn, chúng tôi sẽ lo phần còn lại.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
