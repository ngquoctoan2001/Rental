'use client';

import { useState, useMemo } from 'react';
import { 
  FilePlus, Search, Filter, Calendar, User, Home, 
  DollarSign, Clock, AlertTriangle, FileCheck, FileX,
  ChevronRight, Download, RefreshCw, FileText, Save
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contractsApi, roomsApi, customersApi } from '@/lib/api';
import { DataTable } from '@/components/shared/DataTable';
import { SlideOver } from '@/components/shared/SlideOver';
import { StatusBadge } from '@/components/shared';
import { Contract, Room, Customer } from '@/types';
import { format, addMonths } from 'date-fns';
import { vi } from 'date-fns/locale';

const emptyForm = {
  customerId: '',
  roomId: '',
  startDate: format(new Date(), 'yyyy-MM-dd'),
  months: 12,
  monthlyPrice: '',
  depositAmount: '',
};

export default function ContractsPage() {
  const queryClient = useQueryClient();
  const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
  const [selectedContract, setSelectedContract] = useState<Contract | null>(null);
  const [form, setForm] = useState({ ...emptyForm });

  // Queries
  const { data: contracts = [], isLoading } = useQuery<Contract[]>({
    queryKey: ['contracts'],
    queryFn: async () => {
      const resp = await contractsApi.list();
      const body = resp.data as Contract[] | { items: Contract[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: rooms = [] } = useQuery<Room[]>({
    queryKey: ['rooms-available'],
    queryFn: async () => {
      const resp = await roomsApi.list();
      const body = resp.data as Room[] | { items: Room[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: customers = [] } = useQuery<Customer[]>({
    queryKey: ['customers'],
    queryFn: async () => {
      const resp = await customersApi.list();
      const body = resp.data as Customer[] | { items: Customer[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: any) => contractsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
      setIsSlideOverOpen(false);
      setForm({ ...emptyForm });
    },
  });

  const signMutation = useMutation({
    mutationFn: (id: string) => contractsApi.sign(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['contracts'] }),
  });

  const terminateMutation = useMutation({
    mutationFn: (id: string) => contractsApi.terminate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      setSelectedContract(null);
    },
  });

  // Derived values
  const selectedRoom = rooms.find(r => r.id === form.roomId);
  const endDate = form.startDate
    ? format(addMonths(new Date(form.startDate), Number(form.months) || 12), 'dd/MM/yyyy')
    : '—';

  const handleCreate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.customerId || !form.roomId) return;
    createMutation.mutate({
      customerId: form.customerId,
      roomId: form.roomId,
      startDate: form.startDate,
      months: Number(form.months),
      monthlyRent: Number(form.monthlyPrice) || selectedRoom?.basePrice || 0,
      depositAmount: Number(form.depositAmount) || 0,
    });
  };

  // Columns definition
  const columns = [
    {
      key: 'room',
      label: 'Phòng',
      sortable: true,
      render: (room: Room) => (
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-slate-100 flex items-center justify-center text-slate-600 font-bold">
            {room?.roomNumber || 'N/A'}
          </div>
          <div>
            <p className="font-bold text-slate-900">{room?.roomNumber}</p>
            <p className="text-xs text-slate-400">Tầng {room?.floor}</p>
          </div>
        </div>
      )
    },
    {
      key: 'customer',
      label: 'Khách thuê',
      sortable: true,
      render: (customer: Customer) => (
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 text-xs font-bold">
            {customer?.fullName?.charAt(0) || '?'}
          </div>
          <p className="font-medium text-slate-700">{customer?.fullName || 'Khách lẻ'}</p>
        </div>
      )
    },
    {
      key: 'startDate',
      label: 'Thời hạn',
      render: (_: any, row: Contract) => (
        <div className="text-sm">
          <div className="flex items-center gap-1 text-slate-600">
            <Calendar className="w-3 h-3" />
            <span>{format(new Date(row.startDate), 'dd/MM/yyyy')}</span>
          </div>
          <div className="flex items-center gap-1 text-slate-400 text-xs mt-0.5">
            <Clock className="w-3 h-3" />
            <span>Đến: {row.endDate ? format(new Date(row.endDate), 'dd/MM/yyyy') : 'Vô thời hạn'}</span>
          </div>
        </div>
      )
    },
    {
      key: 'monthlyPrice',
      label: 'Giá thuê',
      sortable: true,
      render: (val: number) => (
        <span className="font-bold text-slate-900">{val?.toLocaleString('vi-VN')}đ</span>
      )
    },
    {
      key: 'status',
      label: 'Trạng thái',
      render: (val: string) => (
        <StatusBadge status={
          val === 'active' ? 'active' : 
          val === 'expiring' ? 'terminating' : 
          val === 'terminated' ? 'terminating' : val
        } />
      )
    },
    {
      key: 'actions',
      label: '',
      render: (_: any, row: Contract) => (
        <button 
          onClick={(e) => { e.stopPropagation(); setSelectedContract(row); }}
          className="p-2 hover:bg-slate-100 rounded-xl text-slate-400 transition-colors"
        >
          <ChevronRight className="w-5 h-5" />
        </button>
      )
    }
  ];

  return (
    <div className="space-y-8 pb-10">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Hợp đồng</h1>
          <p className="text-slate-500 mt-1">Quản lý vòng đời hợp đồng từ khi bắt đầu đến khi thanh lý.</p>
        </div>
        <button 
          onClick={() => setIsSlideOverOpen(true)}
          className="flex items-center justify-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100"
        >
          <FilePlus className="w-5 h-5" />
          Tạo hợp đồng mới
        </button>
      </div>

      {/* Expiry Alert Banner */}
      <div className="bg-amber-50 border border-amber-100 rounded-3xl p-6 flex items-center justify-between shadow-sm">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl bg-amber-100 text-amber-600 flex items-center justify-center shadow-inner">
            <AlertTriangle className="w-6 h-6" />
          </div>
          <div>
            <h4 className="font-bold text-amber-900">3 Hợp đồng sắp hết hạn</h4>
            <p className="text-sm text-amber-700 mt-0.5">Vui lòng liên hệ khách thuê để gia hạn hoặc chuẩn bị thanh lý trong 30 ngày tới.</p>
          </div>
        </div>
        <button className="px-5 py-2.5 bg-amber-600 text-white rounded-xl text-sm font-bold hover:bg-amber-700 transition-colors shadow-md shadow-amber-100">
          Xem danh sách
        </button>
      </div>

      {/* Main Table Container */}
      <div className="bg-white/50 p-6 rounded-[2rem] border border-slate-100 backdrop-blur-sm shadow-sm">
        <DataTable 
          columns={columns} 
          data={contracts} 
          isLoading={isLoading}
          onRowClick={(row) => setSelectedContract(row)}
          searchPlaceholder="Tìm theo tên khách, số phòng..."
        />
      </div>

      {/* SlideOver: Create Contract */}
      <SlideOver 
        isOpen={isSlideOverOpen} 
        onClose={() => setIsSlideOverOpen(false)}
        title="Thiết lập hợp đồng mới"
        width="max-w-xl"
      >
        <form onSubmit={handleCreate} className="space-y-8">
          <div className="space-y-6">
             <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                <User className="w-4 h-4 text-indigo-500" /> Khách thuê *
              </label>
              <select
                required
                value={form.customerId}
                onChange={e => setForm(f => ({ ...f, customerId: e.target.value }))}
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none appearance-none bg-white">
                <option value="">Chọn khách thuê từ danh sách...</option>
                {customers.map(c => <option key={c.id} value={c.id}>{c.fullName} - {c.phoneNumber}</option>)}
              </select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                <Home className="w-4 h-4 text-indigo-500" /> Chọn phòng *
              </label>
              <select
                required
                value={form.roomId}
                onChange={e => {
                  const room = rooms.find(r => r.id === e.target.value);
                  setForm(f => ({
                    ...f,
                    roomId: e.target.value,
                    monthlyPrice: room ? String(room.basePrice) : f.monthlyPrice,
                  }));
                }}
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none appearance-none bg-white">
                <option value="">Chọn phòng còn trống...</option>
                {rooms.filter(r => r.status === 'available').map(r => (
                  <option key={r.id} value={r.id}>Phòng {r.roomNumber} - {r.basePrice.toLocaleString()}đ</option>
                ))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-6">
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                  <Calendar className="w-4 h-4 text-indigo-500" /> Ngày bắt đầu
                </label>
                <input
                  type="date"
                  value={form.startDate}
                  onChange={e => setForm(f => ({ ...f, startDate: e.target.value }))}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                  <Clock className="w-4 h-4 text-indigo-500" /> Kỳ hạn (tháng)
                </label>
                <input
                  type="number"
                  min={1}
                  value={form.months}
                  onChange={e => setForm(f => ({ ...f, months: Number(e.target.value) }))}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-6">
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                  <DollarSign className="w-4 h-4 text-indigo-500" /> Giá thuê hàng tháng
                </label>
                <div className="relative">
                  <input
                    type="number"
                    min={0}
                    value={form.monthlyPrice}
                    onChange={e => setForm(f => ({ ...f, monthlyPrice: e.target.value }))}
                    className="w-full pl-4 pr-10 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                  />
                  <span className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 font-bold">đ</span>
                </div>
              </div>
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                  <DollarSign className="w-4 h-4 text-indigo-500" /> Tiền cọc
                </label>
                <div className="relative">
                  <input
                    type="number"
                    min={0}
                    value={form.depositAmount}
                    onChange={e => setForm(f => ({ ...f, depositAmount: e.target.value }))}
                    className="w-full pl-4 pr-10 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                  />
                  <span className="absolute right-4 top-1/2 -translate-y-1/2 text-slate-400 font-bold">đ</span>
                </div>
              </div>
            </div>
          </div>

          <div className="p-6 bg-slate-50 rounded-3xl border border-slate-100 space-y-4">
            <h5 className="font-bold text-slate-800 text-sm uppercase tracking-wider">Tóm tắt hợp đồng</h5>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-slate-500">Ngày kết thúc dự kiến:</span>
                <span className="font-bold text-slate-700">{endDate}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">Tổng tiền cọc:</span>
                <span className="font-bold text-indigo-600">
                  {Number(form.depositAmount || 0).toLocaleString('vi-VN')}đ
                </span>
              </div>
            </div>
          </div>

          <div className="flex gap-4 pt-2">
            <button 
              type="button"
              className="flex-1 py-4 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors"
              onClick={() => setIsSlideOverOpen(false)}
            >
              Hủy
            </button>
            <button
              type="submit"
              disabled={createMutation.isPending}
              className="flex-[2] py-4 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60 flex items-center justify-center gap-2"
            >
              {createMutation.isPending ? (
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
              ) : (
                <Save className="w-5 h-5" />
              )}
              Ký kết hợp đồng
            </button>
          </div>
        </form>
      </SlideOver>

      {/* Detail SlideOver or Modal */}
      <SlideOver
        isOpen={!!selectedContract}
        onClose={() => setSelectedContract(null)}
        title="Chi tiết hợp đồng"
        width="max-w-2xl"
      >
        {selectedContract && (
          <div className="space-y-8">
            <div className="flex items-center justify-between">
              <StatusBadge status={
                selectedContract.status === 'active' ? 'active' : 'terminating'
              } />
              <div className="flex gap-2">
                <button className="p-2 bg-indigo-50 text-indigo-600 rounded-xl hover:bg-indigo-100 transition-colors">
                  <Download className="w-5 h-5" />
                </button>
                <button className="p-2 bg-indigo-50 text-indigo-600 rounded-xl hover:bg-indigo-100 transition-colors">
                  <RefreshCw className="w-5 h-5" />
                </button>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-8">
              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className="w-12 h-12 rounded-2xl bg-slate-100 flex items-center justify-center text-slate-600 font-bold text-lg border border-slate-200 shadow-inner">
                    {selectedContract.room?.roomNumber}
                  </div>
                  <div>
                    <h4 className="font-bold text-slate-900">Phòng {selectedContract.room?.roomNumber}</h4>
                    <p className="text-sm text-slate-500">Tòa nhà C - Tầng {selectedContract.room?.floor}</p>
                  </div>
                </div>
                <div className="p-4 bg-slate-50 rounded-2xl border border-slate-100">
                  <p className="text-xs font-bold text-slate-400 uppercase mb-2">Thông tin thuê</p>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-slate-500 underline decoration-slate-200 underline-offset-4">Bắt đầu:</span>
                      <span className="font-medium text-slate-800">{format(new Date(selectedContract.startDate), 'dd/MM/yyyy')}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-slate-500 underline decoration-slate-200 underline-offset-4">Kết thúc:</span>
                      <span className="font-medium text-slate-800">{selectedContract.endDate ? format(new Date(selectedContract.endDate), 'dd/MM/yyyy') : 'Vô thời hạn'}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className="w-12 h-12 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 font-bold text-lg border border-indigo-50 shadow-inner">
                    {selectedContract.customer?.fullName.charAt(0)}
                  </div>
                  <div>
                    <h4 className="font-bold text-slate-900">{selectedContract.customer?.fullName}</h4>
                    <p className="text-sm text-slate-500">{selectedContract.customer?.phoneNumber}</p>
                  </div>
                </div>
                <div className="p-4 bg-indigo-50/30 rounded-2xl border border-indigo-100">
                  <p className="text-xs font-bold text-indigo-400 uppercase mb-2">Tài chính</p>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-indigo-600/70">Tiền cọc:</span>
                      <span className="font-bold text-indigo-700">{selectedContract.depositAmount.toLocaleString()}đ</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-indigo-600/70">Giá thuê:</span>
                      <span className="font-bold text-indigo-700 font-mono">{selectedContract.monthlyPrice.toLocaleString()}đ</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <h4 className="font-bold text-slate-900 border-b border-slate-100 pb-2">Lịch sử hóa đơn</h4>
              <div className="space-y-3">
                {[1, 2].map(i => (
                  <div key={i} className="flex items-center justify-between p-4 bg-white border border-slate-100 rounded-2xl hover:border-indigo-200 transition-colors shadow-sm">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                        <FileText className="w-5 h-5" />
                      </div>
                      <div>
                        <p className="font-bold text-slate-800">Hóa đơn tháng {i+1}/2026</p>
                        <p className="text-xs text-slate-400">Đã thanh toán ngày 05/{i+1}/2026</p>
                      </div>
                    </div>
                    <span className="text-emerald-600 font-bold text-sm">+3.650.000đ</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="flex gap-4 pt-10 mt-10 border-t border-slate-100">
              <button
                onClick={() => { if (confirm('Thanh lý hợp đồng này?')) terminateMutation.mutate(selectedContract.id); }}
                disabled={terminateMutation.isPending}
                className="flex-1 py-4 bg-rose-50 text-rose-600 rounded-3xl font-bold hover:bg-rose-100 transition-colors flex items-center justify-center gap-2 disabled:opacity-60"
              >
                <FileX className="w-5 h-5" /> Thanh lý HĐ
              </button>
              <button
                onClick={() => { if (!selectedContract.signedByCustomer && confirm('Xác nhận khách đã ký hợp đồng?')) signMutation.mutate(selectedContract.id); }}
                disabled={signMutation.isPending || selectedContract.signedByCustomer}
                className="flex-1 py-4 bg-emerald-50 text-emerald-700 rounded-3xl font-bold hover:bg-emerald-100 transition-colors flex items-center justify-center gap-2 disabled:opacity-60"
              >
                <FileCheck className="w-5 h-5" /> {selectedContract.signedByCustomer ? 'Đã ký' : 'Xác nhận ký'}
              </button>
              <button className="flex-1 py-4 bg-indigo-600 text-white rounded-3xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 flex items-center justify-center gap-2">
                <RefreshCw className="w-5 h-5" /> Gia hạn HĐ
              </button>
            </div>
          </div>
        )}
      </SlideOver>
    </div>
  );
}
