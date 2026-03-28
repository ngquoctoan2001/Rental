'use client';

import { useState } from 'react';
import { 
  ArrowUpCircle, ArrowDownCircle, Wallet, Search, Filter, 
  Calendar, Download, MoreVertical, Plus, CreditCard,
  Banknote, Landmark, Smartphone, Tag, FileText, Loader2
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { transactionsApi } from '@/lib/api';
import { DataTable } from '@/components/shared/DataTable';
import { SlideOver } from '@/components/shared/SlideOver';
import { StatCard, StatusBadge } from '@/components/shared';
import { Transaction } from '@/types';
import { format } from 'date-fns';

const emptyForm = {
  type: 'income' as 'income' | 'expense',
  amount: '',
  category: '',
  paymentMethod: 'cash' as 'cash' | 'bank_transfer',
  description: '',
};

export default function TransactionsPage() {
  const [isAddOpen, setIsAddOpen] = useState(false);
  const [dateRange, setDateRange] = useState({ start: '', end: '' });
  const [form, setForm] = useState(emptyForm);
  const queryClient = useQueryClient();

  // Queries
  const { data: transactions = [], isLoading } = useQuery<Transaction[]>({
    queryKey: ['transactions', dateRange],
    queryFn: async () => {
      const resp = await transactionsApi.list();
      const body = resp.data as Transaction[] | { items: Transaction[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  // Note: Cash payments are created via Invoices page (POST /invoices/{id}/cash-payment).
  // There is no generic freeform transaction creation API.

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

  // Calculate stats
  const totalIncome = transactions
    .filter((t: Transaction) => t.type === 'income' && t.status === 'completed')
    .reduce((sum: number, t: Transaction) => sum + t.amount, 0);
  
  const totalExpense = transactions
    .filter((t: Transaction) => t.type === 'expense' && t.status === 'completed')
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
      key: 'type',
      label: 'Loại',
      render: (val: string) => (
        <div className="flex items-center gap-2">
          {val === 'income' ? (
            <div className="w-8 h-8 rounded-lg bg-emerald-50 text-emerald-600 flex items-center justify-center">
              <ArrowUpCircle className="w-5 h-5" />
            </div>
          ) : (
            <div className="w-8 h-8 rounded-lg bg-rose-50 text-rose-600 flex items-center justify-center">
              <ArrowDownCircle className="w-5 h-5" />
            </div>
          )}
          <span className={`text-sm font-bold ${val === 'income' ? 'text-emerald-700' : 'text-rose-700'}`}>
            {val === 'income' ? 'Thu' : 'Chi'}
          </span>
        </div>
      )
    },
    {
      key: 'amount',
      label: 'Số tiền',
      sortable: true,
      render: (val: number, row: Transaction) => (
        <span className={`font-black text-lg ${row.type === 'income' ? 'text-emerald-600' : 'text-rose-600'}`}>
          {row.type === 'income' ? '+' : '-'}{val.toLocaleString()}đ
        </span>
      )
    },
    {
      key: 'paymentMethod',
      label: 'Phương thức',
      render: (val: string) => {
        const icons = {
          cash: <Banknote className="w-3.5 h-3.5" />,
          bank_transfer: <Landmark className="w-3.5 h-3.5" />,
          momo: <Smartphone className="w-3.5 h-3.5" />,
          vnpay: <Smartphone className="w-3.5 h-3.5" />,
          zalo: <Smartphone className="w-3.5 h-3.5" />
        };
        const labels = {
          cash: 'Tiền mặt',
          bank_transfer: 'Chuyển khoản',
          momo: 'Momo',
          vnpay: 'VNPay',
          zalo: 'ZaloPay'
        };
        return (
          <div className="flex items-center gap-2 px-3 py-1 bg-slate-100 rounded-full text-slate-600 text-[10px] font-bold uppercase tracking-tight">
            {icons[val as keyof typeof icons]}
            {labels[val as keyof typeof labels]}
          </div>
        );
      }
    },
    {
      key: 'description',
      label: 'Nội dung',
      render: (val: string) => (
        <span className="text-slate-500 text-sm italic line-clamp-1">{val || 'Bản ghi không có mô tả'}</span>
      )
    },
    {
      key: 'status',
      label: 'Trạng thái',
      render: (val: string) => (
        <StatusBadge status={val === 'completed' ? 'active' : 'pending'} />
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
            Giao dịch mới
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
             <div className="flex items-center gap-2 px-4 py-2 bg-white border border-slate-200 rounded-xl text-xs font-bold text-slate-600 cursor-pointer hover:bg-slate-50 transition-colors">
              <Calendar className="w-4 h-4 text-indigo-500" />
              <span>Mar 01 - Mar 31, 2026</span>
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

      {/* Add Transaction SlideOver */}
      <SlideOver 
        isOpen={isAddOpen} 
        onClose={() => setIsAddOpen(false)} 
        title="Ghi nhận giao dịch thủ công"
        width="max-w-md"
      >
        <div className="space-y-8">
          <div className="flex p-1 bg-slate-100 rounded-2xl">
            <button 
              type="button"
              onClick={() => setForm(f => ({ ...f, type: 'income' }))}
              className={`flex-1 py-3 rounded-xl shadow-sm text-sm font-bold transition-all ${form.type === 'income' ? 'bg-white text-emerald-600' : 'text-slate-500'}`}
            >
              THU (Income)
            </button>
            <button 
              type="button"
              onClick={() => setForm(f => ({ ...f, type: 'expense' }))}
              className={`flex-1 py-3 rounded-xl text-sm font-bold transition-all ${form.type === 'expense' ? 'bg-white text-rose-600' : 'text-slate-500'}`}
            >
              CHI (Expense)
            </button>
          </div>

            <form className="space-y-6" onSubmit={e => { e.preventDefault(); alert('Giao dịch được tạo tự động khi thanh toán hóa đơn. Vui lòng vào trang Hóa đơn để ghi nhận thu tiền.'); setIsAddOpen(false); }}>
            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1">Số tiền (VNĐ)</label>
              <div className="relative">
                <input 
                  type="number" 
                  value={form.amount}
                  onChange={e => setForm(f => ({ ...f, amount: e.target.value }))}
                  className="w-full px-6 py-4 bg-slate-50 border-none rounded-2xl text-2xl font-black text-slate-900 focus:ring-2 focus:ring-indigo-500/20 outline-none" 
                  placeholder="0" 
                  required
                  min="1"
                />
                <span className="absolute right-6 top-1/2 -translate-y-1/2 font-bold text-slate-400">đ</span>
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                <Tag className="w-4 h-4 text-indigo-500" /> Phân loại
              </label>
              <select 
                value={form.category}
                onChange={e => setForm(f => ({ ...f, category: e.target.value }))}
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none appearance-none bg-white"
              >
                <option value="">Chọn hạng mục...</option>
                <option value="rent">Tiền phòng</option>
                <option value="utility">Tiền điện nước</option>
                <option value="deposit">Tiền cọc</option>
                <option value="maintenance">Bảo trì/Sửa chữa</option>
                <option value="other">Khác</option>
              </select>
            </div>

            <div className="space-y-2">
               <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                <CreditCard className="w-4 h-4 text-indigo-500" /> Phương thức
              </label>
              <div className="grid grid-cols-2 gap-3">
                <button 
                  type="button" 
                  onClick={() => setForm(f => ({ ...f, paymentMethod: 'cash' }))}
                  className={`p-4 border-2 rounded-2xl text-center transition-all ${form.paymentMethod === 'cash' ? 'border-indigo-600 bg-indigo-50' : 'border-slate-200 opacity-60'}`}
                >
                  <Banknote className={`w-6 h-6 mx-auto mb-2 ${form.paymentMethod === 'cash' ? 'text-indigo-600' : 'text-slate-400'}`} />
                  <span className={`text-xs font-bold ${form.paymentMethod === 'cash' ? 'text-indigo-600' : 'text-slate-500'}`}>Tiền mặt</span>
                </button>
                <button 
                  type="button" 
                  onClick={() => setForm(f => ({ ...f, paymentMethod: 'bank_transfer' }))}
                  className={`p-4 border-2 rounded-2xl text-center transition-all ${form.paymentMethod === 'bank_transfer' ? 'border-indigo-600 bg-indigo-50' : 'border-slate-200 opacity-60'}`}
                >
                  <Landmark className={`w-6 h-6 mx-auto mb-2 ${form.paymentMethod === 'bank_transfer' ? 'text-indigo-600' : 'text-slate-400'}`} />
                  <span className={`text-xs font-bold ${form.paymentMethod === 'bank_transfer' ? 'text-indigo-600' : 'text-slate-500'}`}>Chuyển khoản</span>
                </button>
              </div>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-bold text-slate-700 ml-1 flex items-center gap-2">
                <FileText className="w-4 h-4 text-indigo-500" /> Ghi chú
              </label>
              <textarea 
                rows={4} 
                value={form.description}
                onChange={e => setForm(f => ({ ...f, description: e.target.value }))}
                className="w-full px-4 py-3 border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 outline-none resize-none text-sm" 
                placeholder="Nhập lý do thu/chi..." 
              />
            </div>

            <div className="flex gap-4 pt-6">
              <button 
                type="button"
                onClick={() => setIsAddOpen(false)}
                className="flex-1 py-4 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors"
              >
                Hủy bỏ
              </button>
              <button 
                type="submit"
                className="flex-[2] py-4 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 flex items-center justify-center gap-2"
              >
                Ghi sổ giao dịch
              </button>
            </div>
          </form>
        </div>
      </SlideOver>
    </div>
  );
}
