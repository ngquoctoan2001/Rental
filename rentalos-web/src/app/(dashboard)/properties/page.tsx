'use client';

import { useState } from 'react';
import { 
  Building2, MapPin, Users, Home, Plus, 
  ChevronRight, ArrowUpRight, TrendingUp, MoreVertical,
  Layers, Search, Filter, Edit3, Trash2
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, Cell 
} from 'recharts';
import { useQuery } from '@tanstack/react-query';
import { propertiesApi } from '@/lib/api';

const MOCK_PROPERTIES = [
  { 
    id: '1', 
    name: 'Nhà trọ Blue Moon', 
    address: '123 Cách Mạng Tháng 8, Q10, HCM', 
    totalRooms: 45, 
    occupied: 42, 
    revenue: 150000000,
    image: 'https://images.unsplash.com/photo-1554995207-c18c20360a59?auto=format&fit=crop&w=400&q=80'
  },
  { 
    id: '2', 
    name: 'Căn hộ dịch vụ Sunrise', 
    address: '456 Võ Văn Kiệt, Q1, HCM', 
    totalRooms: 12, 
    occupied: 10, 
    revenue: 240000000,
    image: 'https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=400&q=80'
  },
  { 
    id: '3', 
    name: 'Ký túc xá Joy Hostel', 
    address: '789 Sư Vạn Hạnh, Q10, HCM', 
    totalRooms: 120, 
    occupied: 120, 
    revenue: 85000000,
    image: 'https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?auto=format&fit=crop&w=400&q=80'
  },
];

const MOCK_REVENUE_HISTORY = [
  { month: 'Tháng 10', amount: 120 },
  { month: 'Tháng 11', amount: 145 },
  { month: 'Tháng 12', amount: 130 },
  { month: 'Tháng 01', amount: 160 },
  { month: 'Tháng 02', amount: 140 },
  { month: 'Tháng 03', amount: 150 },
];

