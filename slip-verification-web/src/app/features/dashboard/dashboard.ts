import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { Subject, takeUntil, interval } from 'rxjs';
import { DashboardService, DashboardStats, RecentActivity } from './services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatButtonModule
  ],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private readonly destroy$ = new Subject<void>();

  // State
  loading = true;
  stats: DashboardStats | null = null;
  recentActivities: RecentActivity[] = [];
  error: string | null = null;

  // Stats display configuration
  statsConfig = [
    { key: 'totalTransactions', title: 'Total Transactions', icon: 'receipt_long', color: 'bg-blue-500' },
    { key: 'todayTransactions', title: 'Today\'s Transactions', icon: 'today', color: 'bg-indigo-500' },
    { key: 'verifiedCount', title: 'Verified', icon: 'check_circle', color: 'bg-green-500' },
    { key: 'pendingCount', title: 'Pending', icon: 'pending', color: 'bg-orange-500' }
  ];

  ngOnInit(): void {
    this.loadDashboardData();
    
    // Auto-refresh every 30 seconds
    interval(30000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadDashboardData(false));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadDashboardData(showLoading: boolean = true): void {
    if (showLoading) {
      this.loading = true;
    }
    this.error = null;

    // Load stats
    this.dashboardService.getStats()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.stats = data;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading stats:', err);
          this.setMockStats();
          this.loading = false;
        }
      });

    // Load recent activities
    this.dashboardService.getRecentActivities(5)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.recentActivities = data;
        },
        error: (err) => {
          console.error('Error loading activities:', err);
          this.setMockActivities();
        }
      });
  }

  getStatValue(key: string): string {
    if (!this.stats) return '0';
    const value = (this.stats as any)[key];
    
    if (key.includes('Revenue')) {
      return this.formatCurrency(value);
    }
    if (key === 'successRate') {
      return `${value}%`;
    }
    return value?.toString() || '0';
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('th-TH', {
      style: 'currency',
      currency: 'THB',
      maximumFractionDigits: 0
    }).format(amount);
  }

  getActivityIcon(activity: RecentActivity): string {
    return activity.icon || 'info';
  }

  getActivityColor(activity: RecentActivity): string {
    return activity.color || '#666';
  }

  private setMockStats(): void {
    this.stats = {
      totalTransactions: 1250,
      totalRevenue: 4567890,
      verifiedCount: 1089,
      pendingCount: 125,
      rejectedCount: 36,
      successRate: 87.12,
      averageProcessingTime: 2.5,
      todayTransactions: 42,
      todayRevenue: 156750
    };
  }

  private setMockActivities(): void {
    this.recentActivities = [
      {
        id: '1',
        type: 'SlipVerification',
        description: 'Slip #REF1234 verified',
        status: 'Verified',
        amount: 5000,
        createdAt: new Date().toISOString(),
        timeAgo: '2 hours ago',
        icon: 'check_circle',
        color: '#4CAF50'
      },
      {
        id: '2',
        type: 'SlipVerification',
        description: 'Slip #REF1235 pending',
        status: 'Pending',
        amount: 12500,
        createdAt: new Date().toISOString(),
        timeAgo: '5 hours ago',
        icon: 'pending',
        color: '#FF9800'
      },
      {
        id: '3',
        type: 'SlipVerification',
        description: 'Slip #REF1236 uploaded',
        status: 'Processing',
        amount: 8750,
        createdAt: new Date().toISOString(),
        timeAgo: '1 day ago',
        icon: 'upload',
        color: '#2196F3'
      }
    ];
  }
}
