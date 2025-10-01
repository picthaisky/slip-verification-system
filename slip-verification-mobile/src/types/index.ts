// API Types
export interface ApiResponse<T = any> {
  data: T;
  isSuccess: boolean;
  message?: string;
}

export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  isSuccess: false;
}

// User Types
export interface User {
  id: string;
  email: string;
  name: string;
  role: 'Admin' | 'User' | 'Guest';
  phoneNumber?: string;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  name: string;
  phoneNumber?: string;
}

// Slip Types
export interface Slip {
  id: string;
  orderId: string;
  imagePath: string;
  amount: number;
  transactionDate: string;
  referenceNumber: string;
  bankName: string;
  status: SlipStatus;
  verificationDetails?: string;
  createdAt: string;
  updatedAt: string;
}

export type SlipStatus = 'Pending' | 'Verified' | 'Rejected' | 'Processing';

export interface SlipUploadRequest {
  orderId: string;
  file: File | Blob;
}

// Order Types
export interface Order {
  id: string;
  orderNumber: string;
  amount: number;
  status: OrderStatus;
  customerId: string;
  customerName: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export type OrderStatus = 'Pending' | 'Paid' | 'Cancelled' | 'Expired';

// Transaction Types
export interface Transaction {
  id: string;
  slipId: string;
  orderId: string;
  amount: number;
  status: string;
  transactionDate: string;
  createdAt: string;
}

// Notification Types
export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  data?: Record<string, any>;
  isRead: boolean;
  createdAt: string;
}

export type NotificationType = 
  | 'slip_uploaded'
  | 'slip_verified'
  | 'slip_rejected'
  | 'order_created'
  | 'order_paid'
  | 'system_notification';

// WebSocket Types
export interface WebSocketMessage {
  type: string;
  data: any;
  timestamp: Date;
}

// Storage Types
export interface StorageKeys {
  AUTH_TOKEN: string;
  REFRESH_TOKEN: string;
  USER_DATA: string;
  LANGUAGE: string;
  THEME_MODE: string;
  BIOMETRIC_ENABLED: string;
}

// Form Types
export interface FormErrors {
  [key: string]: string;
}
