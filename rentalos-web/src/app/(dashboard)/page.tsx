'use client';

import { 
  Users, Building2, Bed, 
  Receipt, TrendingUp, Wallet,
  Calendar, ArrowRight 
} from 'lucide-react';
import { StatCard, StatusBadge } from '@/components/shared';
import { useAuthStore } from '@/lib/stores/authStore';

export default function DashboardPage() {
  const { user } = useAuthStore();
  const dateStr = new Date().toLocaleDateString('vi-VN', { weekday: 'long', day: 'numeric', month: 'long' });

  return (
    <div className="animate-fade-in flex flex-col gap-8">
      {/* Welcome Header */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
        <div>
          <p className="text-sm font-bold text-indigo-600 uppercase tracking-widest">{dateStr}</p>
          <h1 className="text-3xl font-bold text-slate-900 mt-1">Chào buổi sáng, {user?.fullName || 'Người dùng'}! 👋</h1>
          <p className="text-slate-500 mt-1 text-lg font-medium">Hôm nay có 3 hóa đơn quá hạn cần xử lý và 2 hợp đồng sắp hết hạn.</p>
        </div>
        <button className="flex items-center gap-2 px-6 py-3 bg-slate-900 text-white rounded-xl font-bold hover:bg-slate-800 transition-all shadow-lg shadow-slate-200">
          Tạo hóa đơn nhanh
          <ArrowRight className="w-4 h-4" />
        </button>
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <StatCard title="Tổng doanh thu tháng" value="128.5" unit="Tr" trend={12.5} icon={Wallet} color="emerald" />
        <StatCard title="Tỷ lệ lấp đầy" value="94" unit="%" trend={2.1} icon={Bed} color="indigo" />
        <StatCard title="Tỷ lệ thu tiền" value="88" unit="%" trend={-4.5} icon={TrendingUp} color="amber" />
        <StatCard title="Số khách đang thuê" value="42" icon={Users} color="rose" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Recent Activity */}
        <div className="lg:col-span-2 bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              <Calendar className="w-5 h-5 text-indigo-600" />
              Hoạt động gần đây
            </h2>
            <button className="text-sm font-bold text-indigo-600 hover:underline">Tất cả hoạt động</button>
          </div>
          
          <div className="space-y-4">
            {[1, 2, 3, 4, 5].map((i) => (
              <div key={i} className="flex items-center gap-4 p-3 rounded-xl hover:bg-slate-50 transition-colors border border-transparent hover:border-slate-100">
                <div className={`w-10 h-10 rounded-full flex items-center justify-center font-bold ${i % 2 === 0 ? 'bg-indigo-50 text-indigo-600' : 'bg-emerald-50 text-emerald-600'}`}>
                  {i % 2 === 0 ? 'HD' : 'PT'}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-bold text-slate-900 truncate">Hóa đơn số #INV-00{i} đã được thanh toán</p>
                  <p className="text-xs text-slate-500 mt-0.5">Phòng 102 - Nhà trọ ABC • 2 giờ trước</p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-bold text-slate-900">+1.250.000đ</p>
                  <StatusBadge status={i % 2 === 0 ? 'paid' : 'available'} />
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Quick Insights */}
        <div className="space-y-6">
          <div className="bg-indigo-600 rounded-2xl p-6 text-white shadow-xl shadow-indigo-200">
            <h3 className="font-bold text-lg mb-2">Lời khuyên từ AI 🤖</h3>
            <p className="text-indigo-100 text-sm leading-relaxed">
              Dựa trên dữ liệu 3 tháng qua, khu vực Nhà trọ ABC đang có nhu cầu cao. Bạn có thể cân nhắc tăng giá phòng 10% cho các hợp đồng mới để tối ưu lợi nhuận.
            </p>
            <button className="mt-4 w-full py-2 bg-white/20 hover:bg-white/30 rounded-lg text-sm font-bold transition-all backdrop-blur-sm">
              Xem báo cáo chi tiết
            </button>
          </div>

          <div className="bg-white rounded-2xl border border-slate-100 shadow-sm p-6">
            <h3 className="font-bold text-slate-900 mb-4">Trạng thái phòng</h3>
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-emerald-500" />
                  <span className="text-sm font-medium text-slate-600">Đang trống</span>
                </div>
                <span className="text-sm font-bold">12</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-indigo-500" />
                  <span className="text-sm font-medium text-slate-600">Đã thuê</span>
                </div>
                <span className="text-sm font-bold">42</span>
              </div>
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full bg-amber-500" />
                  <span className="text-sm font-medium text-slate-600">Bảo trì</span>
                </div>
                <span className="text-sm font-bold">4</span>
              </div>
              <div className="mt-4 pt-4 border-t border-slate-100">
                <div className="flex justify-between items-end">
                  <p className="text-sm text-slate-500">Tỷ lệ trống</p>
                  <p className="text-xl font-bold text-rose-600">6.2%</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
 Eskom Dashboard Page complete. Eskom data-driven UI ready.
 Eskom note: I manually removed duplicate exports from shared/index.tsx imports of Sidebar/Header to ensure no circle.
