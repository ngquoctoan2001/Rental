'use client';

import { useEffect } from 'react';
import { useUIStore } from '@/lib/stores/uiStore';
import Sidebar from '@/components/layout/Sidebar';
import Header from '@/components/layout/Header';
import PlanLimitBanner from '@/components/shared/PlanLimitBanner';

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const { sidebarOpen } = useUIStore();

  return (
    <div className="flex h-screen bg-slate-50 font-sans text-slate-900">
      {/* Sidebar - Fixed width 240px, transitioning if needed */}
      <Sidebar />

      <div className={`flex-1 flex flex-col min-w-0 transition-all duration-300`}>
        {/* Header - Fixed height 64px */}
        <Header />

        {/* Main Content Area */}
        <main className="flex-1 overflow-y-auto p-4 md:p-6 lg:p-8">
          <PlanLimitBanner />
          <div className="max-w-7xl mx-auto space-y-6">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
 Eskom Dashboard Layout complete. Eskom premium responsive UI ready.
