import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
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
  private hubConnection: signalR.HubConnection | null = null;
  private reconnecting$ = new Subject<boolean>();
  
  isConnected = signal<boolean>(false);
  isReconnecting = signal<boolean>(false);
  
  private initializeConnection(): void {
    const token = this.authService.getToken();
    
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/ws`, {
        accessTokenFactory: () => token || '',
        transport: signalR.HttpTransportType.WebSockets | 
                   signalR.HttpTransportType.LongPolling,
        skipNegotiation: false
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff
          if (retryContext.elapsedMilliseconds < 60000) {
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 10000);
          }
          return null; // Stop reconnecting after 1 minute
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();
    
    this.setupEventHandlers();
  }
  
  private setupEventHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting(() => {
      this.isReconnecting.set(true);
      this.reconnecting$.next(true);
      console.log('SignalR reconnecting...');
    });
    
    this.hubConnection.onreconnected(() => {
      this.isReconnecting.set(false);
      this.reconnecting$.next(false);
      console.log('SignalR reconnected');
    });
    
    this.hubConnection.onclose((error) => {
      this.isConnected.set(false);
      console.error('SignalR connection closed:', error);
      // Attempt to reconnect after 5 seconds
      setTimeout(() => this.startConnection(), 5000);
    });
  }
  
  async connect(): Promise<void> {
    return this.startConnection();
  }

  async startConnection(): Promise<void> {
    try {
      if (!this.hubConnection) {
        this.initializeConnection();
      }

      if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
        return;
      }

      await this.hubConnection?.start();
      this.isConnected.set(true);
      console.log('SignalR connected');
    } catch (error) {
      this.isConnected.set(false);
      console.error('Error connecting to SignalR:', error);
      setTimeout(() => this.startConnection(), 5000);
    }
  }
  
  async joinOrderRoom(orderId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinOrderRoom', orderId);
    }
  }
  
  async leaveOrderRoom(orderId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveOrderRoom', orderId);
    }
  }
  
  onSlipVerified(): Observable<any> {
    return new Observable(observer => {
      if (!this.hubConnection) {
        this.initializeConnection();
      }

      this.hubConnection?.on('SlipVerified', (data) => {
        observer.next(data);
      });

      return () => {
        this.hubConnection?.off('SlipVerified');
      };
    });
  }
  
  onPaymentReceived(): Observable<any> {
    return new Observable(observer => {
      if (!this.hubConnection) {
        this.initializeConnection();
      }

      this.hubConnection?.on('PaymentReceived', (data) => {
        observer.next(data);
      });

      return () => {
        this.hubConnection?.off('PaymentReceived');
      };
    });
  }
  
  onSystemMessage(): Observable<any> {
    return new Observable(observer => {
      if (!this.hubConnection) {
        this.initializeConnection();
      }

      this.hubConnection?.on('SystemMessage', (data) => {
        observer.next(data);
      });

      return () => {
        this.hubConnection?.off('SystemMessage');
      };
    });
  }

  disconnect(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
      this.hubConnection = null;
      this.isConnected.set(false);
    }
  }

  async stopConnection(): Promise<void> {
    await this.hubConnection?.stop();
    this.isConnected.set(false);
  }

  // Legacy methods for backward compatibility
  emit(event: string, data: any): void {
    console.warn('emit() is not supported with SignalR. Use specific hub methods instead.');
  }

  on(event: string): Observable<any> {
    return new Observable(observer => {
      if (!this.hubConnection) {
        this.initializeConnection();
      }

      this.hubConnection?.on(event, (data: any) => {
        observer.next(data);
      });

      return () => {
        this.hubConnection?.off(event);
      };
    });
  }

  getMessages(): Observable<WebSocketMessage> {
    console.warn('getMessages() is deprecated. Use specific event handlers like onSlipVerified(), onPaymentReceived(), etc.');
    return new Observable(() => {});
  }

  getReconnectingStatus(): Observable<boolean> {
    return this.reconnecting$.asObservable();
  }
}
