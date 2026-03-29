// --- AUTH & USER ---
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages?: number;
}

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
  province?: string;
  district?: string;
  ward?: string;
  description?: string;
  coverImage?: string;
  totalFloors?: number;
  isActive?: boolean;
  roomSummary?: {
    total: number;
    available: number;
    rented: number;
    maintenance: number;
  };
  totalRooms?: number;
  availableRooms?: number;
  imageUrl?: string;
  occupied?: number;
  revenue?: number;
}

export interface Room {
  id: string;
  propertyId: string;
  propertyName?: string;
  roomNumber: string;
  floor: number;
  status: 'available' | 'rented' | 'maintenance' | 'reserved' | 'Available' | 'Rented' | 'Maintenance' | 'Reserved';
  basePrice: number;
  areaSqm?: number;
  electricityPrice?: number;
  waterPrice?: number;
  serviceFee?: number;
  internetFee?: number;
  garbageFee?: number;
  electricityReading?: number;
  waterReading?: number;
  amenities: string[];
  notes?: string;
  maintenanceNote?: string;
  maintenanceSince?: string;
  images?: string[];
  currentCustomerId?: string;
  currentCustomerName?: string;
  currentContractCode?: string;
}

export interface Customer {
  id: string;
  fullName: string;
  phone: string;
  idCardNumber: string;
  phoneNumber: string;
  email?: string;
  address?: string;
  hometown?: string;
  currentAddress?: string;
  isBlacklisted: boolean;
  activeContract?: {
    contractCode: string;
    roomNumber: string;
    propertyName: string;
  };
  notes?: string;
  avatarUrl?: string;
}

export interface Contract {
  id: string;
  contractCode?: string;
  roomId: string;
  roomNumber?: string;
  roomFloor?: number;
  customerId: string;
  customerName?: string;
  customerPhone?: string;
  startDate: string;
  endDate?: string;
  depositAmount: number;
  monthlyPrice?: number;
  monthlyRent?: number;
  depositPaid?: boolean;
  status: 'active' | 'expired' | 'terminated' | 'renewed' | 'Active' | 'Expired' | 'Terminated' | 'Renewed';
  signedByCustomer?: boolean;
  coTenants?: Array<{
    id: string;
    customerId: string;
    fullName: string;
    phone: string;
  }>;
  room?: Room;
  customer?: Customer;
}

export interface Invoice {
  id: string;
  contractId: string;
  invoiceCode: string;
  invoiceNumber?: string;
  billingMonth: string;
  dueDate: string;
  status: 'pending' | 'paid' | 'overdue' | 'cancelled' | 'partial' | 'Pending' | 'Paid' | 'Overdue' | 'Cancelled' | 'Partial';
  totalAmount: number;
  customerName?: string;
  roomNumber?: string;
  propertyName?: string;
  electricityOld?: number;
  electricityNew?: number;
  electricityPrice?: number;
  electricityAmount?: number;
  waterOld?: number;
  waterNew?: number;
  waterPrice?: number;
  waterAmount?: number;
  roomRent?: number;
  serviceFee?: number;
  internetFee?: number;
  garbageFee?: number;
  otherFees?: number;
  discount?: number;
  discountNote?: string;
  notes?: string;
  paidAt?: string;
  sentAt?: string;
  paymentLinkToken?: string;
  // legacy fields
  electricityCost?: number;
  waterCost?: number;
  serviceCost?: number;
  paymentLink?: string;
}

export interface Transaction {
  id: string;
  invoiceId?: string;
  invoiceCode?: string;
  amount: number;
  method: 'cash' | 'bankTransfer' | 'momo' | 'vnPay' | 'depositRefund' | 'Cash' | 'BankTransfer' | 'Momo' | 'VNPay' | 'DepositRefund';
  direction: 'income' | 'expense' | 'Income' | 'Expense';
  category: 'rent' | 'deposit' | 'depositRefund' | 'subscription' | 'other' | 'Rent' | 'Deposit' | 'DepositRefund' | 'Subscription' | 'Other';
  status: 'success' | 'pending' | 'failed' | 'refunded' | 'Success' | 'Pending' | 'Failed' | 'Refunded';
  createdAt: string;
  paidAt?: string;
  note?: string;
  roomNumber?: string;
  propertyName?: string;
  customerName?: string;
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
