'use client';

import { useState } from 'react';
import { 
  Receipt, Search, Filter, Download, Plus, 
  Send, DollarSign, Clock, CheckCircle2, AlertCircle,
  MoreVertical, FileText, Share2, Wallet
} from 'lucide-react';
import { DataTable } from '@/components/shared/DataTable';
import { StatusBadge } from '@/components/shared';

const MOCK_INVOICES = [
  { id: 'HD001', tenant: 'Nguyễn Văn A', room: '101', amount: 4500000, status: 'unpaid', dueDate: '2026-03-30', type: 'Phòng + Điện nước' },
  { id: 'HD002', tenant: 'Trần Thị B', room: '202', amount: 3800000, status: 'paid', dueDate: '2026-03-25', type: 'Phòng' },
  { id: 'HD003', tenant: 'Lê Văn C', room: '303', amount: 5200000, status: 'overdue', dueDate: '2026-03-15', type: 'Phòng + Dịch vụ' },
  { id: 'HD004', tenant: 'Phạm Minh D', room: '404', amount: 4100000, status: 'unpaid', dueDate: '2026-04-05', type: 'Phòng' },
];

export default function BillingPage() {
  const [filter, setFilter] = useState('all');

  const columns = [
    {
      key: 'id',
      label: 'Mã HĐ',
      render: (val: string) => <span className="font-black text-slate-400 group-hover:text-indigo-600 transition-colors uppercase">{val}</span>
    },
    {
      key: 'tenant',
      label: 'Khách thuê / Phòng',
      render: (val: string, row: any) => (
        <div className="space-y-0.5">
          <p className="font-black text-slate-800 text-sm">{val}</p>
          <p className="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Phòng {row.room}</p>
        </div>
      )
    },
    {
      key: 'amount',
      label: 'Số tiền',
      sortable: true,
      render: (val: number) => (
        <span className="font-black text-slate-900">{val.toLocaleString()}đ</span>
      )
    },
    {
      key: 'dueDate',
      label: 'Hạn thanh toán',
      render: (val: string, row: any) => {
        const isOverdue = row.status === 'overdue';
        return (
          <div className="flex items-center gap-2">
            <Clock className={`w-3.5 h-3.5 ${isOverdue ? 'text-rose-500' : 'text-slate-400'}`} />
            <span className={`text-xs font-bold ${isOverdue ? 'text-rose-600' : 'text-slate-600'}`}>{val}</span>
          </div>
        );
      }
    },
    {
      key: 'status',
      label: 'Trạng thái',
      render: (val: string) => {
        const statusMap: any = {
          paid: { label: 'Đã thu', type: 'success', icon: CheckCircle2 },
          unpaid: { label: 'Chờ thu', type: 'warning', icon: AlertCircle },
          overdue: { label: 'Quá hạn', type: 'error', icon: AlertCircle },
        };
        const s = statusMap[val];
        return (
          <div className={`flex items-center gap-1.5 px-3 py-1 rounded-full w-fit ${
            s.type === 'success' ? 'bg-emerald-100 text-emerald-700' : 
            s.type === 'warning' ? 'bg-amber-100 text-amber-700' : 
            'bg-rose-100 text-rose-700'
          }`}>
             <s.icon className="w-3.5 h-3.5" />
             <span className="text-[10px] font-black uppercase tracking-widest">{s.label}</span>
          </div>
        );
      }
    },
    {
      key: 'actions',
      label: '',
      render: (_: any, row: any) => (
        <div className="flex items-center gap-2">
          {row.status !== 'paid' && (
            <button title="Gửi nhắc nợ Zalo" className="p-2.5 bg-indigo-50 text-indigo-600 rounded-xl hover:bg-indigo-600 hover:text-white transition-all shadow-sm">
              <Share2 className="w-4 h-4" />
            </button>
          )}
          <button title="Thu tiền nhanh" className="p-2.5 bg-emerald-50 text-emerald-600 rounded-xl hover:bg-emerald-600 hover:text-white transition-all shadow-sm">
            <DollarSign className="w-4 h-4" />
          </button>
          <button className="p-2.5 text-slate-400 hover:text-slate-600 rounded-xl transition-all">
            <MoreVertical className="w-4 h-4" />
          </button>
        </div>
      )
    }
  ];

  return (
    <div className="space-y-8 pb-20">
      {/* Header & Stats */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-6">
        <div className="space-y-1">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">Hóa đơn & Thu tiền</h1>
          <p className="text-slate-500 font-medium italic">Quản lý các khoản thu và theo dõi tiến độ thanh toán.</p>
        </div>
        
        <div className="flex gap-3">
          <button className="flex items-center gap-2 px-6 py-3.5 bg-white border border-slate-100 text-slate-700 rounded-[1.25rem] font-black hover:bg-slate-50 transition-all shadow-sm">
            <Plus className="w-5 h-5 text-indigo-500" />
            Tạo hóa đơn
          </button>
          <button className="flex items-center gap-2 px-6 py-3.5 bg-slate-900 text-white rounded-[1.25rem] font-black hover:bg-black transition-all shadow-xl shadow-slate-200 group">
            <Receipt className="w-5 h-5 text-indigo-400 group-hover:rotate-12 transition-transform" />
            Chốt số hàng loạt
          </button>
        </div>
      </div>

      {/* Filter Tabs & Search */}
      <div className="flex flex-col md:flex-row items-center justify-between gap-4 p-4 bg-white rounded-3xl border border-slate-100 shadow-sm">
        <div className="flex items-center gap-2 p-1 bg-slate-100 rounded-2xl border border-slate-200 shadow-inner">
          {[
            { id: 'all', label: 'Tất cả' },
            { id: 'unpaid', label: 'Chờ thu' },
            { id: 'overdue', label: 'Quá hạn' },
            { id: 'paid', label: 'Đã thu' },
          ].map(tab => (
            <button
              key={tab.id}
              onClick={() => setFilter(tab.id)}
              className={`px-5 py-2 rounded-xl text-xs font-black transition-all ${
                filter === tab.id 
                ? 'bg-white text-indigo-600 shadow-sm' 
                : 'text-slate-500 hover:text-slate-700'
              }`}
            >
              {tab.label}
            </button>
          ))}
        </div>

        <div className="flex items-center gap-3 w-full md:w-auto">
          <div className="relative flex-1 md:w-[300px]">
            <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input 
              type="text" 
              placeholder="Tên khách, mã HĐ..."
              className="w-full pl-10 pr-4 py-3 bg-slate-50 border border-slate-100 rounded-xl text-xs font-bold outline-none focus:bg-white focus:ring-4 focus:ring-indigo-500/10 transition-all"
            />
          </div>
          <button className="p-3 bg-white border border-slate-100 rounded-xl text-slate-500 hover:text-indigo-600 transition-all">
            <Filter className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
         <div className="p-6 bg-white rounded-[2rem] border border-slate-100 shadow-sm space-y-3 relative overflow-hidden group">
            <div className="absolute -top-4 -right-4 w-16 h-16 bg-slate-50 rounded-full group-hover:scale-150 transition-transform duration-700" />
            <p className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng cần thu</p>
            <h4 className="text-2xl font-black text-slate-900">125.400.000đ</h4>
            <div className="flex items-center gap-1.5 text-[10px] font-bold text-slate-400">
               <Receipt className="w-3.5 h-3.5" /> 24 hóa đơn
            </div>
         </div>
         <div className="p-6 bg-white rounded-[2rem] border border-slate-100 shadow-sm space-y-3 relative overflow-hidden group">
            <div className="absolute -top-4 -right-4 w-16 h-16 bg-emerald-50 rounded-full group-hover:scale-150 transition-transform duration-700" />
            <p className="text-[10px] font-black text-emerald-400 uppercase tracking-widest">Đã thu hồi</p>
            <h4 className="text-2xl font-black text-emerald-600">85.200.000đ</h4>
            <div className="flex items-center gap-1.5 text-[10px] font-bold text-emerald-500">
               <CheckCircle2 className="w-3.5 h-3.5" /> 68% hoàn tất
            </div>
         </div>
         <div className="p-6 bg-white rounded-[2rem] border border-slate-100 shadow-sm space-y-3 relative overflow-hidden group">
            <div className="absolute -top-4 -right-4 w-16 h-16 bg-amber-50 rounded-full group-hover:scale-150 transition-transform duration-700" />
            <p className="text-[10px] font-black text-amber-500 uppercase tracking-widest">Chưa thanh toán</p>
            <h4 className="text-2xl font-black text-amber-600">32.200.000đ</h4>
            <div className="flex items-center gap-1.5 text-[10px] font-bold text-amber-500">
               <Clock className="w-3.5 h-3.5" /> 8 hóa đơn
            </div>
         </div>
         <div className="p-6 bg-white rounded-[2rem] border border-slate-100 shadow-sm space-y-3 relative overflow-hidden group">
            <div className="absolute -top-4 -right-4 w-16 h-16 bg-rose-50 rounded-full group-hover:scale-150 transition-transform duration-700" />
            <p className="text-[10px] font-black text-rose-500 uppercase tracking-widest">Nợ quá hạn</p>
            <h4 className="text-2xl font-black text-rose-600">7.000.000đ</h4>
            <div className="flex items-center gap-1.5 text-[10px] font-bold text-rose-400 underline cursor-help">
               <AlertCircle className="w-3.5 h-3.5" /> Thúc đẩy thu hồi
            </div>
         </div>
      </div>

      {/* Invoice Table Area */}
      <div className="bg-white rounded-[2.5rem] border border-slate-100 shadow-sm overflow-hidden p-6">
        <DataTable 
          columns={columns} 
          data={MOCK_INVOICES} 
        />
      </div>

      {/* Floating Action Bar (Batch Selection) */}
      <div className="fixed bottom-10 left-1/2 -translate-x-1/2 p-4 bg-slate-900 rounded-[2rem] shadow-2xl text-white flex items-center gap-6 border border-white/10 backdrop-blur-md z-50">
         <div className="flex items-center gap-3 px-2">
            <div className="w-10 h-10 rounded-xl bg-indigo-500/20 flex items-center justify-center text-indigo-400">
               <FileText className="w-5 h-5" />
            </div>
            <div>
               <p className="text-xs font-black">0 Hóa đơn đã chọn</p>
               <p className="text-[8px] font-bold text-slate-400 uppercase tracking-tight italic">Chọn HĐ để xử lý hàng loạt</p>
            </div>
         </div>
         <div className="h-10 w-[1px] bg-white/10" />
         <div className="flex gap-2">
            <button className="flex items-center gap-2 px-5 py-2.5 bg-emerald-500 text-white rounded-xl text-[10px] font-black hover:bg-emerald-600 transition-all">
              <CheckCircle2 className="w-3.5 h-3.5" /> Đã thu
            </button>
            <button className="flex items-center gap-2 px-5 py-2.5 bg-indigo-600 text-white rounded-xl text-[10px] font-black hover:bg-indigo-700 transition-all uppercase tracking-widest">
              <Share2 className="w-3.5 h-3.5" /> Gửi nhắc nợ ZALO
            </button>
            <button className="flex items-center gap-2 px-5 py-2.5 bg-white/10 text-white rounded-xl text-[10px] font-black hover:bg-white/20 transition-all">
              <Download className="w-3.5 h-3.5" /> Xuất Excel
            </button>
         </div>
      </div>
    </div>
  );
}
