'use client';

import { useState, useMemo } from 'react';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, Cell, Legend, PieChart, Pie
} from 'recharts';
import { 
  Download, Filter, Calendar, TrendingUp, TrendingDown, 
  Wallet, Receipt, CreditCard, Landmark, Building2,
  FileText, FileJson, ArrowUpRight, ArrowDownRight, Loader2
} from 'lucide-react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { reportsApi } from '@/lib/api';
import { StatCard } from '@/components/shared';
import { DataTable } from '@/components/shared/DataTable';
import { format } from 'date-fns';

const METHOD_COLORS: Record<string, string> = {
  cash: '#4f46e5',
  bank_transfer: '#10b981',
  momo: '#ec4899',
  vnpay: '#f59e0b',
  zalo: '#06b6d4',
};
const METHOD_LABELS: Record<string, string> = {
  cash: 'Tiền mặt',
  bank_transfer: 'Chuyển khoản',
  momo: 'MoMo',
  vnpay: 'VNPay',
  zalo: 'ZaloPay',
};

export default function ReportsPage() {
  const [period, setPeriod] = useState('this_month');

  // API Data
  const { data: dashboard, isLoading: dashLoading } = useQuery({
    queryKey: ['reports-dashboard'],
    queryFn: () => reportsApi.dashboard().then(r => r.data),
  });

  const { data: revenue, isLoading: revLoading } = useQuery({
    queryKey: ['reports-revenue', period],
    queryFn: () => reportsApi.revenue({ period }).then(r => r.data),
  });

  const { data: overdueTrend } = useQuery({
    queryKey: ['reports-overdue-trend'],
    queryFn: () => reportsApi.overdueTrend(6).then(r => r.data),
  });

  const { data: topRooms } = useQuery({
    queryKey: ['reports-top-rooms'],
    queryFn: () => reportsApi.topRooms(10).then(r => r.data),
  });

  const currentMonth = format(new Date(), 'yyyy-MM');
  const { data: monthlySummary } = useQuery({
    queryKey: ['reports-monthly', currentMonth],
    queryFn: () => reportsApi.monthlySummary(currentMonth).then(r => r.data),
  });

  const exportMutation = useMutation({
    mutationFn: () => reportsApi.export('revenue', { period }),
    onSuccess: (resp) => {
      const url = window.URL.createObjectURL(new Blob([resp.data]));
      const a = document.createElement('a');
      a.href = url;
      a.download = `report-${period}-${format(new Date(), 'yyyy-MM-dd')}.xlsx`;
      a.click();
      window.URL.revokeObjectURL(url);
    },
  });

  // Chart data derived from API
  const revenueChartData = useMemo(() => {
    if (!revenue?.byMonth?.length) return [];
    return revenue.byMonth.map((m: any) => ({
      name: m.month?.slice(5) ?? m.month, // "2026-03" → "03"
      amount: m.collected ?? 0,
    }));
  }, [revenue]);

  const methodChartData = useMemo(() => {
    if (!revenue?.byMethod?.length) return [];
    return revenue.byMethod.map((m: any) => ({
      name: METHOD_LABELS[m.method] ?? m.method,
      value: m.amount ?? 0,
      color: METHOD_COLORS[m.method] ?? '#94a3b8',
    }));
  }, [revenue]);

  const propertyTableData = useMemo(() => {
    if (!revenue?.byProperty?.length) return [];
    return revenue.byProperty.map((p: any, i: number) => ({
      id: String(i),
      name: p.propertyName,
      income: p.collected ?? 0,
      rate: p.collectionRate ?? 0,
      status: (p.collectionRate ?? 0) >= 90 ? 'Hoàn hảo' : (p.collectionRate ?? 0) >= 70 ? 'Ổn định' : 'Cần chú ý',
    }));
  }, [revenue]);

  // KPI values
  const totalRevenue = revenue?.summary?.totalRevenue ?? dashboard?.revenue?.thisMonth ?? 0;
  const collected = revenue ? (totalRevenue * (revenue.summary?.collectionRate ?? 100) / 100) : 0;
  const outstanding = totalRevenue - collected;
  const collectionRate = revenue?.summary?.collectionRate ?? 0;
  const revenueChange = dashboard?.revenue?.changePercent ?? 0;

  const columns = [
    {
      key: 'name',
      label: 'Nhà trọ',
      render: (val: string) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-slate-100 flex items-center justify-center text-slate-500">
            <Building2 className="w-5 h-5" />
          </div>
          <span className="font-bold text-slate-700">{val}</span>
        </div>
      )
    },
    {
      key: 'income',
      label: 'Doanh thu',
      sortable: true,
      render: (val: number) => (
        <span className="font-black text-slate-900">{val.toLocaleString()}đ</span>
      )
    },
    {
      key: 'rate',
      label: 'Tỷ lệ thu hồi',
      render: (val: number) => (
        <div className="flex items-center gap-3">
          <div className="flex-1 h-1.5 bg-slate-100 rounded-full overflow-hidden min-w-[100px]">
            <div 
              className={`h-full rounded-full ${val >= 90 ? 'bg-emerald-500' : 'bg-amber-500'}`} 
              style={{ width: `${val}%` }} 
            />
          </div>
          <span className="text-xs font-black text-slate-600">{val}%</span>
        </div>
      )
    },
     {
      key: 'status',
      label: 'Đánh giá',
      render: (val: string) => (
        <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${
          val === 'Hoàn hảo' ? 'bg-emerald-100 text-emerald-700' : 
          val === 'Ổn định' ? 'bg-indigo-100 text-indigo-700' : 
          'bg-rose-100 text-rose-700'
        }`}>
          {val}
        </span>
      )
    }
  ];

  return (
    <div className="space-y-8 pb-20">
      {/* Header & Tabs */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div className="space-y-1">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">Trung tâm Phân tích</h1>
          <p className="text-slate-500 font-medium italic">Thống kê dữ liệu tài chính & hiệu suất vận hành.</p>
        </div>
        
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 p-1 bg-slate-100 rounded-[1.25rem] border border-slate-200 shadow-inner">
            {[
              { id: 'this_month', label: 'Tháng này' },
              { id: 'last_month', label: 'Tháng trước' },
              { id: 'this_quarter', label: 'Quý này' },
              { id: 'this_year', label: 'Năm nay' },
            ].map(tab => (
              <button
                key={tab.id}
                onClick={() => setPeriod(tab.id)}
                className={`px-5 py-2 rounded-xl text-xs font-black transition-all ${
                  period === tab.id 
                  ? 'bg-white text-indigo-600 shadow-sm' 
                  : 'text-slate-500 hover:text-slate-700'
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>
          <button
            onClick={() => exportMutation.mutate()}
            disabled={exportMutation.isPending}
            className="flex items-center gap-2 px-5 py-3 bg-white border border-slate-200 rounded-2xl font-bold text-slate-600 hover:bg-slate-50 transition-all shadow-sm disabled:opacity-60"
          >
            {exportMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
            Xuất Excel
          </button>
        </div>
      </div>

      {/* KPI Section */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8 space-y-4 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-indigo-50/50 rounded-full -mr-16 -mt-16 group-hover:scale-110 transition-transform duration-500" />
          <div className="flex items-start justify-between relative">
            <div className="w-14 h-14 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600">
              <Wallet className="w-8 h-8" />
            </div>
            {revenueChange !== 0 && (
              <div className={`flex items-center gap-1 font-black text-xs px-3 py-1.5 rounded-full ${revenueChange >= 0 ? 'text-emerald-600 bg-emerald-50' : 'text-rose-600 bg-rose-50'}`}>
                {revenueChange >= 0 ? <ArrowUpRight className="w-3 h-3" /> : <ArrowDownRight className="w-3 h-3" />}
                {Math.abs(revenueChange)}%
              </div>
            )}
          </div>
          <div>
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Tổng doanh thu</p>
            {revLoading ? <div className="h-10 bg-slate-100 rounded animate-pulse mt-1" /> : (
              <h3 className="text-4xl font-black text-slate-900 mt-1">{totalRevenue.toLocaleString()}đ</h3>
            )}
          </div>
        </div>

        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8 space-y-4 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-emerald-50/50 rounded-full -mr-16 -mt-16 group-hover:scale-110 transition-transform duration-500" />
          <div className="flex items-start justify-between relative">
            <div className="w-14 h-14 rounded-2xl bg-emerald-50 flex items-center justify-center text-emerald-600">
              <Receipt className="w-8 h-8" />
            </div>
            <div className="flex items-center gap-1 text-indigo-600 font-black text-xs bg-indigo-50 px-3 py-1.5 rounded-full">
              {collectionRate.toFixed(1)}% thu hồi
            </div>
          </div>
          <div>
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Số tiền đã thu</p>
            {revLoading ? <div className="h-10 bg-slate-100 rounded animate-pulse mt-1" /> : (
              <h3 className="text-4xl font-black text-slate-900 mt-1">{collected.toLocaleString()}đ</h3>
            )}
          </div>
        </div>

        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8 space-y-4 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-rose-50/50 rounded-full -mr-16 -mt-16 group-hover:scale-110 transition-transform duration-500" />
          <div className="flex items-start justify-between relative">
            <div className="w-14 h-14 rounded-2xl bg-rose-50 flex items-center justify-center text-rose-600">
              <FileText className="w-8 h-8" />
            </div>
            <div className="flex items-center gap-1 text-rose-600 font-black text-xs bg-rose-50 px-3 py-1.5 rounded-full">
              <ArrowDownRight className="w-3 h-3" />
              {totalRevenue > 0 ? (100 - collectionRate).toFixed(1) : 0}%
            </div>
          </div>
          <div>
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Công nợ (Unpaid)</p>
            {revLoading ? <div className="h-10 bg-slate-100 rounded animate-pulse mt-1" /> : (
              <h3 className="text-4xl font-black text-slate-900 mt-1 text-rose-600">{outstanding.toLocaleString()}đ</h3>
            )}
          </div>
        </div>
      </div>

      {/* Main Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8">
          <div className="flex items-center justify-between mb-8">
            <h3 className="text-xl font-black text-slate-900">Doanh thu theo thời gian</h3>
          </div>
          <div className="h-[350px] w-full">
            {revLoading ? (
              <div className="h-full bg-slate-50 rounded-2xl animate-pulse" />
            ) : revenueChartData.length === 0 ? (
              <div className="h-full flex items-center justify-center text-slate-400 font-bold">Chưa có dữ liệu</div>
            ) : (
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={revenueChartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                <defs>
                  <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#4f46e5" stopOpacity={0.8}/>
                    <stop offset="95%" stopColor="#818cf8" stopOpacity={0.2}/>
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f1f5f9" />
                <XAxis dataKey="name" axisLine={false} tickLine={false} tick={{ fontSize: 10, fontWeight: 700, fill: '#94a3b8' }} dy={10} />
                <YAxis axisLine={false} tickLine={false} tick={{ fontSize: 10, fontWeight: 700, fill: '#94a3b8' }} tickFormatter={(v) => `${(v/1000000)}M`} />
                <Tooltip 
                  cursor={{ fill: '#f8fafc', radius: 12 }}
                  contentStyle={{ border: 'none', borderRadius: '16px', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)', fontWeight: 800 }}
                  formatter={(v: any) => [`${v.toLocaleString()}đ`, 'Doanh thu']}
                />
                <Bar dataKey="amount" fill="url(#revenueGradient)" radius={[10, 10, 0, 0]} barSize={40} />
              </BarChart>
            </ResponsiveContainer>
            )}
          </div>
        </div>

        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8">
          <h3 className="text-xl font-black text-slate-900 mb-8">Phương thức thanh toán</h3>
          <div className="h-[300px] relative">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={methodChartData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={100}
                  paddingAngle={8}
                  dataKey="value"
                >
                  {methodChartData.map((entry: any, index: number) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
            <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
              <div className="text-center">
                <p className="text-[10px] font-black text-slate-400">TỔNG</p>
                <p className="text-lg font-black text-slate-900 leading-none">
                  {Math.round(methodChartData.reduce((s: number, m: any) => s + m.value, 0) / 1000000)}M
                </p>
              </div>
            </div>
          </div>
          <div className="space-y-3 mt-4">
            {methodChartData.map((item: any, idx: number) => {
              const total = methodChartData.reduce((s: number, m: any) => s + m.value, 0);
              return (
                <div key={idx} className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full" style={{ backgroundColor: item.color }} />
                    <span className="text-xs font-bold text-slate-600">{item.name}</span>
                  </div>
                  <span className="text-xs font-black text-slate-900">{total > 0 ? Math.round((item.value / total) * 100) : 0}%</span>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Property Revenue Table */}
      <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm overflow-hidden">
        <div className="p-8 border-b border-slate-50 flex items-center justify-between">
          <div>
            <h3 className="text-xl font-black text-slate-900 leading-tight">Doanh thu chi tiết theo cơ sở</h3>
            <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-wider">Dữ liệu tổng hợp từ các bản ghi hóa đơn</p>
          </div>
          <button 
            onClick={() => exportMutation.mutate()}
            disabled={exportMutation.isPending}
            className="flex items-center gap-2 px-5 py-3 bg-indigo-600 rounded-xl text-xs font-black text-white hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60"
          >
            {exportMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Download className="w-4 h-4" />}
            Xuất Excel
          </button>
        </div>
        <div className="p-4">
          <DataTable 
            columns={columns} 
            data={propertyTableData}
            isLoading={revLoading}
          />
        </div>
      </div>

      {/* Overdue Trend */}
      <div className="bg-white rounded-3xl border border-slate-100 shadow-sm overflow-hidden">
        <div className="px-8 py-6 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h3 className="text-xl font-black text-slate-900">Xu hướng quá hạn</h3>
            <p className="text-sm text-slate-500 mt-1">Số hóa đơn và tổng tiền quá hạn theo tháng</p>
          </div>
          <TrendingDown className="w-6 h-6 text-rose-400" />
        </div>
        <div className="p-8">
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={overdueTrend ?? []} barSize={28}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="month" tick={{ fontSize: 12, fill: '#64748b' }} />
              <YAxis yAxisId="left" orientation="left" tick={{ fontSize: 12, fill: '#64748b' }} />
              <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12, fill: '#94a3b8' }} />
              <Tooltip formatter={(value: any, name: string) => [
                name === 'overdueAmount' ? `${Number(value).toLocaleString('vi-VN')}đ` : value,
                name === 'overdueAmount' ? 'Số tiền' : 'Số hóa đơn',
              ]} />
              <Bar yAxisId="left" dataKey="overdueCount" fill="#fca5a5" radius={[6,6,0,0]} name="overdueCount" />
              <Bar yAxisId="right" dataKey="overdueAmount" fill="#ef4444" radius={[6,6,0,0]} name="overdueAmount" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>

      {/* Top Rooms */}
      <div className="bg-white rounded-3xl border border-slate-100 shadow-sm overflow-hidden">
        <div className="px-8 py-6 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h3 className="text-xl font-black text-slate-900">Phòng doanh thu cao nhất</h3>
            <p className="text-sm text-slate-500 mt-1">Top 10 phòng có doanh thu tốt nhất</p>
          </div>
          <TrendingUp className="w-6 h-6 text-emerald-400" />
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50">
                <th className="px-6 py-4 text-left font-bold text-slate-500">#</th>
                <th className="px-6 py-4 text-left font-bold text-slate-500">Phòng</th>
                <th className="px-6 py-4 text-left font-bold text-slate-500">Nhà trọ</th>
                <th className="px-6 py-4 text-right font-bold text-slate-500">Tháng lấp đầy</th>
                <th className="px-6 py-4 text-right font-bold text-slate-500">Doanh thu</th>
              </tr>
            </thead>
            <tbody>
              {(topRooms ?? []).map((room: any, i: number) => (
                <tr key={i} className="border-b border-slate-50 hover:bg-slate-50 transition-colors">
                  <td className="px-6 py-4 font-bold text-slate-400">{i + 1}</td>
                  <td className="px-6 py-4 font-bold text-slate-800">{room.roomNumber}</td>
                  <td className="px-6 py-4 text-slate-500">{room.propertyName}</td>
                  <td className="px-6 py-4 text-right text-slate-600">{room.occupancyMonths} tháng</td>
                  <td className="px-6 py-4 text-right font-bold text-emerald-600">{Number(room.totalRevenue).toLocaleString('vi-VN')}đ</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Monthly Summary */}
      {monthlySummary && (
        <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-8">
          <h3 className="text-xl font-black text-slate-900 mb-6">Tóm tắt tháng {currentMonth}</h3>
          <div className="grid grid-cols-2 md:grid-cols-3 gap-6">
            <div className="bg-indigo-50 rounded-2xl p-5">
              <p className="text-xs font-bold text-indigo-400 uppercase tracking-widest">Tổng lập hoá đơn</p>
              <p className="text-2xl font-black text-indigo-700 mt-2">{Number(monthlySummary.totalInvoiced).toLocaleString('vi-VN')}đ</p>
            </div>
            <div className="bg-emerald-50 rounded-2xl p-5">
              <p className="text-xs font-bold text-emerald-400 uppercase tracking-widest">Đã thu</p>
              <p className="text-2xl font-black text-emerald-700 mt-2">{Number(monthlySummary.totalCollected).toLocaleString('vi-VN')}đ</p>
            </div>
            <div className="bg-rose-50 rounded-2xl p-5">
              <p className="text-xs font-bold text-rose-400 uppercase tracking-widest">Còn tồn</p>
              <p className="text-2xl font-black text-rose-700 mt-2">{Number(monthlySummary.outstandingAmount).toLocaleString('vi-VN')}đ</p>
            </div>
            <div className="bg-sky-50 rounded-2xl p-5">
              <p className="text-xs font-bold text-sky-400 uppercase tracking-widest">HĐ mới</p>
              <p className="text-2xl font-black text-sky-700 mt-2">{monthlySummary.newContracts}</p>
            </div>
            <div className="bg-amber-50 rounded-2xl p-5">
              <p className="text-xs font-bold text-amber-400 uppercase tracking-widest">HĐ thanh lý</p>
              <p className="text-2xl font-black text-amber-700 mt-2">{monthlySummary.terminatedContracts}</p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
