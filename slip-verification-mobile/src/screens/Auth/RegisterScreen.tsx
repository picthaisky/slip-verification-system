import React, { useState } from 'react';
import {
  View,
  StyleSheet,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
} from 'react-native';
import { TextInput, Button, Text, useTheme } from 'react-native-paper';
import { authApi } from '../../api/endpoints/auth';
import { t } from '../../locales';
import Icon from 'react-native-vector-icons/MaterialCommunityIcons';

const RegisterScreen = ({ navigation }: any) => {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [showPassword, setShowPassword] = useState(false);

  const theme = useTheme();

  const handleRegister = async () => {
    if (!name || !email || !password || !confirmPassword) {
      setError(t('errors.validationError'));
      return;
    }

    if (password !== confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    setLoading(true);
    setError('');

    try {
      await authApi.register({ name, email, password, phoneNumber });
      navigation.navigate('Login');
    } catch (err: any) {
      setError(err.response?.data?.message || t('auth.registerError'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
      style={styles.container}
    >
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.header}>
          <Icon
            name="account-plus"
            size={80}
            color={theme.colors.primary}
          />
          <Text variant="headlineMedium" style={styles.title}>
            {t('auth.register')}
          </Text>
        </View>

        <View style={styles.form}>
          <TextInput
            label={t('auth.name')}
            value={name}
            onChangeText={setName}
            mode="outlined"
            left={<TextInput.Icon icon="account" />}
            style={styles.input}
          />

          <TextInput
            label={t('auth.email')}
            value={email}
            onChangeText={setEmail}
            mode="outlined"
            keyboardType="email-address"
            autoCapitalize="none"
            left={<TextInput.Icon icon="email" />}
            style={styles.input}
          />

          <TextInput
            label={t('auth.phoneNumber')}
            value={phoneNumber}
            onChangeText={setPhoneNumber}
            mode="outlined"
            keyboardType="phone-pad"
            left={<TextInput.Icon icon="phone" />}
            style={styles.input}
          />

          <TextInput
            label={t('auth.password')}
            value={password}
            onChangeText={setPassword}
            mode="outlined"
            secureTextEntry={!showPassword}
            left={<TextInput.Icon icon="lock" />}
            right={
              <TextInput.Icon
                icon={showPassword ? 'eye-off' : 'eye'}
                onPress={() => setShowPassword(!showPassword)}
              />
            }
            style={styles.input}
          />

          <TextInput
            label="Confirm Password"
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            mode="outlined"
            secureTextEntry={!showPassword}
            left={<TextInput.Icon icon="lock-check" />}
            style={styles.input}
          />

          {error ? (
            <Text style={[styles.error, { color: theme.colors.error }]}>
              {error}
            </Text>
          ) : null}

          <Button
            mode="contained"
            onPress={handleRegister}
            loading={loading}
            disabled={loading}
            style={styles.button}
          >
            {t('auth.register')}
          </Button>

          <View style={styles.loginContainer}>
            <Text>{t('auth.alreadyHaveAccount')} </Text>
            <Button
              mode="text"
              onPress={() => navigation.navigate('Login')}
              compact
            >
              {t('auth.login')}
            </Button>
          </View>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: 20,
  },
  header: {
    alignItems: 'center',
    marginBottom: 40,
  },
  title: {
    marginTop: 20,
    fontWeight: 'bold',
  },
  form: {
    width: '100%',
  },
  input: {
    marginBottom: 16,
  },
  button: {
    marginTop: 8,
    paddingVertical: 8,
  },
  loginContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 20,
  },
  error: {
    marginBottom: 16,
    textAlign: 'center',
  },
});

export default RegisterScreen;
