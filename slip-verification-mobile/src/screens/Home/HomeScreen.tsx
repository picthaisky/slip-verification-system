import React, { useEffect, useState, useCallback } from 'react';
import { View, StyleSheet, ScrollView, RefreshControl } from 'react-native';
import { Text, Card, Button, useTheme, ActivityIndicator } from 'react-native-paper';
import Icon from 'react-native-vector-icons/MaterialCommunityIcons';
import { useAppSelector } from '../../hooks/useRedux';
import { t } from '../../locales';
import dashboardApi, { DashboardStats, RecentActivity } from '../../api/endpoints/dashboard';

const HomeScreen = ({ navigation }: any) => {
  const theme = useTheme();
  const user = useAppSelector(state => state.auth.user);

  const [refreshing, setRefreshing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [dashboardStats, setDashboardStats] = useState<DashboardStats | null>(null);
  const [recentActivities, setRecentActivities] = useState<RecentActivity[]>([]);
  const [error, setError] = useState<string | null>(null);

  const loadDashboardData = useCallback(async (showLoading = true) => {
    if (showLoading) {
      setLoading(true);
    }
    setError(null);

    try {
      const [statsData, activitiesData] = await Promise.all([
        dashboardApi.getDashboardStats(),
        dashboardApi.getRecentActivities(5),
      ]);

      setDashboardStats(statsData);
      setRecentActivities(activitiesData);
    } catch (err) {
      console.error('Error loading dashboard:', err);
      setError('Failed to load dashboard data');
      // Set fallback mock data
      setMockData();
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  const setMockData = () => {
    setDashboardStats({
      totalTransactions: 42,
      totalRevenue: 156750,
      verifiedCount: 35,
      pendingCount: 5,
      rejectedCount: 2,
      successRate: 83.3,
      averageProcessingTime: 2.5,
      todayTransactions: 12,
      todayRevenue: 45000,
    });

    setRecentActivities([
      {
        id: '1',
        type: 'SlipVerification',
        description: 'Slip #1234 verified',
        status: 'Verified',
        amount: 5000,
        createdAt: new Date().toISOString(),
        timeAgo: '2 hours ago',
        icon: 'check-circle',
        color: '#4CAF50',
      },
      {
        id: '2',
        type: 'SlipVerification',
        description: 'Slip #1235 pending',
        status: 'Pending',
        amount: 12500,
        createdAt: new Date().toISOString(),
        timeAgo: '5 hours ago',
        icon: 'clock-outline',
        color: '#FF9800',
      },
      {
        id: '3',
        type: 'SlipVerification',
        description: 'Slip #1236 uploaded',
        status: 'Processing',
        amount: 8750,
        createdAt: new Date().toISOString(),
        timeAgo: '1 day ago',
        icon: 'upload',
        color: '#2196F3',
      },
    ]);
  };

  useEffect(() => {
    loadDashboardData();
  }, [loadDashboardData]);

  const onRefresh = useCallback(() => {
    setRefreshing(true);
    loadDashboardData(false);
  }, [loadDashboardData]);

  const stats = dashboardStats ? [
    {
      title: t('home.totalTransactions'),
      value: dashboardStats.totalTransactions.toString(),
      icon: 'receipt-text',
      color: theme.colors.primary,
    },
    {
      title: t('home.pendingVerification'),
      value: dashboardStats.pendingCount.toString(),
      icon: 'clock-outline',
      color: '#FF9800',
    },
    {
      title: t('home.verified'),
      value: dashboardStats.verifiedCount.toString(),
      icon: 'check-circle',
      color: '#4CAF50',
    },
    {
      title: t('home.rejected'),
      value: dashboardStats.rejectedCount.toString(),
      icon: 'close-circle',
      color: '#F44336',
    },
  ] : [];

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('th-TH', {
      style: 'currency',
      currency: 'THB',
      maximumFractionDigits: 0,
    }).format(amount);
  };

  if (loading) {
    return (
      <View style={styles.loadingContainer}>
        <ActivityIndicator size="large" color={theme.colors.primary} />
        <Text style={styles.loadingText}>Loading dashboard...</Text>
      </View>
    );
  }

  return (
    <ScrollView
      style={styles.container}
      contentContainerStyle={styles.content}
      refreshControl={
        <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
      }
    >
      <View style={styles.header}>
        <View>
          <Text variant="titleSmall" style={styles.greeting}>
            {t('home.welcome')}
          </Text>
          <Text variant="headlineSmall" style={styles.username}>
            {user?.name || 'User'}
          </Text>
        </View>
        <Icon name="bell-outline" size={24} color={theme.colors.primary} />
      </View>

      {/* Today's Summary */}
      {dashboardStats && (
        <Card style={styles.summaryCard}>
          <Card.Content>
            <Text variant="titleMedium" style={styles.summaryTitle}>
              Today's Summary
            </Text>
            <View style={styles.summaryRow}>
              <View style={styles.summaryItem}>
                <Text style={styles.summaryValue}>
                  {dashboardStats.todayTransactions}
                </Text>
                <Text style={styles.summaryLabel}>Transactions</Text>
              </View>
              <View style={styles.summaryDivider} />
              <View style={styles.summaryItem}>
                <Text style={styles.summaryValue}>
                  {formatCurrency(dashboardStats.todayRevenue)}
                </Text>
                <Text style={styles.summaryLabel}>Revenue</Text>
              </View>
            </View>
          </Card.Content>
        </Card>
      )}

      <Text variant="titleMedium" style={styles.sectionTitle}>
        {t('home.transactionSummary')}
      </Text>

      <View style={styles.statsGrid}>
        {stats.map((stat, index) => (
          <Card key={index} style={styles.statCard}>
            <Card.Content style={styles.statContent}>
              <Icon name={stat.icon} size={32} color={stat.color} />
              <Text variant="headlineMedium" style={styles.statValue}>
                {stat.value}
              </Text>
              <Text variant="bodySmall" style={styles.statTitle}>
                {stat.title}
              </Text>
            </Card.Content>
          </Card>
        ))}
      </View>

      <Text variant="titleMedium" style={styles.sectionTitle}>
        {t('home.quickActions')}
      </Text>

      <Card style={styles.actionCard}>
        <Card.Content>
          <Button
            mode="contained"
            icon="upload"
            onPress={() => navigation.navigate('SlipUpload')}
            style={styles.actionButton}
          >
            {t('home.uploadSlip')}
          </Button>
          <Button
            mode="outlined"
            icon="history"
            onPress={() => navigation.navigate('History')}
            style={styles.actionButton}
          >
            {t('home.viewHistory')}
          </Button>
        </Card.Content>
      </Card>

      <Text variant="titleMedium" style={styles.sectionTitle}>
        {t('home.recentActivity')}
      </Text>

      <Card style={styles.activityCard}>
        <Card.Content>
          {recentActivities.length > 0 ? (
            recentActivities.map((activity) => (
              <View key={activity.id} style={styles.activityItem}>
                <Icon
                  name={activity.icon || 'information'}
                  size={24}
                  color={activity.color || '#666'}
                />
                <View style={styles.activityText}>
                  <Text variant="bodyMedium">{activity.description}</Text>
                  <Text variant="bodySmall" style={styles.activityTime}>
                    {activity.timeAgo}
                  </Text>
                </View>
                {activity.amount && (
                  <Text style={styles.activityAmount}>
                    {formatCurrency(activity.amount)}
                  </Text>
                )}
              </View>
            ))
          ) : (
            <Text style={styles.emptyText}>No recent activities</Text>
          )}
        </Card.Content>
      </Card>
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    padding: 16,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 16,
    opacity: 0.7,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    marginBottom: 24,
  },
  greeting: {
    opacity: 0.7,
  },
  username: {
    fontWeight: 'bold',
  },
  summaryCard: {
    marginBottom: 16,
    backgroundColor: '#1976D2',
  },
  summaryTitle: {
    color: 'white',
    marginBottom: 12,
  },
  summaryRow: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    alignItems: 'center',
  },
  summaryItem: {
    alignItems: 'center',
  },
  summaryValue: {
    color: 'white',
    fontSize: 24,
    fontWeight: 'bold',
  },
  summaryLabel: {
    color: 'rgba(255, 255, 255, 0.8)',
    marginTop: 4,
  },
  summaryDivider: {
    width: 1,
    height: 40,
    backgroundColor: 'rgba(255, 255, 255, 0.3)',
  },
  sectionTitle: {
    marginTop: 16,
    marginBottom: 12,
    fontWeight: 'bold',
  },
  statsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginHorizontal: -6,
  },
  statCard: {
    width: '48%',
    margin: '1%',
    marginBottom: 12,
  },
  statContent: {
    alignItems: 'center',
    paddingVertical: 16,
  },
  statValue: {
    marginTop: 8,
    fontWeight: 'bold',
  },
  statTitle: {
    textAlign: 'center',
    opacity: 0.7,
    marginTop: 4,
  },
  actionCard: {
    marginBottom: 8,
  },
  actionButton: {
    marginVertical: 6,
  },
  activityCard: {
    marginBottom: 16,
  },
  activityItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 12,
    borderBottomWidth: 1,
    borderBottomColor: '#eee',
  },
  activityText: {
    marginLeft: 16,
    flex: 1,
  },
  activityTime: {
    opacity: 0.6,
    marginTop: 4,
  },
  activityAmount: {
    fontWeight: 'bold',
    color: '#4CAF50',
  },
  emptyText: {
    textAlign: 'center',
    opacity: 0.6,
    paddingVertical: 16,
  },
});

export default HomeScreen;
