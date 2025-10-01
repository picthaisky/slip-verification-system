import { io, Socket } from 'socket.io-client';
import config from '../utils/config';
import storageService from './storage.service';
import { WebSocketMessage } from '../types';

class WebSocketService {
  private socket: Socket | null = null;
  private listeners: Map<string, Set<(data: any) => void>> = new Map();
  private isConnected: boolean = false;

  async connect(): Promise<void> {
    if (this.socket?.connected) {
      return;
    }

    const token = await storageService.getAuthToken();
    
    this.socket = io(config.WS_URL, {
      auth: { token },
      reconnection: true,
      reconnectionDelay: 1000,
      reconnectionDelayMax: 5000,
      reconnectionAttempts: 5,
      transports: ['websocket'],
    });

    this.socket.on('connect', () => {
      console.log('WebSocket connected');
      this.isConnected = true;
    });

    this.socket.on('disconnect', () => {
      console.log('WebSocket disconnected');
      this.isConnected = false;
    });

    this.socket.on('error', (error) => {
      console.error('WebSocket error:', error);
    });

    // Generic message handler
    this.socket.on('message', (message: WebSocketMessage) => {
      this.handleMessage('message', message);
    });

    // Notification handler
    this.socket.on('notification', (notification: any) => {
      this.handleMessage('notification', notification);
    });

    // Slip status update handler
    this.socket.on('slip_status_updated', (data: any) => {
      this.handleMessage('slip_status_updated', data);
    });
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.disconnect();
      this.socket = null;
      this.isConnected = false;
      this.listeners.clear();
    }
  }

  on(event: string, callback: (data: any) => void): void {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, new Set());
    }
    this.listeners.get(event)?.add(callback);
  }

  off(event: string, callback: (data: any) => void): void {
    this.listeners.get(event)?.delete(callback);
  }

  emit(event: string, data: any): void {
    if (this.socket?.connected) {
      this.socket.emit(event, data);
    }
  }

  private handleMessage(event: string, data: any): void {
    const callbacks = this.listeners.get(event);
    if (callbacks) {
      callbacks.forEach(callback => callback(data));
    }
  }

  getConnectionStatus(): boolean {
    return this.isConnected;
  }
}

export default new WebSocketService();
