import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService, PagedResult } from '../../../core/services/api.service';

export interface Order {
  id: string;
  orderNumber: string;
  customerName: string;
  customerEmail?: string;
  totalAmount: number;
  status: string;
  paymentStatus: string;
  createdAt: string;
  updatedAt?: string;
}

export interface OrderFilter {
  status?: string;
  paymentStatus?: string;
  startDate?: string;
  endDate?: string;
}

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly api = inject(ApiService);

  /**
   * Get paginated orders
   */
  getOrders(
    page: number = 1,
    pageSize: number = 10,
    filters?: OrderFilter
  ): Observable<PagedResult<Order>> {
    let url = `/api/v1/orders?page=${page}&pageSize=${pageSize}`;
    
    if (filters?.status) url += `&status=${filters.status}`;
    if (filters?.paymentStatus) url += `&paymentStatus=${filters.paymentStatus}`;
    if (filters?.startDate) url += `&startDate=${filters.startDate}`;
    if (filters?.endDate) url += `&endDate=${filters.endDate}`;
    
    return this.api.get<PagedResult<Order>>(url);
  }

  /**
   * Get single order by ID
   */
  getOrder(id: string): Observable<Order> {
    return this.api.get<Order>(`/api/v1/orders/${id}`);
  }

  /**
   * Get orders pending payment
   */
  getPendingPaymentOrders(): Observable<Order[]> {
    return this.api.get<Order[]>('/api/v1/orders/pending-payment');
  }

  /**
   * Update order status
   */
  updateOrderStatus(id: string, status: string): Observable<Order> {
    return this.api.put<Order>(`/api/v1/orders/${id}/status`, { status });
  }
}