export default function PropertiesPage() {
  const { data: properties = [], isLoading } = useQuery({
    queryKey: ['properties'],
    queryFn: async () => {
      const resp = await propertiesApi.list();
      return resp.data;
    }
  });

  const [selectedId, setSelectedId] = useState<string | null>(null);
  
  // Update selectedId when properties data arrives if not set
  const currentSelectedId = selectedId || properties[0]?.id;
  const selected = properties.find(p => p.id === currentSelectedId) || properties[0];

  if (isLoading) {
    return (
      <div className="flex h-[calc(100vh-120px)] items-center justify-center">
        <div className="w-12 h-12 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin" />
      </div>
    );
  }

  if (properties.length === 0) {
    return (
      <div className="flex flex-col h-[calc(100vh-120px)] items-center justify-center bg-white rounded-[2.5rem] border border-dashed border-slate-200">
        <Building2 className="w-16 h-16 text-slate-200 mb-4" />
        <h2 className="text-xl font-bold text-slate-400">Chưa có cơ sở kinh doanh nào</h2>
        <button className="mt-6 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100">
          Thêm cơ sở đầu tiên
        </button>
      </div>
    );
  }

  return (
    <div className="flex h-[calc(100vh-120px)] gap-8 -m-8 p-8 overflow-hidden bg-slate-50/50">
      {/* Sidebar: Property List */}
      <aside className="w-[380px] flex flex-col gap-6">
        <div className="flex items-center justify-between">
           <h1 className="text-2xl font-black text-slate-900 tracking-tight">Cơ sở (Properties)</h1>
           <button className="p-3 bg-indigo-600 text-white rounded-2xl hover:bg-indigo-700 shadow-lg shadow-indigo-100 transition-all">
             <Plus className="w-5 h-5" />
           </button>
        </div>

        <div className="relative">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm theo tên hoặc địa chỉ..."
            className="w-full pl-10 pr-4 py-3.5 bg-white border border-slate-100 rounded-2xl text-xs font-bold shadow-sm outline-none focus:ring-4 focus:ring-indigo-500/10 transition-all"
          />
        </div>

        <div className="flex-1 overflow-y-auto pr-2 space-y-3 custom-scrollbar">
          {properties.map((prop: any) => (
            <button
              key={prop.id}
              onClick={() => setSelectedId(prop.id)}
              className={`w-full group p-4 rounded-[1.5rem] border transition-all text-left relative overflow-hidden ${
                currentSelectedId === prop.id 
                ? 'bg-white border-indigo-200 shadow-lg shadow-indigo-100/50' 
                : 'bg-white/60 border-slate-100 hover:bg-white hover:border-slate-200'
              }`}
            >
              <div className="flex gap-4">
                <div className="w-20 h-20 rounded-2xl overflow-hidden flex-shrink-0 border border-slate-100 shadow-inner">
                  <img src={prop.imageUrl} alt={prop.name} className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" />
                </div>
                <div className="flex-1 min-w-0 py-1">
                  <h3 className={`font-black text-sm truncate ${selectedId === prop.id ? 'text-indigo-600' : 'text-slate-800'}`}>{prop.name}</h3>
                  <div className="flex items-center gap-1 mt-1 text-slate-400">
                    <MapPin className="w-3 h-3 flex-shrink-0" />
                    <span className="text-[10px] font-bold truncate">{prop.address}</span>
                  </div>
                  <div className="flex items-center gap-4 mt-3">
                     <div className="flex items-center gap-1.5">
                       <Users className="w-3 h-3 text-slate-400" />
                       <span className="text-[10px] font-black text-slate-600">{Math.round(((prop.occupied ?? (prop.totalRooms - prop.availableRooms)) / prop.totalRooms) * 100)}%</span>
                     </div>
                     <div className="flex items-center gap-1.5 font-black text-indigo-500 text-[10px]">
                       <TrendingUp className="w-3 h-3" />
                       {(prop.revenue ?? 0).toLocaleString()}đ
                     </div>
                  </div>
                </div>
              </div>
            </button>
          ))}
        </div>
      </aside>

      {/* Main: Property Details */}
      <main className="flex-1 bg-white rounded-[2.5rem] border border-slate-100 shadow-sm overflow-hidden flex flex-col relative">
        <div className="absolute top-6 left-6 z-10">
          <button className="p-3 bg-white/80 backdrop-blur-md rounded-2xl border border-white/50 shadow-sm hover:bg-white transition-all text-slate-600">
            <Edit3 className="w-4 h-4" />
          </button>
        </div>
        <div className="absolute top-6 right-6 z-10 flex gap-2">
          <button className="p-3 bg-white/80 backdrop-blur-md rounded-2xl border border-white/50 shadow-sm hover:bg-white transition-all text-slate-400">
            <MoreVertical className="w-4 h-4" />
          </button>
        </div>

        <div className="h-[250px] w-full relative overflow-hidden">
          <img src={selected.imageUrl} className="w-full h-full object-cover" alt="Banner" />
          <div className="absolute inset-0 bg-gradient-to-t from-white via-white/10 to-transparent" />
        </div>

        <div className="flex-1 overflow-y-auto px-10 pb-10 custom-scrollbar -mt-20 relative z-10">
          <div className="space-y-8">
            <header className="space-y-2">
              <h2 className="text-4xl font-black text-slate-900 tracking-tight">{selected.name}</h2>
              <div className="flex items-center gap-2 text-slate-500 font-bold">
                <MapPin className="w-4 h-4 text-rose-500" />
                {selected.address}
              </div>
            </header>

            {/* Stats Overview */}
            <div className="grid grid-cols-4 gap-4">
              <div className="p-6 bg-slate-50 rounded-3xl border border-slate-100 space-y-2">
                <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng số phòng</p>
                <p className="text-2xl font-black text-slate-900">{selected.totalRooms}</p>
                <div className="flex items-center gap-1.5 mt-2">
                   <div className="flex-1 h-1 bg-slate-200 rounded-full overflow-hidden">
                      <div className="h-full bg-slate-900 w-full" />
                   </div>
                </div>
              </div>
              <div className="p-6 bg-indigo-50 rounded-3xl border border-indigo-100 space-y-2">
                <p className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">Đang ở</p>
                <p className="text-2xl font-black text-indigo-600">{(selected.occupied ?? (selected.totalRooms - selected.availableRooms))}</p>
                 <div className="flex items-center gap-1.5 mt-2">
                   <div className="flex-1 h-1 bg-indigo-200 rounded-full overflow-hidden">
                      <div className="h-full bg-indigo-600" style={{ width: `${((selected.occupied ?? (selected.totalRooms - selected.availableRooms))/selected.totalRooms)*100}%` }} />
                   </div>
                </div>
              </div>
              <div className="p-6 bg-emerald-50 rounded-3xl border border-emerald-100 space-y-2">
                <p className="text-[10px] font-black text-emerald-400 uppercase tracking-widest">Trống</p>
                <p className="text-2xl font-black text-emerald-600">{selected.totalRooms - (selected.occupied ?? (selected.totalRooms - selected.availableRooms))}</p>
              </div>
              <div className="p-6 bg-purple-50 rounded-3xl border border-purple-100 space-y-2">
                <p className="text-[10px] font-black text-purple-400 uppercase tracking-widest">Hiệu suất tháng</p>
                <p className="text-2xl font-black text-purple-600">{Math.round(((selected.occupied ?? (selected.totalRooms - selected.availableRooms))/selected.totalRooms)*100)}%</p>
              </div>
            </div>

            {/* Revenue Chart Section */}
            <div className="bg-slate-50/50 rounded-[2rem] border border-slate-100 p-8 space-y-6">
               <div className="flex items-center justify-between">
                  <h3 className="text-xl font-black text-slate-900">Doanh thu 6 tháng qua</h3>
                  <div className="flex items-center gap-2 text-emerald-600 text-xs font-black bg-emerald-100 px-3 py-1.5 rounded-full">
                    <TrendingUp className="w-3.5 h-3.5" /> +15% so với năm ngoái
                  </div>
               </div>
               <div className="h-[250px] w-full">
                  <ResponsiveContainer width="100%" height="100%">
                    <BarChart data={MOCK_REVENUE_HISTORY}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                      <XAxis dataKey="month" axisLine={false} tickLine={false} tick={{ fontSize: 10, fontWeight: 700, fill: '#94a3b8' }} />
                      <YAxis hide />
                      <Tooltip cursor={{ fill: '#f8fafc' }} contentStyle={{ borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }} />
                      <Bar dataKey="amount" fill="#4f46e5" radius={[10, 10, 0, 0]} barSize={40}>
                         {MOCK_REVENUE_HISTORY.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={index === 5 ? '#4f46e5' : '#cbd5e1'} />
                        ))}
                      </Bar>
                    </BarChart>
                  </ResponsiveContainer>
               </div>
            </div>

            {/* Sub-modules Shortcut */}
            <div className="grid grid-cols-2 gap-4">
               <button className="flex items-center gap-4 p-6 bg-white border border-slate-100 rounded-3xl hover:border-indigo-200 transition-all group shadow-sm">
                  <div className="w-12 h-12 rounded-2xl bg-indigo-50 flex items-center justify-center text-indigo-600 group-hover:bg-indigo-600 group-hover:text-white transition-colors">
                    <Home className="w-6 h-6" />
                  </div>
                  <div className="text-left">
                    <p className="text-sm font-black text-slate-800">Quản lý Mặt bằng</p>
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-tight">DS Phòng, Tầng, Thiết bị...</p>
                  </div>
                  <ChevronRight className="w-5 h-5 text-slate-200 ml-auto" />
               </button>
               <button className="flex items-center gap-4 p-6 bg-white border border-slate-100 rounded-3xl hover:border-purple-200 transition-all group shadow-sm">
                  <div className="w-12 h-12 rounded-2xl bg-purple-50 flex items-center justify-center text-purple-600 group-hover:bg-purple-600 group-hover:text-white transition-colors">
                    <Layers className="w-6 h-6" />
                  </div>
                  <div className="text-left">
                    <p className="text-sm font-black text-slate-800">Quản lý Hợp đồng</p>
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-tight">Ký mới, Gia hạn, Thanh lý</p>
                  </div>
                  <ChevronRight className="w-5 h-5 text-slate-200 ml-auto" />
               </button>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
