'use client';

import { useState } from 'react';
import { 
  Users, UserPlus, Mail, Shield, Building2, 
  MoreVertical, Check, X, RefreshCw, Trash2,
  Lock, Settings, Eye, Edit3, Trash, Loader2
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { StatCard, StatusBadge } from '@/components/shared';
import { SlideOver } from '@/components/shared/SlideOver';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { staffApi, propertiesApi } from '@/lib/api';
import { format, formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';

const inviteSchema = z.object({
  email: z.string().email('Email không hợp lệ'),
  role: z.enum(['manager', 'staff']),
  assignedPropertyIds: z.array(z.string()).optional(),
});

type InviteFormData = z.infer<typeof inviteSchema>;

const PERMISSIONS = [
  { action: 'Xem báo cáo', manager: true, staff: false },
  { action: 'Quản lý phòng', manager: true, staff: true },
  { action: 'Quản lý khách thuê', manager: true, staff: true },
  { action: 'Sửa hóa đơn', manager: true, staff: false },
  { action: 'Xóa hợp đồng', manager: false, staff: false },
  { action: 'Cấu hình hệ thống', manager: false, staff: false },
];

export default function StaffPage() {
  const [isInviteOpen, setIsInviteOpen] = useState(false);
  const [isEditOpen, setIsEditOpen] = useState(false);
  const [editingStaff, setEditingStaff] = useState<any>(null);
  const [editForm, setEditForm] = useState({ role: '', assignedPropertyIds: [] as string[] });
  const [isActivityLogOpen, setIsActivityLogOpen] = useState(false);
  const [activityLogStaffId, setActivityLogStaffId] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const { register, handleSubmit, reset, formState: { errors } } = useForm<InviteFormData>({
    resolver: zodResolver(inviteSchema),
    defaultValues: { assignedPropertyIds: [] }
  });

  const { data: staffList = [], isLoading } = useQuery({
    queryKey: ['staff'],
    queryFn: async () => {
      const resp = await staffApi.list();
      return Array.isArray(resp.data) ? resp.data : [];
    },
  });

  const { data: properties = [] } = useQuery({
    queryKey: ['properties'],
    queryFn: async () => {
      const resp = await propertiesApi.list();
      const body = resp.data as any[] | { items: any[] };
      return Array.isArray(body) ? body : body.items ?? [];
    },
  });

  const activeStaff = (staffList as any[]).filter((s: any) => s.isActive);
  const pendingStaff = (staffList as any[]).filter((s: any) => !s.isActive);

  const inviteMutation = useMutation({
    mutationFn: (data: InviteFormData) => staffApi.invite({
      email: data.email,
      role: data.role,
      assignedPropertyIds: data.assignedPropertyIds ?? [],
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staff'] });
      setIsInviteOpen(false);
      reset();
    },
  });

  const deactivateMutation = useMutation({
    mutationFn: (id: string) => staffApi.deactivate(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['staff'] }),
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: any }) => staffApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['staff'] });
      setIsEditOpen(false);
      setEditingStaff(null);
    },
  });

  const { data: activityLog = [] } = useQuery({
    queryKey: ['staff-activity', activityLogStaffId],
    queryFn: () => activityLogStaffId
      ? staffApi.getActivityLog(activityLogStaffId).then(r => Array.isArray(r.data) ? r.data : (r.data as any)?.items ?? [])
      : Promise.resolve([]),
    enabled: !!activityLogStaffId && isActivityLogOpen,
  });

  const onInvite = (data: InviteFormData) => {
    inviteMutation.mutate(data);
  };

  return (
    <div className="space-y-8 pb-20">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div className="space-y-1">
          <h1 className="text-3xl font-black text-slate-900 tracking-tight">Đội ngũ vận hành</h1>
          <p className="text-slate-500 font-medium italic">Quản lý nhân viên và phân quyền truy cập cơ sở.</p>
        </div>
        <button 
          onClick={() => setIsInviteOpen(true)}
          className="flex items-center gap-2 px-6 py-3.5 bg-indigo-600 text-white rounded-[1.25rem] font-black hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 group"
        >
          <UserPlus className="w-5 h-5 group-hover:scale-110 transition-transform" />
          Mời nhân viên
        </button>
      </div>

      {/* Staff Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {isLoading ? (
          Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="bg-white rounded-[2rem] border border-slate-100 p-6 animate-pulse h-52" />
          ))
        ) : (
          <>
            {activeStaff.map((staff: any) => (
              <motion.div 
                key={staff.id}
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-6 relative group overflow-hidden"
              >
                <div className="absolute top-0 right-0 p-4 opacity-0 group-hover:opacity-100 transition-opacity flex gap-1">
                  <button
                    onClick={() => {
                      setEditingStaff(staff);
                      setEditForm({ role: staff.role ?? '', assignedPropertyIds: staff.assignedPropertyIds ?? [] });
                      setIsEditOpen(true);
                    }}
                    className="p-2 hover:bg-indigo-50 rounded-xl text-slate-400 hover:text-indigo-500 transition-colors"
                    title="Chỉnh sửa"
                  >
                    <Edit3 className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => { setActivityLogStaffId(staff.id); setIsActivityLogOpen(true); }}
                    className="p-2 hover:bg-slate-100 rounded-xl text-slate-400 hover:text-slate-600 transition-colors"
                    title="Lịch sử hoạt động"
                  >
                    <Eye className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => deactivateMutation.mutate(staff.id)}
                    disabled={deactivateMutation.isPending}
                    className="p-2 hover:bg-red-50 rounded-xl text-slate-400 hover:text-red-500 transition-colors"
                    title="Vô hiệu hóa"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </div>

                <div className="flex items-center gap-4 mb-6">
                  <div className="w-16 h-16 rounded-2xl bg-indigo-50 border border-indigo-100 flex items-center justify-center text-indigo-600 font-black text-xl shadow-inner uppercase">
                    {(staff.fullName ?? staff.email).split(' ').map((n: string) => n[0]).join('').slice(0, 2)}
                  </div>
                  <div>
                    <h3 className="text-lg font-black text-slate-900 leading-tight">{staff.fullName}</h3>
                    <p className="text-xs font-medium text-slate-400">{staff.email}</p>
                  </div>
                </div>

                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Vai trò</span>
                    <span className={`px-3 py-1 rounded-full text-[10px] font-black uppercase tracking-widest ${
                      staff.role === 'manager' ? 'bg-purple-100 text-purple-700' : 'bg-blue-100 text-blue-700'
                    }`}>
                      {staff.role === 'manager' ? 'Quản lý' : 'Nhân viên'}
                    </span>
                  </div>

                  <div className="flex items-center justify-between pt-4 border-t border-slate-50 text-[10px] font-bold text-slate-400">
                    <div className="flex items-center gap-1.5">
                      <div className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                      {staff.lastLoginAt
                        ? `Truy cập ${formatDistanceToNow(new Date(staff.lastLoginAt), { addSuffix: true, locale: vi })}`
                        : 'Chưa đăng nhập'}
                    </div>
                  </div>
                </div>
              </motion.div>
            ))}

            {/* Pending Invites */}
            {pendingStaff.map((invite: any) => (
              <div 
                key={invite.id}
                className="bg-slate-50/50 rounded-[2rem] border-2 border-dashed border-slate-200 p-6 flex flex-col justify-between"
              >
                <div className="space-y-4">
                  <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-xl bg-white border border-slate-200 flex items-center justify-center text-slate-400 italic">
                      ?
                    </div>
                    <div>
                      <p className="text-sm font-black text-slate-600 truncate">{invite.email}</p>
                      <p className="text-[10px] font-bold text-amber-600 uppercase">Đang chờ xác nhận</p>
                    </div>
                  </div>
                  <div className="flex items-center justify-between">
                    <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest italic">
                      Tạo: {format(new Date(invite.createdAt), 'dd/MM/yyyy')}
                    </span>
                  </div>
                </div>
                <div className="flex gap-2 mt-6">
                  <button
                    onClick={() => deactivateMutation.mutate(invite.id)}
                    className="flex-1 py-2.5 bg-white border border-slate-200 rounded-xl text-xs font-black text-slate-600 hover:bg-white transition-all"
                  >Hủy</button>
                  <button
                    onClick={() => inviteMutation.mutate({ email: invite.email, role: invite.role, assignedPropertyIds: [] })}
                    disabled={inviteMutation.isPending}
                    className="flex-1 py-2.5 bg-indigo-50 text-indigo-600 rounded-xl text-xs font-black hover:bg-indigo-100 transition-all flex items-center justify-center gap-2"
                  >
                    <RefreshCw className="w-3.5 h-3.5" /> Gửi lại
                  </button>
                </div>
              </div>
            ))}
          </>
        )}
      </div>

      {/* Permission Matrix Area */}
      <div className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-8">
        <div className="flex items-center gap-4 mb-8">
          <div className="w-12 h-12 rounded-2xl bg-slate-900 flex items-center justify-center text-white">
            <Shield className="w-6 h-6" />
          </div>
          <div>
            <h3 className="text-xl font-black text-slate-900 leading-tight">Ma trận quyền hạn</h3>
            <p className="text-xs font-bold text-slate-400 mt-1 uppercase tracking-wider">Chi tiết chức năng theo từng cấp bậc</p>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-slate-50">
                <th className="text-left pb-4 text-[10px] font-black text-slate-400 uppercase tracking-[0.2em]">Tính năng / Vai trò</th>
                <th className="pb-4 text-[10px] font-black text-purple-600 uppercase tracking-[0.2em] text-center">Manager</th>
                <th className="pb-4 text-[10px] font-black text-slate-600 uppercase tracking-[0.2em] text-center">Staff</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-50">
              {PERMISSIONS.map((p, idx) => (
                <tr key={idx} className="group hover:bg-slate-50/50 transition-colors">
                  <td className="py-4 text-sm font-bold text-slate-700 italic">{p.action}</td>
                  <td className="py-4 text-center">
                     <div className="flex justify-center">
                      <div className={`w-6 h-6 rounded-lg flex items-center justify-center ${p.manager ? 'bg-purple-100 text-purple-600' : 'bg-slate-100 text-slate-300'}`}>
                        {p.manager ? <Check className="w-3.5 h-3.5" /> : <X className="w-3.5 h-3.5" />}
                      </div>
                    </div>
                  </td>
                  <td className="py-4 text-center">
                     <div className="flex justify-center">
                      <div className={`w-6 h-6 rounded-lg flex items-center justify-center ${p.staff ? 'bg-blue-100 text-blue-600' : 'bg-slate-100 text-slate-300'}`}>
                        {p.staff ? <Check className="w-3.5 h-3.5" /> : <X className="w-3.5 h-3.5" />}
                      </div>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Invite SlideOver */}
      <SlideOver
        isOpen={isInviteOpen}
        onClose={() => setIsInviteOpen(false)}
        title="Mời nhân viên mới"
      >
        <form onSubmit={handleSubmit(onInvite)} className="space-y-6">
          <div className="space-y-2">
            <label className="text-xs font-black text-slate-400 uppercase tracking-widest pl-1">Email người nhận</label>
            <div className="relative">
              <Mail className="absolute left-4 top-1/2 -translate-y-1/2 w-5 h-5 text-slate-400" />
              <input 
                {...register('email')}
                type="email" 
                placeholder="vd: nhanvien@gmail.com"
                className="w-full pl-12 pr-4 py-3.5 bg-slate-50 border border-slate-100 rounded-xl text-sm font-bold outline-none focus:ring-4 focus:ring-indigo-500/10 focus:bg-white transition-all"
              />
            </div>
            {errors.email && <p className="text-rose-500 text-[10px] font-black pl-1">{errors.email.message}</p>}
          </div>

          <div className="space-y-2">
            <label className="text-xs font-black text-slate-400 uppercase tracking-widest pl-1">Vai trò</label>
            <div className="grid grid-cols-2 gap-3">
              {(['manager', 'staff'] as const).map(role => (
                <label key={role} className="cursor-pointer group">
                  <input type="radio" {...register('role')} value={role} className="sr-only peer" />
                  <div className="p-3 bg-slate-50 border border-slate-100 rounded-xl text-center peer-checked:bg-white peer-checked:border-indigo-600 peer-checked:ring-2 peer-checked:ring-indigo-500/10 transition-all">
                    <p className="text-[10px] font-black text-slate-400 group-hover:text-slate-600 peer-checked:text-indigo-600 uppercase">
                      {role === 'manager' ? 'Quản lý' : 'Nhân sự'}
                    </p>
                  </div>
                </label>
              ))}
            </div>
          </div>

          {(properties as any[]).length > 0 && (
            <div className="space-y-2">
              <label className="text-xs font-black text-slate-400 uppercase tracking-widest pl-1">Phụ trách nhà trọ</label>
              <div className="p-4 bg-slate-50 border border-slate-100 rounded-2xl space-y-3">
                {(properties as any[]).map((p: any) => (
                  <label key={p.id} className="flex items-center gap-3 cursor-pointer group">
                    <input type="checkbox" value={p.id} {...register('assignedPropertyIds')} className="w-5 h-5 rounded-lg border-slate-200 text-indigo-600 focus:ring-indigo-500 transition-all" />
                    <span className="text-sm font-bold text-slate-600 group-hover:text-slate-900">{p.name}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          <div className="pt-8 flex gap-3">
            <button type="button" onClick={() => { setIsInviteOpen(false); reset(); }} className="flex-1 py-4 bg-white border border-slate-100 rounded-2xl text-sm font-black text-slate-600 hover:bg-slate-50 transition-all">Hủy</button>
            <button type="submit" disabled={inviteMutation.isPending} className="flex-1 py-4 bg-indigo-600 text-white rounded-2xl text-sm font-black hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100 disabled:opacity-70 flex items-center justify-center gap-2">
              {inviteMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
              Gửi lời mời
            </button>
          </div>
        </form>
      </SlideOver>
      {/* Edit Staff Modal */}
      {isEditOpen && editingStaff && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm">
          <div className="bg-white rounded-[2rem] shadow-2xl p-8 w-full max-w-md space-y-6">
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-900">Sửa thông tin nhân viên</h3>
              <button onClick={() => { setIsEditOpen(false); setEditingStaff(null); }} className="p-2 hover:bg-slate-100 rounded-xl">
                <X className="w-5 h-5 text-slate-500" />
              </button>
            </div>
            <div className="text-sm text-slate-500">{editingStaff.email}</div>
            <form onSubmit={e => { e.preventDefault(); updateMutation.mutate({ id: editingStaff.id, data: { role: editForm.role, assignedPropertyIds: editForm.assignedPropertyIds } }); }} className="space-y-4">
              <div>
                <label className="text-sm font-bold text-slate-700 mb-2 block">Vai trò</label>
                <div className="grid grid-cols-2 gap-3">
                  {(['manager', 'staff'] as const).map(role => (
                    <button type="button" key={role} onClick={() => setEditForm(f => ({ ...f, role }))} className={`py-3 rounded-xl border-2 font-bold text-sm transition-all ${editForm.role === role ? 'border-indigo-600 bg-indigo-50 text-indigo-700' : 'border-slate-200 text-slate-500'}`}>
                      {role === 'manager' ? 'Quản lý' : 'Nhân viên'}
                    </button>
                  ))}
                </div>
              </div>
              {(properties as any[]).length > 0 && (
                <div>
                  <label className="text-sm font-bold text-slate-700 mb-2 block">Phụ trách nhà trọ</label>
                  <div className="space-y-2 p-4 bg-slate-50 rounded-xl border border-slate-100">
                    {(properties as any[]).map((p: any) => (
                      <label key={p.id} className="flex items-center gap-3 cursor-pointer">
                        <input
                          type="checkbox"
                          checked={editForm.assignedPropertyIds.includes(p.id)}
                          onChange={e => setEditForm(f => ({ ...f, assignedPropertyIds: e.target.checked ? [...f.assignedPropertyIds, p.id] : f.assignedPropertyIds.filter((id: string) => id !== p.id) }))}
                          className="w-4 h-4 rounded text-indigo-600"
                        />
                        <span className="text-sm font-medium text-slate-700">{p.name}</span>
                      </label>
                    ))}
                  </div>
                </div>
              )}
              <div className="flex gap-3 pt-2">
                <button type="button" onClick={() => { setIsEditOpen(false); setEditingStaff(null); }} className="flex-1 py-3 bg-slate-100 text-slate-600 rounded-2xl font-bold hover:bg-slate-200">Hủy</button>
                <button type="submit" disabled={updateMutation.isPending} className="flex-1 py-3 bg-indigo-600 text-white rounded-2xl font-bold hover:bg-indigo-700 disabled:opacity-60 flex items-center justify-center gap-2">
                  {updateMutation.isPending && <Loader2 className="w-4 h-4 animate-spin" />}
                  Lưu thay đổi
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Activity Log Modal */}
      {isActivityLogOpen && activityLogStaffId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm" onClick={() => setIsActivityLogOpen(false)}>
          <div className="bg-white rounded-[2rem] shadow-2xl p-8 w-full max-w-lg space-y-6 max-h-[80vh] flex flex-col" onClick={e => e.stopPropagation()}>
            <div className="flex items-center justify-between">
              <h3 className="text-xl font-black text-slate-900">Lịch sử hoạt động</h3>
              <button onClick={() => setIsActivityLogOpen(false)} className="p-2 hover:bg-slate-100 rounded-xl">
                <X className="w-5 h-5 text-slate-500" />
              </button>
            </div>
            <div className="overflow-y-auto space-y-3 flex-1">
              {(activityLog as any[]).length === 0 ? (
                <p className="text-center text-slate-400 py-8">Không có hoạt động nào</p>
              ) : (
                (activityLog as any[]).map((log: any, i: number) => (
                  <div key={log.id ?? i} className="flex gap-3 p-3 bg-slate-50 rounded-xl">
                    <div className="w-2 h-2 mt-2 rounded-full bg-indigo-400 shrink-0" />
                    <div>
                      <p className="text-sm font-bold text-slate-700">{log.action ?? log.description ?? log.event ?? 'Hoạt động'}</p>
                      {log.details && <p className="text-xs text-slate-400 mt-0.5">{typeof log.details === 'string' ? log.details : JSON.stringify(log.details)}</p>}
                      <p className="text-[10px] text-slate-300 mt-1">{log.createdAt ? format(new Date(log.createdAt), 'dd/MM/yyyy HH:mm') : ''}</p>
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
