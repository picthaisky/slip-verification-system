import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

export interface DashboardStats {
  totalTransactions: number;
  totalRevenue: number;
  verifiedCount: number;
  pendingCount: number;
  rejectedCount: number;
  successRate: number;
  averageProcessingTime: number;
  todayTransactions: number;
  todayRevenue: number;
}

export interface RecentActivity {
  id: string;
  type: string;
  description: string;
  relatedEntityId?: string;
  status: string;
  amount?: number;
  createdAt: string;
  timeAgo: string;
  icon: string;
  color: string;
}

export interface ChartData {
  labels: string[];
  datasets: ChartDataset[];
}

export interface ChartDataset {
  label: string;
  data: number[];
  backgroundColor: string;
  borderColor: string;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly api = inject(ApiService);

  /**
   * Get dashboard statistics
   */
  getStats(): Observable<DashboardStats> {
    return this.api.get<DashboardStats>('/api/v1/dashboard/stats');
  }

  /**
   * Get recent activities
   */
  getRecentActivities(count: number = 10): Observable<RecentActivity[]> {
    return this.api.get<RecentActivity[]>(`/api/v1/dashboard/recent-activities?count=${count}`);
  }

  /**
   * Get chart data for visualizations
   */
  getChartData(period: string = 'daily', count: number = 7): Observable<ChartData> {
    return this.api.get<ChartData>(`/api/v1/dashboard/chart-data?period=${period}&count=${count}`);
  }
}
