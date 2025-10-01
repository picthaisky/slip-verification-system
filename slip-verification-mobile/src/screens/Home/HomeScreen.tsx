import React from 'react';
import { View, StyleSheet, ScrollView, RefreshControl } from 'react-native';
import { Text, Card, Button, useTheme } from 'react-native-paper';
import Icon from 'react-native-vector-icons/MaterialCommunityIcons';
import { useAppSelector } from '../../hooks/useRedux';
import { t } from '../../locales';

const HomeScreen = ({ navigation }: any) => {
  const theme = useTheme();
  const user = useAppSelector(state => state.auth.user);
  const [refreshing, setRefreshing] = React.useState(false);

  const onRefresh = React.useCallback(() => {
    setRefreshing(true);
    setTimeout(() => setRefreshing(false), 2000);
  }, []);

  const stats = [
    {
      title: t('home.totalTransactions'),
      value: '42',
      icon: 'receipt-text',
      color: theme.colors.primary,
    },
    {
      title: t('home.pendingVerification'),
      value: '5',
      icon: 'clock-outline',
      color: '#FF9800',
    },
    {
      title: t('home.verified'),
      value: '35',
      icon: 'check-circle',
      color: '#4CAF50',
    },
    {
      title: t('home.rejected'),
      value: '2',
      icon: 'close-circle',
      color: '#F44336',
    },
  ];

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
          <View style={styles.activityItem}>
            <Icon name="check-circle" size={24} color="#4CAF50" />
            <View style={styles.activityText}>
              <Text variant="bodyMedium">Slip #1234 verified</Text>
              <Text variant="bodySmall" style={styles.activityTime}>
                2 hours ago
              </Text>
            </View>
          </View>
          <View style={styles.activityItem}>
            <Icon name="clock-outline" size={24} color="#FF9800" />
            <View style={styles.activityText}>
              <Text variant="bodyMedium">Slip #1235 pending</Text>
              <Text variant="bodySmall" style={styles.activityTime}>
                5 hours ago
              </Text>
            </View>
          </View>
          <View style={styles.activityItem}>
            <Icon name="upload" size={24} color={theme.colors.primary} />
            <View style={styles.activityText}>
              <Text variant="bodyMedium">Slip #1236 uploaded</Text>
              <Text variant="bodySmall" style={styles.activityTime}>
                1 day ago
              </Text>
            </View>
          </View>
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
  },
  activityText: {
    marginLeft: 16,
    flex: 1,
  },
  activityTime: {
    opacity: 0.6,
    marginTop: 4,
  },
});

export default HomeScreen;
