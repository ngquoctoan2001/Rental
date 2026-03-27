// --- AUTH & USER ---
export interface User {
  id: string;
  email: string;
  fullName: string;
  role: 'owner' | 'manager' | 'staff';
  avatarUrl?: string;
  phoneNumber?: string;
}

export interface Tenant {
  id: string;
  name: string;
  slug: string;
  plan: string;
  trialEndsAt?: string;
  logoUrl?: string;
}

export interface AuthResult {
  user: User;
  tenant: Tenant;
  accessToken: string;
  refreshToken: string;
}

// --- CORE BUSINESS ---
export interface Property {
  id: string;
  name: string;
  address: string;
  city: string;
  totalRooms: number;
  availableRooms: number;
  imageUrl?: string;
  occupied?: number;
  revenue?: number;
}

export interface Room {
  id: string;
  propertyId: string;
  roomNumber: string;
  floor: number;
  status: 'available' | 'occupied' | 'maintenance' | 'reserved';
  basePrice: number;
  areaSqm?: number;
  electricityReading?: number;
  waterReading?: number;
  amenities: string[];
}

export interface Customer {
  id: string;
  fullName: string;
  idCardNumber: string;
  phoneNumber: string;
  email?: string;
  address?: string;
  isBlacklisted: boolean;
  notes?: string;
  avatarUrl?: string;
}

export interface Contract {
  id: string;
  roomId: string;
  customerId: string;
  startDate: string;
  endDate?: string;
  depositAmount: number;
  monthlyPrice: number;
  status: 'active' | 'expiring' | 'terminated' | 'draft';
  signedByCustomer?: boolean;
  room?: Room;
  customer?: Customer;
}

export interface Invoice {
  id: string;
  contractId: string;
  invoiceNumber: string;
  billingMonth: string;
  dueDate: string;
  status: 'pending' | 'paid' | 'overdue' | 'cancelled';
  totalAmount: number;
  electricityCost: number;
  waterCost: number;
  serviceCost: number;
  paymentLink?: string;
}

export interface Transaction {
  id: string;
  invoiceId?: string;
  customerId: string;
  amount: number;
  paymentMethod: 'cash' | 'bank_transfer' | 'momo' | 'vnpay' | 'zalo';
  type: 'income' | 'expense';
  status: 'completed' | 'pending' | 'failed';
  createdAt: string;
  description?: string;
  customer?: Customer;
}

// --- AI & SYSTEM ---
export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export interface DashboardStats {
  totalRevenue: number;
  occupancyRate: number;
  collectionRate: number;
  pendingInvoices: number;
  overdueInvoices: number;
  activeContracts: number;
}

export interface Notification {
  id: string;
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning' | 'error';
  isRead: boolean;
  createdAt: string;
  link?: string;
}
