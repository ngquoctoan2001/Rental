'use client';

import { useState } from 'react';
import { 
  FileText, Search, CreditCard, Send, CheckCircle2, 
  AlertCircle, Calendar, Zap, Droplets, ArrowRight,
  Calculator, Download, Copy, Trash2, Clock, Loader2
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { invoicesApi, contractsApi, transactionsApi } from '@/lib/api';
import { Modal } from '@/components/shared/Modal';
import { DataTable } from '@/components/shared/DataTable';
import { StatusBadge, StatCard } from '@/components/shared';
import { Invoice, Contract } from '@/types';
import { format } from 'date-fns';

export default function InvoicesPage() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<'billing' | 'meter'>('billing');
  const [selectedMonth, setSelectedMonth] = useState(format(new Date(), 'yyyy-MM'));
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);

  // Queries
  const { data: invoices = [], isLoading: isInvoicesLoading } = useQuery<Invoice[]>({
    queryKey: ['invoices', selectedMonth],
    queryFn: async () => {
      const resp = await invoicesApi.list({ month: selectedMonth });
      const body = resp.data as Invoice[] | { items: Invoice[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: contracts = [] } = useQuery<Contract[]>({
    queryKey: ['contracts-active'],
    queryFn: async () => {
      const resp = await contractsApi.list({ status: 'active' });
      const body = resp.data as Contract[] | { items: Contract[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  // Mutations
  const bulkGenerateMutation = useMutation({
    mutationFn: () => invoicesApi.bulkGenerate({ month: selectedMonth }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['invoices'] }),
  });

  const cashPaymentMutation = useMutation({
    mutationFn: (invoiceId: string) => transactionsApi.recordCash({ invoiceId, method: 'cash' }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      setIsDetailOpen(false);
    },
  });

  const sendInvoiceMutation = useMutation({
    mutationFn: (invoiceId: string) => invoicesApi.send(invoiceId),
  });

  const stats = {
    pending: invoices.filter((i: Invoice) => i.status === 'pending').length,
    overdue: invoices.filter((i: Invoice) => i.status === 'overdue').length,
    collected: invoices.filter((i: Invoice) => i.status === 'paid').reduce((sum: number, i: Invoice) => sum + i.totalAmount, 0),
  };

  const invoiceColumns = [
    {
      key: 'invoiceNumber',
      label: 'Số HĐ',
      render: (val: string, row: Invoice) => (
        <span className="font-mono text-xs font-bold text-slate-500">{val || `INV-${row.id.substring(0,6)}`}</span>
      )
    },
    {
      key: 'room',
      label: 'Phòng',
      render: (_: any, row: any) => (
        <span className="font-bold text-slate-900">{row.contract?.room?.roomNumber || 'N/A'}</span>
      )
    },
    {
      key: 'customer',
      label: 'Khách thuê',
      render: (_: any, row: any) => (
        <span className="text-sm text-slate-600">{row.contract?.customer?.fullName || 'Khách lẻ'}</span>
      )
    },
    {
      key: 'totalAmount',
      label: 'Tổng tiền',
      sortable: true,
      render: (val: number) => (
        <span className="font-bold text-slate-900">{val.toLocaleString()}đ</span>
      )
    },
    {
      key: 'status',
      label: 'Trạng thái',
      render: (val: string) => (
        <StatusBadge status={
          val === 'paid' ? 'paid' : 
          val === 'overdue' ? 'overdue' : 'pending'
        } />
      )
    },
    {
      key: 'actions',
      label: '',
      render: (_: any, row: Invoice) => (
        <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
          <button 
            onClick={(e) => { e.stopPropagation(); setSelectedInvoice(row); setIsDetailOpen(true); }}
            className="p-2 hover:bg-indigo-50 text-indigo-600 rounded-lg transition-colors"
          >
            <Send className="w-4 h-4" />
          </button>
          <button className="p-2 hover:bg-slate-100 text-slate-400 rounded-lg transition-colors">
            <Copy className="w-4 h-4" />
          </button>
        </div>
      )
    }
  ];

  return (
    <div className="space-y-8 pb-10">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 text-slate-800">
        <div>
          <h1 className="text-3xl font-extrabold tracking-tight">Hóa đơn & Thu phí</h1>
          <p className="text-slate-500 mt-1">Quản lý các khoản phí định kỳ, chốt số điện nước và theo dõi công nợ.</p>
        </div>
        <div className="flex items-center gap-3">
          <select 
            value={selectedMonth}
            onChange={(e) => setSelectedMonth(e.target.value)}
            className="px-4 py-3 bg-white border border-slate-200 rounded-2xl font-bold text-sm outline-none focus:ring-2 focus:ring-indigo-500/20"
          >
            <option value="2026-03">Tháng 03/2026</option>
            <option value="2026-02">Tháng 02/2026</option>
            <option value="2026-01">Tháng 01/2026</option>
          </select>
          <button
            onClick={() => { if (confirm(`Tạo hóa đơn tháng ${selectedMonth} cho tất cả hợp đồng đang hoạt động?`)) bulkGenerateMutation.mutate(); }}
            disabled={bulkGenerateMutation.isPending}
            className="flex items-center justify-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60"
          >
            {bulkGenerateMutation.isPending ? <Loader2 className="w-5 h-5 animate-spin" /> : <Calculator className="w-5 h-5" />}
            Tạo hóa đơn loạt
          </button>
        </div>
      </div>

      {/* Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard 
          title="Chờ thanh toán" 
          value={stats.pending.toString()} 
          icon={Clock}
          color="amber"
        />
        <StatCard 
          title="Nợ quá hạn" 
          value={stats.overdue.toString()} 
          icon={AlertCircle}
          color="rose"
        />
        <StatCard 
          title="Tổng đã thu (tháng)" 
          value={stats.collected.toLocaleString() + 'đ'} 
          icon={CheckCircle2}
          color="emerald"
        />
      </div>

      {/* Main Content Area */}
      <div className="bg-white rounded-[2.5rem] border border-slate-100 shadow-sm overflow-hidden">
        {/* Tabs Manager */}
        <div className="flex p-2 bg-slate-50/50 border-b border-slate-100">
          <button 
            onClick={() => setActiveTab('billing')}
            className={`flex-1 py-3 px-6 rounded-2xl text-sm font-bold transition-all flex items-center justify-center gap-2 ${activeTab === 'billing' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
          >
            <CreditCard className="w-4 h-4" />
            Danh sách hóa đơn
          </button>
          <button 
            onClick={() => setActiveTab('meter')}
            className={`flex-1 py-3 px-6 rounded-2xl text-sm font-bold transition-all flex items-center justify-center gap-2 ${activeTab === 'meter' ? 'bg-white text-indigo-600 shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
          >
            <Zap className="w-4 h-4" />
            Chốt số điện nước (Meter Reading)
          </button>
        </div>

        <div className="p-8">
          {activeTab === 'billing' ? (
            <DataTable 
              columns={invoiceColumns} 
              data={invoices} 
              isLoading={isInvoicesLoading}
              onRowClick={(row) => { setSelectedInvoice(row); setIsDetailOpen(true); }}
              searchPlaceholder="Tìm theo số phòng, tên khách..."
            />
          ) : (
            <div className="space-y-6">
              <div className="flex items-center justify-between mb-2">
                <div>
                  <h3 className="text-xl font-bold text-slate-900">Bảng chốt số tháng {selectedMonth}</h3>
                  <p className="text-sm text-slate-500">Nhập chỉ số mới nhất để hệ thống tự động tính thành tiền.</p>
                </div>
                <button className="flex items-center gap-2 px-4 py-2 bg-emerald-600 text-white rounded-xl text-sm font-bold hover:bg-emerald-700 transition-colors">
                  <CheckCircle2 className="w-4 h-4" />
                  Xác nhận & Gửi hóa đơn
                </button>
              </div>

              <div className="overflow-x-auto custom-scrollbar border border-slate-100 rounded-3xl shadow-sm">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="bg-slate-50 border-b border-slate-100">
                      <th className="px-6 py-4 text-xs font-bold text-slate-500 uppercase">Phòng</th>
                      <th className="px-6 py-4 text-xs font-bold text-indigo-600 uppercase">Điện (Cũ)</th>
                      <th className="px-6 py-4 text-xs font-bold text-indigo-600 uppercase">Điện (Mới)</th>
                      <th className="px-6 py-4 text-xs font-bold text-blue-600 uppercase">Nước (Cũ)</th>
                      <th className="px-6 py-4 text-xs font-bold text-blue-600 uppercase">Nước (Mới)</th>
                      <th className="px-6 py-4 text-xs font-bold text-slate-800 uppercase">Dự toán phí</th>
                    </tr>
                  </thead>
                  <tbody>
                    {contracts.map((contract: Contract) => (
                      <tr key={contract.id} className="border-b border-slate-50 hover:bg-slate-50/30 transition-colors">
                        <td className="px-6 py-4 font-bold text-slate-900">Phòng {contract.room?.roomNumber}</td>
                        <td className="px-6 py-4 text-slate-400 font-mono text-sm underline decoration-slate-200">1,245</td>
                        <td className="px-6 py-4">
                          <input 
                            type="number" 
                            className="w-24 px-3 py-1.5 bg-indigo-50 border border-indigo-100 rounded-lg focus:ring-2 focus:ring-indigo-500/20 outline-none font-bold text-indigo-600 text-center" 
                            placeholder="..."
                          />
                        </td>
                        <td className="px-6 py-4 text-slate-400 font-mono text-sm underline decoration-slate-200">352</td>
                        <td className="px-6 py-4">
                          <input 
                            type="number" 
                            className="w-24 px-3 py-1.5 bg-blue-50 border border-blue-100 rounded-lg focus:ring-2 focus:ring-blue-500/20 outline-none font-bold text-blue-600 text-center" 
                            placeholder="..."
                          />
                        </td>
                        <td className="px-6 py-4">
                          <span className="font-bold text-slate-900">{(contract.monthlyPrice + 150000).toLocaleString()}đ</span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Invoice Detail Modal */}
      <Modal isOpen={isDetailOpen} onClose={() => setIsDetailOpen(false)} title="Hóa đơn dịch vụ" size="lg">
        {selectedInvoice && (
          <div className="space-y-8">
            <div className="flex justify-between items-start">
              <div className="space-y-1">
                <h3 className="text-2xl font-black text-slate-900 tracking-tight">INV-012938</h3>
                <p className="text-sm text-slate-500 italic">Ngày tạo: 12/03/2026</p>
              </div>
              <StatusBadge status={selectedInvoice.status === 'paid' ? 'paid' : 'pending'} />
            </div>

            <div className="grid grid-cols-2 gap-8 p-6 bg-slate-50 rounded-3xl border border-slate-100">
              <div className="space-y-4">
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Phòng</p>
                  <p className="font-bold text-slate-800">204 - Tòa nhà A</p>
                </div>
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Khách hàng</p>
                  <p className="font-bold text-slate-800">Nguyễn Quốc Toản</p>
                </div>
              </div>
              <div className="space-y-4">
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Kỳ thanh toán</p>
                  <p className="font-bold text-slate-800">Tháng 03/2026</p>
                </div>
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Hạn chót</p>
                  <p className="font-bold text-rose-600 underline decoration-rose-200 underline-offset-4">15/03/2026</p>
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <h4 className="font-bold text-slate-900 border-b border-slate-100 pb-2 flex items-center justify-between">
                Chi tiết dịch vụ
                <span className="text-xs font-normal text-slate-400">Đơn vị: VNĐ</span>
              </h4>
              <div className="space-y-3">
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Tiền phòng tháng 03</span>
                  <span className="font-bold text-slate-800">3.500.000</span>
                </div>
                <div className="flex justify-between items-center py-2 text-sm">
                  <div className="flex flex-col">
                    <span className="text-slate-600 font-medium flex items-center gap-1.5 line-through decoration-slate-200">Tiền điện</span>
                    <span className="text-[10px] text-indigo-500 font-bold">(1245 {`->`} 1312) x 3.500</span>
                  </div>
                  <span className="font-bold text-slate-800">234.500</span>
                </div>
                <div className="flex justify-between items-center py-2 text-sm">
                   <div className="flex flex-col">
                    <span className="text-slate-600 font-medium flex items-center gap-1.5 line-through decoration-slate-200">Tiền nước</span>
                    <span className="text-[10px] text-blue-500 font-bold">(352 {`->`} 364) x 15.000</span>
                  </div>
                  <span className="font-bold text-slate-800">180.000</span>
                </div>
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Phí rác & Internet</span>
                  <span className="font-bold text-slate-800">100.000</span>
                </div>
              </div>
              <div className="mt-4 pt-6 border-t-2 border-dashed border-slate-100 flex justify-between items-center">
                <span className="text-lg font-black text-slate-900 uppercase italic tracking-tighter">Tổng cộng</span>
                <span className="text-2xl font-black text-indigo-600">4.014.500 <span className="text-xs font-bold underline">đ</span></span>
              </div>
            </div>

            <div className="flex gap-4 pt-6">
              <button className="flex-1 py-4 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors flex items-center justify-center gap-2">
                <Download className="w-4 h-4" /> Bản in PDF
              </button>
              <button
                onClick={() => sendInvoiceMutation.mutate(selectedInvoice.id)}
                disabled={sendInvoiceMutation.isPending}
                className="flex-1 py-4 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 flex items-center justify-center gap-2 disabled:opacity-60"
              >
                {sendInvoiceMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
                Gửi nhắc nợ
              </button>
            </div>
            
            <button
              onClick={() => { if (confirm('Xác nhận đã nhận tiền mặt?')) cashPaymentMutation.mutate(selectedInvoice.id); }}
              disabled={cashPaymentMutation.isPending || selectedInvoice.status === 'paid'}
              className="w-full py-4 bg-emerald-600 text-white rounded-2xl font-extrabold hover:bg-emerald-700 transition-all shadow-lg shadow-emerald-100 flex items-center justify-center gap-3 mt-4 text-lg disabled:opacity-60"
            >
              {cashPaymentMutation.isPending ? <Loader2 className="w-6 h-6 animate-spin" /> : <CheckCircle2 className="w-6 h-6" />}
              {selectedInvoice.status === 'paid' ? 'ĐÃ THANH TOÁN' : 'Xác nhận THU TIỀN MẶT'}
            </button>
          </div>
        )}
      </Modal>
    </div>
  );
}
