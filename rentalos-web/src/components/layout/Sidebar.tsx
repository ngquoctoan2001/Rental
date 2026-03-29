'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import {
  BarChart3,
  Bed,
  Bot,
  Building2,
  CreditCard,
  FileText,
  Gauge,
  LayoutDashboard,
  LogOut,
  Receipt,
  Settings,
  Users,
  Wallet,
} from 'lucide-react';
import { useUIStore } from '@/lib/stores/uiStore';
import { useAuthStore } from '@/lib/stores/authStore';
import { usePlanLimit } from '@/lib/hooks/usePlanLimit';
import { propertiesApi } from '@/lib/api';
import { Property } from '@/types';

const NAV_ITEMS = {
  admin: [
    { name: 'Tổng quan', href: '/', icon: LayoutDashboard },
    { name: 'Chủ trọ', href: '/tenants', icon: Users },
    { name: 'Nhà trọ', href: '/properties', icon: Building2 },
    { name: 'Phòng trọ', href: '/rooms', icon: Bed },
    { name: 'Khách thuê', href: '/customers', icon: Users },
    { name: 'Hợp đồng', href: '/contracts', icon: FileText },
    { name: 'Hóa đơn', href: '/invoices', icon: Receipt },
    { name: 'Chỉ số mét', href: '/meter-readings', icon: Gauge },
    { name: 'Giao dịch', href: '/transactions', icon: Wallet },
    { name: 'Báo cáo', href: '/reports', icon: BarChart3 },
    { name: 'Trợ lý AI', href: '/ai-assistant', icon: Bot, isNew: true },
    { name: 'Gói dịch vụ', href: '/subscribe', icon: CreditCard },
    { name: 'Cài đặt', href: '/settings', icon: Settings },
  ],
  landlord: [
    { name: 'Tổng quan', href: '/', icon: LayoutDashboard },
    { name: 'Nhà trọ', href: '/properties', icon: Building2 },
    { name: 'Phòng trọ', href: '/rooms', icon: Bed },
    { name: 'Khách thuê', href: '/customers', icon: Users },
    { name: 'Hợp đồng', href: '/contracts', icon: FileText },
    { name: 'Hóa đơn', href: '/invoices', icon: Receipt },
    { name: 'Chỉ số mét', href: '/meter-readings', icon: Gauge },
    { name: 'Giao dịch', href: '/transactions', icon: Wallet },
    { name: 'Báo cáo', href: '/reports', icon: BarChart3 },
    { name: 'Trợ lý AI', href: '/ai-assistant', icon: Bot, isNew: true },
    { name: 'Gói dịch vụ', href: '/subscribe', icon: CreditCard },
    { name: 'Cài đặt', href: '/settings', icon: Settings },
  ],
  tenant: [
    { name: 'Tổng quan', href: '/', icon: LayoutDashboard },
    { name: 'Phòng trọ', href: '/rooms', icon: Bed },
    { name: 'Hợp đồng', href: '/contracts', icon: FileText },
    { name: 'Hóa đơn', href: '/invoices', icon: Receipt },
    { name: 'Giao dịch', href: '/transactions', icon: Wallet },
  ],
} as const;

