'use client';

import { useState } from 'react';
import {
  FilePlus, Calendar, User, Home,
  DollarSign, Clock, AlertTriangle, FileCheck, FileX,
  ChevronRight, Download, RefreshCw, FileText, Save, Loader2, X
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { contractsApi, roomsApi, customersApi } from '@/lib/api';
import { DataTable } from '@/components/shared/DataTable';
import { SlideOver } from '@/components/shared/SlideOver';
import { StatusBadge } from '@/components/shared';
import { Contract, Room, Customer } from '@/types';
import { format, addMonths } from 'date-fns';

const emptyForm = {
  customerId: '',
  roomId: '',
  startDate: format(new Date(), 'yyyy-MM-dd'),
  months: 12,
  monthlyPrice: '',
  depositAmount: '',
};

const emptyTerminate = { reason: '', type: 'Normal', depositRefunded: '' };
const emptyRenew = { months: 12, newMonthlyRent: '' };

export default function ContractsPage() {
  const queryClient = useQueryClient();
  const [isSlideOverOpen, setIsSlideOverOpen] = useState(false);
  const [selectedContract, setSelectedContract] = useState<Contract | null>(null);
  const [form, setForm] = useState({ ...emptyForm });
  const [terminateForm, setTerminateForm] = useState({ ...emptyTerminate });
  const [showTerminateModal, setShowTerminateModal] = useState(false);
  const [renewForm, setRenewForm] = useState({ ...emptyRenew });
  const [showRenewModal, setShowRenewModal] = useState(false);

  const { data: contracts = [], isLoading } = useQuery<Contract[]>({
    queryKey: ['contracts'],
    queryFn: async () => {
      const resp = await contractsApi.list();
      const body = resp.data as Contract[] | { items: Contract[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: expiringContracts = [] } = useQuery<Contract[]>({
    queryKey: ['contracts-expiring'],
    queryFn: async () => {
      const resp = await contractsApi.expiring();
      return Array.isArray(resp.data) ? resp.data : [];
    }
  });

  const { data: contractInvoices = [] } = useQuery({
    queryKey: ['contract-invoices', selectedContract?.id],
    queryFn: async () => {
      const resp = await contractsApi.getInvoices(selectedContract!.id);
      return Array.isArray(resp.data) ? resp.data : [];
    },
    enabled: !!selectedContract,
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

  const createMutation = useMutation({
    mutationFn: (data: any) => contractsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      queryClient.invalidateQueries({ queryKey: ['rooms-available'] });
      setIsSlideOverOpen(false);
      setForm({ ...emptyForm });
    },
  });

  const signMutation = useMutation({
    mutationFn: (id: string) => contractsApi.sign(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['contracts'] }),
  });

  const terminateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => contractsApi.terminate(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      setSelectedContract(null);
      setShowTerminateModal(false);
      setTerminateForm({ ...emptyTerminate });
    },
  });

  const renewMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => contractsApi.renew(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
      setShowRenewModal(false);
      setRenewForm({ ...emptyRenew });
    },
  });

  const handleDownloadPdf = async (id: string) => {
    try {
      const resp = await contractsApi.getPdf(id);
      const url = resp.data?.url;
      if (url) window.open(url, '_blank');
    } catch {
      alert('Không thể tải PDF hợp đồng.');
    }
  };

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

  const handleTerminate = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedContract) return;
    terminateMutation.mutate({
      id: selectedContract.id,
      data: {
        id: selectedContract.id,
        reason: terminateForm.reason,
        type: terminateForm.type,
        depositRefunded: terminateForm.depositRefunded ? Number(terminateForm.depositRefunded) : null,
        terminatedAt: new Date().toISOString(),
      },
    });
  };

  const handleRenew = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedContract) return;
    const start = selectedContract.endDate
      ? new Date(selectedContract.endDate)
      : new Date();
    const end = addMonths(start, Number(renewForm.months));
    renewMutation.mutate({
      id: selectedContract.id,
      data: {
        oldContractId: selectedContract.id,
        startDate: format(start, 'yyyy-MM-dd'),
        endDate: format(end, 'yyyy-MM-dd'),
        newMonthlyRent: renewForm.newMonthlyRent ? Number(renewForm.newMonthlyRent) : null,
      },
    });
  };

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

      {expiringContracts.length > 0 && (
        <div className="bg-amber-50 border border-amber-100 rounded-3xl p-6 flex items-center justify-between shadow-sm">
          <div className="flex items-center gap-4">
            <div className="w-12 h-12 rounded-2xl bg-amber-100 text-amber-600 flex items-center justify-center shadow-inner">
              <AlertTriangle className="w-6 h-6" />
            </div>
            <div>
              <h4 className="font-bold text-amber-900">{expiringContracts.length} Hợp đồng sắp hết hạn</h4>
              <p className="text-sm text-amber-700 mt-0.5">Vui lòng liên hệ khách thuê để gia hạn hoặc chuẩn bị thanh lý trong 30 ngày tới.</p>
            </div>
          </div>
          <button
            onClick={() => setSelectedContract(expiringContracts[0])}
            className="px-5 py-2.5 bg-amber-600 text-white rounded-xl text-sm font-bold hover:bg-amber-700 transition-colors shadow-md shadow-amber-100"
          >
            Xem danh sách
          </button>
        </div>
      )}

      <div className="bg-white/50 p-6 rounded-[2rem] border border-slate-100 backdrop-blur-sm shadow-sm">
        <DataTable
          columns={columns}
          data={contracts}
          isLoading={isLoading}
          onRowClick={(row) => setSelectedContract(row)}
          searchPlaceholder="Tìm theo tên khách, số phòng..."
        />
      </div>

      {/* Create Contract SlideOver */}
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
                <Loader2 className="w-5 h-5 animate-spin" />
              ) : (
                <Save className="w-5 h-5" />
              )}
              Ký kết hợp đồng
            </button>
          </div>
        </form>
      </SlideOver>

      {/* Contract Detail SlideOver */}
      <SlideOver
        isOpen={!!selectedContract && !showTerminateModal && !showRenewModal}
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
                <button
                  onClick={() => handleDownloadPdf(selectedContract.id)}
                  className="p-2 bg-indigo-50 text-indigo-600 rounded-xl hover:bg-indigo-100 transition-colors"
                  title="Tải PDF hợp đồng"
                >
                  <Download className="w-5 h-5" />
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
                    <p className="text-sm text-slate-500">Tầng {selectedContract.room?.floor}</p>
                  </div>
                </div>
                <div className="p-4 bg-slate-50 rounded-2xl border border-slate-100">
                  <p className="text-xs font-bold text-slate-400 uppercase mb-2">Thông tin thuê</p>
                  <div className="space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-slate-500">Bắt đầu:</span>
                      <span className="font-medium text-slate-800">{format(new Date(selectedContract.startDate), 'dd/MM/yyyy')}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-slate-500">Kết thúc:</span>
                      <span className="font-medium text-slate-800">{selectedContract.endDate ? format(new Date(selectedContract.endDate), 'dd/MM/yyyy') : 'Vô thời hạn'}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div className="space-y-4">
                <div className="flex items-center gap-3">
                  <div className="w-12 h-12 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600 font-bold text-lg border border-indigo-50 shadow-inner">
                    {selectedContract.customer?.fullName?.charAt(0)}
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
                      <span className="font-bold text-indigo-700">{selectedContract.depositAmount?.toLocaleString()}đ</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-indigo-600/70">Giá thuê:</span>
                      <span className="font-bold text-indigo-700">{selectedContract.monthlyPrice?.toLocaleString()}đ</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <h4 className="font-bold text-slate-900 border-b border-slate-100 pb-2">Lịch sử hóa đơn</h4>
              <div className="space-y-3 max-h-60 overflow-y-auto">
                {contractInvoices.length === 0 && (
                  <p className="text-sm text-slate-400 text-center py-4">Chưa có hóa đơn nào.</p>
                )}
                {contractInvoices.map((inv: any) => (
                  <div key={inv.id} className="flex items-center justify-between p-4 bg-white border border-slate-100 rounded-2xl shadow-sm">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400">
                        <FileText className="w-5 h-5" />
                      </div>
                      <div>
                        <p className="font-bold text-slate-800">{inv.invoiceCode}</p>
                        <p className="text-xs text-slate-400">
                          {inv.billingMonth ? `Tháng ${new Date(inv.billingMonth).getMonth() + 1}/${new Date(inv.billingMonth).getFullYear()}` : ''}
                        </p>
                      </div>
                    </div>
                    <span className={`font-bold text-sm ${inv.status === 'Paid' ? 'text-emerald-600' : 'text-amber-600'}`}>
                      {inv.totalAmount?.toLocaleString()}đ
                    </span>
                  </div>
                ))}
              </div>
            </div>

            <div className="flex gap-4 pt-4 border-t border-slate-100">
              <button
                onClick={() => setShowTerminateModal(true)}
                disabled={selectedContract.status === 'Terminated'}
                className="flex-1 py-4 bg-rose-50 text-rose-600 rounded-3xl font-bold hover:bg-rose-100 transition-colors flex items-center justify-center gap-2 disabled:opacity-40"
              >
                <FileX className="w-5 h-5" /> Thanh lý HĐ
              </button>
              <button
                onClick={() => { if (!selectedContract.signedByCustomer) signMutation.mutate(selectedContract.id); }}
                disabled={signMutation.isPending || !!selectedContract.signedByCustomer}
                className="flex-1 py-4 bg-emerald-50 text-emerald-700 rounded-3xl font-bold hover:bg-emerald-100 transition-colors flex items-center justify-center gap-2 disabled:opacity-40"
              >
                <FileCheck className="w-5 h-5" /> {selectedContract.signedByCustomer ? 'Đã ký' : 'Xác nhận ký'}
              </button>
              <button
                onClick={() => setShowRenewModal(true)}
                disabled={selectedContract.status === 'Terminated'}
                className="flex-1 py-4 bg-indigo-600 text-white rounded-3xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 flex items-center justify-center gap-2 disabled:opacity-40"
              >
                <RefreshCw className="w-5 h-5" /> Gia hạn HĐ
              </button>
            </div>
          </div>
        )}
      </SlideOver>

      {/* Terminate Modal */}
      {showTerminateModal && selectedContract && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-[2rem] shadow-2xl p-8 w-full max-w-md space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-900">Thanh lý hợp đồng</h3>
              <button onClick={() => setShowTerminateModal(false)} className="p-2 hover:bg-slate-100 rounded-xl">
                <X className="w-5 h-5 text-slate-500" />
              </button>
            </div>
            <form onSubmit={handleTerminate} className="space-y-4">
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Lý do thanh lý</label>
                <textarea
                  value={terminateForm.reason}
                  onChange={e => setTerminateForm(f => ({ ...f, reason: e.target.value }))}
                  rows={3}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-rose-500/20 outline-none resize-none"
                  placeholder="Nhập lý do thanh lý hợp đồng..."
                />
              </div>
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Loại thanh lý</label>
                <select
                  value={terminateForm.type}
                  onChange={e => setTerminateForm(f => ({ ...f, type: e.target.value }))}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-rose-500/20 outline-none bg-white"
                >
                  <option value="Normal">Hết hạn bình thường</option>
                  <option value="Breach">Vi phạm hợp đồng</option>
                  <option value="Mutual">Thỏa thuận hai bên</option>
                </select>
              </div>
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Tiền cọc hoàn trả (đ)</label>
                <input
                  type="number"
                  min={0}
                  value={terminateForm.depositRefunded}
                  onChange={e => setTerminateForm(f => ({ ...f, depositRefunded: e.target.value }))}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-rose-500/20 outline-none"
                  placeholder="Để trống nếu không hoàn cọc"
                />
              </div>
              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => setShowTerminateModal(false)} className="flex-1 py-3 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200">
                  Hủy
                </button>
                <button type="submit" disabled={terminateMutation.isPending} className="flex-1 py-3 bg-rose-600 text-white rounded-2xl font-bold hover:bg-rose-700 disabled:opacity-60 flex items-center justify-center gap-2">
                  {terminateMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
                  Xác nhận thanh lý
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Renew Modal */}
      {showRenewModal && selectedContract && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-[2rem] shadow-2xl p-8 w-full max-w-md space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-900">Gia hạn hợp đồng</h3>
              <button onClick={() => setShowRenewModal(false)} className="p-2 hover:bg-slate-100 rounded-xl">
                <X className="w-5 h-5 text-slate-500" />
              </button>
            </div>
            <form onSubmit={handleRenew} className="space-y-4">
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Số tháng gia hạn</label>
                <input
                  type="number"
                  min={1}
                  value={renewForm.months}
                  onChange={e => setRenewForm(f => ({ ...f, months: Number(e.target.value) }))}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                />
              </div>
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Giá thuê mới (đ, để trống giữ nguyên)</label>
                <input
                  type="number"
                  min={0}
                  value={renewForm.newMonthlyRent}
                  onChange={e => setRenewForm(f => ({ ...f, newMonthlyRent: e.target.value }))}
                  placeholder={selectedContract.monthlyPrice?.toLocaleString()}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                />
              </div>
              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => setShowRenewModal(false)} className="flex-1 py-3 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200">
                  Hủy
                </button>
                <button type="submit" disabled={renewMutation.isPending} className="flex-1 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 disabled:opacity-60 flex items-center justify-center gap-2">
                  {renewMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
                  Xác nhận gia hạn
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
