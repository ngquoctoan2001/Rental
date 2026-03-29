'use client';

import { useState } from 'react';
import { 
  Gauge, Search, Plus, MoreVertical, Zap, Droplet, Calendar,
  TrendingUp, Filter, Upload, Trash2, Edit2, Eye
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { meterReadingsApi, roomsApi } from '@/lib/api';
import { Modal } from '@/components/shared/Modal';
import { StatCard } from '@/components/shared';

export default function MeterReadingsPage() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [monthFilter, setMonthFilter] = useState('');
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [selectedReading, setSelectedReading] = useState<any>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  
  const [form, setForm] = useState({
    roomId: '',
    readingDate: new Date().toISOString().split('T')[0],
    electricityReading: 0,
    waterReading: 0,
    note: '',
  });

  // Queries
  const { data: meterData, isLoading } = useQuery({
    queryKey: ['meterReadings', page, pageSize, monthFilter],
    queryFn: async () => {
      const resp = await meterReadingsApi.list({
        page,
        pageSize,
        month: monthFilter,
      });
      return resp.data as any;
    }
  });

  const { data: rooms = [] } = useQuery({
    queryKey: ['rooms'],
    queryFn: async () => {
      const resp = await roomsApi.list();
      const body = resp.data as any[] | { items: any[] };
      return Array.isArray(body) ? body : body.items ?? [];
    }
  });

  const meterReadings = meterData?.items ?? [];
  const totalCount = meterData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / pageSize);

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: any) => meterReadingsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['meterReadings'] });
      setIsCreateModalOpen(false);
      setForm({
        roomId: '',
        readingDate: new Date().toISOString().split('T')[0],
        electricityReading: 0,
        waterReading: 0,
        note: '',
      });
    }
  });

  const updateMutation = useMutation({
    mutationFn: (data: any) => meterReadingsApi.update(selectedReading.id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['meterReadings'] });
      setIsDetailModalOpen(false);
      setSelectedReading(null);
    }
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => meterReadingsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['meterReadings'] });
      setIsDetailModalOpen(false);
      setSelectedReading(null);
    }
  });

  const handleOpenDetail = (reading: any) => {
    setSelectedReading({
      ...reading,
      note: reading.note ?? '',
    });
    setIsDetailModalOpen(true);
  };

  // Calculate statistics
  const stats = {
    total: totalCount,
    thisMonth: meterReadings.length,
    avgElectricity: meterReadings.length > 0 
      ? Math.round(meterReadings.reduce((sum: number, r: any) => sum + (r.electricityReading || 0), 0) / meterReadings.length)
      : 0,
    avgWater: meterReadings.length > 0
      ? Math.round(meterReadings.reduce((sum: number, r: any) => sum + (r.waterReading || 0), 0) / meterReadings.length)
      : 0,
  };

  const filteredReadings = meterReadings.filter((reading: any) =>
    !searchTerm || 
    reading.roomNumber?.toLowerCase().includes(searchTerm.toLowerCase()) ||
    reading.propertyName?.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-8 pb-10">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Chỉ số mét</h1>
          <p className="text-slate-500 mt-1">Theo dõi và quản lý chỉ số điện, nước của các phòng.</p>
        </div>
        <button 
          onClick={() => setIsCreateModalOpen(true)}
          className="flex items-center justify-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 hover:scale-[1.02] active:scale-[0.98]"
        >
          <Plus className="w-5 h-5" />
          Thêm chỉ số mới
        </button>
      </div>

      {/* Stats Summary */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <StatCard 
          title="Tổng chỉ số" 
          value={stats.total.toString()} 
          icon={Gauge}
          trend={{ value: 8, isPositive: true }}
        />
        <StatCard 
          title="Tháng này" 
          value={stats.thisMonth.toString()} 
          icon={Calendar}
          color="blues"
        />
        <StatCard 
          title="Trung bình điện" 
          value={`${stats.avgElectricity}`} 
          icon={Zap}
          color="yellow"
        />
        <StatCard 
          title="Trung bình nước" 
          value={`${stats.avgWater}`} 
          icon={Droplet}
          color="blue"
        />
      </div>

      {/* Search & Filter */}
      <div className="flex flex-col md:flex-row gap-4 items-center bg-white/50 p-2 rounded-2xl border border-slate-100 backdrop-blur-sm">
        <div className="relative flex-1 group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
          <input 
            type="text" 
            placeholder="Tìm theo số phòng hoặc tên bất động sản..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-12 pr-4 py-3 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all shadow-sm"
          />
        </div>
        <input 
          type="month" 
          value={monthFilter}
          onChange={(e) => {
            setMonthFilter(e.target.value);
            setPage(1);
          }}
          className="px-4 py-3 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 outline-none transition-all"
        />
      </div>

      {/* Meter Readings Table */}
      {isLoading ? (
        <div className="bg-white rounded-3xl border border-slate-100 p-6">
          <div className="space-y-3">
            {[1, 2, 3, 4, 5].map(i => (
              <div key={i} className="h-16 bg-slate-100 rounded-lg animate-pulse" />
            ))}
          </div>
        </div>
      ) : (
        <div className="bg-white rounded-3xl border border-slate-100 overflow-hidden shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-slate-50 border-b border-slate-100">
                <tr>
                  <th className="px-6 py-4 text-left font-bold text-slate-600">Phòng</th>
                  <th className="px-6 py-4 text-left font-bold text-slate-600">Bất động sản</th>
                  <th className="px-6 py-4 text-left font-bold text-slate-600">Ngày đọc</th>
                  <th className="px-6 py-4 text-center font-bold text-slate-600">Điện</th>
                  <th className="px-6 py-4 text-center font-bold text-slate-600">Nước</th>
                  <th className="px-6 py-4 text-right font-bold text-slate-600">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                <AnimatePresence>
                  {filteredReadings.length > 0 ? (
                    filteredReadings.map((reading: any) => (
                      <motion.tr
                        key={reading.id}
                        initial={{ opacity: 0 }}
                        animate={{ opacity: 1 }}
                        exit={{ opacity: 0 }}
                        className="hover:bg-slate-50 transition-colors"
                      >
                        <td className="px-6 py-4 font-medium text-slate-900">{reading.roomNumber || 'N/A'}</td>
                        <td className="px-6 py-4 text-slate-600">{reading.propertyName || 'N/A'}</td>
                        <td className="px-6 py-4 text-slate-600">
                          {new Date(reading.readingDate).toLocaleDateString('vi-VN')}
                        </td>
                        <td className="px-6 py-4 text-center">
                          <div className="inline-flex items-center gap-1.5 bg-yellow-50 px-3 py-1.5 rounded-lg">
                            <Zap className="w-4 h-4 text-yellow-600" />
                            <span className="font-medium text-yellow-900">{reading.electricityReading} kWh</span>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <div className="inline-flex items-center gap-1.5 bg-blue-50 px-3 py-1.5 rounded-lg">
                            <Droplet className="w-4 h-4 text-blue-600" />
                            <span className="font-medium text-blue-900">{reading.waterReading} m³</span>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-right">
                          <div className="flex items-center justify-end gap-2">
                            <button
                              onClick={() => handleOpenDetail(reading)}
                              className="p-2 hover:bg-slate-100 rounded-lg transition-colors text-slate-600 hover:text-indigo-600"
                            >
                              <Eye className="w-4 h-4" />
                            </button>
                            <button
                              onClick={() => {
                                setSelectedReading(reading);
                                setIsDetailModalOpen(true);
                              }}
                              className="p-2 hover:bg-slate-100 rounded-lg transition-colors text-slate-600 hover:text-indigo-600"
                            >
                              <Edit2 className="w-4 h-4" />
                            </button>
                          </div>
                        </td>
                      </motion.tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={6} className="px-6 py-12 text-center text-slate-500">
                        Không tìm thấy chỉ số mét nào
                      </td>
                    </tr>
                  )}
                </AnimatePresence>
              </tbody>
            </table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-between px-6 py-4 border-t border-slate-100 bg-slate-50">
              <span className="text-sm text-slate-600">
                Trang {page} của {totalPages} ({totalCount} chỉ số)
              </span>
              <div className="flex gap-2">
                <button
                  onClick={() => setPage(Math.max(1, page - 1))}
                  disabled={page === 1}
                  className="px-4 py-2 border border-slate-200 rounded-lg font-medium text-slate-600 hover:bg-white disabled:opacity-50 transition-all"
                >
                  Trước
                </button>
                <button
                  onClick={() => setPage(Math.min(totalPages, page + 1))}
                  disabled={page === totalPages}
                  className="px-4 py-2 border border-slate-200 rounded-lg font-medium text-slate-600 hover:bg-white disabled:opacity-50 transition-all"
                >
                  Tiếp
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Create Meter Reading Modal */}
      <Modal 
        isOpen={isCreateModalOpen} 
        onClose={() => setIsCreateModalOpen(false)} 
        title="Thêm chỉ số mét mới"
        size="md"
      >
        <div className="space-y-4">
          <div>
            <label className="block text-xs font-bold text-slate-600 mb-2">Phòng <span className="text-red-500">*</span></label>
            <select
              value={form.roomId}
              onChange={(e) => setForm(f => ({ ...f, roomId: e.target.value }))}
              className="w-full px-4 py-2 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all"
            >
              <option value="">Chọn phòng...</option>
              {rooms.map(room => (
                <option key={room.id} value={room.id}>
                  {room.roomNumber} - {room.propertyName || 'N/A'}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-600 mb-2">Ngày đọc <span className="text-red-500">*</span></label>
            <input
              type="date"
              value={form.readingDate}
              onChange={(e) => setForm(f => ({ ...f, readingDate: e.target.value }))}
              className="w-full px-4 py-2 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-bold text-slate-600 mb-2">Chỉ số điện <span className="text-red-500">*</span></label>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  value={form.electricityReading}
                  onChange={(e) => setForm(f => ({ ...f, electricityReading: Number(e.target.value) }))}
                  className="flex-1 px-4 py-2 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 outline-none"
                  placeholder="0"
                />
                <span className="text-sm text-slate-500">kWh</span>
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-600 mb-2">Chỉ số nước <span className="text-red-500">*</span></label>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  value={form.waterReading}
                  onChange={(e) => setForm(f => ({ ...f, waterReading: Number(e.target.value) }))}
                  className="flex-1 px-4 py-2 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 outline-none"
                  placeholder="0"
                />
                <span className="text-sm text-slate-500">m³</span>
              </div>
            </div>
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-600 mb-2">Ghi chú</label>
            <textarea
              value={form.note}
              onChange={(e) => setForm(f => ({ ...f, note: e.target.value }))}
              className="w-full px-4 py-2 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all resize-none"
              rows={3}
              placeholder="Nhập ghi chú (tùy chọn)..."
            />
          </div>

          <div className="flex gap-3 pt-4">
            <button
              onClick={() => setIsCreateModalOpen(false)}
              className="flex-1 px-4 py-2 border border-slate-200 rounded-lg font-bold text-slate-600 hover:bg-slate-50 transition-all"
            >
              Hủy
            </button>
            <button
              onClick={() => createMutation.mutate(form)}
              disabled={!form.roomId || createMutation.isPending}
              className="flex-1 px-4 py-2 bg-indigo-600 text-white rounded-lg font-bold hover:bg-indigo-700 transition-all disabled:opacity-50"
            >
              {createMutation.isPending ? 'Đang lưu...' : 'Lưu'}
            </button>
          </div>
        </div>
      </Modal>

      {/* Detail Modal */}
      <Modal 
        isOpen={isDetailModalOpen && selectedReading} 
        onClose={() => {
          setIsDetailModalOpen(false);
          setSelectedReading(null);
        }} 
        title="Chi tiết chỉ số mét"
        size="md"
      >
        {selectedReading && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4 pb-4 border-b border-slate-100">
              <div>
                <label className="block text-xs font-bold text-slate-600 mb-1">Phòng</label>
                <p className="text-sm text-slate-900">{selectedReading.roomNumber}</p>
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-600 mb-1">Bất động sản</label>
                <p className="text-sm text-slate-900">{selectedReading.propertyName}</p>
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-600 mb-1">Ngày đọc</label>
                <p className="text-sm text-slate-900">{new Date(selectedReading.readingDate).toLocaleDateString('vi-VN')}</p>
              </div>
              <div>
                <label className="block text-xs font-bold text-slate-600 mb-1">Tạo lúc</label>
                <p className="text-sm text-slate-900">{new Date(selectedReading.createdAt).toLocaleString('vi-VN')}</p>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="bg-yellow-50 p-4 rounded-lg border border-yellow-100">
                <div className="flex items-center gap-2 mb-2">
                  <Zap className="w-4 h-4 text-yellow-600" />
                  <span className="text-xs font-bold text-yellow-700">Điện</span>
                </div>
                <input
                  type="number"
                  min={0}
                  value={selectedReading.electricityReading}
                  onChange={(e) => setSelectedReading((prev: any) => ({ ...prev, electricityReading: Number(e.target.value) }))}
                  className="w-full bg-transparent text-2xl font-bold text-yellow-900 outline-none"
                />
                <p className="text-xs text-yellow-600 mt-1">kWh</p>
              </div>

              <div className="bg-blue-50 p-4 rounded-lg border border-blue-100">
                <div className="flex items-center gap-2 mb-2">
                  <Droplet className="w-4 h-4 text-blue-600" />
                  <span className="text-xs font-bold text-blue-700">Nước</span>
                </div>
                <input
                  type="number"
                  min={0}
                  value={selectedReading.waterReading}
                  onChange={(e) => setSelectedReading((prev: any) => ({ ...prev, waterReading: Number(e.target.value) }))}
                  className="w-full bg-transparent text-2xl font-bold text-blue-900 outline-none"
                />
                <p className="text-xs text-blue-600 mt-1">m³</p>
              </div>
            </div>

            <div>
              <label className="block text-xs font-bold text-slate-600 mb-1">Ghi chú</label>
              <textarea
                rows={3}
                value={selectedReading.note ?? ''}
                onChange={(e) => setSelectedReading((prev: any) => ({ ...prev, note: e.target.value }))}
                className="w-full px-4 py-2 border border-slate-200 rounded-lg focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all resize-none"
              />
            </div>

            <div className="flex gap-3 pt-4 border-t border-slate-100">
              <button
                onClick={() => {
                  setIsDetailModalOpen(false);
                  setSelectedReading(null);
                }}
                className="flex-1 px-4 py-2 border border-slate-200 rounded-lg font-bold text-slate-600 hover:bg-slate-50 transition-all"
              >
                Đóng
              </button>
              <button
                onClick={() => updateMutation.mutate({
                  readingDate: selectedReading.readingDate,
                  electricityReading: selectedReading.electricityReading,
                  waterReading: selectedReading.waterReading,
                  note: selectedReading.note,
                  electricityImage: selectedReading.electricityImage,
                  waterImage: selectedReading.waterImage,
                })}
                disabled={updateMutation.isPending}
                className="flex items-center justify-center gap-2 px-4 py-2 bg-indigo-600 text-white rounded-lg font-bold hover:bg-indigo-700 transition-all disabled:opacity-50"
              >
                {updateMutation.isPending ? 'Đang lưu...' : 'Lưu'}
              </button>
              <button
                onClick={() => deleteMutation.mutate(selectedReading.id)}
                disabled={deleteMutation.isPending}
                className="flex items-center justify-center gap-2 px-4 py-2 bg-red-50 text-red-600 rounded-lg font-bold hover:bg-red-100 transition-all disabled:opacity-50"
              >
                <Trash2 className="w-4 h-4" />
                {deleteMutation.isPending ? 'Đang xóa...' : 'Xóa'}
              </button>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}
