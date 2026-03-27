'use client';

import { useState } from 'react';
import { 
  UserPlus, Search, Filter, MoreVertical, Phone, Mail, 
  MapPin, CreditCard, ShieldAlert, History, FileText, 
  Upload, Camera, CheckCircle2, AlertCircle, FileUp
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customersApi } from '@/lib/api';
import { Modal } from '@/components/shared/Modal';
import { StatCard, StatusBadge } from '@/components/shared';
import { Customer } from '@/types';

export default function CustomersPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedCustomer, setSelectedCustomer] = useState<Customer | null>(null);
  const [addStep, setAddStep] = useState<'method' | 'ocr' | 'form'>('method');
  const [isOcrLoading, setIsOcrLoading] = useState(false);
  const [isImportModalOpen, setIsImportModalOpen] = useState(false);
  const [importResult, setImportResult] = useState<{ imported: number; skipped: number; errors: string[] } | null>(null);
  const [form, setForm] = useState({
    fullName: '', idCardNumber: '', phoneNumber: '',
    email: '', address: '', notes: '',
  });

  const resetForm = () => setForm({ fullName: '', idCardNumber: '', phoneNumber: '', email: '', address: '', notes: '' });

  // Queries
  const { data: customers = [], isLoading } = useQuery<Customer[]>({
    queryKey: ['customers'],
    queryFn: async () => {
      const resp = await customersApi.list();
      const body = resp.data as Customer[] | { items: Customer[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const filteredCustomers = customers.filter(c => 
    c.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    c.phoneNumber.includes(searchTerm) ||
    c.idCardNumber.includes(searchTerm)
  );

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: any) => customersApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      setIsAddModalOpen(false);
      setAddStep('method');
      resetForm();
    }
  });

  const importCsvMutation = useMutation({
    mutationFn: (file: File) => customersApi.importCsv(file),
    onSuccess: (resp) => {
      queryClient.invalidateQueries({ queryKey: ['customers'] });
      setImportResult(resp.data as any);
    },
  });

  const handleOpenDetail = (customer: Customer) => {
    setSelectedCustomer(customer);
    setIsDetailModalOpen(true);
  };

  const handleOcrUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setIsOcrLoading(true);
    try {
      const resp = await customersApi.ocrIdCard(file);
      const ocr = resp.data as any;
      setForm(f => ({
        ...f,
        fullName: ocr.fullName ?? f.fullName,
        idCardNumber: ocr.idCardNumber ?? f.idCardNumber,
        address: ocr.address ?? ocr.hometown ?? f.address,
      }));
      setAddStep('form');
    } catch {
      setAddStep('form'); // fall through to manual form on error
    } finally {
      setIsOcrLoading(false);
    }
  };

  return (
    <div className="space-y-8 pb-10">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Khách thuê</h1>
          <p className="text-slate-500 mt-1">Quản lý danh sách người thuê và thông tin định danh.</p>
        </div>
        <div className="flex gap-3 flex-wrap">
          <button
            onClick={() => { setImportResult(null); setIsImportModalOpen(true); }}
            className="flex items-center justify-center gap-2 px-5 py-3 bg-slate-100 text-slate-700 rounded-2xl font-bold hover:bg-slate-200 transition-all"
          >
            <FileUp className="w-5 h-5" />
            Nhập CSV
          </button>
          <button 
            onClick={() => {
              setAddStep('method');
              setIsAddModalOpen(true);
            }}
            className="flex items-center justify-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 hover:scale-[1.02] active:scale-[0.98]"
          >
            <UserPlus className="w-5 h-5" />
            Thêm khách mới
          </button>
        </div>
      </div>

      {/* Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <StatCard 
          title="Tổng khách thuê" 
          value={customers.length.toString()} 
          icon={History}
          trend={{ value: 12, isPositive: true }}
        />
        <StatCard 
          title="Đang thuê" 
          value={customers.filter(c => !c.isBlacklisted).length.toString()} 
          icon={CheckCircle2}
          color="emerald"
        />
        <StatCard 
          title="Trong danh sách đen" 
          value={customers.filter(c => c.isBlacklisted).length.toString()} 
          icon={ShieldAlert}
          color="rose"
        />
      </div>

      {/* Search & Filter Bar */}
      <div className="flex flex-col md:flex-row gap-4 items-center bg-white/50 p-2 rounded-2xl border border-slate-100 backdrop-blur-sm">
        <div className="relative flex-1 group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
          <input 
            type="text" 
            placeholder="Tìm theo tên, số điện thoại hoặc CMND/CCCD..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-12 pr-4 py-3 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all shadow-sm"
          />
        </div>
        <button className="flex items-center gap-2 px-5 py-3 bg-white border border-slate-200 rounded-xl font-medium text-slate-600 hover:bg-slate-50 transition-all">
          <Filter className="w-4 h-4" />
          Bộ lọc nâng cao
        </button>
      </div>

      {/* Customer Grid */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {[1, 2, 3, 4, 5, 6].map(i => (
            <div key={i} className="h-64 bg-white rounded-3xl border border-slate-100 animate-pulse shadow-sm" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <AnimatePresence mode="popLayout">
            {filteredCustomers.map((customer) => (
              <motion.div
                key={customer.id}
                layout
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                exit={{ opacity: 0, scale: 0.9 }}
                whileHover={{ y: -5 }}
                className="group bg-white rounded-3xl border border-slate-100 p-6 shadow-sm hover:shadow-xl hover:shadow-indigo-500/5 transition-all cursor-pointer relative overflow-hidden"
                onClick={() => handleOpenDetail(customer)}
              >
                {/* Decorative Background Element */}
                <div className="absolute -right-4 -top-4 w-24 h-24 bg-indigo-50 rounded-full opacity-0 group-hover:opacity-100 transition-opacity" />
                
                <div className="flex items-start justify-between relative z-10">
                  <div className="flex items-center gap-4">
                    <div className="w-14 h-14 rounded-2xl bg-indigo-100 flex items-center justify-center text-indigo-600 font-bold text-xl uppercase shadow-inner">
                      {customer.fullName.charAt(0)}
                    </div>
                    <div>
                      <h3 className="font-bold text-slate-900 group-hover:text-indigo-600 transition-colors line-clamp-1">{customer.fullName}</h3>
                      <div className="flex items-center gap-1.5 text-slate-500 text-xs mt-1">
                        <StatusBadge status={customer.isBlacklisted ? 'danger' : 'success'}>
                          {customer.isBlacklisted ? 'Blacklist' : 'Đang thuê'}
                        </StatusBadge>
                      </div>
                    </div>
                  </div>
                  <button className="p-2 hover:bg-slate-100 rounded-xl text-slate-400 transition-colors">
                    <MoreVertical className="w-5 h-5" />
                  </button>
                </div>

                <div className="mt-6 space-y-3 relative z-10">
                  <div className="flex items-center gap-3 text-sm text-slate-600">
                    <div className="w-8 h-8 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400">
                      <Phone className="w-4 h-4" />
                    </div>
                    <span>{customer.phoneNumber}</span>
                  </div>
                  <div className="flex items-center gap-3 text-sm text-slate-600">
                    <div className="w-8 h-8 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400">
                      <CreditCard className="w-4 h-4" />
                    </div>
                    <span>{customer.idCardNumber}</span>
                  </div>
                  <div className="flex items-center gap-3 text-sm text-slate-600">
                    <div className="w-8 h-8 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400">
                      <MapPin className="w-4 h-4" />
                    </div>
                    <span className="line-clamp-1">{customer.address || 'Chưa cập nhật địa chỉ'}</span>
                  </div>
                </div>

                <div className="mt-6 pt-4 border-t border-slate-50 flex items-center justify-between relative z-10">
                  <div className="flex -space-x-2">
                    {[1, 2].map(i => (
                      <div key={i} className="w-7 h-7 rounded-full border-2 border-white bg-slate-200" />
                    ))}
                    <div className="w-7 h-7 rounded-full border-2 border-white bg-indigo-600 text-[10px] text-white flex items-center justify-center font-bold">+1</div>
                  </div>
                  <span className="text-xs text-slate-400 font-medium">Cập nhật 2 ngày trước</span>
                </div>
              </motion.div>
            ))}
          </AnimatePresence>
        </div>
      )}

      {filteredCustomers.length === 0 && !isLoading && (
        <div className="py-20 text-center bg-white rounded-3xl border border-dashed border-slate-200">
          <div className="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-4 text-slate-300">
            <History className="w-8 h-8" />
          </div>
          <h3 className="text-lg font-bold text-slate-900">Không tìm thấy khách hàng nào</h3>
          <p className="text-slate-500 mt-1 max-w-md mx-auto">Thử thay đổi từ khóa tìm kiếm hoặc thêm mới một khách thuê để bắt đầu.</p>
        </div>
      )}

      {/* Add Customer Modal */}
      <Modal 
        isOpen={isAddModalOpen} 
        onClose={() => setIsAddModalOpen(false)} 
        title="Tiếp nhận khách mới"
        size="lg"
      >
        {addStep === 'method' && (
          <div className="space-y-6">
            <p className="text-slate-600 text-center mb-8">Chọn phương thức nhập liệu để tiết kiệm thời gian nhất cho bạn.</p>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <button 
                onClick={() => setAddStep('ocr')}
                className="group flex flex-col items-center p-8 bg-indigo-50/50 rounded-3xl border-2 border-indigo-100 hover:border-indigo-600 hover:bg-white transition-all text-center"
              >
                <div className="w-16 h-16 rounded-2xl bg-indigo-600 text-white flex items-center justify-center mb-4 shadow-lg group-hover:scale-110 transition-transform">
                  <Camera className="w-8 h-8" />
                </div>
                <h4 className="font-bold text-slate-900 text-lg">Quét CCCD (AI)</h4>
                <p className="text-sm text-slate-500 mt-2">Tự động nhận diện thông tin từ ảnh chụp nhanh chóng.</p>
              </button>
              <button 
                onClick={() => setAddStep('form')}
                className="group flex flex-col items-center p-8 bg-slate-50 rounded-3xl border-2 border-slate-100 hover:border-indigo-600 hover:bg-white transition-all text-center"
              >
                <div className="w-16 h-16 rounded-2xl bg-slate-200 text-slate-500 flex items-center justify-center mb-4 group-hover:bg-indigo-600 group-hover:text-white transition-all group-hover:scale-110">
                  <FileText className="w-8 h-8" />
                </div>
                <h4 className="font-bold text-slate-900 text-lg">Nhập thủ công</h4>
                <p className="text-sm text-slate-500 mt-2">Điền thông tin khách thuê theo cách truyền thống.</p>
              </button>
            </div>
          </div>
        )}

        {addStep === 'ocr' && (
          <div className="space-y-6">
            <div className="border-2 border-dashed border-indigo-200 rounded-3xl p-12 text-center bg-indigo-50/20">
              {isOcrLoading ? (
                <div className="space-y-4">
                  <div className="w-16 h-16 border-4 border-indigo-600 border-t-transparent rounded-full animate-spin mx-auto" />
                  <p className="text-indigo-600 font-bold animate-pulse">Đang phân tích thông tin bằng AI...</p>
                </div>
              ) : (
                <>
                  <Upload className="w-12 h-12 text-indigo-400 mx-auto mb-4" />
                  <h4 className="font-bold text-slate-900">Kéo thả ảnh CCCD vào đây</h4>
                  <p className="text-sm text-slate-500 mt-2 mb-6 text-balance">Hệ thống hỗ trợ ảnh .jpg, .png. Đảm bảo ảnh rõ nét để AI nhận diện chính xác nhất.</p>
                  <label className="inline-flex items-center px-6 py-3 bg-indigo-600 text-white rounded-xl font-bold cursor-pointer hover:bg-indigo-700 transition-colors shadow-lg shadow-indigo-100">
                    Chọn ảnh từ máy tính
                    <input type="file" accept="image/*" className="hidden" onChange={handleOcrUpload} />
                  </label>
                  <button onClick={() => setAddStep('method')} className="block w-full mt-4 text-sm text-slate-400 hover:text-slate-600">Quay lại</button>
                </>
              )}
            </div>
          </div>
        )}

        {addStep === 'form' && (
          <form className="space-y-6" onSubmit={(e) => { e.preventDefault(); createMutation.mutate(form); }}>
             <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1">Họ và tên</label>
                <input required value={form.fullName} onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" placeholder="Nguyễn Văn A" />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1">Số CMND/CCCD</label>
                <input required value={form.idCardNumber} onChange={e => setForm(f => ({ ...f, idCardNumber: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" placeholder="012345678901" />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1">Số điện thoại</label>
                <input required value={form.phoneNumber} onChange={e => setForm(f => ({ ...f, phoneNumber: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" placeholder="0901234567" />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1">Email (Nếu có)</label>
                <input value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" placeholder="nguyenvana@gmail.com" />
              </div>
              <div className="md:col-span-2 space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1">Địa chỉ thường trú</label>
                <input value={form.address} onChange={e => setForm(f => ({ ...f, address: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none" placeholder="P.12, Q.Bình Thạnh, TP.HCM" />
              </div>
              <div className="md:col-span-2 space-y-2">
                <label className="text-sm font-bold text-slate-700 ml-1">Ghi chú</label>
                <textarea rows={3} value={form.notes} onChange={e => setForm(f => ({ ...f, notes: e.target.value }))} className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none resize-none" placeholder="Thêm lưu ý về khách hàng này..." />
              </div>
            </div>
            <div className="flex gap-4 pt-4">
              <button 
                type="button" 
                onClick={() => setAddStep('method')}
                className="flex-1 py-4 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors"
              >
                Hủy bỏ
              </button>
              <button 
                type="submit"
                className="flex-[2] py-4 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100"
              >
                Lưu thông tin
              </button>
            </div>
          </form>
        )}
      </Modal>

      {/* Import CSV Modal */}
      <Modal
        isOpen={isImportModalOpen}
        onClose={() => setIsImportModalOpen(false)}
        title="Nhập khách thuê từ CSV"
        size="md"
      >
        <div className="space-y-6">
          {!importResult ? (
            <>
              <div className="bg-slate-50 border border-dashed border-slate-300 rounded-2xl p-6 text-center">
                <FileUp className="w-10 h-10 text-slate-400 mx-auto mb-3" />
                <p className="text-sm font-bold text-slate-600 mb-1">Chọn file CSV để nhập</p>
                <p className="text-xs text-slate-400 mb-4">Định dạng: FullName, PhoneNumber, IdCardNumber, Email, Address</p>
                <label className="cursor-pointer inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 text-white rounded-xl text-sm font-bold hover:bg-indigo-700 transition-colors">
                  <Upload className="w-4 h-4" />
                  {importCsvMutation.isPending ? 'Đang nhập...' : 'Chọn file'}
                  <input
                    type="file"
                    accept=".csv,text/csv"
                    className="hidden"
                    disabled={importCsvMutation.isPending}
                    onChange={(e) => { const f = e.target.files?.[0]; if (f) importCsvMutation.mutate(f); }}
                  />
                </label>
              </div>
              <div className="bg-amber-50 border border-amber-100 rounded-xl p-4 text-xs text-amber-700">
                <p className="font-bold mb-1">Lưu ý định dạng file CSV:</p>
                <code className="block bg-white rounded p-2 text-slate-600 font-mono">
                  FullName,PhoneNumber,IdCardNumber,Email,Address<br/>
                  Nguyễn Văn A,0901234567,012345678901,a@mail.com,HCM
                </code>
              </div>
            </>
          ) : (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-emerald-50 rounded-2xl p-4 text-center">
                  <p className="text-3xl font-black text-emerald-700">{importResult.imported}</p>
                  <p className="text-sm text-emerald-500 font-bold">Nhập thành công</p>
                </div>
                <div className="bg-amber-50 rounded-2xl p-4 text-center">
                  <p className="text-3xl font-black text-amber-700">{importResult.skipped}</p>
                  <p className="text-sm text-amber-500 font-bold">Bỏ qua</p>
                </div>
              </div>
              {importResult.errors.length > 0 && (
                <div className="bg-rose-50 rounded-xl p-4 max-h-40 overflow-y-auto">
                  <p className="text-xs font-bold text-rose-600 mb-2">Chi tiết lỗi:</p>
                  {importResult.errors.map((err, i) => (
                    <p key={i} className="text-xs text-rose-500">{err}</p>
                  ))}
                </div>
              )}
              <button
                onClick={() => setIsImportModalOpen(false)}
                className="w-full py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-colors"
              >
                Đóng
              </button>
            </div>
          )}
        </div>
      </Modal>

      {/* Detail Modal */}
      <Modal
        isOpen={isDetailModalOpen}
        onClose={() => setIsDetailModalOpen(false)}
        title="Hồ sơ khách thuê"
        size="xl"
      >
        {selectedCustomer && (
          <div className="flex flex-col md:flex-row gap-8">
            <div className="w-full md:w-64 space-y-6">
              <div className="aspect-square rounded-3xl bg-indigo-100 flex items-center justify-center text-indigo-600 text-5xl font-bold border-4 border-indigo-50">
                {selectedCustomer.fullName.charAt(0)}
              </div>
              <div className="space-y-2">
                <button className="w-full py-2.5 px-4 bg-indigo-50 text-indigo-600 rounded-xl text-sm font-bold text-left border border-indigo-100">Thông tin cá nhân</button>
                <button className="w-full py-2.5 px-4 text-slate-600 rounded-xl text-sm font-medium text-left hover:bg-slate-50">Lịch sử hợp đồng</button>
                <button className="w-full py-2.5 px-4 text-slate-600 rounded-xl text-sm font-medium text-left hover:bg-slate-50">Hóa đơn đã đóng</button>
              </div>
              {selectedCustomer.isBlacklisted && (
                <div className="p-4 bg-rose-50 border border-rose-100 rounded-2xl flex gap-3 items-start">
                  <ShieldAlert className="w-5 h-5 text-rose-600 shrink-0" />
                  <div>
                    <h5 className="text-sm font-bold text-rose-900 uppercase tracking-wider">Blacklisted</h5>
                    <p className="text-xs text-rose-600 mt-1 leading-relaxed">Khách hàng này đang nằm trong danh sách hạn chế của hệ thống.</p>
                  </div>
                </div>
              )}
            </div>
            <div className="flex-1 space-y-8">
              <div className="bg-slate-50 rounded-3xl p-6 border border-slate-100 grid grid-cols-2 gap-y-6">
                <div>
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">Họ và tên</p>
                  <p className="font-bold text-slate-800 text-lg">{selectedCustomer.fullName}</p>
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">Số CCCD</p>
                  <p className="font-bold text-slate-800 text-lg">{selectedCustomer.idCardNumber}</p>
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">Số điện thoại</p>
                  <p className="font-bold text-slate-800 text-lg">{selectedCustomer.phoneNumber}</p>
                </div>
                <div>
                  <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">Email</p>
                  <p className="font-bold text-slate-800 text-lg">{selectedCustomer.email || 'N/A'}</p>
                </div>
              </div>

              <div>
                <h4 className="font-bold text-slate-900 mb-4 flex items-center gap-2">
                  <FileText className="w-5 h-5 text-indigo-600" />
                  Hợp đồng đang thực hiện
                </h4>
                <div className="p-4 bg-white border border-slate-100 rounded-2xl flex items-center justify-between shadow-sm">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 rounded-xl bg-indigo-50 flex items-center justify-center text-indigo-600">
                      <History className="w-6 h-6" />
                    </div>
                    <div>
                      <p className="font-bold text-slate-800">Phòng 203 - Tòa nhà A</p>
                      <p className="text-sm text-slate-500">Bắt đầu: 12/01/2026 - Hết hạn: 12/01/2027</p>
                    </div>
                  </div>
                  <button className="px-4 py-2 bg-slate-900 text-white rounded-xl text-xs font-bold hover:bg-slate-800 transition-colors">Xem HĐ</button>
                </div>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
