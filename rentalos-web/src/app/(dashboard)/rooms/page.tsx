'use client';

import { useState, useMemo } from 'react';
import {
  BedDouble, Plus, Search, Filter, Zap, Droplets,
  MoreVertical, Edit3, Trash2, QrCode, Wrench,
  CheckCircle2, AlertCircle, Clock, Home, DollarSign,
  ChevronRight, X, Save, Layers, FileUp, Upload
} from 'lucide-react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { roomsApi, propertiesApi } from '@/lib/api';
import { Modal } from '@/components/shared/Modal';
import { StatusBadge, StatCard } from '@/components/shared';
import { DataTable } from '@/components/shared/DataTable';
import { Room, Property } from '@/types';

const STATUS_OPTIONS = [
  { value: 'available', label: 'Trống', color: 'bg-emerald-500', lightColor: 'bg-emerald-50 text-emerald-700 border-emerald-200' },
  { value: 'rented',  label: 'Đã thuê', color: 'bg-indigo-500', lightColor: 'bg-indigo-50 text-indigo-700 border-indigo-200' },
  { value: 'maintenance', label: 'Bảo trì', color: 'bg-amber-500', lightColor: 'bg-amber-50 text-amber-700 border-amber-200' },
  { value: 'reserved', label: 'Đặt trước', color: 'bg-purple-500', lightColor: 'bg-purple-50 text-purple-700 border-purple-200' },
];

const normalizeStatus = (status: Room['status'] | string) => String(status).toLowerCase();

const AMENITIES_LIST = [
  { value: 'ac', label: 'Điều hòa' },
  { value: 'water_heater', label: 'Máy nóng lạnh' },
  { value: 'fridge', label: 'Tủ lạnh' },
  { value: 'kitchen', label: 'Khu bếp' },
  { value: 'private_wc', label: 'WC riêng' },
  { value: 'balcony', label: 'Ban công' },
  { value: 'washing_machine', label: 'Máy giặt' },
  { value: 'internet', label: 'Internet' },
  { value: 'wifi', label: 'Wifi' },
];

const emptyForm = {
  propertyId: '',
  roomNumber: '',
  floor: 1,
  basePrice: '',
  areaSqm: '',
  electricityPrice: '',
  waterPrice: '',
  serviceFee: '',
  internetFee: '',
  garbageFee: '',
  amenities: [] as string[],
  notes: '',
};

