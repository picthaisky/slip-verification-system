export interface SlipVerification {
  id: string;
  orderId: string;
  imagePath: string;
  amount: number;
  transactionDate: Date;
  referenceNumber: string;
  bankName: string;
  status: VerificationStatus;
  createdAt: Date;
  updatedAt?: Date;
}

export enum VerificationStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Verified = 'Verified',
  Failed = 'Failed',
  Rejected = 'Rejected',
  ManualReview = 'ManualReview'
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  totalAmount: number;
  status: OrderStatus;
  createdAt: Date;
  updatedAt?: Date;
}

export enum OrderStatus {
  PendingPayment = 'PendingPayment',
  Paid = 'Paid',
  Processing = 'Processing',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded'
}

export interface Transaction {
  id: string;
  orderId: string;
  slipVerificationId?: string;
  amount: number;
  status: TransactionStatus;
  paymentMethod: string;
  referenceNumber: string;
  createdAt: Date;
  completedAt?: Date;
}

export enum TransactionStatus {
  Pending = 'Pending',
  Processing = 'Processing',
  Success = 'Success',
  Failed = 'Failed',
  Cancelled = 'Cancelled',
  Refunded = 'Refunded'
}
