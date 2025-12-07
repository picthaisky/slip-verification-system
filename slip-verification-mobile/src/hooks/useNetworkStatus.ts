import { useEffect, useState } from 'react';
import NetInfo from '@react-native-community/netinfo';
import { useAppDispatch } from './useRedux';
import { setOnlineStatus } from '../store/slices/appSlice';

export const useNetworkStatus = () => {
  const [isOnline, setIsOnline] = useState(true);
  const dispatch = useAppDispatch();

  useEffect(() => {
    const unsubscribe = NetInfo.addEventListener(state => {
      const online = (state.isConnected && state.isInternetReachable !== false) ?? true;
      setIsOnline(online);
      dispatch(setOnlineStatus(online));
    });

    return () => unsubscribe();
  }, [dispatch]);

  return isOnline;
};