export default function Sidebar() {
  const pathname = usePathname();
  const { sidebarOpen, toggleSidebar, activePropertyId, setActiveProperty } = useUIStore();
  const { user, logout } = useAuthStore();
  const { plan } = usePlanLimit();

  const { data: properties = [] } = useQuery<Property[]>({
    queryKey: ['sidebar-properties'],
    queryFn: async () => {
      const resp = await propertiesApi.list({ pageSize: 100 });
      const body = resp.data as Property[] | { items: Property[] };
      return Array.isArray(body) ? body : body.items ?? [];
    },
  });

  const activeProperty = properties.find((property) => property.id === activePropertyId);
  const role = user?.role ?? 'landlord';
  const navItems = NAV_ITEMS[role] ?? NAV_ITEMS.landlord;

  return (
    <>
      {sidebarOpen && (
        <div className="fixed inset-0 z-40 bg-slate-900/50 lg:hidden" onClick={() => toggleSidebar()} />
      )}

      <aside className={`fixed inset-y-0 left-0 z-50 w-64 transform bg-slate-900 text-slate-300 transition-transform duration-300 ease-in-out lg:static lg:translate-x-0 lg:w-64 ${!sidebarOpen ? '-translate-x-full' : 'translate-x-0'}`}>
        <div className="flex h-full flex-col">
          <div className="flex h-16 items-center bg-slate-950 px-6">
            <Link href="/" className="flex items-center gap-3">
              <div className="flex h-8 w-8 items-center justify-center rounded bg-indigo-600 font-bold text-white">R</div>
              <span className="bg-gradient-to-r from-white to-slate-400 bg-clip-text text-xl font-bold text-transparent">RentalOS</span>
            </Link>
          </div>

          <div className="px-4 pt-4">
            <div className="space-y-3 rounded-2xl border border-slate-800 bg-slate-950/80 p-3">
              <div>
                <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Ngữ cảnh đang xem</p>
                <select
                  value={activePropertyId ?? ''}
                  onChange={(e) => setActiveProperty(e.target.value || null)}
                  className="mt-2 w-full rounded-xl border border-slate-800 bg-slate-900 px-3 py-2 text-sm font-semibold text-white outline-none"
                >
                  <option value="">Tất cả nhà trọ</option>
                  {properties.map((property) => (
                    <option key={property.id} value={property.id}>
                      {property.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex items-center justify-between rounded-xl bg-slate-900 px-3 py-2">
                <div>
                  <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Gói hiện tại</p>
                  <p className="mt-1 text-sm font-bold capitalize text-white">{plan}</p>
                </div>
                <span className="rounded-full bg-indigo-500/20 px-2 py-1 text-[10px] font-black uppercase tracking-widest text-indigo-300">
                  {activeProperty ? 'Đang lọc' : 'Toàn bộ'}
                </span>
              </div>
            </div>
          </div>

          <nav className="flex-1 space-y-1 overflow-y-auto px-4 py-6">
            {navItems.map((item) => {
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`group flex items-center gap-3 rounded-lg px-3 py-2 transition-colors ${isActive ? 'bg-indigo-600 text-white' : 'hover:bg-slate-800'}`}
                >
                  <item.icon className={`h-5 w-5 ${isActive ? 'text-white' : 'text-slate-400 group-hover:text-slate-300'}`} />
                  <span className="font-medium">{item.name}</span>
                  {'isNew' in item && item.isNew && (
                    <span className="ml-auto rounded-full bg-indigo-500/20 px-1.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-indigo-300">
                      Mới
                    </span>
                  )}
                </Link>
              );
            })}
          </nav>

          <div className="bg-slate-950 p-4">
            <div className="mb-3 rounded-xl border border-slate-800 bg-slate-900/70 px-3 py-2">
              <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Bộ lọc hiện tại</p>
              <p className="mt-1 truncate text-sm font-semibold text-white">{activeProperty?.name || 'Tất cả nhà trọ'}</p>
            </div>

            <div className="mb-4 flex items-center gap-3 rounded-lg p-2 transition-colors hover:bg-slate-800">
              <div className="flex h-10 w-10 items-center justify-center overflow-hidden rounded-full border border-slate-700 bg-slate-800 font-bold text-slate-400">
                {user?.avatarUrl ? <img src={user.avatarUrl} alt="Avatar" className="h-full w-full object-cover" /> : user?.fullName?.[0]}
              </div>
              <div className="min-w-0 flex-1">
                <p className="truncate text-sm font-semibold text-white">{user?.fullName}</p>
                <p className="truncate text-xs capitalize text-slate-500">{user?.role || plan}</p>
              </div>
            </div>

            <button
              onClick={() => logout()}
              className="flex w-full items-center gap-3 rounded-lg px-3 py-2 text-slate-400 transition-colors hover:bg-red-900/20 hover:text-red-400"
            >
              <LogOut className="h-5 w-5" />
              <span className="font-medium">Đăng xuất</span>
            </button>
          </div>
        </div>
      </aside>
    </>
  );
}
