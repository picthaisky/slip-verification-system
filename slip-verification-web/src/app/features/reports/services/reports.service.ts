import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

export interface DailyReport {
  date: string;
  totalTransactions: number;
  totalRevenue: number;
  verifiedCount: number;
  pendingCount: number;
  rejectedCount: number;
  transactions: ReportTransaction[];
}

export interface MonthlyReport {
  year: number;
  month: number;
  monthName: string;
  totalTransactions: number;
  totalRevenue: number;
  successRate: number;
  dailyBreakdown: DailySummary[];
  bankBreakdown: BankSummary[];
}

export interface ReportTransaction {
  id: string;
  referenceNumber: string;
  amount: number;
  status: string;
  bankName: string;
  transactionDate: string;
  createdAt: string;
}

export interface DailySummary {
  date: string;
  count: number;
  amount: number;
}

export interface BankSummary {
  bankName: string;
  transactionCount: number;
  totalAmount: number;
  percentage: number;
}

export interface CustomReportRequest {
  startDate: string;
  endDate: string;
  status?: string;
  bankName?: string;
  minAmount?: number;
  maxAmount?: number;
}

export interface ExportOptions {
  format: 'csv' | 'excel' | 'pdf';
  startDate: string;
  endDate: string;
  columns: string[];
}

export interface ReportOptions {
  reportTypes: string[];
  exportFormats: string[];
  availableColumns: string[];
  statusFilters: string[];
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private readonly api = inject(ApiService);

  /**
   * Get daily report for a specific date
   */
  getDailyReport(date?: Date): Observable<DailyReport> {
    const dateParam = date ? `?date=${date.toISOString().split('T')[0]}` : '';
    return this.api.get<DailyReport>(`/api/v1/reports/daily${dateParam}`);
  }

  /**
   * Get monthly report for specific year and month
   */
  getMonthlyReport(year?: number, month?: number): Observable<MonthlyReport> {
    const params = new URLSearchParams();
    if (year) params.append('year', year.toString());
    if (month) params.append('month', month.toString());
    const queryString = params.toString() ? `?${params.toString()}` : '';
    return this.api.get<MonthlyReport>(`/api/v1/reports/monthly${queryString}`);
  }

  /**
   * Generate custom report based on filters
   */
  getCustomReport(request: CustomReportRequest): Observable<DailyReport> {
    return this.api.post<DailyReport>('/api/v1/reports/custom', request);
  }

  /**
   * Export report to file
   */
  exportReport(options: ExportOptions): Observable<Blob> {
    return this.api.post<Blob>('/api/v1/reports/export', options);
  }

  /**
   * Get available report options
   */
  getReportOptions(): Observable<ReportOptions> {
    return this.api.get<ReportOptions>('/api/v1/reports/options');
  }
}
