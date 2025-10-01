import { Injectable, signal } from '@angular/core';
import { Subject, Observable } from 'rxjs';

export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  timestamp: Date;
  read: boolean;
  autoClose?: boolean;
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationSubject = new Subject<Notification>();
  private notificationsSignal = signal<Notification[]>([]);
  
  notifications = this.notificationsSignal.asReadonly();
  unreadCount = signal<number>(0);

  show(type: NotificationType, title: string, message: string, autoClose = true, duration = 5000): void {
    const notification: Notification = {
      id: this.generateId(),
      type,
      title,
      message,
      timestamp: new Date(),
      read: false,
      autoClose,
      duration
    };

    this.addNotification(notification);
    this.notificationSubject.next(notification);

    if (autoClose) {
      setTimeout(() => {
        this.remove(notification.id);
      }, duration);
    }
  }

  success(title: string, message: string): void {
    this.show('success', title, message);
  }

  error(title: string, message: string, autoClose = false): void {
    this.show('error', title, message, autoClose);
  }

  warning(title: string, message: string): void {
    this.show('warning', title, message);
  }

  info(title: string, message: string): void {
    this.show('info', title, message);
  }

  getNotifications(): Observable<Notification> {
    return this.notificationSubject.asObservable();
  }

  markAsRead(id: string): void {
    const notifications = this.notificationsSignal();
    const notification = notifications.find(n => n.id === id);
    if (notification && !notification.read) {
      notification.read = true;
      this.notificationsSignal.set([...notifications]);
      this.updateUnreadCount();
    }
  }

  markAllAsRead(): void {
    const notifications = this.notificationsSignal();
    notifications.forEach(n => n.read = true);
    this.notificationsSignal.set([...notifications]);
    this.unreadCount.set(0);
  }

  remove(id: string): void {
    const notifications = this.notificationsSignal();
    this.notificationsSignal.set(notifications.filter(n => n.id !== id));
    this.updateUnreadCount();
  }

  clear(): void {
    this.notificationsSignal.set([]);
    this.unreadCount.set(0);
  }

  private addNotification(notification: Notification): void {
    const notifications = this.notificationsSignal();
    this.notificationsSignal.set([notification, ...notifications]);
    this.updateUnreadCount();
  }

  private updateUnreadCount(): void {
    const notifications = this.notificationsSignal();
    const unread = notifications.filter(n => !n.read).length;
    this.unreadCount.set(unread);
  }

  private generateId(): string {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  playSound(): void {
    // Play notification sound
    const audio = new Audio('assets/sounds/notification.mp3');
    audio.play().catch(() => {
      // Ignore if sound fails to play
    });
  }
}
