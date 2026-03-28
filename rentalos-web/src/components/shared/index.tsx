'use client';

import { 
  CheckCircle2, AlertCircle, Clock, 
  AlertTriangle, XCircle, TrendingUp, TrendingDown 
} from 'lucide-react';
import { usePlanLimit } from '@/lib/hooks/usePlanLimit';

// --- STAT CARD ---
export function StatCard({ title, value, unit = '', trend, icon: Icon, color = 'indigo' }: any) {
  const colors: any = {
    indigo: 'bg-indigo-50 text-indigo-600',
    emerald: 'bg-emerald-50 text-emerald-600',
    amber: 'bg-amber-50 text-amber-600',
    rose: 'bg-rose-50 text-rose-600',
  };

  return (
    <div className="bg-white p-6 rounded-2xl border border-slate-100 shadow-sm hover:shadow-md transition-shadow group">
      <div className="flex justify-between items-start">
        <div className={`p-3 rounded-xl ${colors[color] || colors.indigo} group-hover:scale-110 transition-transform`}>
          <Icon className="w-6 h-6" />
        </div>
        {typeof trend === 'number' && (
          <div className={`flex items-center gap-1 text-xs font-bold ${trend > 0 ? 'text-emerald-600' : 'text-rose-600'}`}>
            {trend > 0 ? <TrendingUp className="w-3 h-3" /> : <TrendingDown className="w-3 h-3" />}
            {Math.abs(trend)}%
          </div>
        )}
      </div>
      <div className="mt-4">
        <p className="text-sm font-medium text-slate-500">{title}</p>
        <h3 className="text-3xl font-bold text-slate-900 mt-1">
          {value}
          {unit && <span className="text-lg font-semibold text-slate-400 ml-1">{unit}</span>}
        </h3>
      </div>
    </div>
  );
}

// --- STATUS BADGE ---
export function StatusBadge({ status, type = 'room', children }: { status: string, type?: 'room' | 'invoice' | 'contract', children?: React.ReactNode }) {
  const normalizedStatus = status.toLowerCase();
  const configs: any = {
    available: { label: 'Trống', color: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
    rented: { label: 'Đã thuê', color: 'bg-indigo-50 text-indigo-700 border-indigo-200', icon: Clock },
    occupied: { label: 'Đã thuê', color: 'bg-indigo-50 text-indigo-700 border-indigo-200', icon: Clock },
    maintenance: { label: 'Bảo trì', color: 'bg-amber-50 text-amber-700 border-amber-200', icon: AlertCircle },
    reserved: { label: 'Đặt trước', color: 'bg-violet-50 text-violet-700 border-violet-200', icon: Clock },
    pending: { label: 'Chờ thanh toán', color: 'bg-amber-50 text-amber-700 border-amber-200', icon: Clock },
    paid: { label: 'Đã thanh toán', color: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
    overdue: { label: 'Quá hạn', color: 'bg-rose-50 text-rose-700 border-rose-200', icon: AlertTriangle },
    active: { label: 'Đang hiệu lực', color: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
    expired: { label: 'Hết hạn', color: 'bg-amber-50 text-amber-700 border-amber-200', icon: AlertTriangle },
    renewed: { label: 'Đã gia hạn', color: 'bg-sky-50 text-sky-700 border-sky-200', icon: CheckCircle2 },
    terminated: { label: 'Đã thanh lý', color: 'bg-rose-50 text-rose-700 border-rose-200', icon: XCircle },
    partial: { label: 'Thanh toán một phần', color: 'bg-blue-50 text-blue-700 border-blue-200', icon: Clock },
    success: { label: 'Thành công', color: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
    failed: { label: 'Thất bại', color: 'bg-rose-50 text-rose-700 border-rose-200', icon: XCircle },
    refunded: { label: 'Đã hoàn', color: 'bg-slate-50 text-slate-700 border-slate-200', icon: AlertCircle },
  };

  const config = configs[normalizedStatus] || { label: status, color: 'bg-slate-50 text-slate-700 border-slate-200', icon: AlertCircle };
  const Icon = config.icon;

  return (
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full border text-xs font-bold ${config.color}`}>
      <Icon className="w-3.5 h-3.5" />
      {children || config.label}
    </span>
  );
}

// --- PLAN LIMIT BANNER ---
export function PlanLimitBanner() {
  const { plan } = usePlanLimit();
  if (plan !== 'expired') return null;

  return (
    <div className="bg-rose-600 text-white px-6 py-3 rounded-xl mb-6 flex items-center justify-between shadow-lg shadow-rose-200/50 animate-pulse">
      <div className="flex items-center gap-3">
        <XCircle className="w-6 h-6" />
        <div>
          <p className="font-bold">Gói dịch vụ đã hết hạn!</p>
          <p className="text-sm text-rose-100">Vui lòng gia hạn để tiếp tục quản lý nhà trọ và sử dụng các tính năng premium.</p>
        </div>
      </div>
      <button className="bg-white text-rose-600 px-4 py-2 rounded-lg font-bold hover:bg-rose-50 transition-colors">
        Nâng cấp ngay
      </button>
    </div>
  );
}

