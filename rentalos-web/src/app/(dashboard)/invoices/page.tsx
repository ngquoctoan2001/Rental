'use client';

import { useState, useMemo } from 'react';
import {
  FileText, CreditCard, Send, CheckCircle2,
  AlertCircle, Zap, Droplets, ArrowRight,
  Calculator, Download, Clock, Loader2, X, Plus
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { invoicesApi, transactionsApi, contractsApi } from '@/lib/api';
import { Modal } from '@/components/shared/Modal';
import { DataTable } from '@/components/shared/DataTable';
import { StatusBadge, StatCard } from '@/components/shared';
import { Invoice } from '@/types';
import { format } from 'date-fns';

function buildMonthOptions() {
  const now = new Date();
  return Array.from({ length: 12 }, (_, i) => {
    const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
    const value = format(d, 'yyyy-MM');
    const label = `Tháng ${d.getMonth() + 1}/${d.getFullYear()}`;
    return { value, label };
  });
}

interface MeterEntry {
  invoiceId: string;
  electricityNew: string;
  waterNew: string;
}

export default function InvoicesPage() {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<'billing' | 'meter'>('billing');
  const MONTHS = useMemo(() => buildMonthOptions(), []);
  const [selectedMonth, setSelectedMonth] = useState(MONTHS[0].value);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [cashAmount, setCashAmount] = useState('');
  const [showCashModal, setShowCashModal] = useState(false);
  const [meterEntries, setMeterEntries] = useState<Record<string, MeterEntry>>({});
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [createForm, setCreateForm] = useState({ contractId: '', otherFees: '', discount: '', notes: '' });

  const { data: invoices = [], isLoading: isInvoicesLoading } = useQuery<Invoice[]>({
    queryKey: ['invoices', selectedMonth],
    queryFn: async () => {
      const resp = await invoicesApi.list({ month: selectedMonth });
      const body = resp.data as Invoice[] | { items: Invoice[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: pendingMeterInvoices = [], isLoading: isMeterLoading } = useQuery<Invoice[]>({
    queryKey: ['invoices-pending-meter'],
    queryFn: async () => {
      const resp = await invoicesApi.pendingMeter();
      return Array.isArray(resp.data) ? resp.data : [];
    },
    enabled: activeTab === 'meter',
  });

  const bulkGenerateMutation = useMutation({
    mutationFn: () => invoicesApi.bulkGenerate({ billingMonth: selectedMonth + '-01', sendNotification: false, overwriteExisting: false }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['invoices'] }),
  });

  const createInvoiceMutation = useMutation({
    mutationFn: (data: any) => invoicesApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      setIsCreateOpen(false);
      setCreateForm({ contractId: '', otherFees: '', discount: '', notes: '' });
    },
  });

  const sendInvoiceMutation = useMutation({
    mutationFn: (invoiceId: string) => invoicesApi.send(invoiceId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['invoices'] }),
  });

  const cancelInvoiceMutation = useMutation({
    mutationFn: (invoiceId: string) => invoicesApi.cancel(invoiceId, { invoiceId, reason: 'Manual cancel' }),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['invoices'] }); setIsDetailOpen(false); },
  });

  const cashPaymentMutation = useMutation({
    mutationFn: ({ invoiceId, amount }: { invoiceId: string; amount: number }) =>
      transactionsApi.recordCash({ invoiceId, amount, paidAt: new Date().toISOString() }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
      setShowCashModal(false);
      setIsDetailOpen(false);
      setCashAmount('');
    },
  });

  const updateMeterMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => invoicesApi.updateMeter(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['invoices-pending-meter'] });
      queryClient.invalidateQueries({ queryKey: ['invoices'] });
    },
  });

  const handleBulkMeterSave = async () => {
    const entries = Object.values(meterEntries).filter(e => e.electricityNew || e.waterNew);
    if (entries.length === 0) return;
    for (const entry of entries) {
      await updateMeterMutation.mutateAsync({
        id: entry.invoiceId,
        data: {
          invoiceId: entry.invoiceId,
          electricityNew: entry.electricityNew ? Number(entry.electricityNew) : undefined,
          waterNew: entry.waterNew ? Number(entry.waterNew) : undefined,
        },
      });
    }
    setMeterEntries({});
  };

  const setMeterEntry = (invoiceId: string, field: 'electricityNew' | 'waterNew', value: string) => {
    setMeterEntries(prev => ({
      ...prev,
      [invoiceId]: { ...prev[invoiceId], invoiceId, [field]: value },
    }));
  };

  const stats = {
    pending: invoices.filter(i => ['pending', 'Pending'].includes(i.status)).length,
    overdue: invoices.filter(i => ['overdue', 'Overdue'].includes(i.status)).length,
    collected: invoices.filter(i => ['paid', 'Paid'].includes(i.status)).reduce((sum, i) => sum + i.totalAmount, 0),
  };

  const invoiceColumns = [
    {
      key: 'invoiceCode',
      label: 'Mã HĐ',
      render: (val: string, row: Invoice) => (
        <span className="font-mono text-xs font-bold text-slate-500">{val || row.invoiceNumber || `INV-${row.id.substring(0, 6)}`}</span>
      )
    },
    {
      key: 'roomNumber',
      label: 'Phòng',
      render: (val: string, row: any) => (
        <span className="font-bold text-slate-900">{val || row.contract?.room?.roomNumber || 'N/A'}</span>
      )
    },
    {
      key: 'customerName',
      label: 'Khách thuê',
      render: (val: string, row: any) => (
        <span className="text-sm text-slate-600">{val || row.contract?.customer?.fullName || 'Khách lẻ'}</span>
      )
    },
    {
      key: 'totalAmount',
      label: 'Tổng tiền',
      sortable: true,
      render: (val: number) => (
        <span className="font-bold text-slate-900">{val?.toLocaleString()}đ</span>
      )
    },
    {
      key: 'dueDate',
      label: 'Hạn',
      render: (val: string) => (
        <span className="text-sm text-slate-500">{val ? format(new Date(val), 'dd/MM/yyyy') : ''}</span>
      )
    },
    {
      key: 'status',
      label: 'Trạng thái',
      render: (val: string) => (
        <StatusBadge status={
          ['paid', 'Paid'].includes(val) ? 'paid' :
          ['overdue', 'Overdue'].includes(val) ? 'overdue' : 'pending'
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
        </div>
      )
    }
  ];

  const isPaid = (inv: Invoice) => ['paid', 'Paid'].includes(inv.status);

  return (
    <div className="space-y-8 pb-10">
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
            {MONTHS.map(m => <option key={m.value} value={m.value}>{m.label}</option>)}
          </select>
          <button
            onClick={() => setIsCreateOpen(true)}
            className="flex items-center justify-center gap-2 px-5 py-3 bg-white border border-slate-200 text-slate-700 rounded-2xl font-bold hover:bg-slate-50 transition-all"
          >
            <Plus className="w-5 h-5" />
            Tạo thủ công
          </button>
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

      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard title="Chờ thanh toán" value={stats.pending.toString()} icon={Clock} color="amber" />
        <StatCard title="Nợ quá hạn" value={stats.overdue.toString()} icon={AlertCircle} color="rose" />
        <StatCard title="Tổng đã thu (tháng)" value={stats.collected.toLocaleString() + 'đ'} icon={CheckCircle2} color="emerald" />
      </div>

      <div className="bg-white rounded-[2.5rem] border border-slate-100 shadow-sm overflow-hidden">
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
            Chốt số điện nước {pendingMeterInvoices.length > 0 && <span className="ml-1 px-2 py-0.5 bg-amber-100 text-amber-700 rounded-full text-[10px] font-black">{pendingMeterInvoices.length}</span>}
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
                  <h3 className="text-xl font-bold text-slate-900">Hóa đơn chờ chốt số</h3>
                  <p className="text-sm text-slate-500">Nhập chỉ số mới để hệ thống tính thành tiền và hoàn thành hóa đơn.</p>
                </div>
                <button
                  onClick={handleBulkMeterSave}
                  disabled={updateMeterMutation.isPending || Object.keys(meterEntries).length === 0}
                  className="flex items-center gap-2 px-4 py-2 bg-emerald-600 text-white rounded-xl text-sm font-bold hover:bg-emerald-700 transition-colors disabled:opacity-60"
                >
                  {updateMeterMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <CheckCircle2 className="w-4 h-4" />}
                  Lưu chỉ số đã nhập
                </button>
              </div>

              {isMeterLoading ? (
                <div className="flex items-center justify-center py-12"><Loader2 className="w-6 h-6 animate-spin text-indigo-500" /></div>
              ) : pendingMeterInvoices.length === 0 ? (
                <div className="text-center py-12 text-slate-400">Không có hóa đơn nào cần chốt số.</div>
              ) : (
                <div className="overflow-x-auto border border-slate-100 rounded-3xl shadow-sm">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="bg-slate-50 border-b border-slate-100">
                        <th className="px-6 py-4 text-xs font-bold text-slate-500 uppercase">Phòng / Khách</th>
                        <th className="px-6 py-4 text-xs font-bold text-indigo-600 uppercase flex items-center gap-1"><Zap className="w-3 h-3" /> Điện (Cũ)</th>
                        <th className="px-6 py-4 text-xs font-bold text-indigo-600 uppercase">Điện (Mới)</th>
                        <th className="px-6 py-4 text-xs font-bold text-blue-600 uppercase flex items-center gap-1"><Droplets className="w-3 h-3" /> Nước (Cũ)</th>
                        <th className="px-6 py-4 text-xs font-bold text-blue-600 uppercase">Nước (Mới)</th>
                      </tr>
                    </thead>
                    <tbody>
                      {pendingMeterInvoices.map((inv: Invoice) => (
                        <tr key={inv.id} className="border-b border-slate-50 hover:bg-slate-50/30 transition-colors">
                          <td className="px-6 py-4">
                            <p className="font-bold text-slate-900">Phòng {inv.roomNumber}</p>
                            <p className="text-xs text-slate-400">{inv.customerName}</p>
                          </td>
                          <td className="px-6 py-4 text-slate-400 font-mono text-sm">{(inv.electricityOld ?? 0).toLocaleString()}</td>
                          <td className="px-6 py-4">
                            <input
                              type="number"
                              min={inv.electricityOld ?? 0}
                              value={meterEntries[inv.id]?.electricityNew ?? ''}
                              onChange={e => setMeterEntry(inv.id, 'electricityNew', e.target.value)}
                              className="w-28 px-3 py-1.5 bg-indigo-50 border border-indigo-100 rounded-lg focus:ring-2 focus:ring-indigo-500/20 outline-none font-bold text-indigo-600 text-center"
                              placeholder="..."
                            />
                          </td>
                          <td className="px-6 py-4 text-slate-400 font-mono text-sm">{(inv.waterOld ?? 0).toLocaleString()}</td>
                          <td className="px-6 py-4">
                            <input
                              type="number"
                              min={inv.waterOld ?? 0}
                              value={meterEntries[inv.id]?.waterNew ?? ''}
                              onChange={e => setMeterEntry(inv.id, 'waterNew', e.target.value)}
                              className="w-28 px-3 py-1.5 bg-blue-50 border border-blue-100 rounded-lg focus:ring-2 focus:ring-blue-500/20 outline-none font-bold text-blue-600 text-center"
                              placeholder="..."
                            />
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* Invoice Detail Modal */}
      <Modal isOpen={isDetailOpen} onClose={() => setIsDetailOpen(false)} title="Chi tiết hóa đơn" size="lg">
        {selectedInvoice && (
          <div className="space-y-6">
            <div className="flex justify-between items-start">
              <div className="space-y-1">
                <h3 className="text-2xl font-black text-slate-900 tracking-tight">
                  {selectedInvoice.invoiceCode || selectedInvoice.invoiceNumber || `INV-${selectedInvoice.id.substring(0, 6)}`}
                </h3>
                {selectedInvoice.sentAt && (
                  <p className="text-sm text-slate-500 italic">Đã gửi: {format(new Date(selectedInvoice.sentAt), 'dd/MM/yyyy HH:mm')}</p>
                )}
              </div>
              <StatusBadge status={isPaid(selectedInvoice) ? 'paid' : ['overdue', 'Overdue'].includes(selectedInvoice.status) ? 'overdue' : 'pending'} />
            </div>

            <div className="grid grid-cols-2 gap-6 p-6 bg-slate-50 rounded-3xl border border-slate-100">
              <div className="space-y-3">
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Phòng</p>
                  <p className="font-bold text-slate-800">{selectedInvoice.roomNumber || 'N/A'} - {selectedInvoice.propertyName || ''}</p>
                </div>
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Khách hàng</p>
                  <p className="font-bold text-slate-800">{selectedInvoice.customerName || 'N/A'}</p>
                </div>
              </div>
              <div className="space-y-3">
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Kỳ thanh toán</p>
                  <p className="font-bold text-slate-800">
                    {selectedInvoice.billingMonth ? `Tháng ${new Date(selectedInvoice.billingMonth).getMonth() + 1}/${new Date(selectedInvoice.billingMonth).getFullYear()}` : ''}
                  </p>
                </div>
                <div>
                  <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Hạn chót</p>
                  <p className="font-bold text-rose-600">
                    {selectedInvoice.dueDate ? format(new Date(selectedInvoice.dueDate), 'dd/MM/yyyy') : ''}
                  </p>
                </div>
              </div>
            </div>

            <div className="space-y-3">
              <h4 className="font-bold text-slate-900 border-b border-slate-100 pb-2 flex items-center justify-between">
                Chi tiết dịch vụ
                <span className="text-xs font-normal text-slate-400">Đơn vị: VNĐ</span>
              </h4>
              {(selectedInvoice.roomRent ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Tiền phòng</span>
                  <span className="font-bold text-slate-800">{selectedInvoice.roomRent?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.electricityAmount ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <div>
                    <span className="text-slate-600 font-medium flex items-center gap-1.5"><Zap className="w-3 h-3 text-indigo-500" /> Tiền điện</span>
                    {selectedInvoice.electricityOld !== undefined && (
                      <span className="text-[10px] text-indigo-500 font-bold">
                        ({selectedInvoice.electricityOld} → {selectedInvoice.electricityNew}) x {selectedInvoice.electricityPrice?.toLocaleString()}đ
                      </span>
                    )}
                  </div>
                  <span className="font-bold text-slate-800">{selectedInvoice.electricityAmount?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.waterAmount ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <div>
                    <span className="text-slate-600 font-medium flex items-center gap-1.5"><Droplets className="w-3 h-3 text-blue-500" /> Tiền nước</span>
                    {selectedInvoice.waterOld !== undefined && (
                      <span className="text-[10px] text-blue-500 font-bold">
                        ({selectedInvoice.waterOld} → {selectedInvoice.waterNew}) x {selectedInvoice.waterPrice?.toLocaleString()}đ
                      </span>
                    )}
                  </div>
                  <span className="font-bold text-slate-800">{selectedInvoice.waterAmount?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.serviceFee ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Phí dịch vụ</span>
                  <span className="font-bold text-slate-800">{selectedInvoice.serviceFee?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.internetFee ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Internet</span>
                  <span className="font-bold text-slate-800">{selectedInvoice.internetFee?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.garbageFee ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Rác & vệ sinh</span>
                  <span className="font-bold text-slate-800">{selectedInvoice.garbageFee?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.otherFees ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-slate-600 font-medium">Phí khác{selectedInvoice.notes ? ` (${selectedInvoice.notes})` : ''}</span>
                  <span className="font-bold text-slate-800">{selectedInvoice.otherFees?.toLocaleString()}</span>
                </div>
              )}
              {(selectedInvoice.discount ?? 0) > 0 && (
                <div className="flex justify-between items-center py-2 text-sm">
                  <span className="text-emerald-600 font-medium">Giảm giá</span>
                  <span className="font-bold text-emerald-600">-{selectedInvoice.discount?.toLocaleString()}</span>
                </div>
              )}
              <div className="mt-4 pt-6 border-t-2 border-dashed border-slate-100 flex justify-between items-center">
                <span className="text-lg font-black text-slate-900 uppercase italic tracking-tighter">Tổng cộng</span>
                <span className="text-2xl font-black text-indigo-600">{selectedInvoice.totalAmount?.toLocaleString()} <span className="text-xs font-bold">đ</span></span>
              </div>
            </div>

            <div className="flex gap-4 pt-2">
              <button
                onClick={() => sendInvoiceMutation.mutate(selectedInvoice.id)}
                disabled={sendInvoiceMutation.isPending || isPaid(selectedInvoice)}
                className="flex-1 py-4 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 flex items-center justify-center gap-2 disabled:opacity-60"
              >
                {sendInvoiceMutation.isPending ? <Loader2 className="w-4 h-4 animate-spin" /> : <Send className="w-4 h-4" />}
                Gửi nhắc nợ
              </button>
              <button
                onClick={() => { if (confirm('Hủy hóa đơn này?')) cancelInvoiceMutation.mutate(selectedInvoice.id); }}
                disabled={cancelInvoiceMutation.isPending || isPaid(selectedInvoice)}
                className="py-4 px-5 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors disabled:opacity-60 flex items-center justify-center gap-2"
              >
                <X className="w-4 h-4" /> Hủy HĐ
              </button>
            </div>

            {!isPaid(selectedInvoice) && (
              <button
                onClick={() => { setCashAmount(String(selectedInvoice.totalAmount)); setShowCashModal(true); }}
                className="w-full py-4 bg-emerald-600 text-white rounded-2xl font-extrabold hover:bg-emerald-700 transition-all shadow-lg shadow-emerald-100 flex items-center justify-center gap-3 text-lg"
              >
                <CheckCircle2 className="w-6 h-6" />
                Thu tiền mặt
              </button>
            )}
            {isPaid(selectedInvoice) && (
              <div className="w-full py-4 bg-emerald-50 text-emerald-700 rounded-2xl font-extrabold flex items-center justify-center gap-3 text-lg border border-emerald-200">
                <CheckCircle2 className="w-6 h-6" />
                ĐÃ THANH TOÁN {selectedInvoice.paidAt ? `(${format(new Date(selectedInvoice.paidAt), 'dd/MM/yyyy')})` : ''}
              </div>
            )}
          </div>
        )}
      </Modal>

      {/* Cash Payment Modal */}
      {showCashModal && selectedInvoice && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-[2rem] shadow-2xl p-8 w-full max-w-sm space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-900">Xác nhận thu tiền mặt</h3>
              <button onClick={() => setShowCashModal(false)} className="p-2 hover:bg-slate-100 rounded-xl">
                <X className="w-5 h-5 text-slate-500" />
              </button>
            </div>
            <div className="space-y-4">
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Số tiền thực nhận (đ)</label>
                <input
                  type="number"
                  min={1}
                  value={cashAmount}
                  onChange={e => setCashAmount(e.target.value)}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-emerald-500/20 outline-none font-bold text-lg text-emerald-600"
                />
                <p className="text-xs text-slate-400 mt-1">Tổng hóa đơn: {selectedInvoice.totalAmount.toLocaleString()}đ</p>
              </div>
              <div className="flex gap-3">
                <button type="button" onClick={() => setShowCashModal(false)} className="flex-1 py-3 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200">
                  Hủy
                </button>
                <button
                  onClick={() => cashPaymentMutation.mutate({ invoiceId: selectedInvoice.id, amount: Number(cashAmount) })}
                  disabled={cashPaymentMutation.isPending || !cashAmount}
                  className="flex-1 py-3 bg-emerald-600 text-white rounded-2xl font-bold hover:bg-emerald-700 disabled:opacity-60 flex items-center justify-center gap-2"
                >
                  {cashPaymentMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
                  Xác nhận
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Manual Create Invoice Modal */}
      {isCreateOpen && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-[2rem] shadow-2xl p-8 w-full max-w-md space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-900">Tạo hóa đơn thủ công</h3>
              <button onClick={() => setIsCreateOpen(false)} className="p-2 hover:bg-slate-100 rounded-xl"><X className="w-5 h-5 text-slate-500" /></button>
            </div>
            <form onSubmit={(e) => { e.preventDefault(); createInvoiceMutation.mutate({ contractId: createForm.contractId || undefined, billingMonth: selectedMonth + '-01', otherFees: createForm.otherFees ? Number(createForm.otherFees) : undefined, discount: createForm.discount ? Number(createForm.discount) : undefined, notes: createForm.notes || undefined }); }} className="space-y-4">
              <ContractSelector value={createForm.contractId} onChange={(v) => setCreateForm(f => ({ ...f, contractId: v }))} />
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-bold text-slate-700 mb-2 block">Phí khác (đ)</label>
                  <input type="number" min={0} value={createForm.otherFees} onChange={e => setCreateForm(f => ({ ...f, otherFees: e.target.value }))} placeholder="0" className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" />
                </div>
                <div>
                  <label className="text-sm font-bold text-slate-700 mb-2 block">Giảm giá (đ)</label>
                  <input type="number" min={0} value={createForm.discount} onChange={e => setCreateForm(f => ({ ...f, discount: e.target.value }))} placeholder="0" className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" />
                </div>
              </div>
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Ghi chú</label>
                <textarea rows={2} value={createForm.notes} onChange={e => setCreateForm(f => ({ ...f, notes: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none resize-none text-sm" />
              </div>
              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => setIsCreateOpen(false)} className="flex-1 py-3 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200">Hủy</button>
                <button type="submit" disabled={createInvoiceMutation.isPending} className="flex-1 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 disabled:opacity-60 flex items-center justify-center gap-2">
                  {createInvoiceMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
                  Tạo hóa đơn
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

function ContractSelector({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const { data: contracts = [] } = useQuery({
    queryKey: ['contracts-active'],
    queryFn: () => contractsApi.list({ status: 'active' }).then(r => Array.isArray(r.data) ? r.data : (r.data as any)?.items ?? []),
  });
  return (
    <div>
      <label className="text-sm font-bold text-slate-700 mb-2 block">Hợp đồng</label>
      <select value={value} onChange={e => onChange(e.target.value)} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none bg-white text-sm">
        <option value="">-- Chọn hợp đồng --</option>
        {contracts.map((c: any) => (
          <option key={c.id} value={c.id}>{c.customer?.fullName ?? 'N/A'} - Phòng {c.room?.roomNumber ?? 'N/A'}</option>
        ))}
      </select>
    </div>
  );
}
