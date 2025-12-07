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
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { TransactionService, Transaction, TransactionFilter } from '../services/transaction.service';

@Component({
  selector: 'app-transaction-list',
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
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './transaction-list.html',
  styleUrls: ['./transaction-list.scss']
})
export class TransactionList implements OnInit {
  private readonly transactionService = inject(TransactionService);

  // State
  loading = false;
  transactions: Transaction[] = [];
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  // Filters
  filters: TransactionFilter = {};
  statusOptions = ['All', 'Verified', 'Pending', 'Rejected', 'Processing'];
  selectedStatus = 'All';
  startDate: Date | null = null;
  endDate: Date | null = null;
  searchText = '';

  // Table
  displayedColumns = ['referenceNumber', 'amount', 'transactionType', 'status', 'bankName', 'createdAt', 'actions'];

  ngOnInit(): void {
    this.loadTransactions();
  }

  loadTransactions(): void {
    this.loading = true;
    
    const filters: TransactionFilter = {};
    if (this.selectedStatus !== 'All') filters.status = this.selectedStatus;
    if (this.startDate) filters.startDate = this.startDate.toISOString();
    if (this.endDate) filters.endDate = this.endDate.toISOString();

    this.transactionService.getTransactions(this.pageIndex + 1, this.pageSize, filters)
      .subscribe({
        next: (result) => {
          this.transactions = result.items;
          this.totalCount = result.totalCount;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading transactions:', error);
          this.setMockData();
          this.loading = false;
        }
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadTransactions();
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    this.loadTransactions();
  }

  clearFilters(): void {
    this.selectedStatus = 'All';
    this.startDate = null;
    this.endDate = null;
    this.searchText = '';
    this.pageIndex = 0;
    this.loadTransactions();
  }

  viewDetails(transaction: Transaction): void {
    console.log('View details:', transaction);
    // TODO: Navigate to transaction details or open modal
  }

  getStatusColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'verified': return 'primary';
      case 'pending': return 'accent';
      case 'rejected': return 'warn';
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
    this.transactions = [
      {
        id: '1',
        orderId: 'order-1',
        amount: 5000,
        transactionType: 'Payment',
        status: 'Verified',
        referenceNumber: 'REF001',
        bankName: 'Bangkok Bank',
        createdAt: new Date().toISOString()
      },
      {
        id: '2',
        orderId: 'order-2',
        amount: 12500,
        transactionType: 'Payment',
        status: 'Pending',
        referenceNumber: 'REF002',
        bankName: 'Kasikorn Bank',
        createdAt: new Date().toISOString()
      },
      {
        id: '3',
        orderId: 'order-3',
        amount: 8750,
        transactionType: 'Payment',
        status: 'Verified',
        referenceNumber: 'REF003',
        bankName: 'SCB',
        createdAt: new Date().toISOString()
      },
      {
        id: '4',
        orderId: 'order-4',
        amount: 3200,
        transactionType: 'Refund',
        status: 'Rejected',
        referenceNumber: 'REF004',
        bankName: 'Krungthai Bank',
        createdAt: new Date().toISOString()
      }
    ];
    this.totalCount = 4;
  }
}
