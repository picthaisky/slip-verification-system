# SignalR Real-time Notification System - Quick Start Guide

## Overview

This guide shows how to quickly integrate SignalR real-time notifications into your Slip Verification System.

## Backend Integration

### Step 1: Inject the Service

The `IRealtimeNotificationService` is already registered in DI. Just inject it into your service or controller:

```csharp
using SlipVerification.Application.Interfaces;

public class SlipProcessingService
{
    private readonly IRealtimeNotificationService _realtimeNotificationService;
    
    public SlipProcessingService(IRealtimeNotificationService realtimeNotificationService)
    {
        _realtimeNotificationService = realtimeNotificationService;
    }
}
```

### Step 2: Send Notifications

#### Notify User About Slip Verification

```csharp
public async Task ProcessSlipAsync(Guid userId, Guid orderId, decimal amount)
{
    // Your slip verification logic...
    
    // Send real-time notification
    await _realtimeNotificationService.NotifySlipVerifiedAsync(
        userId,
        new SlipVerificationResult
        {
            OrderId = orderId,
            Amount = amount,
            Status = "Verified"
        }
    );
}
```

#### Notify Order Room About Payment

```csharp
public async Task ConfirmPaymentAsync(Guid orderId)
{
    // Your payment confirmation logic...
    
    // Notify all users watching this order
    await _realtimeNotificationService.NotifyPaymentReceivedAsync(
        orderId,
        new PaymentNotification
        {
            OrderId = orderId,
            Amount = 1250.00m,
            Status = "Completed",
            TransactionId = "TXN-" + DateTime.UtcNow.Ticks,
            Timestamp = DateTime.UtcNow
        }
    );
}
```

#### Broadcast System Message

```csharp
public async Task NotifyMaintenanceAsync()
{
    await _realtimeNotificationService.BroadcastSystemMessageAsync(
        "Scheduled maintenance will begin in 30 minutes. Please save your work."
    );
}
```

## Frontend Integration

### Step 1: Inject the Service

```typescript
import { Component, OnInit, inject } from '@angular/core';
import { WebsocketService } from '@core/services/websocket.service';

@Component({
  selector: 'app-dashboard',
  template: `...`
})
export class DashboardComponent implements OnInit {
  private readonly wsService = inject(WebsocketService);
  
  ngOnInit(): void {
    // Connect to SignalR
    this.wsService.startConnection();
  }
}
```

### Step 2: Listen for Notifications

#### Listen for Slip Verification

```typescript
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { WebsocketService } from '@core/services/websocket.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-slip-list',
  template: `
    <div class="notifications">
      @for (notification of recentNotifications; track notification.orderId) {
        <div class="notification">
          Order {{ notification.orderId }} - ${{ notification.amount }} - {{ notification.status }}
        </div>
      }
    </div>
  `
})
export class SlipListComponent implements OnInit, OnDestroy {
  private readonly wsService = inject(WebsocketService);
  private readonly destroy$ = new Subject<void>();
  
  recentNotifications: any[] = [];
  
  ngOnInit(): void {
    // Connect and listen for notifications
    this.wsService.startConnection();
    
    this.wsService.onSlipVerified()
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => {
        console.log('Slip verified:', notification);
        this.recentNotifications.unshift(notification);
        
        // Show toast notification
        this.showToast(`Payment of $${notification.amount} verified!`);
        
        // Refresh data
        this.loadSlips();
      });
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  private showToast(message: string): void {
    // Your toast implementation
  }
  
  private loadSlips(): void {
    // Refresh slip list
  }
}
```

#### Listen for Payment Updates in Order Detail

```typescript
@Component({
  selector: 'app-order-detail',
  template: `
    <div class="order-detail">
      <h2>Order #{{ order.id }}</h2>
      <div class="status">Status: {{ order.status }}</div>
      
      @if (wsService.isConnected()) {
        <span class="badge badge-success">Live Updates Active</span>
      }
    </div>
  `
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  private readonly wsService = inject(WebsocketService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy$ = new Subject<void>();
  
  order: any = {};
  orderId: string = '';
  
  ngOnInit(): void {
    this.orderId = this.route.snapshot.params['id'];
    this.loadOrder();
    
    // Connect and join order room
    this.wsService.startConnection().then(() => {
      this.wsService.joinOrderRoom(this.orderId);
    });
    
    // Listen for payment updates
    this.wsService.onPaymentReceived()
      .pipe(takeUntil(this.destroy$))
      .subscribe(payment => {
        if (payment.orderId === this.orderId) {
          console.log('Payment received:', payment);
          this.order.status = payment.status;
          this.order.amount = payment.amount;
          this.showSuccessMessage('Payment confirmed!');
        }
      });
  }
  
  ngOnDestroy(): void {
    // Leave order room
    this.wsService.leaveOrderRoom(this.orderId);
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  private loadOrder(): void {
    // Load order details
  }
  
  private showSuccessMessage(message: string): void {
    // Show success message
  }
}
```

