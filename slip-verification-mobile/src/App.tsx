import React, { useEffect } from 'react';
import { StatusBar } from 'react-native';
import { Provider as ReduxProvider } from 'react-redux';
import { PaperProvider } from 'react-native-paper';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { SafeAreaProvider } from 'react-native-safe-area-context';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { store } from './store/store';
import AppNavigator from './navigation/AppNavigator';
import { lightTheme, darkTheme } from './theme';
import { useAppSelector } from './hooks/useRedux';
import { useNetworkStatus } from './hooks/useNetworkStatus';
import websocketService from './services/websocket.service';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

const AppContent = () => {
  const themeMode = useAppSelector(state => state.app.theme);
  const isAuthenticated = useAppSelector(state => state.auth.isAuthenticated);
  const theme = themeMode === 'dark' ? darkTheme : lightTheme;
  
  // Monitor network status
  useNetworkStatus();

  // Setup WebSocket connection when authenticated
  useEffect(() => {
    if (isAuthenticated) {
      websocketService.connect();
    } else {
      websocketService.disconnect();
    }

    return () => {
      websocketService.disconnect();
    };
  }, [isAuthenticated]);

  return (
    <PaperProvider theme={theme}>
      <SafeAreaProvider>
        <GestureHandlerRootView style={{ flex: 1 }}>
          <StatusBar
            barStyle={themeMode === 'dark' ? 'light-content' : 'dark-content'}
            backgroundColor={theme.colors.background}
          />
          <AppNavigator />
        </GestureHandlerRootView>
      </SafeAreaProvider>
    </PaperProvider>
  );
};

const App = () => {
  return (
    <ReduxProvider store={store}>
      <QueryClientProvider client={queryClient}>
        <AppContent />
      </QueryClientProvider>
    </ReduxProvider>
  );
};

export default App;
