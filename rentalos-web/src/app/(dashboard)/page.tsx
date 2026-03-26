'use client';

import { useQuery } from '@tanstack/react-query';
import { 
  Users, Home, DollarSign, 
} from 'lucide-react';
import { 
  XAxis, YAxis, 
  CartesianGrid, Tooltip, ResponsiveContainer,
  AreaChart, Area
} from 'recharts';
import { reportsApi } from '@/lib/api/reports';
import { formatCurrency } from '@/lib/utils';
import { StatCard } from '@/components/shared';

export default function DashboardPage() {
  const { data: dashboardData, isLoading } = useQuery({
    queryKey: ['dashboard-stats'],
    queryFn: async () => {
      const resp = await reportsApi.dashboard();
      return resp.data;
    },
  });

  if (isLoading) {
    return <div className="p-8">Đang tải...</div>;
  }

  const stats = [
    { label: 'Tổng số phòng', value: dashboardData?.totalRooms || 0, icon: Home, color: 'indigo' },
    { label: 'Phòng trống', value: dashboardData?.availableRooms || 0, icon: Home, color: 'emerald' },
    { label: 'Khách thuê', value: dashboardData?.totalTenants || 0, icon: Users, color: 'violet' },
    { label: 'Doanh thu tháng', value: formatCurrency(dashboardData?.monthlyRevenue || 0), icon: DollarSign, color: 'amber' },
  ];

  const revenueData = [
    { month: 'T10', amount: 120 },
    { month: 'T11', amount: 145 },
    { month: 'T12', amount: 180 },
    { month: 'T1', amount: 190 },
    { month: 'T2', amount: 210 },
    { month: 'T3', amount: 205 },
  ];

  return (
    <div className="space-y-8 p-4">
      <h1 className="text-3xl font-black">Dashboard</h1>
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {stats.map((stat, i) => (
          <StatCard 
            key={i}
            title={stat.label}
            value={stat.value}
            icon={stat.icon}
            color={stat.color}
          />
        ))}
      </div>
      <div className="bg-white p-8 rounded-[2.5rem] border border-slate-100 shadow-sm h-[400px]">
        <ResponsiveContainer width="100%" height="100%">
          <AreaChart data={revenueData}>
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
            <XAxis dataKey="month" axisLine={false} tickLine={false} />
            <YAxis hide />
            <Tooltip />
            <Area type="monotone" dataKey="amount" stroke="#6366f1" fill="#6366f1" fillOpacity={0.1} />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
