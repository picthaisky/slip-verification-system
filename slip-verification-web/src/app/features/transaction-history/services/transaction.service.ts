import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, PagedResult } from '../../../core/services/api.service';

export interface Transaction {
  id: string;
  orderId: string;
  slipId?: string;
  amount: number;
  transactionType: string;
  status: string;
  referenceNumber?: string;
  bankName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TransactionFilter {
  status?: string;
  bankName?: string;
  startDate?: string;
  endDate?: string;
  minAmount?: number;
  maxAmount?: number;
}

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly api = inject(ApiService);

  /**
   * Get paginated transactions
   */
  getTransactions(
    page: number = 1,
    pageSize: number = 10,
    filters?: TransactionFilter
  ): Observable<PagedResult<Transaction>> {
    let url = `/api/v1/transactions?page=${page}&pageSize=${pageSize}`;
    
    if (filters?.status) url += `&status=${filters.status}`;
    if (filters?.bankName) url += `&bankName=${filters.bankName}`;
    if (filters?.startDate) url += `&startDate=${filters.startDate}`;
    if (filters?.endDate) url += `&endDate=${filters.endDate}`;
    if (filters?.minAmount) url += `&minAmount=${filters.minAmount}`;
    if (filters?.maxAmount) url += `&maxAmount=${filters.maxAmount}`;
    
    return this.api.get<PagedResult<Transaction>>(url);
  }

  /**
   * Get single transaction by ID
   */
  getTransaction(id: string): Observable<Transaction> {
    return this.api.get<Transaction>(`/api/v1/transactions/${id}`);
  }

  /**
   * Get transactions for an order
   */
  getOrderTransactions(orderId: string): Observable<Transaction[]> {
    return this.api.get<Transaction[]>(`/api/v1/transactions/order/${orderId}`);
  }
}
