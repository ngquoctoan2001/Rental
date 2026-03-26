'use client';

import { useState } from 'react';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, Cell, Legend, PieChart, Pie
} from 'recharts';
import { 
  Download, Filter, Calendar, TrendingUp, TrendingDown, 
  Wallet, Receipt, CreditCard, Landmark, Building2,
  FileText, FileJson, ArrowUpRight, ArrowDownRight
} from 'lucide-react';
import { useQuery } from '@tanstack/react-query';
import { reportsApi } from '@/lib/api';
import { StatCard } from '@/components/shared';
import { DataTable } from '@/components/shared/DataTable';

const MOCK_REVENUE_DATA = [
  { name: '01/03', amount: 45000000 },
  { name: '05/03', amount: 120000000 },
  { name: '10/03', amount: 85000000 },
  { name: '15/03', amount: 65000000 },
  { name: '20/03', amount: 40000000 },
  { name: '25/03', amount: 15000000 },
  { name: '30/03', amount: 30000000 },
];

const MOCK_METHOD_DATA = [
  { name: 'Tiền mặt', value: 85000000, color: '#4f46e5' },
  { name: 'Chuyển khoản', value: 240000000, color: '#10b981' },
  { name: 'MoMo/VNPay', value: 120000000, color: '#ec4899' },
];

const MOCK_PROPERTY_DATA = [
  { id: '1', name: 'Nhà trọ Blue Moon', income: 150000000, rate: 95, status: 'Ổn định' },
  { id: '2', name: 'Căn hộ dịch vụ Sunrise', income: 240000000, rate: 82, status: 'Cần chú ý' },
  { id: '3', name: 'Ký túc xá Joy', income: 85000000, rate: 100, status: 'Hoàn hảo' },
];

export default function ReportsPage() {
  const [period, setPeriod] = useState('month');

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
        
        <div className="flex items-center gap-2 p-1 bg-slate-100 rounded-[1.25rem] border border-slate-200 shadow-inner">
          {[
            { id: 'month', label: 'Tháng này' },
            { id: 'last_month', label: 'Tháng trước' },
            { id: 'quarter', label: 'Quý này' },
            { id: 'year', label: 'Năm nay' },
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
          <button className="p-2 text-slate-400 hover:text-slate-600">
            <Calendar className="w-4 h-4" />
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
            <div className="flex items-center gap-1 text-emerald-600 font-black text-xs bg-emerald-50 px-3 py-1.5 rounded-full">
              <ArrowUpRight className="w-3 h-3" />
              12.5%
            </div>
          </div>
          <div>
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Tổng doanh thu dự kiến</p>
            <h3 className="text-4xl font-black text-slate-900 mt-1">450.000.000đ</h3>
          </div>
        </div>

        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8 space-y-4 relative overflow-hidden group">
          <div className="absolute top-0 right-0 w-32 h-32 bg-emerald-50/50 rounded-full -mr-16 -mt-16 group-hover:scale-110 transition-transform duration-500" />
          <div className="flex items-start justify-between relative">
            <div className="w-14 h-14 rounded-2xl bg-emerald-50 flex items-center justify-center text-emerald-600">
              <Receipt className="w-8 h-8" />
            </div>
            <div className="flex items-center gap-1 text-indigo-600 font-black text-xs bg-indigo-50 px-3 py-1.5 rounded-full">
              88.5% thu hồi
            </div>
          </div>
          <div>
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Số tiền đã thu</p>
            <h3 className="text-4xl font-black text-slate-900 mt-1">398.240.000đ</h3>
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
              3.2%
            </div>
          </div>
          <div>
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Công nợ (Unpaid)</p>
            <h3 className="text-4xl font-black text-slate-900 mt-1 text-rose-600">51.760.000đ</h3>
          </div>
        </div>
      </div>

      {/* Main Charts */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8">
          <div className="flex items-center justify-between mb-8">
            <h3 className="text-xl font-black text-slate-900">Doanh thu theo thời gian</h3>
            <button className="flex items-center gap-2 text-xs font-bold text-slate-500 hover:text-indigo-600 transition-colors">
              <Download className="w-4 h-4" /> Xuất ảnh
            </button>
          </div>
          <div className="h-[350px] w-full">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={MOCK_REVENUE_DATA} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
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
          </div>
        </div>

        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8">
          <h3 className="text-xl font-black text-slate-900 mb-8">Phương thức thanh toán</h3>
          <div className="h-[300px] relative">
            <ResponsiveContainer width="100%" height="100%">
              <PieChart>
                <Pie
                  data={MOCK_METHOD_DATA}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={100}
                  paddingAngle={8}
                  dataKey="value"
                >
                  {MOCK_METHOD_DATA.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip />
              </PieChart>
            </ResponsiveContainer>
            <div className="absolute inset-0 flex items-center justify-center pointer-events-none">
              <div className="text-center">
                <p className="text-[10px] font-black text-slate-400">TỔNG</p>
                <p className="text-lg font-black text-slate-900 leading-none">445M</p>
              </div>
            </div>
          </div>
          <div className="space-y-3 mt-4">
            {MOCK_METHOD_DATA.map((item, idx) => (
              <div key={idx} className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <div className="w-3 h-3 rounded-full" style={{ backgroundColor: item.color }} />
                  <span className="text-xs font-bold text-slate-600">{item.name}</span>
                </div>
                <span className="text-xs font-black text-slate-900">{Math.round((item.value / 445000000) * 100)}%</span>
              </div>
            ))}
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
          <div className="flex gap-3">
            <button className="flex items-center gap-2 px-5 py-3 bg-white border border-slate-100 rounded-xl text-xs font-black text-slate-700 hover:bg-slate-50 transition-all">
              <FileText className="w-4 h-4 text-rose-500" /> Xuất PDF
            </button>
            <button className="flex items-center gap-2 px-5 py-3 bg-indigo-600 rounded-xl text-xs font-black text-white hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100">
              <Download className="w-4 h-4" /> Xuất Excel
            </button>
          </div>
        </div>
        <div className="p-4">
          <DataTable 
            columns={columns} 
            data={MOCK_PROPERTY_DATA}
          />
        </div>
      </div>
    </div>
  );
}