export default function RoomsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('all');
  const [filterPropertyId, setFilterPropertyId] = useState<string>('all');
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [editingRoom, setEditingRoom] = useState<Room | null>(null);
  const [selectedRoom, setSelectedRoom] = useState<Room | null>(null);
  const [form, setForm] = useState({ ...emptyForm });
  const [activeTab, setActiveTab] = useState<'info' | 'meter' | 'history'>('info');
  const [isImportOpen, setIsImportOpen] = useState(false);
  const [importResult, setImportResult] = useState<{ imported: number; skipped: number; errors: string[] } | null>(null);

  // Data
  const { data: rooms = [], isLoading } = useQuery<Room[]>({
    queryKey: ['rooms', filterPropertyId],
    queryFn: async () => {
      const resp = await roomsApi.list(filterPropertyId !== 'all' ? filterPropertyId : undefined);
      const body = resp.data as Room[] | { items: Room[] };
      return Array.isArray(body) ? body : (body.items ?? []);
    },
  });

  const { data: properties = [] } = useQuery<Property[]>({
    queryKey: ['properties'],
    queryFn: async () => {
      const resp = await propertiesApi.list();
      const body = resp.data as Property[] | { items: Property[] };
      return Array.isArray(body) ? body : (body.items ?? []);
    },
  });

  // Mutations
  const createMutation = useMutation({
    mutationFn: (data: any) => roomsApi.create(data),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['rooms'] }); closeForm(); },
    onError: (error: any) => alert(error?.message || 'Không thể tạo phòng.'),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => roomsApi.update(id, data),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['rooms'] }); closeForm(); },
    onError: (error: any) => alert(error?.message || 'Không thể cập nhật phòng.'),
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => roomsApi.remove(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
      setIsDetailOpen(false);
    },
  });

  const changeStatusMutation = useMutation({
    mutationFn: ({ id, status }: { id: string; status: string }) => roomsApi.changeStatus(id, { id, newStatus: status }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['rooms'] }),
  });

  const importCsvMutation = useMutation({
    mutationFn: ({ propertyId, file }: { propertyId: string; file: File }) => roomsApi.importCsv(propertyId, file),
    onSuccess: (resp) => {
      queryClient.invalidateQueries({ queryKey: ['rooms'] });
      setImportResult(resp.data as any);
    },
  });

  // Filtering
  const filtered = useMemo(() =>
    rooms.filter(r => {
      const matchSearch = r.roomNumber.toLowerCase().includes(search.toLowerCase());
      const matchStatus = filterStatus === 'all' || normalizeStatus(r.status) === filterStatus;
      return matchSearch && matchStatus;
    }), [rooms, search, filterStatus]);

  // Stats
  const stats = useMemo(() => ({
    total: rooms.length,
    available: rooms.filter(r => normalizeStatus(r.status) === 'available').length,
    occupied: rooms.filter(r => normalizeStatus(r.status) === 'rented').length,
    maintenance: rooms.filter(r => normalizeStatus(r.status) === 'maintenance').length,
  }), [rooms]);

  // Helpers
  const openCreate = () => { setEditingRoom(null); setForm({ ...emptyForm, propertyId: filterPropertyId !== 'all' ? filterPropertyId : '' }); setIsFormOpen(true); };
  const openEdit = async (room: Room) => {
    const resp = await roomsApi.getById(room.id);
    const fullRoom = resp.data as Room;

    setEditingRoom(fullRoom);
    setForm({
      propertyId: fullRoom.propertyId ?? '',
      roomNumber: fullRoom.roomNumber,
      floor: fullRoom.floor,
      basePrice: String(fullRoom.basePrice),
      areaSqm: String(fullRoom.areaSqm ?? ''),
      electricityPrice: String(fullRoom.electricityPrice ?? ''),
      waterPrice: String(fullRoom.waterPrice ?? ''),
      serviceFee: String(fullRoom.serviceFee ?? ''),
      internetFee: String(fullRoom.internetFee ?? ''),
      garbageFee: String(fullRoom.garbageFee ?? ''),
      amenities: [...(fullRoom.amenities ?? [])],
      notes: fullRoom.notes ?? '',
    });
    setIsFormOpen(true);
  };
  const closeForm = () => { setIsFormOpen(false); setEditingRoom(null); };

  const toggleAmenity = (a: string) =>
    setForm(f => ({ ...f, amenities: f.amenities.includes(a) ? f.amenities.filter(x => x !== a) : [...f.amenities, a] }));

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      propertyId: form.propertyId || undefined,
      roomNumber: form.roomNumber,
      floor: Number(form.floor),
      basePrice: Number(form.basePrice),
      areaSqm: form.areaSqm ? Number(form.areaSqm) : undefined,
      electricityPrice: form.electricityPrice ? Number(form.electricityPrice) : 3500,
      waterPrice: form.waterPrice ? Number(form.waterPrice) : 15000,
      serviceFee: form.serviceFee ? Number(form.serviceFee) : 0,
      internetFee: form.internetFee ? Number(form.internetFee) : 0,
      garbageFee: form.garbageFee ? Number(form.garbageFee) : 0,
      amenities: form.amenities,
      notes: form.notes,
    };
    if (editingRoom) {
      updateMutation.mutate({ id: editingRoom.id, data: payload });
    } else {
      createMutation.mutate(payload);
    }
  };

  const isPending = createMutation.isPending || updateMutation.isPending;

  const getStatusCfg = (status: string) => STATUS_OPTIONS.find(s => s.value === normalizeStatus(status)) ?? STATUS_OPTIONS[0];

  return (
    <div className="space-y-8 pb-10">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight">Quản lý Phòng</h1>
          <p className="text-slate-500 mt-1">Theo dõi tình trạng và vận hành tất cả phòng cho thuê.</p>
        </div>
        <div className="flex gap-3 flex-wrap">
          <button
            onClick={() => { setImportResult(null); setIsImportOpen(true); }}
            className="flex items-center justify-center gap-2 px-5 py-3 bg-slate-100 text-slate-700 rounded-2xl font-bold hover:bg-slate-200 transition-all"
          >
            <FileUp className="w-5 h-5" />
            Nhập CSV
          </button>
          <button
            onClick={openCreate}
            className="flex items-center justify-center gap-2 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 hover:scale-[1.02] active:scale-[0.98]"
          >
            <Plus className="w-5 h-5" />
            Thêm phòng mới
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          { label: 'Tổng số phòng', value: stats.total, icon: BedDouble, color: 'indigo' },
          { label: 'Đang trống', value: stats.available, icon: CheckCircle2, color: 'emerald' },
          { label: 'Đã cho thuê', value: stats.occupied, icon: Home, color: 'indigo' },
          { label: 'Đang bảo trì', value: stats.maintenance, icon: Wrench, color: 'amber' },
        ].map(s => (
          <StatCard key={s.label} title={s.label} value={String(s.value)} icon={s.icon} color={s.color as any} />
        ))}
      </div>

      {/* Filters */}
      <div className="flex flex-col md:flex-row gap-3 items-center bg-white/50 p-2 rounded-2xl border border-slate-100 backdrop-blur-sm">
        <div className="relative flex-1 group">
          <Search className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400 group-focus-within:text-indigo-500 transition-colors" />
          <input
            value={search}
            onChange={e => setSearch(e.target.value)}
            placeholder="Tìm theo số phòng..."
            className="w-full pl-12 pr-4 py-3 bg-white border border-slate-200 rounded-xl focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none transition-all shadow-sm"
          />
        </div>

        {/* Property filter */}
        <select
          value={filterPropertyId}
          onChange={e => setFilterPropertyId(e.target.value)}
          className="px-4 py-3 bg-white border border-slate-200 rounded-xl text-sm font-medium text-slate-600 focus:ring-2 focus:ring-indigo-500/20 outline-none"
        >
          <option value="all">Tất cả cơ sở</option>
          {properties.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>

        {/* Status filter */}
        <div className="flex gap-2">
          {[{ value: 'all', label: 'Tất cả' }, ...STATUS_OPTIONS].map(s => (
            <button
              key={s.value}
              onClick={() => setFilterStatus(s.value)}
              className={`px-4 py-2.5 rounded-xl text-sm font-bold transition-all ${
                filterStatus === s.value
                  ? 'bg-indigo-600 text-white shadow-md shadow-indigo-200'
                  : 'bg-white border border-slate-200 text-slate-600 hover:border-indigo-300'
              }`}
            >
              {s.label}
            </button>
          ))}
        </div>
      </div>

      {/* Room Table */}
      {isLoading ? (
        <div className="h-64 rounded-3xl border border-slate-100 bg-white animate-pulse" />
      ) : filtered.length === 0 ? (
        <div className="py-20 text-center bg-white rounded-3xl border border-dashed border-slate-200">
          <BedDouble className="w-14 h-14 text-slate-200 mx-auto mb-4" />
          <h3 className="text-lg font-bold text-slate-900">Không tìm thấy phòng nào</h3>
          <p className="text-slate-500 mt-1">Hãy thêm phòng mới hoặc điều chỉnh bộ lọc.</p>
          <button onClick={openCreate} className="mt-6 px-6 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 shadow-lg shadow-indigo-100">
            Thêm phòng đầu tiên
          </button>
        </div>
      ) : (
        <DataTable
          data={filtered}
          pageSize={12}
          searchPlaceholder="Tìm phòng, khách thuê hoặc cơ sở..."
          onRowClick={(room) => {
            setSelectedRoom(room);
            setActiveTab('info');
            setIsDetailOpen(true);
          }}
          columns={[
            {
              key: 'roomNumber',
              label: 'Phòng',
              sortable: true,
              render: (_value, room) => (
                <div>
                  <p className="font-bold text-slate-900">Phòng {room.roomNumber}</p>
                  <p className="text-xs text-slate-500">Tầng {room.floor}</p>
                </div>
              ),
            },
            {
              key: 'propertyName',
              label: 'Cơ sở',
              sortable: true,
              render: (value) => value || 'Chưa gán cơ sở',
            },
            {
              key: 'currentCustomerName',
              label: 'Khách thuê',
              render: (_value, room) => (
                <div>
                  <p className="font-medium text-slate-800">{room.currentCustomerName || 'Chưa có khách thuê'}</p>
                  <p className="text-xs text-slate-500">{room.currentContractCode || 'Chưa có hợp đồng hoạt động'}</p>
                </div>
              ),
            },
            {
              key: 'basePrice',
              label: 'Giá thuê',
              sortable: true,
              render: (value) => <span className="font-bold text-indigo-600">{Number(value ?? 0).toLocaleString()}đ/tháng</span>,
            },
            {
              key: 'areaSqm',
              label: 'Diện tích',
              sortable: true,
              render: (value) => (value ? `${value} m²` : 'Chưa cập nhật'),
            },
            {
              key: 'status',
              label: 'Trạng thái',
              sortable: true,
              render: (value) => <StatusBadge status={value} />,
            },
          ]}
        />
      )}

      {/* Create / Edit Modal */}
      <Modal
        isOpen={isFormOpen}
        onClose={closeForm}
        title={editingRoom ? `Sửa phòng ${editingRoom.roomNumber}` : 'Thêm phòng mới'}
        size="lg"
      >
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            {!editingRoom && (
              <div className="col-span-2 space-y-1.5">
                <label className="text-sm font-bold text-slate-700">Cơ sở (Property) *</label>
                <select
                  required
                  value={form.propertyId}
                  onChange={e => setForm(f => ({ ...f, propertyId: e.target.value }))}
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
                >
                  <option value="">-- Chọn cơ sở --</option>
                  {properties.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
                </select>
              </div>
            )}
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Số phòng *</label>
              <input
                required
                value={form.roomNumber}
                onChange={e => setForm(f => ({ ...f, roomNumber: e.target.value }))}
                placeholder="101, A-201..."
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Tầng *</label>
              <input
                required
                type="number"
                min={1}
                value={form.floor}
                onChange={e => setForm(f => ({ ...f, floor: Number(e.target.value) }))}
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Giá thuê (VNĐ/tháng) *</label>
              <input
                required
                type="number"
                min={0}
                value={form.basePrice}
                onChange={e => setForm(f => ({ ...f, basePrice: e.target.value }))}
                placeholder="3500000"
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Diện tích (m²)</label>
              <input
                type="number"
                min={0}
                value={form.areaSqm}
                onChange={e => setForm(f => ({ ...f, areaSqm: e.target.value }))}
                placeholder="20"
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Đơn giá điện (đ/kWh)</label>
              <input
                type="number"
                min={0}
                value={form.electricityPrice}
                onChange={e => setForm(f => ({ ...f, electricityPrice: e.target.value }))}
                placeholder="3500"
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Đơn giá nước (đ/m³)</label>
              <input
                type="number"
                min={0}
                value={form.waterPrice}
                onChange={e => setForm(f => ({ ...f, waterPrice: e.target.value }))}
                placeholder="15000"
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Phí Internet (đ/tháng)</label>
              <input
                type="number"
                min={0}
                value={form.internetFee}
                onChange={e => setForm(f => ({ ...f, internetFee: e.target.value }))}
                placeholder="100000"
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
            <div className="space-y-1.5">
              <label className="text-sm font-bold text-slate-700">Phí rác (đ/tháng)</label>
              <input
                type="number"
                min={0}
                value={form.garbageFee}
                onChange={e => setForm(f => ({ ...f, garbageFee: e.target.value }))}
                placeholder="20000"
                className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none"
              />
            </div>
          </div>

          {/* Amenities */}
          <div className="space-y-2">
            <label className="text-sm font-bold text-slate-700">Tiện ích</label>
            <div className="flex flex-wrap gap-2">
              {AMENITIES_LIST.map(a => (
                <button
                  key={a.value}
                  type="button"
                  onClick={() => toggleAmenity(a.value)}
                  className={`px-3 py-1.5 rounded-xl text-xs font-bold transition-all border ${
                    form.amenities.includes(a.value)
                      ? 'bg-indigo-600 text-white border-indigo-600 shadow-md shadow-indigo-100'
                      : 'bg-white text-slate-600 border-slate-200 hover:border-indigo-300'
                  }`}
                >
                  {a.label}
                </button>
              ))}
            </div>
          </div>

          {/* Notes */}
          <div className="space-y-1.5">
            <label className="text-sm font-bold text-slate-700">Ghi chú</label>
            <textarea
              rows={2}
              value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
              placeholder="Thêm ghi chú về phòng..."
              className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 outline-none resize-none"
            />
          </div>

          <div className="flex gap-3 pt-2">
            <button type="button" onClick={closeForm} className="flex-1 py-3.5 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200 transition-colors">
              Hủy
            </button>
            <button
              type="submit"
              disabled={isPending}
              className="flex-[2] py-3.5 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-60 flex items-center justify-center gap-2"
            >
              {isPending ? (
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin" />
              ) : (
                <Save className="w-5 h-5" />
              )}
              {editingRoom ? 'Lưu thay đổi' : 'Tạo phòng'}
            </button>
          </div>
        </form>
      </Modal>

      {/* Room Detail Modal */}
      <Modal
        isOpen={isDetailOpen}
        onClose={() => setIsDetailOpen(false)}
        title={`Chi tiết Phòng ${selectedRoom?.roomNumber}`}
        size="xl"
      >
        {selectedRoom && (
          <div className="space-y-6">
            {/* Tab navigation */}
            <div className="flex gap-1 bg-slate-50 rounded-2xl p-1">
              {(['info', 'meter', 'history'] as const).map(tab => (
                <button
                  key={tab}
                  onClick={() => setActiveTab(tab)}
                  className={`flex-1 py-2.5 rounded-xl text-sm font-bold transition-all ${
                    activeTab === tab ? 'bg-white shadow-sm text-indigo-600' : 'text-slate-500 hover:text-slate-700'
                  }`}
                >
                  {tab === 'info' ? 'Thông tin' : tab === 'meter' ? 'Chỉ số điện/nước' : 'Lịch sử'}
                </button>
              ))}
            </div>

            {activeTab === 'info' && (
              <div className="space-y-6">
                {/* Status change */}
                <div className="flex items-center justify-between p-4 bg-slate-50 rounded-2xl border border-slate-100">
                  <div>
                    <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Trạng thái hiện tại</p>
                    <span className={`text-sm font-bold px-3 py-1 rounded-full border ${getStatusCfg(selectedRoom.status).lightColor}`}>
                      {getStatusCfg(selectedRoom.status).label}
                    </span>
                  </div>
                  <div className="flex gap-2">
                    {STATUS_OPTIONS.filter(s => s.value !== selectedRoom.status).map(s => (
                      <button
                        key={s.value}
                        onClick={() => changeStatusMutation.mutate({ id: selectedRoom.id, status: s.value })}
                        className="px-3 py-1.5 bg-white border border-slate-200 text-slate-600 rounded-xl text-xs font-bold hover:border-indigo-300 hover:text-indigo-600 transition-colors"
                      >
                        → {s.label}
                      </button>
                    ))}
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  {[
                    { label: 'Số phòng', value: selectedRoom.roomNumber },
                    { label: 'Tầng', value: selectedRoom.floor },
                    { label: 'Giá thuê', value: `${(selectedRoom.basePrice ?? 0).toLocaleString()}đ/tháng` },
                    { label: 'Diện tích', value: selectedRoom.areaSqm ? `${selectedRoom.areaSqm} m²` : 'Chưa cập nhật' },
                    { label: 'Khách thuê', value: selectedRoom.currentCustomerName || 'Hiện chưa có khách thuê' },
                    { label: 'Hợp đồng', value: selectedRoom.currentContractCode || 'Chưa có hợp đồng hoạt động' },
                  ].map(i => (
                    <div key={i.label} className="bg-slate-50 rounded-2xl p-4">
                      <p className="text-xs font-bold text-slate-400 uppercase tracking-widest">{i.label}</p>
                      <p className="text-lg font-black text-slate-900 mt-1">{i.value}</p>
                    </div>
                  ))}
                </div>

                {selectedRoom.amenities?.length > 0 && (
                  <div>
                    <p className="text-xs font-bold text-slate-400 uppercase tracking-widest mb-2">Tiện ích</p>
                    <div className="flex flex-wrap gap-2">
                      {selectedRoom.amenities.map(a => (
                        <span key={a} className="px-3 py-1 bg-indigo-50 text-indigo-700 text-xs font-bold rounded-xl border border-indigo-100">{a}</span>
                      ))}
                    </div>
                  </div>
                )}

                <div className="flex gap-3 pt-2">
                  <button
                    onClick={() => { setIsDetailOpen(false); openEdit(selectedRoom); }}
                    className="flex items-center gap-2 px-5 py-3 bg-white border border-slate-200 text-slate-700 rounded-2xl font-bold hover:border-indigo-300 hover:text-indigo-600 transition-all"
                  >
                    <Edit3 className="w-4 h-4" /> Chỉnh sửa
                  </button>
                  <button
                    onClick={() => { if (confirm('Xóa phòng này?')) deleteMutation.mutate(selectedRoom.id); }}
                    className="flex items-center gap-2 px-5 py-3 bg-rose-50 border border-rose-100 text-rose-600 rounded-2xl font-bold hover:bg-rose-100 transition-all"
                  >
                    <Trash2 className="w-4 h-4" /> Xóa phòng
                  </button>
                </div>
              </div>
            )}

            {activeTab === 'meter' && (
              <RoomMeterTab roomId={selectedRoom.id} />
            )}

            {activeTab === 'history' && (
              <RoomHistoryTab roomId={selectedRoom.id} />
            )}
          </div>
        )}
      </Modal>

      {/* Import CSV Modal */}
      <Modal
        isOpen={isImportOpen}
        onClose={() => setIsImportOpen(false)}
        title="Nhập phòng từ CSV"
        size="md"
      >
        <div className="space-y-6">
          {!importResult ? (
            <>
              <div className="space-y-3">
                <label className="text-sm font-bold text-slate-700">Chọn nhà trọ</label>
                <select
                  id="import-property"
                  className="w-full px-4 py-3 rounded-xl border border-slate-200 focus:ring-2 focus:ring-indigo-500/20 outline-none"
                >
                  <option value="">-- Chọn nhà trọ --</option>
                  {properties.map((p: any) => (
                    <option key={p.id} value={p.id}>{p.name}</option>
                  ))}
                </select>
              </div>
              <div className="bg-slate-50 border border-dashed border-slate-300 rounded-2xl p-6 text-center">
                <FileUp className="w-10 h-10 text-slate-400 mx-auto mb-3" />
                <p className="text-sm font-bold text-slate-600 mb-1">Chọn file CSV để nhập</p>
                <p className="text-xs text-slate-400 mb-4">Định dạng: RoomNumber, Floor, BasePrice, ElectricityPrice, WaterPrice, AreaSqm</p>
                <label className="cursor-pointer inline-flex items-center gap-2 px-5 py-2.5 bg-indigo-600 text-white rounded-xl text-sm font-bold hover:bg-indigo-700 transition-colors">
                  <Upload className="w-4 h-4" />
                  {importCsvMutation.isPending ? 'Đang nhập...' : 'Chọn file'}
                  <input
                    type="file"
                    accept=".csv,text/csv"
                    className="hidden"
                    disabled={importCsvMutation.isPending}
                    onChange={(e) => {
                      const f = e.target.files?.[0];
                      const sel = (document.getElementById('import-property') as HTMLSelectElement)?.value;
                      if (f && sel) importCsvMutation.mutate({ propertyId: sel, file: f });
                      else if (!sel) alert('Vui lòng chọn nhà trọ trước');
                    }}
                  />
                </label>
              </div>
              <div className="bg-amber-50 border border-amber-100 rounded-xl p-4 text-xs text-amber-700">
                <p className="font-bold mb-1">Lưu ý định dạng file CSV:</p>
                <code className="block bg-white rounded p-2 text-slate-600 font-mono">
                  RoomNumber,Floor,BasePrice,ElectricityPrice,WaterPrice,AreaSqm<br/>
                  101,1,3500000,3500,15000,25
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
                onClick={() => setIsImportOpen(false)}
                className="w-full py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 transition-colors"
              >
                Đóng
              </button>
            </div>
          )}
        </div>
      </Modal>
    </div>
  );
}

