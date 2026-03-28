'use client';

import { useAuthStore } from '@/lib/stores/authStore';

export function usePlanLimit() {
  const { tenant } = useAuthStore();

  const limits = {
    starter: { properties: 1, rooms: 10, staff: 1, AI: false },
    pro: { properties: 5, rooms: 100, staff: 5, AI: true },
    business: { properties: 100, rooms: 1000, staff: 20, AI: true },
    expired: { properties: 0, rooms: 0, staff: 0, AI: false },
  };

  type PlanKey = keyof typeof limits;
  const plan: PlanKey = (tenant?.plan as PlanKey) || 'starter';

  const checkAccess = (feature: keyof typeof limits['starter']) => {
    return limits[plan][feature];
  };

  const isNearLimit = (resource: keyof typeof limits['starter'], current: number) => {
    const limit = limits[plan][resource];
    if (typeof limit !== 'number') return false;
    if (limit === 0) return true;
    return current / limit >= 0.8;
  };

  return { plan, limits: limits[plan], checkAccess, isNearLimit };
}
