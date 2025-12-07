import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { ReportsService, DailyReport, MonthlyReport, ReportTransaction } from './services/reports.service';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatTableModule,
    MatPaginatorModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './reports.html',
  styleUrls: ['./reports.scss']
})
export class ReportsComponent implements OnInit {
  private readonly reportsService = inject(ReportsService);

  // State
  loading = false;
  activeTab = 0;
  
  // Date selection
  selectedDate = new Date();
  selectedYear = new Date().getFullYear();
  selectedMonth = new Date().getMonth() + 1;
  
  // Reports data
  dailyReport: DailyReport | null = null;
  monthlyReport: MonthlyReport | null = null;
  
  // Table
  displayedColumns: string[] = ['referenceNumber', 'amount', 'status', 'bankName', 'transactionDate'];
  transactions: ReportTransaction[] = [];
  pageSize = 10;
  pageIndex = 0;
  totalTransactions = 0;

  // Options
  months = [
    { value: 1, name: 'January' },
    { value: 2, name: 'February' },
    { value: 3, name: 'March' },
    { value: 4, name: 'April' },
    { value: 5, name: 'May' },
    { value: 6, name: 'June' },
    { value: 7, name: 'July' },
    { value: 8, name: 'August' },
    { value: 9, name: 'September' },
    { value: 10, name: 'October' },
    { value: 11, name: 'November' },
    { value: 12, name: 'December' }
  ];

  years: number[] = [];

  ngOnInit(): void {
    // Generate years list
    const currentYear = new Date().getFullYear();
    this.years = Array.from({ length: 5 }, (_, i) => currentYear - i);
    
    // Load initial report
    this.loadDailyReport();
  }

  onTabChange(index: number): void {
    this.activeTab = index;
    if (index === 0) {
      this.loadDailyReport();
    } else if (index === 1) {
      this.loadMonthlyReport();
    }
  }

  loadDailyReport(): void {
    this.loading = true;
    this.reportsService.getDailyReport(this.selectedDate).subscribe({
      next: (report) => {
        this.dailyReport = report;
        this.transactions = report.transactions;
        this.totalTransactions = report.transactions.length;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading daily report:', error);
        this.loading = false;
        // Set mock data for demonstration
        this.setMockDailyReport();
      }
    });
  }

  loadMonthlyReport(): void {
    this.loading = true;
    this.reportsService.getMonthlyReport(this.selectedYear, this.selectedMonth).subscribe({
      next: (report) => {
        this.monthlyReport = report;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading monthly report:', error);
        this.loading = false;
        // Set mock data for demonstration
        this.setMockMonthlyReport();
      }
    });
  }

  onDateChange(): void {
    this.loadDailyReport();
  }

  onMonthYearChange(): void {
    this.loadMonthlyReport();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
  }

  exportToCsv(): void {
    const options = {
      format: 'csv' as const,
      startDate: this.selectedDate.toISOString().split('T')[0],
      endDate: this.selectedDate.toISOString().split('T')[0],
      columns: this.displayedColumns
    };

    this.reportsService.exportReport(options).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `report_${options.startDate}.csv`;
        a.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Export error:', error);
        // Fallback: generate CSV locally
        this.exportLocalCsv();
      }
    });
  }

  private exportLocalCsv(): void {
    if (!this.dailyReport) return;
    
    const headers = ['Reference Number', 'Amount', 'Status', 'Bank Name', 'Transaction Date'];
    const rows = this.transactions.map(t => [
      t.referenceNumber,
      t.amount.toString(),
      t.status,
      t.bankName,
      new Date(t.transactionDate).toLocaleString()
    ]);

    const csv = [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
    const blob = new Blob([csv], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `report_${this.selectedDate.toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  getStatusColor(status: string): string {
    switch (status.toLowerCase()) {
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

  private setMockDailyReport(): void {
    this.dailyReport = {
      date: new Date().toISOString(),
      totalTransactions: 42,
      totalRevenue: 156750,
      verifiedCount: 35,
      pendingCount: 5,
      rejectedCount: 2,
      transactions: [
        {
          id: '1',
          referenceNumber: 'REF001',
          amount: 5000,
          status: 'Verified',
          bankName: 'Bangkok Bank',
          transactionDate: new Date().toISOString(),
          createdAt: new Date().toISOString()
        },
        {
          id: '2',
          referenceNumber: 'REF002',
          amount: 12500,
          status: 'Pending',
          bankName: 'Kasikorn Bank',
          transactionDate: new Date().toISOString(),
          createdAt: new Date().toISOString()
        },
        {
          id: '3',
          referenceNumber: 'REF003',
          amount: 8750,
          status: 'Verified',
          bankName: 'SCB',
          transactionDate: new Date().toISOString(),
          createdAt: new Date().toISOString()
        }
      ]
    };
    this.transactions = this.dailyReport.transactions;
    this.totalTransactions = this.transactions.length;
  }

  private setMockMonthlyReport(): void {
    this.monthlyReport = {
      year: this.selectedYear,
      month: this.selectedMonth,
      monthName: this.months.find(m => m.value === this.selectedMonth)?.name || '',
      totalTransactions: 1250,
      totalRevenue: 4567890,
      successRate: 87.5,
      dailyBreakdown: Array.from({ length: 30 }, (_, i) => ({
        date: new Date(this.selectedYear, this.selectedMonth - 1, i + 1).toISOString(),
        count: Math.floor(Math.random() * 50) + 20,
        amount: Math.floor(Math.random() * 200000) + 50000
      })),
      bankBreakdown: [
        { bankName: 'Bangkok Bank', transactionCount: 450, totalAmount: 1567890, percentage: 34.3 },
        { bankName: 'Kasikorn Bank', transactionCount: 380, totalAmount: 1234567, percentage: 27.0 },
        { bankName: 'SCB', transactionCount: 280, totalAmount: 987654, percentage: 21.6 },
        { bankName: 'Krungthai Bank', transactionCount: 140, totalAmount: 777779, percentage: 17.1 }
      ]
    };
  }
}