function RoomMeterTab({ roomId }: { roomId: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ['room-meter', roomId],
    queryFn: async () => {
      const resp = await roomsApi.getMeterReadings(roomId);
      return (resp.data as any)?.items ?? resp.data ?? [];
    },
  });

  if (isLoading) return <div className="h-32 animate-pulse bg-slate-50 rounded-2xl" />;

  const readings = Array.isArray(data) ? data : [];

  return (
    <div className="space-y-3">
      {readings.length === 0 ? (
        <div className="text-center py-12 text-slate-400">
          <Zap className="w-10 h-10 mx-auto mb-2 opacity-30" />
          <p className="font-bold">Chưa có chỉ số điện nước nào</p>
        </div>
      ) : (
        readings.map((r: any, i: number) => (
          <div key={i} className="flex items-center justify-between p-4 bg-slate-50 rounded-2xl border border-slate-100">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-xl bg-amber-50 flex items-center justify-center text-amber-600">
                <Zap className="w-5 h-5" />
              </div>
              <div>
                <p className="font-bold text-slate-900">{r.month ?? r.readingDate}</p>
                <p className="text-xs text-slate-500">Điện: {r.electricityReading} kWh · Nước: {r.waterReading} m³</p>
              </div>
            </div>
          </div>
        ))
      )}
    </div>
  );
}

function RoomHistoryTab({ roomId }: { roomId: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ['room-history', roomId],
    queryFn: async () => {
      const resp = await roomsApi.getHistory(roomId);
      return (resp.data as any)?.items ?? resp.data ?? [];
    },
  });

  if (isLoading) return <div className="h-32 animate-pulse bg-slate-50 rounded-2xl" />;

  const history = Array.isArray(data) ? data : [];

  return (
    <div className="space-y-3">
      {history.length === 0 ? (
        <div className="text-center py-12 text-slate-400">
          <Clock className="w-10 h-10 mx-auto mb-2 opacity-30" />
          <p className="font-bold">Chưa có lịch sử phòng này</p>
        </div>
      ) : (
        history.map((h: any, i: number) => (
          <div key={i} className="flex items-center gap-4 p-4 bg-slate-50 rounded-2xl border border-slate-100">
            <div className="w-2 h-2 rounded-full bg-indigo-500 shrink-0" />
            <div>
              <p className="font-bold text-slate-900">{h.customerName ?? h.action}</p>
              <p className="text-xs text-slate-500">{h.startDate} – {h.endDate ?? 'Hiện tại'}</p>
            </div>
          </div>
        ))
      )}
    </div>
  );
}
