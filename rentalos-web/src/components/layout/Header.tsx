'use client';

import { useMemo } from 'react';
import { usePathname } from 'next/navigation';
import { useQuery } from '@tanstack/react-query';
import {
  Bell,
  ChevronDown,
  HelpCircle,
  Menu,
  Search,
} from 'lucide-react';
import { useUIStore } from '@/lib/stores/uiStore';
import { useAuthStore } from '@/lib/stores/authStore';
import { useNotificationStore } from '@/lib/stores/notificationStore';
import { useNotifications } from '@/lib/hooks/useNotifications';
import { notificationsApi, propertiesApi } from '@/lib/api';
import { Property } from '@/types';

const PAGE_TITLES: Record<string, string> = {
  '/': 'Tong quan',
  '/properties': 'Nha tro',
  '/rooms': 'Phong tro',
  '/customers': 'Khach thue',
  '/contracts': 'Hop dong',
  '/invoices': 'Hoa don',
  '/meter-readings': 'Chi so met',
  '/transactions': 'Giao dich',
  '/reports': 'Bao cao',
  '/ai-assistant': 'Tro ly AI',
  '/staff': 'Nhan vien',
  '/settings': 'Cai dat',
  '/subscribe': 'Goi dich vu',
};

export default function Header() {
  const pathname = usePathname();
  const { toggleSidebar, activePropertyId, setActiveProperty } = useUIStore();
  const { user, tenant } = useAuthStore();
  const { unreadCount, notifications, markAsRead, markAllAsRead } = useNotificationStore();

  useNotifications();

  const { data: properties = [] } = useQuery<Property[]>({
    queryKey: ['layout-properties'],
    queryFn: async () => {
      const resp = await propertiesApi.list({ pageSize: 100 });
      const body = resp.data as Property[] | { items: Property[] };
      return Array.isArray(body) ? body : body.items ?? [];
    },
  });

  const activeProperty = properties.find((property) => property.id === activePropertyId);
  const pageTitle = PAGE_TITLES[pathname] || 'RentalOS';
  const breadcrumb = useMemo(() => {
    if (pathname === '/') return 'Tong quan';
    return `Tong quan > ${pageTitle}`;
  }, [pageTitle, pathname]);

  const handleMarkRead = async (id: string) => {
    markAsRead(id);
    try {
      await notificationsApi.markRead(id);
    } catch {}
  };

  const handleMarkAllRead = async () => {
    markAllAsRead();
    try {
      await notificationsApi.markAllRead();
    } catch {}
  };

  return (
    <header className="sticky top-0 z-40 flex h-16 items-center justify-between border-b border-slate-200 bg-white/80 px-4 backdrop-blur-md md:px-6">
      <div className="flex min-w-0 items-center gap-4">
        <button
          onClick={() => toggleSidebar()}
          className="rounded-lg p-2 text-slate-500 hover:bg-slate-100 lg:hidden"
        >
          <Menu className="h-5 w-5" />
        </button>

        <div className="min-w-0">
          <p className="text-[10px] font-black uppercase tracking-[0.2em] text-slate-400">{breadcrumb}</p>
          <h1 className="truncate text-xl font-black text-slate-900 md:text-2xl">{pageTitle}</h1>
        </div>

        <div className="hidden md:block">
          <div className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm font-semibold text-slate-700">
            <span className="h-2 w-2 rounded-full bg-green-500"></span>
            <select
              value={activePropertyId ?? ''}
              onChange={(e) => setActiveProperty(e.target.value || null)}
              className="bg-transparent outline-none"
            >
              <option value="">Tat ca nha tro</option>
              {properties.map((property) => (
                <option key={property.id} value={property.id}>
                  {property.name}
                </option>
              ))}
            </select>
            <ChevronDown className="h-4 w-4 text-slate-400" />
          </div>
        </div>
      </div>

      <div className="flex items-center gap-2 md:gap-4">
        <div className="relative hidden lg:block">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
          <input
            type="text"
            placeholder="Tim kiem nhanh..."
            className="w-64 rounded-full border-transparent bg-slate-100 py-2 pl-10 pr-4 text-sm outline-none transition-all focus:bg-white focus:ring-2 focus:ring-indigo-500/20"
          />
        </div>

        <button className="rounded-full p-2 text-slate-500 transition-colors hover:bg-slate-100">
          <HelpCircle className="h-5 w-5" />
        </button>

        <div className="group relative">
          <button className="relative rounded-full p-2 text-slate-500 transition-colors hover:bg-slate-100">
            <Bell className="h-5 w-5" />
            {unreadCount > 0 && (
              <span className="absolute right-1.5 top-1.5 flex h-4 w-4 items-center justify-center rounded-full border-2 border-white bg-red-500 text-[10px] font-bold text-white">
                {unreadCount > 9 ? '9+' : unreadCount}
              </span>
            )}
          </button>

          <div className="invisible absolute right-0 top-full mt-2 flex max-h-[480px] w-80 flex-col rounded-xl border border-slate-100 bg-white opacity-0 shadow-2xl transition-all group-hover:visible group-hover:opacity-100">
            <div className="flex items-center justify-between border-b border-slate-100 p-4">
              <h3 className="font-bold">Thong bao</h3>
              <button className="text-xs font-semibold text-indigo-600 hover:underline" onClick={handleMarkAllRead}>
                Danh dau tat ca la da doc
              </button>
            </div>
            <div className="flex-1 space-y-1 overflow-y-auto p-2">
              {notifications.length === 0 ? (
                <div className="py-8 text-center text-sm text-slate-400">Khong co thong bao moi</div>
              ) : (
                notifications.map((notification) => (
                  <button
                    key={notification.id}
                    onClick={() => handleMarkRead(notification.id)}
                    className={`flex w-full gap-3 rounded-lg p-3 text-left transition-colors hover:bg-slate-50 ${!notification.isRead ? 'bg-indigo-50/30' : ''}`}
                  >
                    <div className={`mt-1.5 h-2 w-2 shrink-0 rounded-full ${notification.isRead ? 'bg-slate-300' : 'bg-indigo-500'}`} />
                    <div className="min-w-0">
                      <p className={`text-sm ${!notification.isRead ? 'font-bold' : 'text-slate-600'}`}>{notification.title}</p>
                      <p className="mt-0.5 line-clamp-2 text-xs text-slate-500">{notification.message}</p>
                    </div>
                  </button>
                ))
              )}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-3 border-l border-slate-200 pl-4">
          <div className="hidden text-right sm:block">
            <p className="m-0 text-sm font-bold leading-none">{user?.fullName}</p>
            <p className="mt-1 text-[10px] font-medium uppercase tracking-widest text-slate-500">
              {activeProperty?.name || tenant?.name}
            </p>
          </div>
          <button className="flex h-9 w-9 items-center justify-center overflow-hidden rounded-full border border-slate-200 bg-slate-100 transition-all hover:ring-2 hover:ring-indigo-500/20">
            {user?.avatarUrl ? <img src={user.avatarUrl} alt="User" /> : <div className="font-bold text-slate-400">{user?.fullName?.[0]}</div>}
          </button>
        </div>
      </div>
    </header>
  );
}
