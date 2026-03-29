'use client';

import { useState } from 'react';
import { 
  Building2, MapPin, Users, Home, Plus, 
  ChevronRight, TrendingUp, MoreVertical,
  Layers, Search, Edit3, Save, Trash2, Loader2
} from 'lucide-react';
import { motion } from 'framer-motion';
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, Cell 
} from 'recharts';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { propertiesApi, reportsApi } from '@/lib/api';
import { Property } from '@/types';
import { Modal } from '@/components/shared/Modal';
import { usePlanLimit } from '@/lib/hooks/usePlanLimit';

export default function PropertiesPage() {
  const queryClient = useQueryClient();
  const { checkAccess } = usePlanLimit();
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState({ name: '', address: '', totalFloors: 1, description: '' });

  const { data: properties = [], isLoading } = useQuery<Property[]>({
    queryKey: ['properties'],
    queryFn: async () => {
      const resp = await propertiesApi.list();
      const body = resp.data as Property[] | { items: Property[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const [selectedId, setSelectedId] = useState<string | null>(null);
  
  const createMutation = useMutation({
    mutationFn: (data: any) => propertiesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      setIsCreateOpen(false);
      setCreateForm({ name: '', address: '', totalFloors: 1, description: '' });
    },
    onError: (error: any) => {
      alert(error?.message || 'Không thể tạo cơ sở mới.');
    },
  });

  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editForm, setEditForm] = useState({ name: '', address: '', totalFloors: 1, description: '' });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => propertiesApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      setIsEditOpen(false);
    },
    onError: (error: any) => {
      alert(error?.message || 'Không thể cập nhật cơ sở.');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => propertiesApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['properties'] });
      setSelectedId(null);
    },
  });

  const propertyLimit = Number(checkAccess('properties')) || 0;
  const canCreateProperty = propertyLimit === 0 ? false : properties.length < propertyLimit;

  // Update selectedId when properties data arrives if not set
  const currentSelectedId = selectedId || properties[0]?.id;
  const selected = properties.find(p => p.id === currentSelectedId) || properties[0];
  const selectedTotalRooms = selected?.roomSummary?.total ?? selected?.totalRooms ?? 0;
  const selectedAvailableRooms = selected?.roomSummary?.available ?? selected?.availableRooms ?? 0;
  const selectedOccupiedRooms = selected?.roomSummary?.rented ?? selected?.occupied ?? Math.max(selectedTotalRooms - selectedAvailableRooms, 0);
  const selectedOccupancyRate = selectedTotalRooms > 0 ? Math.round((selectedOccupiedRooms / selectedTotalRooms) * 100) : 0;

  const { data: revenueData } = useQuery({
    queryKey: ['property-revenue', currentSelectedId],
    queryFn: () => reportsApi.revenue({ period: 'this_year', propertyId: currentSelectedId }).then(r => r.data),
    enabled: !!currentSelectedId,
  });

  const revenueChartData = (revenueData as any)?.byMonth?.map((m: any) => ({
    month: m.month?.slice(5) ?? m.month,
    amount: Math.round((m.collected ?? 0) / 1_000_000),
  })) ?? [];

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
        <button
          onClick={() => {
            if (!canCreateProperty) {
              alert('Gói hiện tại đã đạt giới hạn số cơ sở. Vui lòng nâng cấp gói để thêm cơ sở mới.');
              return;
            }
            setIsCreateOpen(true);
          }}
          className="mt-6 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100">
          Thêm cơ sở đầu tiên
        </button>

        <Modal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Thêm cơ sở mới" size="md">
          <form className="space-y-5" onSubmit={(e) => { e.preventDefault(); createMutation.mutate(createForm); }}>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Tên cơ sở *</label>
              <input required value={createForm.name} onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))} placeholder="VD: Nhà trọ Hoàng Anh" className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none" />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Địa chỉ *</label>
              <input required value={createForm.address} onChange={e => setCreateForm(f => ({ ...f, address: e.target.value }))} placeholder="VD: 123 Đường ABC, Q.1, TP.HCM" className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none" />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Số tầng</label>
              <input type="number" min={1} value={createForm.totalFloors} onChange={e => setCreateForm(f => ({ ...f, totalFloors: Number(e.target.value) }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none" />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Mô tả</label>
              <textarea rows={3} value={createForm.description} onChange={e => setCreateForm(f => ({ ...f, description: e.target.value }))} placeholder="Thêm mô tả về cơ sở..." className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none resize-none" />
            </div>
            <div className="flex gap-3 pt-2">
              <button type="button" onClick={() => setIsCreateOpen(false)} className="flex-1 py-3.5 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors">Hủy</button>
              <button type="submit" disabled={createMutation.isPending} className="flex-[2] py-3.5 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60 flex items-center justify-center gap-2">
                {createMutation.isPending ? <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" /> : <Save className="w-5 h-5" />}
                Tạo cơ sở
              </button>
            </div>
          </form>
        </Modal>
      </div>
    );
  }

  return (
    <div className="flex h-[calc(100vh-120px)] gap-8 -m-8 p-8 overflow-hidden bg-slate-50/50">
      {/* Sidebar: Property List */}
      <aside className="w-[380px] flex flex-col gap-6">
        <div className="flex items-center justify-between">
           <h1 className="text-2xl font-black text-slate-900 tracking-tight">Cơ sở (Properties)</h1>
           <button
             onClick={() => {
               if (!canCreateProperty) {
                 alert('Gói hiện tại đã đạt giới hạn số cơ sở. Vui lòng nâng cấp gói để thêm cơ sở mới.');
                 return;
               }
               setIsCreateOpen(true);
             }}
             className="p-3 bg-indigo-600 text-white rounded-2xl hover:bg-indigo-700 shadow-lg shadow-indigo-100 transition-all">
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
                <div className="w-20 h-20 rounded-2xl overflow-hidden flex-shrink-0 border border-slate-100 shadow-inner bg-indigo-50 flex items-center justify-center">
                  {prop.imageUrl
                    ? <img src={prop.imageUrl} alt={prop.name} className="w-full h-full object-cover group-hover:scale-110 transition-transform duration-500" />
                    : <Building2 className="w-8 h-8 text-indigo-300" />}
                </div>
                <div className="flex-1 min-w-0 py-1">
                  <h3 className={`font-black text-sm truncate ${selectedId === prop.id ? 'text-indigo-600' : 'text-slate-800'}`}>{prop.name}</h3>
                  <div className="flex items-center gap-1 mt-1 text-slate-400">
                    <MapPin className="w-3 h-3 flex-shrink-0" />
                    <span className="text-[10px] font-bold truncate">{prop.address}</span>
                  </div>
                  <div className="flex items-center gap-4 mt-3">
                    {(() => {
                      const totalRooms = prop.roomSummary?.total ?? prop.totalRooms ?? 0;
                      const availableRooms = prop.roomSummary?.available ?? prop.availableRooms ?? 0;
                      const occupiedRooms = prop.roomSummary?.rented ?? prop.occupied ?? Math.max(totalRooms - availableRooms, 0);
                      const occupancy = totalRooms > 0 ? Math.round((occupiedRooms / totalRooms) * 100) : 0;

                      return (
                        <>
                     <div className="flex items-center gap-1.5">
                       <Users className="w-3 h-3 text-slate-400" />
                       <span className="text-[10px] font-black text-slate-600">{occupancy}%</span>
                     </div>
                     <div className="flex items-center gap-1.5 font-black text-indigo-500 text-[10px]">
                       <TrendingUp className="w-3 h-3" />
                       {(prop.revenue ?? 0).toLocaleString()}đ
                     </div>
                        </>
                      );
                    })()}
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
          <button
            onClick={() => { if (selected) { setEditForm({ name: selected.name, address: selected.address, totalFloors: (selected as any).totalFloors ?? 1, description: (selected as any).description ?? '' }); setIsEditOpen(true); } }}
            className="p-3 bg-white/80 backdrop-blur-md rounded-2xl border border-white/50 shadow-sm hover:bg-white transition-all text-slate-600"
            title="Chỉnh sửa thông tin"
          >
            <Edit3 className="w-4 h-4" />
          </button>
        </div>
        <div className="absolute top-6 right-6 z-10 flex gap-2">
          <button
            onClick={() => { if (selected && confirm(`Xóa cơ sở "${selected.name}"? Hành động này không thể hoàn tác.`)) deleteMutation.mutate(selected.id); }}
            disabled={deleteMutation.isPending}
            className="p-3 bg-white/80 backdrop-blur-md rounded-2xl border border-white/50 shadow-sm hover:bg-rose-50 hover:border-rose-200 hover:text-rose-600 transition-all text-slate-400 disabled:opacity-60"
            title="Xóa cơ sở"
          >
            <Trash2 className="w-4 h-4" />
          </button>
        </div>

        <div className="h-[250px] w-full relative overflow-hidden bg-indigo-50 flex items-center justify-center">
          {selected.imageUrl
            ? <img src={selected.imageUrl} className="w-full h-full object-cover" alt="Banner" />
            : <Building2 className="w-24 h-24 text-indigo-200" />}
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
                <p className="text-2xl font-black text-slate-900">{selectedTotalRooms}</p>
                <div className="flex items-center gap-1.5 mt-2">
                   <div className="flex-1 h-1 bg-slate-200 rounded-full overflow-hidden">
                      <div className="h-full bg-slate-900 w-full" />
                   </div>
                </div>
              </div>
              <div className="p-6 bg-indigo-50 rounded-3xl border border-indigo-100 space-y-2">
                <p className="text-[10px] font-black text-indigo-400 uppercase tracking-widest">Đang ở</p>
                <p className="text-2xl font-black text-indigo-600">{selectedOccupiedRooms}</p>
                 <div className="flex items-center gap-1.5 mt-2">
                   <div className="flex-1 h-1 bg-indigo-200 rounded-full overflow-hidden">
                      <div className="h-full bg-indigo-600" style={{ width: `${selectedOccupancyRate}%` }} />
                   </div>
                </div>
              </div>
              <div className="p-6 bg-emerald-50 rounded-3xl border border-emerald-100 space-y-2">
                <p className="text-[10px] font-black text-emerald-400 uppercase tracking-widest">Trống</p>
                <p className="text-2xl font-black text-emerald-600">{Math.max(selectedTotalRooms - selectedOccupiedRooms, 0)}</p>
              </div>
              <div className="p-6 bg-purple-50 rounded-3xl border border-purple-100 space-y-2">
                <p className="text-[10px] font-black text-purple-400 uppercase tracking-widest">Hiệu suất tháng</p>
                <p className="text-2xl font-black text-purple-600">{selectedOccupancyRate}%</p>
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
                    <BarChart data={revenueChartData}>
                      <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                      <XAxis dataKey="month" axisLine={false} tickLine={false} tick={{ fontSize: 10, fontWeight: 700, fill: '#94a3b8' }} />
                      <YAxis hide />
                      <Tooltip cursor={{ fill: '#f8fafc' }} contentStyle={{ borderRadius: '16px', border: 'none', boxShadow: '0 10px 15px -3px rgb(0 0 0 / 0.1)' }} formatter={(v: any) => [`${v}tr đ`, 'Doanh thu']} />
                      <Bar dataKey="amount" fill="#4f46e5" radius={[10, 10, 0, 0]} barSize={40}>
                         {revenueChartData.map((_: any, index: number) => (
                          <Cell key={`cell-${index}`} fill={index === revenueChartData.length - 1 ? '#4f46e5' : '#cbd5e1'} />
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

      {/* Create Property Modal */}
      <Modal isOpen={isCreateOpen} onClose={() => setIsCreateOpen(false)} title="Thêm cơ sở mới" size="md">
        <form
          className="space-y-5"
          onSubmit={(e) => { e.preventDefault(); createMutation.mutate(createForm); }}
        >
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Tên cơ sở *</label>
            <input
              required
              value={createForm.name}
              onChange={e => setCreateForm(f => ({ ...f, name: e.target.value }))}
              placeholder="VD: Nhà trọ Hoàng Anh"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
            />
          </div>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Địa chỉ *</label>
            <input
              required
              value={createForm.address}
              onChange={e => setCreateForm(f => ({ ...f, address: e.target.value }))}
              placeholder="VD: 123 Đường ABC, Q.1, TP.HCM"
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
            />
          </div>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Số tầng</label>
            <input
              type="number"
              min={1}
              value={createForm.totalFloors}
              onChange={e => setCreateForm(f => ({ ...f, totalFloors: Number(e.target.value) }))}
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
            />
          </div>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Mô tả</label>
            <textarea
              rows={3}
              value={createForm.description}
              onChange={e => setCreateForm(f => ({ ...f, description: e.target.value }))}
              placeholder="Thêm mô tả về cơ sở..."
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none resize-none"
            />
          </div>
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={() => setIsCreateOpen(false)} className="flex-1 py-3.5 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors">
              Hủy
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="flex-[2] py-3.5 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60 flex items-center justify-center gap-2"
            >
              {createMutation.isPending ? (
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
              ) : (
                <Save className="w-5 h-5" />
              )}
              Tạo cơ sở
            </button>
          </div>
        </form>
      </Modal>

      {/* Edit Property Modal */}
      <Modal isOpen={isEditOpen} onClose={() => setIsEditOpen(false)} title="Chỉnh sửa cơ sở" size="md">
        <form className="space-y-5" onSubmit={(e) => { e.preventDefault(); if (selected) updateMutation.mutate({ id: selected.id, data: editForm }); }}>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Tên cơ sở *</label>
            <input required value={editForm.name} onChange={e => setEditForm(f => ({ ...f, name: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none" />
          </div>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Địa chỉ *</label>
            <input required value={editForm.address} onChange={e => setEditForm(f => ({ ...f, address: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none" />
          </div>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Số tầng</label>
            <input type="number" min={1} value={editForm.totalFloors} onChange={e => setEditForm(f => ({ ...f, totalFloors: Number(e.target.value) }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none" />
          </div>
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Mô tả</label>
            <textarea rows={3} value={editForm.description} onChange={e => setEditForm(f => ({ ...f, description: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none resize-none" />
          </div>
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={() => setIsEditOpen(false)} className="flex-1 py-3.5 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors">Hủy</button>
            <button type="submit" disabled={updateMutation.isPending} className="flex-[2] py-3.5 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60 flex items-center justify-center gap-2">
              {updateMutation.isPending ? <Loader2 className="w-5 h-5 animate-spin" /> : <Save className="w-5 h-5" />}
              Lưu thay đổi
            </button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
