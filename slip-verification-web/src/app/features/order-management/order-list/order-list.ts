import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { OrderService, Order, OrderFilter } from '../services/order.service';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './order-list.html',
  styleUrls: ['./order-list.scss']
})
export class OrderList implements OnInit {
  private readonly orderService = inject(OrderService);

  // State
  loading = false;
  orders: Order[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  // Filters
  statusOptions = ['All', 'New', 'Processing', 'Completed', 'Cancelled'];
  paymentStatusOptions = ['All', 'Pending', 'Paid', 'Failed', 'Refunded'];
  selectedStatus = 'All';
  selectedPaymentStatus = 'All';
  searchText = '';

  // Table columns
  displayedColumns = ['orderNumber', 'customerName', 'totalAmount', 'status', 'paymentStatus', 'createdAt', 'actions'];

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    
    const filters: OrderFilter = {};
    if (this.selectedStatus !== 'All') filters.status = this.selectedStatus;
    if (this.selectedPaymentStatus !== 'All') filters.paymentStatus = this.selectedPaymentStatus;

    this.orderService.getOrders(this.pageIndex + 1, this.pageSize, filters)
      .subscribe({
        next: (result) => {
          this.orders = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading orders:', error);
          this.setMockData();
          this.loading = false;
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadOrders();
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    this.loadOrders();
  }

  clearFilters(): void {
    this.selectedStatus = 'All';
    this.selectedPaymentStatus = 'All';
    this.searchText = '';
    this.pageIndex = 0;
    this.loadOrders();
  }

  viewDetails(order: Order): void {
    console.log('View order:', order);
    // TODO: Navigate to order details
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'completed':
      case 'paid': return 'primary';
      case 'processing':
      case 'pending': return 'accent';
      case 'cancelled':
      case 'failed': return 'warn';
      default: return '';
    }
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('th-TH', {
      style: 'currency',
      currency: 'THB'
    }).format(amount);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleString('th-TH');
  }

  private setMockData(): void {
    this.orders = [
      {
        id: '1',
        orderNumber: 'ORD-2024-001',
        customerName: 'สมชาย ใจดี',
        customerEmail: 'somchai@example.com',
        totalAmount: 15000,
        status: 'Completed',
        paymentStatus: 'Paid',
        createdAt: new Date().toISOString()
      },
      {
        id: '2',
        orderNumber: 'ORD-2024-002',
        customerName: 'สมหญิง รักดี',
        totalAmount: 8500,
        status: 'Processing',
        paymentStatus: 'Pending',
        createdAt: new Date().toISOString()
      },
      {
        id: '3',
        orderNumber: 'ORD-2024-003',
        customerName: 'วิชัย สุขใจ',
        totalAmount: 22000,
        status: 'New',
        paymentStatus: 'Pending',
        createdAt: new Date().toISOString()
      },
      {
        id: '4',
        orderNumber: 'ORD-2024-004',
        customerName: 'มาลี งามเลิศ',
        totalAmount: 5500,
        status: 'Cancelled',
        paymentStatus: 'Refunded',
        createdAt: new Date().toISOString()
      }
    ];
    this.totalCount = 4;
  }
}
