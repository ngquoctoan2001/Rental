'use client';

import { useState } from 'react';
import { 
  ArrowUpCircle, ArrowDownCircle, Wallet, Search, Filter, 
  Calendar, Download, Plus, Smartphone, FileText, Loader2,
  Banknote, Landmark
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { transactionsApi, contractsApi } from '@/lib/api';
import { DataTable } from '@/components/shared/DataTable';
import { SlideOver } from '@/components/shared/SlideOver';
import { StatCard, StatusBadge } from '@/components/shared';
import { Transaction } from '@/types';
import { format } from 'date-fns';

export default function TransactionsPage() {
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [dateRange, setDateRange] = useState({ start: '', end: '' });
  const queryClient = useQueryClient();

  // Queries
  const { data: transactions = [], isLoading } = useQuery<Transaction[]>({
    queryKey: ['transactions', dateRange],
    queryFn: async () => {
      const params = dateRange.start ? { dateFrom: dateRange.start, dateTo: dateRange.end } : undefined;
      const resp = await transactionsApi.list(params);
      const body = resp.data as Transaction[] | { items: Transaction[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: activeContracts = [] } = useQuery({
    queryKey: ['contracts-active-txn'],
    queryFn: () => contractsApi.list({ status: 'active' }).then(r => Array.isArray(r.data) ? r.data : (r.data as any)?.items ?? []),
  });

  const [refundForm, setRefundForm] = useState({ contractId: '', amount: '', note: '' });

  const recordDepositRefundMutation = useMutation({
    mutationFn: (data: any) => transactionsApi.recordDepositRefund(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transactions'] });
      setIsAddOpen(false);
      setRefundForm({ contractId: '', amount: '', note: '' });
    },
  });

  const exportMutation = useMutation({
    mutationFn: () => transactionsApi.exportExcel(dateRange.start ? dateRange : undefined),
    onSuccess: (resp) => {
      const url = window.URL.createObjectURL(new Blob([resp.data]));
      const a = document.createElement('a');
      a.href = url;
      a.download = `transactions-${format(new Date(), 'yyyy-MM-dd')}.xlsx`;
      a.click();
      window.URL.revokeObjectURL(url);
    },
  });

  const normalizeDirection = (value: Transaction['direction']) => String(value).toLowerCase();
  const normalizeStatus = (value: Transaction['status']) => String(value).toLowerCase();
  const normalizeMethod = (value: Transaction['method']) => String(value).toLowerCase();

  // Calculate stats
  const totalIncome = transactions
    .filter((t: Transaction) => normalizeDirection(t.direction) === 'income' && normalizeStatus(t.status) === 'success')
    .reduce((sum: number, t: Transaction) => sum + t.amount, 0);
  
  const totalExpense = transactions
    .filter((t: Transaction) => normalizeDirection(t.direction) === 'expense' && normalizeStatus(t.status) === 'success')
    .reduce((sum: number, t: Transaction) => sum + t.amount, 0);

  const columns = [
    {
      key: 'createdAt',
      label: 'Thời gian',
      sortable: true,
      render: (val: string) => (
        <div className="text-sm font-medium text-slate-600">
          {format(new Date(val), 'dd/MM/yyyy HH:mm')}
        </div>
      )
    },
    {
      key: 'direction',
      label: 'Loại',
      render: (val: string) => (
        <div className="flex items-center gap-2">
          {String(val).toLowerCase() === 'income' ? (
            <div className="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <ArrowUpCircle className="w-5 h-5" />
            </div>
          ) : (
            <div className="w-8 h-8 rounded-lg bg-rose-50 text-rose-600 flex items-center justify-center">
              <ArrowDownCircle className="w-5 h-5" />
            </div>
          )}
          <span className={`text-sm font-bold ${String(val).toLowerCase() === 'income' ? 'text-emerald-700' : 'text-rose-700'}`}>
            {String(val).toLowerCase() === 'income' ? 'Thu' : 'Chi'}
          </span>
        </div>
      )
    },
    {
      key: 'amount',
      label: 'Số tiền',
      sortable: true,
      render: (val: number, row: Transaction) => (
        <span className={`font-black text-lg ${normalizeDirection(row.direction) === 'income' ? 'text-emerald-600' : 'text-rose-600'}`}>
          {normalizeDirection(row.direction) === 'income' ? '+' : '-'}{val.toLocaleString()}đ
        </span>
      )
    },
    {
      key: 'method',
      label: 'Phương thức',
      render: (val: string) => {
        const normalized = normalizeMethod(val as Transaction['method']);
        const icons = {
          cash: <Banknote className="w-3.5 h-3.5" />,
          banktransfer: <Landmark className="w-3.5 h-3.5" />,
          momo: <Smartphone className="w-3.5 h-3.5" />,
          vnpay: <Smartphone className="w-3.5 h-3.5" />,
          depositrefund: <Landmark className="w-3.5 h-3.5" />,
        };
        const labels = {
          cash: 'Tiền mặt',
          banktransfer: 'Chuyển khoản',
          momo: 'Momo',
          vnpay: 'VNPay',
          depositrefund: 'Hoàn cọc',
        };
        return (
          <div className="flex items-center gap-2 px-3 py-1 bg-slate-100 rounded-full text-slate-600 text-[10px] font-bold uppercase tracking-tight">
            {icons[normalized as keyof typeof icons] ?? <Landmark className="w-3.5 h-3.5" />}
            {labels[normalized as keyof typeof labels] ?? val}
          </div>
        );
      }
    },
    {
      key: 'note',
      label: 'Nội dung',
      render: (val: string) => (
        <span className="text-slate-500 text-sm italic line-clamp-1">{val || 'Bản ghi không có mô tả'}</span>
      )
    },
    {
      key: 'status',
      label: 'Trạng thái',
      render: (val: string) => (
        <StatusBadge status={String(val).toLowerCase()} />
      )
    }
  ];

  return (
    <div className="space-y-8 pb-10">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Dòng tiền & Tài chính</h1>
          <p className="text-slate-500 mt-1">Theo dõi mọi biến động số dư và lịch sử giao dịch chi tiết.</p>
        </div>
        <div className="flex items-center gap-3">
          <button 
            onClick={() => exportMutation.mutate()}
            disabled={exportMutation.isPending}
            className="flex items-center gap-2 px-5 py-3 bg-white border border-slate-200 rounded-2xl font-bold text-slate-600 hover:bg-slate-50 transition-all shadow-sm disabled:opacity-60"
          >
            {exportMutation.isPending ? <Loader2 className="w-5 h-5 animate-spin" /> : <Download className="w-5 h-5" />}
            Xuất Excel
          </button>
          <button 
            onClick={() => setIsAddOpen(true)}
            className="flex items-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100"
          >
            <Plus className="w-5 h-5" />
            Hoàn cọc
          </button>
        </div>
      </div>

      {/* Financial Overview */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard 
          title="Tổng Thu nhập" 
          value={totalIncome.toLocaleString() + 'đ'} 
          icon={ArrowUpCircle}
          color="emerald"
        />
        <StatCard 
          title="Tổng Chi phí" 
          value={totalExpense.toLocaleString() + 'đ'} 
          icon={ArrowDownCircle}
          trend={{ value: 5.2, isPositive: false }}
          color="rose"
        />
        <StatCard 
          title="Lợi nhuận ròng" 
          value={(totalIncome - totalExpense).toLocaleString() + 'đ'} 
          icon={Wallet}
          trend={{ value: 18.5, isPositive: true }}
        />
      </div>

      {/* Filters & Transaction Table */}
      <div className="space-y-4">
        <div className="flex flex-wrap items-center gap-4 bg-white/50 p-4 rounded-3xl border border-slate-100 backdrop-blur-sm shadow-sm ring-1 ring-white/20">
          <div className="relative group min-w-[300px] flex-1">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
            <input 
              type="text" 
              placeholder="Tìm kiếm nội dung giao dịch..."
              className="w-full pl-10 pr-4 py-2.5 bg-white border border-slate-200 rounded-xl text-sm focus:ring-2 focus:ring-indigo-500/20 outline-none"
            />
          </div>
          <div className="flex items-center gap-2">
            <div className="flex items-center gap-2">
              <Calendar className="w-4 h-4 text-indigo-500 shrink-0" />
              <input
                type="date"
                value={dateRange.start}
                onChange={e => setDateRange(d => ({ ...d, start: e.target.value }))}
                className="px-3 py-2 bg-white border border-slate-200 rounded-xl text-xs font-bold text-slate-600 outline-none focus:ring-2 focus:ring-indigo-500/20"
              />
              <span className="text-slate-400 text-xs">→</span>
              <input
                type="date"
                value={dateRange.end}
                onChange={e => setDateRange(d => ({ ...d, end: e.target.value }))}
                className="px-3 py-2 bg-white border border-slate-200 rounded-xl text-xs font-bold text-slate-600 outline-none focus:ring-2 focus:ring-indigo-500/20"
              />
            </div>
            <button className="p-2.5 bg-white border border-slate-200 rounded-xl hover:bg-slate-50 transition-colors">
              <Filter className="w-4 h-4 text-slate-500" />
            </button>
          </div>
        </div>

        <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-6 overflow-hidden">
          <DataTable 
            columns={columns} 
            data={transactions} 
            isLoading={isLoading}
            searchPlaceholder="Tìm kiếm..."
          />
        </div>
      </div>

      {/* Deposit Refund SlideOver */}
      <SlideOver 
        isOpen={isAddOpen} 
        onClose={() => { setIsAddOpen(false); setRefundForm({ contractId: '', amount: '', note: '' }); }} 
        title="Ghi nhận hoàn cọc"
        width="max-w-md"
      >
        <div className="space-y-6">

          <form onSubmit={e => { e.preventDefault(); recordDepositRefundMutation.mutate({ contractId: refundForm.contractId, amount: Number(refundForm.amount), note: refundForm.note || undefined }); }} className="space-y-6">
            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1">Hợp đồng</label>
              <select
                required
                value={refundForm.contractId}
                onChange={e => setRefundForm(f => ({ ...f, contractId: e.target.value }))}
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none bg-white"
              >
                <option value="">-- Chọn hợp đồng --</option>
                {(activeContracts as any[]).map((c: any) => (
                  <option key={c.id} value={c.id}>{c.customer?.fullName ?? 'N/A'} - Phòng {c.room?.roomNumber ?? 'N/A'}</option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1">Số tiền hoàn cọc (đ)</label>
              <div className="relative">
                <input
                  type="number"
                  required
                  min={1}
                  value={refundForm.amount}
                  onChange={e => setRefundForm(f => ({ ...f, amount: e.target.value }))}
                  className="w-full px-6 py-4 bg-slate-50 border-none rounded-2xl text-xl font-black text-slate-900 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                  placeholder="0"
                />
                <span className="absolute right-6 top-1/2 -translate-y-1/2 font-bold text-slate-400">đ</span>
              </div>
            </div>
            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                <FileText className="w-4 h-4 text-indigo-500" /> Ghi chú
              </label>
              <textarea
                rows={3}
                value={refundForm.note}
                onChange={e => setRefundForm(f => ({ ...f, note: e.target.value }))}
                className="w-full px-4 py-3 border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 outline-none resize-none text-sm"
                placeholder="Lý do hoàn cọc..."
              />
            </div>
            <div className="flex gap-4 pt-6">
              <button
                type="button"
                onClick={() => { setIsAddOpen(false); setRefundForm({ contractId: '', amount: '', note: '' }); }}
                className="flex-1 py-4 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors"
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                disabled={recordDepositRefundMutation.isPending}
                className="flex-[2] py-4 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 flex items-center justify-center gap-2"
              >
                {recordDepositRefundMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
                Xác nhận hoàn cọc
              </button>
            </div>
          </form>
        </div>
      </SlideOver>
    </div>
  );
}
