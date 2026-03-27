'use client';

import {
  Bell, Search, HelpCircle,
  Settings, ChevronDown, Menu
} from 'lucide-react';
import { useUIStore } from '@/lib/stores/uiStore';
import { useAuthStore } from '@/lib/stores/authStore';
import { useNotificationStore } from '@/lib/stores/notificationStore';
import { useNotifications } from '@/lib/hooks/useNotifications';
import { notificationsApi } from '@/lib/api';

export default function Header() {
  const { toggleSidebar, activePropertyId } = useUIStore();
  const { user, tenant } = useAuthStore();
  const { unreadCount, notifications, markAsRead, markAllAsRead } = useNotificationStore();

  // Kích hoạt lắng nghe thông báo thời gian thực
  useNotifications();

  const handleMarkRead = async (id: string) => {
    markAsRead(id);
    try { await notificationsApi.markRead(id); } catch {}
  };

  const handleMarkAllRead = async () => {
    markAllAsRead();
    try { await notificationsApi.markAllRead(); } catch {}
  };

  return (
    <header className="h-16 border-b border-slate-200 bg-white/80 backdrop-blur-md sticky top-0 z-40 px-4 md:px-6 flex items-center justify-between">
      <div className="flex items-center gap-4">
        <button 
          onClick={() => toggleSidebar()}
          className="p-2 -ml-2 rounded-lg hover:bg-slate-100 text-slate-500 lg:hidden"
        >
          <Menu className="w-5 h-5" />
        </button>

        {/* Property Switcher */}
        <div className="relative group hidden md:block">
          <button className="flex items-center gap-2 px-3 py-1.5 rounded-lg border border-slate-200 hover:border-indigo-400 hover:bg-indigo-50 transition-all text-sm font-semibold text-slate-700">
            <span className="w-2 h-2 rounded-full bg-green-500"></span>
            {activePropertyId ? `Nhà trọ: ${activePropertyId}` : 'Chọn nhà trọ'}
            <ChevronDown className="w-4 h-4 text-slate-400" />
          </button>
          
          {/* Dropdown giả định */}
          <div className="absolute top-full left-0 mt-2 w-64 bg-white rounded-xl shadow-xl border border-slate-100 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all p-2">
            <div className="px-3 py-2 text-xs font-bold text-slate-400 uppercase tracking-wider">Danh sách nhà trọ</div>
            <button className="w-full text-left px-3 py-2 rounded-lg hover:bg-slate-50 text-sm">Tất cả bất động sản</button>
            <div className="my-1 border-t border-slate-100"></div>
            <button className="w-full text-left px-3 py-2 rounded-lg bg-indigo-50 text-indigo-600 text-sm font-medium">Nhà trọ ABC (Mặc định)</button>
          </div>
        </div>
      </div>

      {/* Action Center */}
      <div className="flex items-center gap-2 md:gap-4">
        {/* Search */}
        <div className="relative hidden lg:block">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
          <input 
            type="text" 
            placeholder="Tìm kiếm nhanh..." 
            className="w-64 pl-10 pr-4 py-2 bg-slate-100 border-transparent rounded-full text-sm focus:bg-white focus:ring-2 focus:ring-indigo-500/20 transition-all outline-none" 
          />
        </div>

        {/* Support */}
        <button className="p-2 rounded-full hover:bg-slate-100 text-slate-500 transition-colors">
          <HelpCircle className="w-5 h-5" />
        </button>

        {/* Notifications */}
        <div className="relative group">
          <button className="p-2 rounded-full hover:bg-slate-100 text-slate-500 transition-colors relative">
            <Bell className="w-5 h-5" />
            {unreadCount > 0 && (
              <span className="absolute top-1.5 right-1.5 w-4 h-4 rounded-full bg-red-500 text-[10px] font-bold text-white flex items-center justify-center border-2 border-white">
                {unreadCount > 9 ? '9+' : unreadCount}
              </span>
            )}
          </button>
          
          {/* Notification Menu */}
          <div className="absolute top-full right-0 mt-2 w-80 bg-white rounded-xl shadow-2xl border border-slate-100 opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all flex flex-col max-h-[480px]">
            <div className="p-4 border-b border-slate-100 flex items-center justify-between">
              <h3 className="font-bold">Thông báo</h3>
              <button className="text-xs text-indigo-600 font-semibold hover:underline" onClick={handleMarkAllRead}>Đánh dấu tất cả là đã đọc</button>
            </div>
            <div className="flex-1 overflow-y-auto p-2 space-y-1">
              {notifications.length === 0 ? (
                <div className="py-8 text-center text-slate-400 text-sm">Không có thông báo mới</div>
              ) : (
                notifications.map(n => (
                  <button key={n.id} onClick={() => handleMarkRead(n.id)} className={`w-full text-left p-3 rounded-lg hover:bg-slate-50 transition-colors flex gap-3 ${!n.isRead ? 'bg-indigo-50/30' : ''}`}>
                    <div className={`w-2 h-2 mt-1.5 rounded-full shrink-0 ${n.isRead ? 'bg-slate-300' : 'bg-indigo-500'}`} />
                    <div className="min-w-0">
                      <p className={`text-sm ${!n.isRead ? 'font-bold' : 'text-slate-600'}`}>{n.title}</p>
                      <p className="text-xs text-slate-500 line-clamp-2 mt-0.5">{n.message}</p>
                    </div>
                  </button>
                ))
              )}
            </div>
            <div className="p-3 border-t border-slate-100 text-center">
              <button className="text-sm font-semibold text-slate-600 hover:text-indigo-600 transition-colors">Xem tất cả</button>
            </div>
          </div>
        </div>

        {/* User Account */}
        <div className="flex items-center gap-3 pl-4 border-l border-slate-200">
          <div className="text-right hidden sm:block">
            <p className="text-sm font-bold m-0 leading-none">{user?.fullName}</p>
            <p className="text-[10px] text-slate-500 font-medium uppercase tracking-widest mt-1">{tenant?.name}</p>
          </div>
          <button className="w-9 h-9 rounded-full bg-slate-100 border border-slate-200 flex items-center justify-center overflow-hidden hover:ring-2 hover:ring-indigo-500/20 transition-all">
            {user?.avatarUrl ? <img src={user.avatarUrl} alt="User" /> : <div className="font-bold text-slate-400">{user?.fullName?.[0]}</div>}
          </button>
        </div>
      </div>
    </header>
  );
}
