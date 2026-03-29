'use client';

import {
  AlertCircle,
  AlertTriangle,
  CheckCircle2,
  Clock,
  TrendingDown,
  TrendingUp,
  XCircle,
} from 'lucide-react';
import { usePlanLimit } from '@/lib/hooks/usePlanLimit';

export function StatCard({ title, value, unit = '', trend, icon: Icon, color = 'indigo' }: any) {
  const colors: Record<string, string> = {
    indigo: 'bg-indigo-50 text-indigo-600',
    emerald: 'bg-emerald-50 text-emerald-600',
    amber: 'bg-amber-50 text-amber-600',
    rose: 'bg-rose-50 text-rose-600',
    violet: 'bg-violet-50 text-violet-600',
  };

  const normalizedTrend =
    typeof trend === 'number'
      ? trend
      : typeof trend?.value === 'number'
        ? (trend.isPositive === false ? -trend.value : trend.value)
        : null;

  return (
    <div className="group rounded-2xl border border-slate-100 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex items-start justify-between">
        <div className={`rounded-xl p-3 transition-transform group-hover:scale-110 ${colors[color] || colors.indigo}`}>
          <Icon className="h-6 w-6" />
        </div>
        {normalizedTrend !== null && (
          <div className={`flex items-center gap-1 text-xs font-bold ${normalizedTrend > 0 ? 'text-emerald-600' : 'text-rose-600'}`}>
            {normalizedTrend > 0 ? <TrendingUp className="h-3 w-3" /> : <TrendingDown className="h-3 w-3" />}
            {Math.abs(normalizedTrend)}%
          </div>
        )}
      </div>
      <div className="mt-4">
        <p className="text-sm font-medium text-slate-500">{title}</p>
        <h3 className="mt-1 text-3xl font-bold text-slate-900">
          {value}
          {unit && <span className="ml-1 text-lg font-semibold text-slate-400">{unit}</span>}
        </h3>
      </div>
    </div>
  );
}

const STATUS_CONFIGS: Record<string, { label: string; color: string; icon: any }> = {
  '0': { label: 'Trống', color: 'bg-emerald-50 text-emerald-700 border-emerald-200', icon: CheckCircle2 },
  '1': { label: 'Đã thuê', color: 'bg-indigo-50 text-indigo-700 border-indigo-200', icon: Clock },
  '2': { label: 'Bảo trì', color: 'bg-amber-50 text-amber-700 border-amber-200', icon: AlertCircle },
  '3': { label: 'Đặt trước', color: 'bg-violet-50 text-violet-700 border-violet-200', icon: Clock },
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
  danger: { label: 'Danh sách đen', color: 'bg-rose-50 text-rose-700 border-rose-200', icon: XCircle },
  blacklisted: { label: 'Danh sách đen', color: 'bg-rose-50 text-rose-700 border-rose-200', icon: XCircle },
};

export function StatusBadge({
  status,
  children,
}: {
  status: unknown;
  type?: 'room' | 'invoice' | 'contract';
  children?: React.ReactNode;
}) {
  const normalizedStatus = String(status ?? '').toLowerCase();
  const config = STATUS_CONFIGS[normalizedStatus] || {
    label: String(status ?? 'Không xác định'),
    color: 'bg-slate-50 text-slate-700 border-slate-200',
    icon: AlertCircle,
  };
  const Icon = config.icon;

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-0.5 text-xs font-bold ${config.color}`}>
      <Icon className="h-3.5 w-3.5" />
      {children || config.label}
    </span>
  );
}

export function PlanLimitBanner() {
  const { plan } = usePlanLimit();
  if (plan !== 'expired') return null;

  return (
    <div className="mb-6 flex animate-pulse items-center justify-between rounded-xl bg-rose-600 px-6 py-3 text-white shadow-lg shadow-rose-200/50">
      <div className="flex items-center gap-3">
        <XCircle className="h-6 w-6" />
        <div>
          <p className="font-bold">Gói dịch vụ đã hết hạn</p>
          <p className="text-sm text-rose-100">Vui lòng gia hạn để tiếp tục quản lý nhà trọ và sử dụng các tính năng premium.</p>
        </div>
      </div>
      <button className="rounded-lg bg-white px-4 py-2 font-bold text-rose-600 transition-colors hover:bg-rose-50">
        Nâng cấp ngay
      </button>
    </div>
  );
}
