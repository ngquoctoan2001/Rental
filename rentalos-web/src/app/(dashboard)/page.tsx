'use client';

import Link from 'next/link';
import { useQuery } from '@tanstack/react-query';
import {
  AlertTriangle,
  ArrowRight,
  BarChart3,
  DollarSign,
  FileWarning,
  Home,
  Receipt,
  Users,
} from 'lucide-react';
import {
  Area,
  AreaChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';
import { reportsApi } from '@/lib/api/reports';
import { formatCurrency } from '@/lib/utils';
import { StatCard } from '@/components/shared';
import { useUIStore } from '@/lib/stores/uiStore';

export default function DashboardPage() {
  const { activePropertyId } = useUIStore();

  const { data: dashboardData, isLoading } = useQuery({
    queryKey: ['dashboard-stats', activePropertyId],
    queryFn: async () => {
      const resp = await reportsApi.dashboard(activePropertyId ? { propertyId: activePropertyId } : undefined);
      return resp.data;
    },
  });

  const { data: revenueReport } = useQuery({
    queryKey: ['revenue-chart', activePropertyId],
    queryFn: async () => {
      const resp = await reportsApi.revenue({
        period: 'last_6_months',
        groupBy: 'month',
        ...(activePropertyId ? { propertyId: activePropertyId } : {}),
      });
      return resp.data;
    },
  });

  const { data: occupancyReport } = useQuery({
    queryKey: ['dashboard-occupancy', activePropertyId],
    queryFn: async () => {
      const resp = await reportsApi.occupancy(activePropertyId ? { propertyId: activePropertyId } : undefined);
      return resp.data;
    },
  });

  const { data: collectionReport } = useQuery({
    queryKey: ['dashboard-collection', activePropertyId],
    queryFn: async () => {
      const resp = await reportsApi.collectionRate(activePropertyId ? { propertyId: activePropertyId } : undefined);
      return resp.data;
    },
  });

  if (isLoading) {
    return <div className="p-8">Dang tai...</div>;
  }

  const stats = [
    { label: 'Tong so phong', value: dashboardData?.totalRooms || 0, icon: Home, color: 'indigo', href: '/rooms' },
    { label: 'Phong trong', value: dashboardData?.availableRooms || 0, icon: Home, color: 'emerald', href: '/rooms?status=available' },
    { label: 'Dang thue', value: dashboardData?.totalTenants || 0, icon: Users, color: 'violet', href: '/contracts' },
    { label: 'Doanh thu thang', value: formatCurrency(dashboardData?.monthlyRevenue || 0), icon: DollarSign, color: 'amber', href: '/reports' },
  ];

  const revenueData = (revenueReport?.byMonth ?? []).map((item: { month: string; collected: number }) => ({
    month: item.month,
    amount: item.collected,
  }));

  const occupancyRate = Number(occupancyReport?.occupancyRate ?? occupancyReport?.rate ?? 0);
  const collectionRate = Number(collectionReport?.rate ?? collectionReport?.collectionRate ?? 0);

  return (
    <div className="space-y-8 p-4">
      {(dashboardData?.totalRooms ?? 0) > 0 && (
        <div className="flex flex-col gap-3 rounded-[2rem] border border-amber-100 bg-amber-50 p-5 md:flex-row md:items-center md:justify-between">
          <div className="flex items-start gap-3">
            <div className="rounded-2xl bg-amber-100 p-3 text-amber-700">
              <AlertTriangle className="h-5 w-5" />
            </div>
            <div>
              <p className="text-sm font-black text-amber-900">Tong quan van hanh theo nha tro dang chon</p>
              <p className="mt-1 text-sm text-amber-700">
                Lap day {occupancyRate.toFixed(0)}% va ty le thu tien {collectionRate.toFixed(0)}%.
              </p>
            </div>
          </div>
          <Link href="/reports" className="inline-flex items-center gap-2 rounded-2xl bg-amber-600 px-5 py-3 text-sm font-bold text-white hover:bg-amber-700">
            Xem bao cao
            <ArrowRight className="h-4 w-4" />
          </Link>
        </div>
      )}

      <div className="grid grid-cols-1 gap-6 md:grid-cols-4">
        {stats.map((stat, index) => (
          <Link key={index} href={stat.href} className="block">
            <StatCard title={stat.label} value={stat.value} icon={stat.icon} color={stat.color} />
          </Link>
        ))}
      </div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,2fr)_minmax(320px,1fr)]">
        <div className="h-[400px] rounded-[2.5rem] border border-slate-100 bg-white p-8 shadow-sm">
          <div className="mb-6 flex items-center justify-between">
            <div>
              <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Doanh thu 6 thang</p>
              <h2 className="mt-1 text-2xl font-black text-slate-900">Dong tien thu ve</h2>
            </div>
            <BarChart3 className="h-5 w-5 text-indigo-500" />
          </div>
          <ResponsiveContainer width="100%" height="80%">
            <AreaChart data={revenueData}>
              <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
              <XAxis dataKey="month" axisLine={false} tickLine={false} />
              <YAxis hide />
              <Tooltip />
              <Area type="monotone" dataKey="amount" stroke="#6366f1" fill="#6366f1" fillOpacity={0.1} />
            </AreaChart>
          </ResponsiveContainer>
        </div>

        <div className="space-y-6">
          <div className="rounded-[2rem] border border-slate-100 bg-white p-6 shadow-sm">
            <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Thong tin nhanh</p>
            <div className="mt-5 space-y-4">
              <Link href="/invoices" className="flex items-center justify-between rounded-2xl bg-slate-50 px-4 py-3">
                <div className="flex items-center gap-3">
                  <Receipt className="h-4 w-4 text-indigo-500" />
                  <span className="text-sm font-bold text-slate-700">Hoa don can xu ly</span>
                </div>
                <span className="text-sm font-black text-slate-900">{dashboardData?.pendingInvoices ?? 0}</span>
              </Link>
              <Link href="/invoices?status=overdue" className="flex items-center justify-between rounded-2xl bg-rose-50 px-4 py-3">
                <div className="flex items-center gap-3">
                  <FileWarning className="h-4 w-4 text-rose-500" />
                  <span className="text-sm font-bold text-rose-700">Hoa don qua han</span>
                </div>
                <span className="text-sm font-black text-rose-700">{dashboardData?.overdueInvoices ?? 0}</span>
              </Link>
            </div>
          </div>

          <div className="rounded-[2rem] border border-slate-100 bg-white p-6 shadow-sm">
            <p className="text-xs font-black uppercase tracking-[0.2em] text-slate-400">Chi so quan tri</p>
            <div className="mt-5 grid grid-cols-2 gap-4">
              <div className="rounded-2xl bg-indigo-50 p-4">
                <p className="text-xs font-bold text-indigo-400">Lap day</p>
                <p className="mt-2 text-2xl font-black text-indigo-700">{occupancyRate.toFixed(0)}%</p>
              </div>
              <div className="rounded-2xl bg-emerald-50 p-4">
                <p className="text-xs font-bold text-emerald-400">Thu tien</p>
                <p className="mt-2 text-2xl font-black text-emerald-700">{collectionRate.toFixed(0)}%</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
