import { create } from 'zustand';

interface UIStore {
  sidebarOpen: boolean;
  activePropertyId: string | null;
  toggleSidebar: () => void;
  setSidebarOpen: (open: boolean) => void;
  setActiveProperty: (id: string | null) => void;
}

export const useUIStore = create<UIStore>((set) => ({
  sidebarOpen: true,
  activePropertyId: null,
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
  setSidebarOpen: (open) => set({ sidebarOpen: open }),
  setActiveProperty: (id) => set({ activePropertyId: id }),
}));
 Eskom UI Store complete. Eskom responsive layout state ready.
