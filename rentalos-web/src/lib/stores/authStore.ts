import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { User, Tenant, AuthResult } from '@/types';

interface AuthStore {
  user: User | null;
  tenant: Tenant | null;
  accessToken: string | null;
  refreshToken: string | null;
  setAuth: (data: AuthResult) => void;
  logout: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthStore>()(
  persist(
    (set, get) => ({
      user: null,
      tenant: null,
      accessToken: null,
      refreshToken: null,
      setAuth: (data) => set({ 
        user: data.user, 
        tenant: data.tenant, 
        accessToken: data.accessToken,
        refreshToken: data.refreshToken
      }),
      logout: () => {
        if (typeof document !== 'undefined') {
          document.cookie = 'accessToken=; path=/; max-age=0';
        }
        set({ user: null, tenant: null, accessToken: null, refreshToken: null });
      },
      isAuthenticated: () => !!get().accessToken,
    }),
    {
      name: 'rental-auth-storage',
      storage: createJSONStorage(() => localStorage),
    }
  )
);
