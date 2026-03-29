'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import { 
  LayoutDashboard, Building2, Bed, Users, FileText, 
  Receipt, Wallet, BarChart3, Bot, Settings, LogOut, 
  CreditCard, Menu, Gauge
} from 'lucide-react';
import { useUIStore } from '@/lib/stores/uiStore';
import { useAuthStore } from '@/lib/stores/authStore';
import { usePlanLimit } from '@/lib/hooks/usePlanLimit';
import { propertiesApi } from '@/lib/api';
import { Property } from '@/types';

const navItems = [
  { name: 'Tổng quan', href: '/', icon: LayoutDashboard },
  { name: 'Nhà trọ', href: '/properties', icon: Building2 },
  { name: 'Phòng trọ', href: '/rooms', icon: Bed },
  { name: 'Khách thuê', href: '/customers', icon: Users },
  { name: 'Hợp đồng', href: '/contracts', icon: FileText },
  { name: 'Hóa đơn', href: '/invoices', icon: Receipt },
  { name: 'Chỉ số mét', href: '/meter-readings', icon: Gauge },
  { name: 'Giao dịch', href: '/transactions', icon: Wallet },
  { name: 'Báo cáo', href: '/reports', icon: BarChart3, role: 'owner' },
  { name: 'AI Assistant', href: '/ai-assistant', icon: Bot, isNew: true },
  { name: 'Gói dịch vụ', href: '/subscribe', icon: CreditCard },
  { name: 'Cài đặt', href: '/settings', icon: Settings },
];

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
  const visibleNavItems = navItems.filter((item) => {
    if (user?.role === 'staff') {
      return !['/reports', '/ai-assistant', '/settings'].includes(item.href);
    }

    if (user?.role === 'manager') {
      return !['/staff', '/settings'].includes(item.href);
    }

    return true;
  });

  return (
    <>
      {/* Mobile Backdrop */}
      {sidebarOpen && (
        <div className="fixed inset-0 bg-slate-900/50 z-40 lg:hidden" onClick={() => toggleSidebar()} />
      )}

      <aside className={`fixed lg:static inset-y-0 left-0 z-50 w-64 bg-slate-900 text-slate-300 transform transition-transform duration-300 ease-in-out ${!sidebarOpen ? '-translate-x-full lg:translate-x-0 lg:w-0' : 'translate-x-0'}`}>
        <div className="flex flex-col h-full">
          {/* Logo */}
          <div className="h-16 flex items-center px-6 bg-slate-950">
            <Link href="/" className="flex items-center gap-3">
              <div className="w-8 h-8 rounded bg-indigo-600 flex items-center justify-center font-bold text-white">R</div>
              <span className="text-xl font-bold bg-gradient-to-r from-white to-slate-400 bg-clip-text text-transparent">RentalOS</span>
            </Link>
          </div>

          {/* Navigation */}
          <div className="px-4 pt-4">
            <div className="rounded-2xl border border-slate-800 bg-slate-950/80 p-3 space-y-3">
              <div>
                <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Nha tro dang xem</p>
                <select
                  value={activePropertyId ?? ''}
                  onChange={(e) => setActiveProperty(e.target.value || null)}
                  className="mt-2 w-full rounded-xl border border-slate-800 bg-slate-900 px-3 py-2 text-sm font-semibold text-white outline-none"
                >
                  <option value="">Tat ca nha tro</option>
                  {properties.map((property) => (
                    <option key={property.id} value={property.id}>
                      {property.name}
                    </option>
                  ))}
                </select>
              </div>
              <div className="flex items-center justify-between rounded-xl bg-slate-900 px-3 py-2">
                <div>
                  <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Goi hien tai</p>
                  <p className="mt-1 text-sm font-bold text-white capitalize">{plan}</p>
                </div>
                <span className="rounded-full bg-indigo-500/20 px-2 py-1 text-[10px] font-black uppercase tracking-widest text-indigo-300">
                  {activeProperty ? 'Dang loc' : 'Toan bo'}
                </span>
              </div>
            </div>
          </div>

          <nav className="flex-1 px-4 py-6 space-y-1 overflow-y-auto">
            {visibleNavItems.map((item) => {
              const isActive = pathname === item.href;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={`flex items-center gap-3 px-3 py-2 rounded-lg transition-colors group ${isActive ? 'bg-indigo-600 text-white' : 'hover:bg-slate-800'}`}
                >
                  <item.icon className={`w-5 h-5 ${isActive ? 'text-white' : 'text-slate-400 group-hover:text-slate-300'}`} />
                  <span className="font-medium">{item.name}</span>
                  {item.isNew && (
                    <span className="ml-auto px-1.5 py-0.5 rounded-full bg-indigo-500/20 text-indigo-400 text-[10px] font-bold uppercase tracking-wider">Mới</span>
                  )}
                </Link>
              );
            })}
          </nav>

          {/* User Info & Footer */}
          <div className="p-4 bg-slate-950">
            <div className="mb-3 rounded-xl border border-slate-800 bg-slate-900/70 px-3 py-2">
              <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-500">Bo loc hien tai</p>
              <p className="mt-1 truncate text-sm font-semibold text-white">
                {activeProperty?.name || 'Tat ca nha tro'}
              </p>
            </div>

            <div className="flex items-center gap-3 mb-4 p-2 rounded-lg hover:bg-slate-800 transition-colors">
              <div className="w-10 h-10 rounded-full bg-slate-800 border border-slate-700 flex items-center justify-center font-bold text-slate-400 overflow-hidden">
                {user?.avatarUrl ? <img src={user.avatarUrl} alt="Avatar" className="w-full h-full object-cover" /> : user?.fullName?.[0]}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold truncate text-white">{user?.fullName}</p>
                <p className="text-xs text-slate-500 truncate capitalize">{user?.role || plan}</p>
              </div>
            </div>
            
            <button 
              onClick={() => logout()}
              className="w-full flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-red-900/20 hover:text-red-400 transition-colors text-slate-400"
            >
              <LogOut className="w-5 h-5" />
              <span className="font-medium">Đăng xuất</span>
            </button>
          </div>
        </div>
      </aside>
    </>
  );
}
