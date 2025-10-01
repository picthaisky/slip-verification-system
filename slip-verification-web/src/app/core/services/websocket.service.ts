import { Injectable, inject, signal } from '@angular/core';
import { io, Socket } from 'socket.io-client';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';

export interface WebSocketMessage {
  type: string;
  data: any;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class WebsocketService {
  private readonly authService = inject(AuthService);
  private socket: Socket | null = null;
  private messageSubject = new Subject<WebSocketMessage>();
  
  isConnected = signal<boolean>(false);
  
  connect(): void {
    if (this.socket?.connected) {
      return;
    }

    const token = this.authService.getToken();
    
    this.socket = io(environment.wsUrl, {
      auth: {
        token: token
      },
      reconnection: true,
      reconnectionDelay: 1000,
      reconnectionDelayMax: 5000,
      reconnectionAttempts: 5
    });

    this.socket.on('connect', () => {
      console.log('WebSocket connected');
      this.isConnected.set(true);
    });

    this.socket.on('disconnect', () => {
      console.log('WebSocket disconnected');
      this.isConnected.set(false);
    });

    this.socket.on('message', (data: any) => {
      const message: WebSocketMessage = {
        type: data.type || 'message',
        data: data.data || data,
        timestamp: new Date()
      };
      this.messageSubject.next(message);
    });

    this.socket.on('error', (error: any) => {
      console.error('WebSocket error:', error);
    });
  }

  disconnect(): void {
    if (this.socket) {
      this.socket.disconnect();
      this.socket = null;
      this.isConnected.set(false);
    }
  }

  emit(event: string, data: any): void {
    if (this.socket?.connected) {
      this.socket.emit(event, data);
    }
  }

  on(event: string): Observable<any> {
    return new Observable(observer => {
      if (!this.socket) {
        this.connect();
      }

      this.socket?.on(event, (data: any) => {
        observer.next(data);
      });

      return () => {
        this.socket?.off(event);
      };
    });
  }

  getMessages(): Observable<WebSocketMessage> {
    return this.messageSubject.asObservable();
  }
}
