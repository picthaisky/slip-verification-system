import apiClient from '../client';

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

/**
 * Get dashboard statistics
 */
export const getDashboardStats = async (): Promise<DashboardStats> => {
  const response = await apiClient.get<DashboardStats>('/api/v1/dashboard/stats');
  // apiClient.get returns ApiResponse<T>, we need to extract the data
  return (response as any).data ?? response;
};

/**
 * Get recent activities
 */
export const getRecentActivities = async (count: number = 10): Promise<RecentActivity[]> => {
  const response = await apiClient.get<RecentActivity[]>(`/api/v1/dashboard/recent-activities?count=${count}`);
  return (response as any).data ?? response;
};

/**
 * Get chart data for visualizations
 */
export const getChartData = async (period: string = 'daily', count: number = 7): Promise<ChartData> => {
  const response = await apiClient.get<ChartData>(`/api/v1/dashboard/chart-data?period=${period}&count=${count}`);
  return (response as any).data ?? response;
};

export default {
  getDashboardStats,
  getRecentActivities,
  getChartData,
};

