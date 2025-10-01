import React from 'react';
import { View, StyleSheet, ScrollView, Alert } from 'react-native';
import { Text, Card, List, Switch, useTheme, Avatar, Divider } from 'react-native-paper';
import { useAppSelector, useAppDispatch } from '../../hooks/useRedux';
import { logout } from '../../store/slices/authSlice';
import { toggleTheme, setLanguage } from '../../store/slices/appSlice';
import { authApi } from '../../api/endpoints/auth';
import { t } from '../../locales';
import Icon from 'react-native-vector-icons/MaterialCommunityIcons';

const ProfileScreen = () => {
  const theme = useTheme();
  const dispatch = useAppDispatch();
  const user = useAppSelector(state => state.auth.user);
  const currentTheme = useAppSelector(state => state.app.theme);
  const language = useAppSelector(state => state.app.language);

  const isDarkMode = currentTheme === 'dark';

  const handleLogout = () => {
    Alert.alert(
      t('auth.logout'),
      'Are you sure you want to logout?',
      [
        {
          text: t('common.cancel'),
          style: 'cancel',
        },
        {
          text: t('auth.logout'),
          style: 'destructive',
          onPress: async () => {
            await authApi.logout();
            dispatch(logout());
          },
        },
      ]
    );
  };

  const handleToggleTheme = () => {
    dispatch(toggleTheme());
  };

  const handleLanguageChange = () => {
    const newLanguage = language === 'th' ? 'en' : 'th';
    dispatch(setLanguage(newLanguage));
  };

  return (
    <ScrollView style={styles.container}>
      <View style={styles.header}>
        <Text variant="headlineMedium" style={styles.title}>
          {t('profile.title')}
        </Text>
      </View>

      <Card style={styles.profileCard}>
        <Card.Content style={styles.profileContent}>
          <Avatar.Text
            size={80}
            label={user?.name?.charAt(0).toUpperCase() || 'U'}
            style={[styles.avatar, { backgroundColor: theme.colors.primary }]}
          />
          <Text variant="headlineSmall" style={styles.name}>
            {user?.name}
          </Text>
          <Text variant="bodyMedium" style={styles.email}>
            {user?.email}
          </Text>
          <Text variant="bodySmall" style={styles.role}>
            {user?.role}
          </Text>
        </Card.Content>
      </Card>

      <Card style={styles.card}>
        <Card.Content>
          <Text variant="titleMedium" style={styles.sectionTitle}>
            {t('profile.settings')}
          </Text>
          <List.Item
            title={t('profile.darkMode')}
            left={(props) => <List.Icon {...props} icon="theme-light-dark" />}
            right={() => (
              <Switch value={isDarkMode} onValueChange={handleToggleTheme} />
            )}
          />
          <Divider />
          <List.Item
            title={t('profile.language')}
            description={language === 'th' ? 'ไทย' : 'English'}
            left={(props) => <List.Icon {...props} icon="translate" />}
            right={(props) => <List.Icon {...props} icon="chevron-right" />}
            onPress={handleLanguageChange}
          />
          <Divider />
          <List.Item
            title={t('profile.notifications')}
            left={(props) => <List.Icon {...props} icon="bell" />}
            right={(props) => <List.Icon {...props} icon="chevron-right" />}
            onPress={() => {}}
          />
          <Divider />
          <List.Item
            title={t('profile.biometric')}
            left={(props) => <List.Icon {...props} icon="fingerprint" />}
            right={() => <Switch value={false} onValueChange={() => {}} />}
          />
        </Card.Content>
      </Card>

      <Card style={styles.card}>
        <Card.Content>
          <List.Item
            title={t('profile.editProfile')}
            left={(props) => <List.Icon {...props} icon="account-edit" />}
            right={(props) => <List.Icon {...props} icon="chevron-right" />}
            onPress={() => {}}
          />
          <Divider />
          <List.Item
            title={t('profile.about')}
            description={`${t('profile.version')} 1.0.0`}
            left={(props) => <List.Icon {...props} icon="information" />}
            right={(props) => <List.Icon {...props} icon="chevron-right" />}
            onPress={() => {}}
          />
          <Divider />
          <List.Item
            title={t('auth.logout')}
            left={(props) => (
              <List.Icon {...props} icon="logout" color={theme.colors.error} />
            )}
            titleStyle={{ color: theme.colors.error }}
            onPress={handleLogout}
          />
        </Card.Content>
      </Card>
    </ScrollView>
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
  profileCard: {
    marginHorizontal: 16,
    marginBottom: 16,
  },
  profileContent: {
    alignItems: 'center',
    paddingVertical: 24,
  },
  avatar: {
    marginBottom: 16,
  },
  name: {
    fontWeight: 'bold',
    marginBottom: 4,
  },
  email: {
    opacity: 0.7,
    marginBottom: 4,
  },
  role: {
    opacity: 0.6,
  },
  card: {
    marginHorizontal: 16,
    marginBottom: 16,
  },
  sectionTitle: {
    marginBottom: 8,
    fontWeight: 'bold',
  },
});

export default ProfileScreen;
