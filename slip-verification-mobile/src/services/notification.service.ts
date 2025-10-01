import PushNotification from 'react-native-push-notification';
import { Platform } from 'react-native';

class NotificationService {
  constructor() {
    this.configure();
  }

  configure(): void {
    PushNotification.configure({
      // Called when Token is generated
      onRegister: (token) => {
        console.log('TOKEN:', token);
      },

      // Called when a remote or local notification is opened or received
      onNotification: (notification) => {
        console.log('NOTIFICATION:', notification);
        // Handle notification tap
        if (notification.userInteraction) {
          // User tapped the notification
          this.handleNotificationTap(notification);
        }
      },

      // Android only
      senderID: 'YOUR_SENDER_ID',
      
      // iOS only
      permissions: {
        alert: true,
        badge: true,
        sound: true,
      },
      
      popInitialNotification: true,
      requestPermissions: Platform.OS === 'ios',
    });

    // Create default channel for Android
    if (Platform.OS === 'android') {
      PushNotification.createChannel(
        {
          channelId: 'default-channel',
          channelName: 'Default Channel',
          channelDescription: 'Default notification channel',
          playSound: true,
          soundName: 'default',
          importance: 4,
          vibrate: true,
        },
        (created) => console.log(`Channel created: ${created}`)
      );
    }
  }

  showLocalNotification(title: string, message: string, data?: any): void {
    PushNotification.localNotification({
      channelId: 'default-channel',
      title,
      message,
      playSound: true,
      soundName: 'default',
      userInfo: data,
    });
  }

  scheduleNotification(title: string, message: string, date: Date, data?: any): void {
    PushNotification.localNotificationSchedule({
      channelId: 'default-channel',
      title,
      message,
      date,
      playSound: true,
      soundName: 'default',
      userInfo: data,
    });
  }

  cancelAllNotifications(): void {
    PushNotification.cancelAllLocalNotifications();
  }

  setApplicationIconBadgeNumber(number: number): void {
    PushNotification.setApplicationIconBadgeNumber(number);
  }

  private handleNotificationTap(notification: any): void {
    // Navigate to specific screen based on notification data
    const { type, id } = notification.data || {};
    
    // TODO: Implement navigation logic
    console.log('Navigate to:', type, id);
  }

  async requestPermissions(): Promise<boolean> {
    if (Platform.OS === 'ios') {
      const permissions = await PushNotification.requestPermissions();
      return permissions.alert || false;
    }
    return true; // Android doesn't need runtime permissions for notifications
  }

  async checkPermissions(): Promise<any> {
    return new Promise((resolve) => {
      PushNotification.checkPermissions(resolve);
    });
  }
}

export default new NotificationService();
