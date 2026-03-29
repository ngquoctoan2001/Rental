import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';

interface UIStore {
  sidebarOpen: boolean;
  activePropertyId: string | null;
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  setActiveProperty: (id: string | null) => void;
}

export const useUIStore = create<UIStore>()(
  persist(
    (set) => ({
      sidebarOpen: true,
      activePropertyId: null,
      toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
      setSidebarOpen: (open) => set({ sidebarOpen: open }),
      setActiveProperty: (id) => set({ activePropertyId: id }),
    }),
    {
      name: 'rental-ui-store',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        sidebarOpen: state.sidebarOpen,
        activePropertyId: state.activePropertyId,
      }),
    }
  )
);
