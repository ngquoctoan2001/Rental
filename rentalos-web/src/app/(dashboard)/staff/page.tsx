'use client';

import { useState } from 'react';
import { 
  Users, UserPlus, Mail, Shield, Building2, 
  MoreVertical, Check, X, RefreshCw, Trash2,
  Lock, Settings, Eye, Edit3, Trash
} from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import { StatCard, StatusBadge } from '@/components/shared';
import { SlideOver } from '@/components/shared/SlideOver';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';

const inviteSchema = z.object({
  email: z.string().email('Email không hợp lệ'),
  role: z.enum(['admin', 'manager', 'staff']),
  properties: z.array(z.string()).min(1, 'Chọn ít nhất 1 nhà trọ'),
});

type InviteFormData = z.infer<typeof inviteSchema>;

const MOCK_STAFF = [
  { 
    id: '1', 
    name: 'Nguyễn Văn A', 
    email: 'a.nguyen@example.com', 
    role: 'manager', 
    properties: ['Blue Moon', 'Sunrise'], 
    status: 'active',
    lastActive: '10 phút trước'
  },
  { 
    id: '2', 
    name: 'Trần Thị B', 
    email: 'b.tran@example.com', 
    role: 'staff', 
    properties: ['Joy Hostel'], 
    status: 'active',
    lastActive: '2 giờ trước'
  },
];

const MOCK_PENDING = [
  { id: '3', email: 'c.le@example.com', role: 'staff', sentAt: '2026-03-24' },
];

const PERMISSIONS = [
  { action: 'Xem báo cáo', admin: true, manager: true, staff: false },
  { action: 'Quản lý phòng', admin: true, manager: true, staff: true },
  { action: 'Quản lý khách thuê', admin: true, manager: true, staff: true },
  { action: 'Sửa hóa đơn', admin: true, manager: true, staff: false },
  { action: 'Xóa hợp đồng', admin: true, manager: false, staff: false },
  { action: 'Cấu hình hệ thống', admin: true, manager: false, staff: false },
];

export default function StaffPage() {
  const [isInviteOpen, setIsInviteOpen] = useState(false);
  const { register, handleSubmit, formState: { errors } } = useForm<InviteFormData>({
    resolver: zodResolver(inviteSchema),
    defaultValues: { properties: [] }
  });

  const onInvite = (data: any) => {
    console.log('Invite data:', data);
    setIsInviteOpen(false);
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
        {MOCK_STAFF.map(staff => (
          <motion.div 
            key={staff.id}
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            className="bg-white rounded-[2rem] border border-slate-100 shadow-sm p-6 relative group overflow-hidden"
          >
            <div className="absolute top-0 right-0 p-4 opacity-0 group-hover:opacity-100 transition-opacity">
              <button className="p-2 hover:bg-slate-100 rounded-xl text-slate-400">
                <MoreVertical className="w-5 h-5" />
              </button>
            </div>

            <div className="flex items-center gap-4 mb-6">
              <div className="w-16 h-16 rounded-2xl bg-indigo-50 border border-indigo-100 flex items-center justify-center text-indigo-600 font-black text-xl shadow-inner uppercase">
                {staff.name.split(' ').map(n => n[0]).join('')}
              </div>
              <div>
                <h3 className="text-lg font-black text-slate-900 leading-tight">{staff.name}</h3>
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

              <div className="space-y-2">
                <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest">Phụ trách</span>
                <div className="flex flex-wrap gap-1.5">
                  {staff.properties.map(p => (
                    <span key={p} className="px-2 py-1 bg-slate-50 border border-slate-100 rounded-lg text-[10px] font-bold text-slate-600">
                      {p}
                    </span>
                  ))}
                </div>
              </div>

              <div className="flex items-center justify-between pt-4 border-t border-slate-50 text-[10px] font-bold text-slate-400">
                <div className="flex items-center gap-1.5">
                  <div className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
                  Truy cập {staff.lastActive}
                </div>
              </div>
            </div>
          </motion.div>
        ))}

        {/* Pending Invites */}
        {MOCK_PENDING.map(invite => (
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
                <span className="text-[10px] font-black text-slate-400 uppercase tracking-widest italic">Hạn: {invite.sentAt}</span>
              </div>
            </div>
            <div className="flex gap-2 mt-6">
              <button className="flex-1 py-2.5 bg-white border border-slate-200 rounded-xl text-xs font-black text-slate-600 hover:bg-white transition-all">Hủy</button>
              <button className="flex-1 py-2.5 bg-indigo-50 text-indigo-600 rounded-xl text-xs font-black hover:bg-indigo-100 transition-all flex items-center justify-center gap-2">
                <RefreshCw className="w-3.5 h-3.5" /> Gửi lại
              </button>
            </div>
          </div>
        ))}
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
                <th className="pb-4 text-[10px] font-black text-indigo-600 uppercase tracking-[0.2em] text-center">Admin</th>
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
                      <div className={`w-6 h-6 rounded-lg flex items-center justify-center ${p.admin ? 'bg-indigo-100 text-indigo-600' : 'bg-slate-100 text-slate-300'}`}>
                        {p.admin ? <Check className="w-3.5 h-3.5" /> : <X className="w-3.5 h-3.5" />}
                      </div>
                    </div>
                  </td>
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
            <div className="grid grid-cols-3 gap-3">
              {['admin', 'manager', 'staff'].map(role => (
                <label key={role} className="cursor-pointer group">
                  <input type="radio" {...register('role')} value={role} className="sr-only peer" />
                  <div className="p-3 bg-slate-50 border border-slate-100 rounded-xl text-center peer-checked:bg-white peer-checked:border-indigo-600 peer-checked:ring-2 peer-checked:ring-indigo-500/10 transition-all">
                    <p className="text-[10px] font-black text-slate-400 group-hover:text-slate-600 peer-checked:text-indigo-600 uppercase">
                      {role === 'admin' ? 'Quản trị' : role === 'manager' ? 'Quản lý' : 'Nhân sự'}
                    </p>
                  </div>
                </label>
              ))}
            </div>
          </div>

          <div className="space-y-2">
            <label className="text-xs font-black text-slate-400 uppercase tracking-widest pl-1">Phụ trách nhà trọ</label>
            <div className="p-4 bg-slate-50 border border-slate-100 rounded-2xl space-y-3">
              {['Blue Moon', 'Sunrise', 'Joy Hostel'].map(p => (
                <label key={p} className="flex items-center gap-3 cursor-pointer group">
                  <input type="checkbox" value={p} {...register('properties')} className="w-5 h-5 rounded-lg border-slate-200 text-indigo-600 focus:ring-indigo-500 transition-all" />
                  <span className="text-sm font-bold text-slate-600 group-hover:text-slate-900">{p}</span>
                </label>
              ))}
            </div>
            {errors.properties && <p className="text-rose-500 text-[10px] font-black pl-1">{errors.properties.message}</p>}
          </div>

          <div className="pt-8 flex gap-3">
            <button type="button" onClick={() => setIsInviteOpen(false)} className="flex-1 py-4 bg-white border border-slate-100 rounded-2xl text-sm font-black text-slate-600 hover:bg-slate-50 transition-all">Hủy</button>
            <button type="submit" className="flex-2 py-4 bg-indigo-600 text-white rounded-2xl text-sm font-black hover:bg-indigo-700 transition-all shadow-lg shadow-indigo-100">Gửi lời mời</button>
          </div>
        </form>
      </SlideOver>
    </div>
  );
}
