'use client';

import { useState } from 'react';
import { 
  Users, Search, Filter, MoreVertical, Phone, Mail, 
  MapPin, CreditCard, Home, FileText, Calendar, TrendingUp,
  AlertCircle, CheckCircle2, Clock, DollarSign
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customersApi, contractsApi, invoicesApi } from '@/lib/api';
import { Modal } from '@/components/shared/Modal';
import { StatCard, StatusBadge } from '@/components/shared';
import { Customer, Contract, Invoice } from '@/types';

export default function TenantsPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'expiring' | 'overdue'>('all');
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [selectedTenant, setSelectedTenant] = useState<(Customer & { contract?: Contract; invoices?: Invoice[] }) | null>(null);
  const [activeDetailTab, setActiveDetailTab] = useState<'profile' | 'contract' | 'invoices' | 'history'>('profile');

  // Queries
  const { data: customers = [], isLoading } = useQuery<Customer[]>({
    queryKey: ['customers'],
    queryFn: async () => {
      const resp = await customersApi.list({ hasActiveContract: true });
      const body = resp.data as Customer[] | { items: Customer[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: contracts = [] } = useQuery<Contract[]>({
    queryKey: ['contracts'],
    queryFn: async () => {
      const resp = await contractsApi.list({ status: 'active' });
      const body = resp.data as Contract[] | { items: Contract[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const { data: invoices = [] } = useQuery<Invoice[]>({
    queryKey: ['invoices'],
    queryFn: async () => {
      const resp = await invoicesApi.list();
      const body = resp.data as Invoice[] | { items: Invoice[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  // Enrich customers with contract and invoice data
  const tenantsWithData = customers
    .map(customer => ({
      ...customer,
      contract: contracts.find(c => c.customerId === customer.id),
      invoices: invoices.filter(i => invoices.some(inv => 
        contracts.find(c => c.customerId === customer.id && c.id === inv.contractId)
      ))
    }))
    .filter(tenant => {
      const searchLower = searchTerm.toLowerCase();
      const matchesSearch = 
        tenant.fullName.toLowerCase().includes(searchLower) ||
        tenant.phoneNumber.includes(searchTerm) ||
        tenant.contract?.contractCode?.includes(searchTerm) ||
        tenant.contract?.roomNumber?.includes(searchTerm);

      if (!matchesSearch) return false;

      if (statusFilter === 'all') return true;
      if (statusFilter === 'active') return tenant.contract?.status === 'active' || tenant.contract?.status === 'Active';
      if (statusFilter === 'expiring') {
        if (!tenant.contract?.endDate) return false;
        const days = Math.ceil((new Date(tenant.contract.endDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24));
        return days > 0 && days <= 30;
      }
      if (statusFilter === 'overdue') {
        const overdueInvoices = tenant.invoices?.filter(inv => 
          (inv.status === 'overdue' || inv.status === 'Overdue') && new Date(inv.dueDate) < new Date()
        ) ?? [];
        return overdueInvoices.length > 0;
      }
      return true;
    });

  // Statistics
  const stats = {
    total: customers.length,
    active: tenantsWithData.filter(t => t.contract?.status === 'active' || t.contract?.status === 'Active').length,
    expiring: tenantsWithData.filter(t => {
      if (!t.contract?.endDate) return false;
      const days = Math.ceil((new Date(t.contract.endDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24));
      return days > 0 && days <= 30;
    }).length,
    withOverdue: tenantsWithData.filter(t => 
      t.invoices?.some(inv => inv.status === 'overdue' || inv.status === 'Overdue')
    ).length,
  };

  const handleOpenDetail = (tenant: typeof tenantsWithData[0]) => {
    setSelectedTenant(tenant);
    setActiveDetailTab('profile');
    setIsDetailModalOpen(true);
  };

  return (
    <div className="space-y-8 pb-10">
      {/* Header Section */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Khách thuê</h1>
          <p className="text-slate-500 mt-1">Quản lý danh sách người thuê hiện tại và theo dõi hợp đồng của họ.</p>
        </div>
      </div>

      {/* Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <StatCard 
          title="Tổng khách thuê" 
          value={stats.total.toString()} 
          icon={Users}
          trend={{ value: 5, isPositive: true }}
        />
        <StatCard 
          title="Hợp đồng hoạt động" 
          value={stats.active.toString()} 
          icon={CheckCircle2}
          color="emerald"
        />
        <StatCard 
          title="Hợp đồng sắp hết hạn" 
          value={stats.expiring.toString()} 
          icon={Clock}
          color="amber"
        />
        <StatCard 
          title="Có hóa đơn quá hạn" 
          value={stats.withOverdue.toString()} 
          icon={AlertCircle}
          color="rose"
        />
      </div>

      {/* Search & Filter Bar */}
      <div className="flex flex-col md:flex-row gap-4 items-center bg-white/50 p-2 rounded-2xl border border-slate-100 backdrop-blur-sm">
        <div className="relative flex-1 group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
          <input 
            type="text" 
            placeholder="Tìm theo tên, số điện thoại, phòng hoặc mã hợp đồng..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-12 pr-4 py-3 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all shadow-sm"
          />
        </div>
        <div className="flex gap-2 flex-wrap">
          {(['all', 'active', 'expiring', 'overdue'] as const).map(filter => (
            <button
              key={filter}
              onClick={() => setStatusFilter(filter)}
              className={`px-4 py-2 rounded-lg font-medium transition-all ${
                statusFilter === filter
                  ? 'bg-indigo-600 text-white'
                  : 'bg-white border border-slate-200 text-slate-600 hover:border-indigo-300'
              }`}
            >
              {filter === 'all' && 'Tất cả'}
              {filter === 'active' && 'Đang thuê'}
              {filter === 'expiring' && 'Sắp hết hạn'}
              {filter === 'overdue' && 'Quá hạn'}
            </button>
          ))}
        </div>
      </div>

      {/* Tenant Grid */}
      {isLoading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2, 3, 4, 5, 6].map(i => (
            <div key={i} className="h-72 bg-white rounded-3xl border border-slate-100 animate-pulse shadow-sm" />
          ))}
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          <AnimatePresence mode="popLayout">
            {tenantsWithData.map((tenant) => {
              const daysUntilExpiry = tenant.contract?.endDate 
                ? Math.ceil((new Date(tenant.contract.endDate).getTime() - new Date().getTime()) / (1000 * 60 * 60 * 24))
                : null;
              const overdueInvoices = tenant.invoices?.filter(inv => 
                (inv.status === 'overdue' || inv.status === 'Overdue') && new Date(inv.dueDate) < new Date()
              ) ?? [];
              const overdueAmount = overdueInvoices.reduce((sum, inv) => sum + inv.totalAmount, 0);

              return (
                <motion.div
                  key={tenant.id}
                  layout
                  initial={{ opacity: 0, scale: 0.9 }}
                  animate={{ opacity: 1, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.9 }}
                  whileHover={{ y: -5 }}
                  className="group bg-white rounded-3xl border border-slate-100 p-6 shadow-sm hover:shadow-xl hover:shadow-indigo-500/5 transition-all cursor-pointer relative overflow-hidden"
                  onClick={() => handleOpenDetail(tenant)}
                >
                  {/* Decorative Background */}
                  <div className="absolute -right-4 -top-4 w-24 h-24 bg-indigo-50 rounded-full opacity-0 group-hover:opacity-100 transition-opacity" />
                  
                  <div className="flex items-start justify-between relative z-10 mb-4">
                    <div className="flex items-center gap-4 flex-1">
                      <div className="w-14 h-14 rounded-2xl bg-indigo-100 flex items-center justify-center text-indigo-600 font-bold text-xl uppercase shadow-inner">
                        {tenant.fullName.charAt(0)}
                      </div>
                      <div className="flex-1">
                        <h3 className="font-bold text-slate-900 group-hover:text-indigo-600 transition-colors line-clamp-1">
                          {tenant.fullName}
                        </h3>
                        <p className="text-xs text-slate-500">
                          {tenant.contract?.contractCode || 'Không có hợp đồng'}
                        </p>
                      </div>
                    </div>
                    <button className="p-2 hover:bg-slate-100 rounded-xl text-slate-400 transition-colors">
                      <MoreVertical className="w-5 h-5" />
                    </button>
                  </div>

                  {/* Contact Info */}
                  <div className="space-y-2 relative z-10 mb-4">
                    <div className="flex items-center gap-3 text-sm text-slate-600">
                      <div className="w-8 h-8 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400">
                        <Phone className="w-4 h-4" />
                      </div>
                      <span>{tenant.phoneNumber}</span>
                    </div>
                    <div className="flex items-center gap-3 text-sm text-slate-600">
                      <div className="w-8 h-8 rounded-lg bg-slate-50 flex items-center justify-center text-slate-400">
                        <Home className="w-4 h-4" />
                      </div>
                      <span className="font-medium">{tenant.contract?.roomNumber || 'N/A'}</span>
                    </div>
                  </div>

                  {/* Contract & Status Info */}
                  <div className="pt-4 border-t border-slate-100 relative z-10 space-y-3">
                    {daysUntilExpiry !== null && (
                      <div className="flex items-center justify-between">
                        <span className="text-xs text-slate-500">Hợp đồng hết hạn trong</span>
                        <StatusBadge status={daysUntilExpiry <= 7 ? 'danger' : daysUntilExpiry <= 30 ? 'warning' : 'success'}>
                          {daysUntilExpiry} ngày
                        </StatusBadge>
                      </div>
                    )}
                    {overdueAmount > 0 && (
                      <div className="flex items-center justify-between bg-rose-50 p-3 rounded-xl">
                        <span className="text-xs font-medium text-rose-700">Nợ quá hạn</span>
                        <span className="text-xs font-bold text-rose-600">{overdueAmount.toLocaleString('vi-VN')} ₫</span>
                      </div>
                    )}
                    {overdueAmount === 0 && daysUntilExpiry !== null && daysUntilExpiry > 7 && (
                      <div className="flex items-center justify-between text-xs text-slate-500">
                        <span>Trạng thái</span>
                        <StatusBadge status="success">Bình thường</StatusBadge>
                      </div>
                    )}
                  </div>
                </motion.div>
              );
            })}
          </AnimatePresence>
        </div>
      )}

      {tenantsWithData.length === 0 && !isLoading && (
        <div className="py-20 text-center bg-white rounded-3xl border border-dashed border-slate-200">
          <div className="w-16 h-16 bg-slate-50 rounded-full flex items-center justify-center mx-auto mb-4 text-slate-300">
            <Users className="w-8 h-8" />
          </div>
          <h3 className="text-lg font-bold text-slate-900">Không tìm thấy khách thuê nào</h3>
          <p className="text-slate-500 mt-1 max-w-md mx-auto">Không có khách thuê nào phù hợp với điều kiện lọc bạn chọn.</p>
        </div>
      )}

      {/* Tenant Detail Modal */}
      <Modal 
        isOpen={isDetailModalOpen} 
        onClose={() => setIsDetailModalOpen(false)} 
        title={`Thông tin khách thuê: ${selectedTenant?.fullName}`}
        size="lg"
      >
        {selectedTenant && (
          <div className="space-y-6">
            {/* Tab Navigation */}
            <div className="flex gap-2 border-b border-slate-200 mb-6 -mx-6 px-6">
              {(['profile', 'contract', 'invoices', 'history'] as const).map(tab => (
                <button
                  key={tab}
                  onClick={() => setActiveDetailTab(tab)}
                  className={`pb-3 px-1 font-medium text-sm border-b-2 transition-all ${
                    activeDetailTab === tab
                      ? 'border-indigo-600 text-indigo-600'
                      : 'border-transparent text-slate-500 hover:text-slate-700'
                  }`}
                >
                  {tab === 'profile' && 'Thông tin'}
                  {tab === 'contract' && 'Hợp đồng'}
                  {tab === 'invoices' && 'Hóa đơn'}
                  {tab === 'history' && 'Lịch sử'}
                </button>
              ))}
            </div>

            {/* Profile Tab */}
            {activeDetailTab === 'profile' && (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Họ tên</label>
                    <p className="text-sm text-slate-900">{selectedTenant.fullName}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Số điện thoại</label>
                    <p className="text-sm text-slate-900">{selectedTenant.phoneNumber}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">CMND/CCCD</label>
                    <p className="text-sm text-slate-900">{selectedTenant.idCardNumber}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Email</label>
                    <p className="text-sm text-slate-900">{selectedTenant.email || 'Chưa cập nhật'}</p>
                  </div>
                </div>
                <div>
                  <label className="block text-xs font-bold text-slate-600 mb-1">Địa chỉ hiện tại</label>
                  <p className="text-sm text-slate-900">{selectedTenant.currentAddress || 'Chưa cập nhật'}</p>
                </div>
                <div>
                  <label className="block text-xs font-bold text-slate-600 mb-1">Quê quán</label>
                  <p className="text-sm text-slate-900">{selectedTenant.hometown || 'Chưa cập nhật'}</p>
                </div>
                <div>
                  <label className="block text-xs font-bold text-slate-600 mb-1">Ghi chú</label>
                  <p className="text-sm text-slate-900">{selectedTenant.notes || 'Không có ghi chú'}</p>
                </div>
              </div>
            )}

            {/* Contract Tab */}
            {activeDetailTab === 'contract' && selectedTenant.contract && (
              <div className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Mã hợp đồng</label>
                    <p className="text-sm font-medium text-slate-900">{selectedTenant.contract.contractCode}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Phòng</label>
                    <p className="text-sm text-slate-900">{selectedTenant.contract.roomNumber}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Từ ngày</label>
                    <p className="text-sm text-slate-900">{new Date(selectedTenant.contract.startDate).toLocaleDateString('vi-VN')}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Đến ngày</label>
                    <p className="text-sm text-slate-900">{selectedTenant.contract.endDate ? new Date(selectedTenant.contract.endDate).toLocaleDateString('vi-VN') : 'Không xác định'}</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Tiền thuê/tháng</label>
                    <p className="text-sm font-medium text-slate-900">{(selectedTenant.contract.monthlyRent || 0).toLocaleString('vi-VN')} ₫</p>
                  </div>
                  <div>
                    <label className="block text-xs font-bold text-slate-600 mb-1">Tiền cọc</label>
                    <p className="text-sm text-slate-900">{selectedTenant.contract.depositAmount.toLocaleString('vi-VN')} ₫</p>
                  </div>
                </div>
                <div className="flex items-center justify-between p-3 bg-indigo-50 rounded-xl">
                  <span className="text-sm font-medium text-indigo-700">Trạng thái</span>
                  <StatusBadge status={selectedTenant.contract.status === 'active' || selectedTenant.contract.status === 'Active' ? 'success' : 'warning'}>
                    {selectedTenant.contract.status}
                  </StatusBadge>
                </div>
              </div>
            )}

            {/* Invoices Tab */}
            {activeDetailTab === 'invoices' && (
              <div className="space-y-3 max-h-96 overflow-y-auto">
                {selectedTenant.invoices && selectedTenant.invoices.length > 0 ? (
                  selectedTenant.invoices.map(invoice => (
                    <div key={invoice.id} className="p-3 bg-slate-50 rounded-lg border border-slate-100">
                      <div className="flex items-center justify-between mb-2">
                        <span className="font-medium text-sm text-slate-900">{invoice.invoiceCode}</span>
                        <StatusBadge status={
                          invoice.status === 'paid' || invoice.status === 'Paid' ? 'success' :
                          invoice.status === 'overdue' || invoice.status === 'Overdue' ? 'danger' :
                          'warning'
                        }>
                          {invoice.status}
                        </StatusBadge>
                      </div>
                      <div className="flex items-center justify-between text-xs text-slate-600">
                        <span>{new Date(invoice.billingMonth).toLocaleDateString('vi-VN', { month: 'long', year: 'numeric' })}</span>
                        <span className="font-bold">{invoice.totalAmount.toLocaleString('vi-VN')} ₫</span>
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="text-center text-slate-500 py-4">Không có hóa đơn nào</p>
                )}
              </div>
            )}

            {/* History Tab */}
            {activeDetailTab === 'history' && (
              <div className="space-y-3 max-h-96 overflow-y-auto">
                <p className="text-center text-slate-500 py-4">Chuyên năng lịch sử sẽ sớm có mặt</p>
              </div>
            )}
          </div>
        )}
      </Modal>
    </div>
  );
}
