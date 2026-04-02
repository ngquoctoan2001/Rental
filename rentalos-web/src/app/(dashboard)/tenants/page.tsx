'use client';

import { useState } from 'react';
import {
  Users, Search, Building2, CheckCircle2, AlertCircle,
  ToggleLeft, ToggleRight, Calendar, CreditCard, Mail, Phone,
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tenantsApi } from '@/lib/api';
import { StatCard, StatusBadge } from '@/components/shared';
import { DataTable } from '@/components/shared/DataTable';
import { format } from 'date-fns';

interface Landlord {
  id: string;
  name: string;
  slug: string;
  ownerEmail: string;
  ownerName: string;
  phone: string;
  plan: string;
  isActive: boolean;
  onboardingDone: boolean;
  trialEndsAt?: string;
  planExpiresAt?: string;
  createdAt: string;
}

export default function TenantsPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<'all' | 'active' | 'inactive'>('all');

  const { data: landlords = [], isLoading } = useQuery<Landlord[]>({
    queryKey: ['admin-landlords'],
    queryFn: async () => {
      const resp = await tenantsApi.listAllLandlords();
      return Array.isArray(resp.data) ? resp.data : [];
    },
  });

  const toggleActiveMutation = useMutation({
    mutationFn: (id: string) => tenantsApi.toggleLandlordActive(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin-landlords'] }),
  });

  const filtered = landlords.filter(l => {
    const q = searchTerm.toLowerCase();
    const matchSearch =
      l.name.toLowerCase().includes(q) ||
      l.ownerEmail.toLowerCase().includes(q) ||
      l.ownerName.toLowerCase().includes(q) ||
      l.slug.toLowerCase().includes(q);
    const matchStatus =
      filterStatus === 'all' ||
      (filterStatus === 'active' && l.isActive) ||
      (filterStatus === 'inactive' && !l.isActive);
    return matchSearch && matchStatus;
  });

  const stats = {
    total: landlords.length,
    active: landlords.filter(l => l.isActive).length,
    inactive: landlords.filter(l => !l.isActive).length,
    onboarded: landlords.filter(l => l.onboardingDone).length,
  };

  return (
    <div className="space-y-8 pb-10">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Chủ trọ</h1>
          <p className="text-slate-500 mt-1">Danh sách tất cả chủ trọ đã đăng ký trên hệ thống.</p>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <StatCard title="Tổng chủ trọ" value={String(stats.total)} icon={Users} />
        <StatCard title="Đang hoạt động" value={String(stats.active)} icon={CheckCircle2} color="emerald" />
        <StatCard title="Đã tắt" value={String(stats.inactive)} icon={AlertCircle} color="rose" />
        <StatCard title="Đã hoàn tất thiết lập" value={String(stats.onboarded)} icon={Building2} color="indigo" />
      </div>

      {/* Filter Bar */}
      <div className="flex flex-col md:flex-row gap-3 items-center bg-white/50 p-2 rounded-2xl border border-slate-100 backdrop-blur-sm">
        <div className="relative flex-1 group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
          <input
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
            placeholder="Tìm theo tên, email, slug..."
            className="w-full pl-12 pr-4 py-3 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all shadow-sm"
          />
        </div>
        <div className="flex gap-2">
          {(['all', 'active', 'inactive'] as const).map(f => (
            <button
              key={f}
              onClick={() => setFilterStatus(f)}
              className={`px-4 py-2.5 rounded-xl text-sm font-bold transition-all ${
                filterStatus === f ? 'bg-indigo-600 text-white shadow-md shadow-indigo-200' : 'bg-white border border-slate-200 text-slate-600 hover:border-indigo-300'
              }`}
            >
              {f === 'all' ? 'Tất cả' : f === 'active' ? 'Hoạt động' : 'Đã tắt'}
            </button>
          ))}
        </div>
      </div>

      {/* Table */}
      {isLoading ? (
        <div className="h-64 rounded-3xl border border-slate-100 bg-white animate-pulse" />
      ) : filtered.length === 0 ? (
        <div className="py-20 text-center bg-white rounded-3xl border border-dashed border-slate-200">
          <Users className="w-14 h-14 text-slate-200 mx-auto mb-4" />
          <h3 className="text-lg font-bold text-slate-900">Không tìm thấy chủ trọ nào</h3>
          <p className="text-slate-500 mt-1">Thử thay đổi từ khoá tìm kiếm hoặc bộ lọc.</p>
        </div>
      ) : (
        <DataTable
          data={filtered}
          pageSize={15}
          searchPlaceholder="Tìm chủ trọ..."
          columns={[
            {
              key: 'name',
              label: 'Nhà trọ',
              sortable: true,
              render: (_v: any, row: Landlord) => (
                <div>
                  <p className="font-bold text-slate-900">{row.name}</p>
                  <p className="text-xs text-slate-400 font-mono">{row.slug}</p>
                </div>
              ),
            },
            {
              key: 'ownerName',
              label: 'Chủ trọ',
              sortable: true,
              render: (_v: any, row: Landlord) => (
                <div>
                  <p className="font-medium text-slate-800">{row.ownerName}</p>
                  <div className="flex items-center gap-1 text-xs text-slate-500 mt-0.5">
                    <Mail className="w-3 h-3" />
                    <span>{row.ownerEmail}</span>
                  </div>
                </div>
              ),
            },
            {
              key: 'phone',
              label: 'Điện thoại',
              render: (v: string) => (
                <div className="flex items-center gap-1 text-sm text-slate-700">
                  <Phone className="w-4 h-4 text-slate-400" />
                  <span>{v || 'Chưa cập nhật'}</span>
                </div>
              ),
            },
            {
              key: 'plan',
              label: 'Gói',
              render: (v: string) => (
                <div className="flex items-center gap-1">
                  <CreditCard className="w-4 h-4 text-slate-400" />
                  <span className="text-sm font-semibold capitalize text-slate-700">{v}</span>
                </div>
              ),
            },
            {
              key: 'createdAt',
              label: 'Đăng ký',
              sortable: true,
              render: (v: string) => (
                <div className="flex items-center gap-1 text-sm text-slate-500">
                  <Calendar className="w-4 h-4" />
                  <span>{v ? format(new Date(v), 'dd/MM/yyyy') : '—'}</span>
                </div>
              ),
            },
            {
              key: 'isActive',
              label: 'Trạng thái',
              render: (v: boolean) => (
                <StatusBadge status={v ? 'active' : 'danger'}>
                  {v ? 'Hoạt động' : 'Đã tắt'}
                </StatusBadge>
              ),
            },
            {
              key: 'id',
              label: '',
              render: (_v: any, row: Landlord) => (
                <button
                  onClick={e => { e.stopPropagation(); toggleActiveMutation.mutate(row.id); }}
                  disabled={toggleActiveMutation.isPending}
                  className={`flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-xs font-bold border transition-all ${
                    row.isActive
                      ? 'bg-rose-50 text-rose-600 border-rose-100 hover:bg-rose-100'
                      : 'bg-emerald-50 text-emerald-700 border-emerald-100 hover:bg-emerald-100'
                  }`}
                >
                  {row.isActive ? <ToggleRight className="w-4 h-4" /> : <ToggleLeft className="w-4 h-4" />}
                  {row.isActive ? 'Vô hiệu hoá' : 'Kích hoạt'}
                </button>
              ),
            },
          ]}
        />
      )}
    </div>
  );
}
