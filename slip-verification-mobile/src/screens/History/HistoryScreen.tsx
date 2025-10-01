import React, { useState } from 'react';
import { View, StyleSheet, FlatList, RefreshControl } from 'react-native';
import { Text, Card, Chip, useTheme, Searchbar } from 'react-native-paper';
import Icon from 'react-native-vector-icons/MaterialCommunityIcons';
import { t } from '../../locales';

const HistoryScreen = () => {
  const theme = useTheme();
  const [refreshing, setRefreshing] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('all');

  // Mock data - replace with actual API call
  const [transactions] = useState([
    {
      id: '1',
      orderId: 'ORD-001',
      amount: 1500.0,
      status: 'Verified',
      bankName: 'Bangkok Bank',
      transactionDate: '2024-01-15T10:30:00Z',
      referenceNumber: 'REF123456',
    },
    {
      id: '2',
      orderId: 'ORD-002',
      amount: 2500.0,
      status: 'Pending',
      bankName: 'Kasikorn Bank',
      transactionDate: '2024-01-14T15:20:00Z',
      referenceNumber: 'REF123457',
    },
    {
      id: '3',
      orderId: 'ORD-003',
      amount: 3500.0,
      status: 'Rejected',
      bankName: 'SCB',
      transactionDate: '2024-01-13T09:15:00Z',
      referenceNumber: 'REF123458',
    },
  ]);

  const onRefresh = React.useCallback(() => {
    setRefreshing(true);
    // Fetch data here
    setTimeout(() => setRefreshing(false), 2000);
  }, []);

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Verified':
        return '#4CAF50';
      case 'Pending':
        return '#FF9800';
      case 'Rejected':
        return '#F44336';
      default:
        return theme.colors.primary;
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'Verified':
        return 'check-circle';
      case 'Pending':
        return 'clock-outline';
      case 'Rejected':
        return 'close-circle';
      default:
        return 'help-circle';
    }
  };

  const renderTransaction = ({ item }: any) => (
    <Card style={styles.card}>
      <Card.Content>
        <View style={styles.cardHeader}>
          <View style={styles.cardHeaderLeft}>
            <Text variant="titleMedium" style={styles.orderId}>
              {item.orderId}
            </Text>
            <Text variant="bodySmall" style={styles.date}>
              {new Date(item.transactionDate).toLocaleDateString()}
            </Text>
          </View>
          <View style={styles.statusContainer}>
            <Icon
              name={getStatusIcon(item.status)}
              size={20}
              color={getStatusColor(item.status)}
            />
            <Text
              variant="bodySmall"
              style={[styles.status, { color: getStatusColor(item.status) }]}
            >
              {item.status}
            </Text>
          </View>
        </View>

        <View style={styles.divider} />

        <View style={styles.detailRow}>
          <Icon name="bank" size={16} color={theme.colors.onSurfaceDisabled} />
          <Text variant="bodyMedium" style={styles.detailText}>
            {item.bankName}
          </Text>
        </View>

        <View style={styles.detailRow}>
          <Icon name="cash" size={16} color={theme.colors.onSurfaceDisabled} />
          <Text variant="bodyMedium" style={styles.detailText}>
            ฿{item.amount.toFixed(2)}
          </Text>
        </View>

        <View style={styles.detailRow}>
          <Icon name="tag" size={16} color={theme.colors.onSurfaceDisabled} />
          <Text variant="bodyMedium" style={styles.detailText}>
            {item.referenceNumber}
          </Text>
        </View>
      </Card.Content>
    </Card>
  );

  const filteredTransactions = transactions.filter(
    (t) =>
      (selectedStatus === 'all' || t.status === selectedStatus) &&
      (t.orderId.toLowerCase().includes(searchQuery.toLowerCase()) ||
        t.referenceNumber.toLowerCase().includes(searchQuery.toLowerCase()))
  );

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <Text variant="headlineMedium" style={styles.title}>
          {t('history.title')}
        </Text>
      </View>

      <View style={styles.searchContainer}>
        <Searchbar
          placeholder={t('common.search')}
          onChangeText={setSearchQuery}
          value={searchQuery}
          style={styles.searchbar}
        />
      </View>

      <View style={styles.filterContainer}>
        <Chip
          selected={selectedStatus === 'all'}
          onPress={() => setSelectedStatus('all')}
          style={styles.chip}
        >
          {t('history.all')}
        </Chip>
        <Chip
          selected={selectedStatus === 'Pending'}
          onPress={() => setSelectedStatus('Pending')}
          style={styles.chip}
        >
          {t('history.pending')}
        </Chip>
        <Chip
          selected={selectedStatus === 'Verified'}
          onPress={() => setSelectedStatus('Verified')}
          style={styles.chip}
        >
          {t('history.verified')}
        </Chip>
        <Chip
          selected={selectedStatus === 'Rejected'}
          onPress={() => setSelectedStatus('Rejected')}
          style={styles.chip}
        >
          {t('history.rejected')}
        </Chip>
      </View>

      <FlatList
        data={filteredTransactions}
        renderItem={renderTransaction}
        keyExtractor={(item) => item.id}
        contentContainerStyle={styles.list}
        refreshControl={
          <RefreshControl refreshing={refreshing} onRefresh={onRefresh} />
        }
        ListEmptyComponent={
          <View style={styles.emptyContainer}>
            <Icon name="folder-open" size={64} color={theme.colors.onSurfaceDisabled} />
            <Text variant="bodyLarge" style={styles.emptyText}>
              {t('history.noTransactions')}
            </Text>
          </View>
        }
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    padding: 16,
    paddingBottom: 8,
  },
  title: {
    fontWeight: 'bold',
  },
  searchContainer: {
    paddingHorizontal: 16,
    paddingBottom: 8,
  },
  searchbar: {
    elevation: 0,
  },
  filterContainer: {
    flexDirection: 'row',
    paddingHorizontal: 16,
    paddingBottom: 8,
  },
  chip: {
    marginRight: 8,
  },
  list: {
    padding: 16,
  },
  card: {
    marginBottom: 12,
  },
  cardHeader: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'flex-start',
  },
  cardHeaderLeft: {
    flex: 1,
  },
  orderId: {
    fontWeight: 'bold',
  },
  date: {
    opacity: 0.6,
    marginTop: 4,
  },
  statusContainer: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  status: {
    marginLeft: 4,
    fontWeight: '500',
  },
  divider: {
    height: 1,
    backgroundColor: '#E0E0E0',
    marginVertical: 12,
  },
  detailRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginVertical: 4,
  },
  detailText: {
    marginLeft: 8,
  },
  emptyContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 64,
  },
  emptyText: {
    marginTop: 16,
    opacity: 0.6,
  },
});

export default HistoryScreen;