#### Listen for System Messages

```typescript
@Component({
  selector: 'app-root',
  template: `
    <div class="app">
      @if (systemMessage) {
        <div class="alert alert-info">
          {{ systemMessage }}
        </div>
      }
      <router-outlet />
    </div>
  `
})
export class AppComponent implements OnInit, OnDestroy {
  private readonly wsService = inject(WebsocketService);
  private readonly destroy$ = new Subject<void>();
  
  systemMessage: string = '';
  
  ngOnInit(): void {
    this.wsService.startConnection();
    
    this.wsService.onSystemMessage()
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        console.log('System message:', message);
        this.systemMessage = message.message;
        
        // Auto-hide after 10 seconds
        setTimeout(() => {
          this.systemMessage = '';
        }, 10000);
      });
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.wsService.stopConnection();
  }
}
```

## Connection Status Display

```typescript
@Component({
  selector: 'app-connection-status',
  template: `
    <div class="connection-status">
      @if (wsService.isConnected()) {
        <span class="status-indicator online"></span>
        <span>Connected</span>
      } @else if (wsService.isReconnecting()) {
        <span class="status-indicator reconnecting"></span>
        <span>Reconnecting...</span>
      } @else {
        <span class="status-indicator offline"></span>
        <span>Disconnected</span>
      }
    </div>
  `,
  styles: [`
    .connection-status {
      display: flex;
      align-items: center;
      gap: 8px;
    }
    
    .status-indicator {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
    
    .status-indicator.online {
      background-color: #10b981;
    }
    
    .status-indicator.reconnecting {
      background-color: #f59e0b;
      animation: pulse 1s infinite;
    }
    
    .status-indicator.offline {
      background-color: #ef4444;
    }
    
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }
  `]
})
export class ConnectionStatusComponent {
  readonly wsService = inject(WebsocketService);
}
```

## Testing the Implementation

### 1. Start the Backend

```bash
cd slip-verification-api
dotnet run --project src/SlipVerification.API
```

### 2. Start the Frontend

```bash
cd slip-verification-web
npm start
```

### 3. Test Real-time Notifications

You can test by:

1. Opening multiple browser windows with the same user
2. Triggering a slip verification
3. Watching the notifications appear in real-time across all windows

Or use the browser console:

```javascript
// In browser console
const hubConnection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/ws", {
    accessTokenFactory: () => "your-jwt-token"
  })
  .build();

await hubConnection.start();
console.log("Connected!");

hubConnection.on("SlipVerified", (data) => {
  console.log("Slip verified:", data);
});
```

## Common Patterns

### Pattern 1: Show Toast on Notification

```typescript
ngOnInit(): void {
  this.wsService.onSlipVerified()
    .pipe(takeUntil(this.destroy$))
    .subscribe(notification => {
      this.notificationService.showSuccess(
        `Payment of $${notification.amount} verified!`
      );
    });
}
```

### Pattern 2: Update List on Notification

```typescript
ngOnInit(): void {
  this.wsService.onPaymentReceived()
    .pipe(takeUntil(this.destroy$))
    .subscribe(payment => {
      // Update the item in the list
      const index = this.orders.findIndex(o => o.id === payment.orderId);
      if (index >= 0) {
        this.orders[index].status = payment.status;
      }
    });
}
```

### Pattern 3: Refresh Data on Notification

```typescript
ngOnInit(): void {
  this.wsService.onSlipVerified()
    .pipe(
      takeUntil(this.destroy$),
      debounceTime(1000) // Prevent too many refreshes
    )
    .subscribe(() => {
      this.loadSlips(); // Refresh from API
    });
}
```

## Configuration

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200"
    ]
  }
}
```

### Frontend (environment.ts)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  // SignalR uses apiUrl, wsUrl is deprecated
};
```

## Troubleshooting

### Connection Fails

1. **Check token**: Ensure JWT token is valid
   ```typescript
   const token = this.authService.getToken();
   console.log('Token:', token);
   ```

2. **Check CORS**: Verify frontend URL is in `Cors:AllowedOrigins`

3. **Check WebSocket**: Open browser DevTools → Network → WS tab

### No Notifications Received

1. **Check connection**: `wsService.isConnected()` should be `true`
2. **Check subscription**: Ensure you're subscribed before notification is sent
3. **Check event name**: Must match exactly (case-sensitive)

### Performance Issues

1. **Unsubscribe properly**: Always use `takeUntil(destroy$)`
2. **Debounce updates**: Use `debounceTime()` for frequent notifications
3. **Batch operations**: Update UI once for multiple notifications

## Next Steps

- Read the full documentation: [SIGNALR_REALTIME_SYSTEM.md](./SIGNALR_REALTIME_SYSTEM.md)
- Review the implementation code in the repository
- Add custom notification types for your specific needs
- Implement analytics tracking for real-time events

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review the full documentation
3. Check server logs for errors
4. Check browser console for client-side errors
