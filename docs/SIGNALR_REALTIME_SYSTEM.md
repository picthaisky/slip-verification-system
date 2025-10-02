# SignalR Real-time Notification System

This document describes the SignalR implementation for real-time notifications in the Slip Verification System.

## Overview

The system uses **ASP.NET Core SignalR** with **Redis backplane** for scalable real-time communication between the server and clients. This allows instant notifications for:

- Slip verification results
- Payment confirmations
- System-wide messages
- Order-specific updates

## Architecture

### Backend Components

#### 1. NotificationHub (`Infrastructure/Hubs/NotificationHub.cs`)

The SignalR Hub that manages WebSocket connections and groups.

**Features:**
- JWT authentication via `[Authorize]` attribute
- Automatic user group management on connect/disconnect
- Order-specific room management
- Connection logging

**Key Methods:**
- `OnConnectedAsync()` - Adds user to their personal group
- `OnDisconnectedAsync()` - Removes user from groups
- `JoinOrderRoom(orderId)` - Client can join order-specific rooms
- `LeaveOrderRoom(orderId)` - Client can leave order-specific rooms

#### 2. RealtimeNotificationService (`Infrastructure/Services/Realtime/RealtimeNotificationService.cs`)

Service for sending notifications through SignalR.

**Methods:**
```csharp
// Notify specific user about slip verification
Task NotifySlipVerifiedAsync(Guid userId, SlipVerificationResult result)

// Notify all users in order room
Task NotifyPaymentReceivedAsync(Guid orderId, PaymentNotification notification)

// Broadcast to all connected users
Task BroadcastSystemMessageAsync(string message)
```

#### 3. SignalR Configuration (`Program.cs`)

**Key Configuration:**
- Keep-alive interval: 15 seconds
- Client timeout: 30 seconds
- Handshake timeout: 15 seconds
- Max message size: 32 KB
- Redis backplane for scaling across multiple servers

**JWT Authentication:**
- Supports JWT tokens in query string (`?access_token=...`)
- Required for SignalR WebSocket connections
- Validates token using same configuration as API

### Frontend Components

#### WebSocket Service (`src/app/core/services/websocket.service.ts`)

Angular service using `@microsoft/signalr` for client-side connections.

**Features:**
- Automatic reconnection with exponential backoff
- Connection state management with Angular signals
- Type-safe event handlers
- Order room management

**Key Methods:**
```typescript
// Connection management
connect(): Promise<void>
disconnect(): void
startConnection(): Promise<void>
stopConnection(): Promise<void>

// Room management
joinOrderRoom(orderId: string): Promise<void>
leaveOrderRoom(orderId: string): Promise<void>

// Event handlers
onSlipVerified(): Observable<any>
onPaymentReceived(): Observable<any>
onSystemMessage(): Observable<any>

// State
isConnected: Signal<boolean>
isReconnecting: Signal<boolean>
```

## Usage Examples

### Backend Usage

#### 1. Send Slip Verification Notification

```csharp
public class SlipVerificationService
{
    private readonly IRealtimeNotificationService _notificationService;

    public async Task VerifySlipAsync(Slip slip)
    {
        // ... verification logic ...

        // Send real-time notification
        await _notificationService.NotifySlipVerifiedAsync(
            slip.UserId,
            new SlipVerificationResult
            {
                OrderId = slip.OrderId,
                Amount = slip.Amount,
                Status = "Verified"
            }
        );
    }
}
```

#### 2. Send Payment Notification to Order Room

```csharp
public class PaymentService
{
    private readonly IRealtimeNotificationService _notificationService;

    public async Task ProcessPaymentAsync(Order order)
    {
        // ... payment processing ...

        // Notify all users watching this order
        await _notificationService.NotifyPaymentReceivedAsync(
            order.Id,
            new PaymentNotification
            {
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Status = "Completed",
                TransactionId = "TXN-12345",
                Timestamp = DateTime.UtcNow
            }
        );
    }
}
```

#### 3. Broadcast System Message

```csharp
public class MaintenanceService
{
    private readonly IRealtimeNotificationService _notificationService;

    public async Task ScheduleMaintenanceAsync()
    {
        await _notificationService.BroadcastSystemMessageAsync(
            "System maintenance scheduled in 30 minutes"
        );
    }
}
```

### Frontend Usage

#### 1. Component with Real-time Notifications

```typescript
import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { WebsocketService } from '@core/services/websocket.service';
import { NotificationService } from '@core/services/notification.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-dashboard',
  template: `
    <div>
      @if (wsService.isConnected()) {
        <span class="status-connected">Connected</span>
      } @else if (wsService.isReconnecting()) {
        <span class="status-reconnecting">Reconnecting...</span>
      } @else {
        <span class="status-disconnected">Disconnected</span>
      }
      
      <!-- Your dashboard content -->
    </div>
  `
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly wsService = inject(WebsocketService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroy$ = new Subject<void>();
  
  ngOnInit(): void {
    // Connect to SignalR
    this.wsService.startConnection();
    
    // Listen for slip verification notifications
    this.wsService.onSlipVerified()
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => {
        console.log('Slip verified:', notification);
        this.notificationService.showSuccess(
          `Payment verified: $${notification.amount}`
        );
        this.refreshData();
      });
    
    // Listen for payment notifications
    this.wsService.onPaymentReceived()
      .pipe(takeUntil(this.destroy$))
      .subscribe(payment => {
        console.log('Payment received:', payment);
        this.handlePaymentNotification(payment);
      });
    
    // Listen for system messages
    this.wsService.onSystemMessage()
      .pipe(takeUntil(this.destroy$))
      .subscribe(message => {
        console.log('System message:', message);
        this.notificationService.showInfo(message.message);
      });
  }
  
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.wsService.stopConnection();
  }
  
  private refreshData(): void {
    // Refresh dashboard data
  }
  
  private handlePaymentNotification(payment: any): void {
    // Handle payment notification
  }
}
```

#### 2. Order Detail Component with Room Management

```typescript
@Component({
  selector: 'app-order-detail',
  template: `...`
})
export class OrderDetailComponent implements OnInit, OnDestroy {
  private readonly wsService = inject(WebsocketService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroy$ = new Subject<void>();
  private orderId: string = '';
  
  ngOnInit(): void {
    // Get order ID from route
    this.orderId = this.route.snapshot.params['id'];
    
    // Join order-specific room
    this.wsService.startConnection().then(() => {
      this.wsService.joinOrderRoom(this.orderId);
    });
    
    // Listen for updates
    this.wsService.onPaymentReceived()
      .pipe(takeUntil(this.destroy$))
      .subscribe(payment => {
        if (payment.orderId === this.orderId) {
          this.updateOrderStatus(payment);
        }
      });
  }
  
  ngOnDestroy(): void {
    // Leave order room when component is destroyed
    this.wsService.leaveOrderRoom(this.orderId);
    this.destroy$.next();
    this.destroy$.complete();
  }
  
  private updateOrderStatus(payment: any): void {
    // Update order status in UI
  }
}
```

## Configuration

### Backend Configuration (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:4200",
      "https://app.slipverification.com"
    ]
  }
}
```

### Frontend Configuration (`environment.ts`)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  wsUrl: 'http://localhost:5000',  // SignalR uses apiUrl, not wsUrl
};
```

## Connection Flow

1. **Client initiates connection:**
   ```typescript
   wsService.startConnection()
   ```

2. **SignalR negotiation:**
   - Client requests connection details from `/ws/negotiate`
   - Server returns connection token and transport options

3. **WebSocket establishment:**
   - Client connects with JWT token in query string
   - Server validates token and authenticates user

4. **User group assignment:**
   - `OnConnectedAsync()` adds user to `user_{userId}` group
   - Client can join additional rooms (e.g., order rooms)

5. **Message flow:**
   - Server sends to groups: `user_{userId}`, `order_{orderId}`, or `All`
   - Client receives via event handlers: `onSlipVerified()`, etc.

6. **Disconnection:**
   - Client calls `stopConnection()`
   - Or network interruption triggers auto-reconnect

## Reconnection Strategy

### Automatic Reconnection
- **Enabled by default** with exponential backoff
- Initial retry: 0ms (immediate)
- Retry delays: 1s, 2s, 4s, 8s, 10s (capped)
- Stops after 60 seconds of failed attempts

### Manual Reconnection
```typescript
// Retry every 5 seconds on close
this.hubConnection.onclose(() => {
  setTimeout(() => this.startConnection(), 5000);
});
```

## Scaling with Redis Backplane

### How it Works

1. **Multiple server instances** can run simultaneously
2. **Redis pub/sub** broadcasts messages across all servers
3. **Users connected to different servers** receive the same messages
4. **Channel prefix** (`SignalR`) isolates SignalR messages

### Redis Configuration

```csharp
signalRBuilder.AddStackExchangeRedis(redisConnection, options =>
{
    options.Configuration.ChannelPrefix = "SignalR";
});
```

### Load Balancing

Configure load balancer with:
- **Sticky sessions** NOT required (thanks to Redis)
- **WebSocket support** enabled
- **Health checks** on `/health` endpoint

## Security

### Authentication
- **JWT tokens** required for all connections
- Tokens passed via query string: `?access_token=...`
- Same validation as REST API

### Authorization
- `[Authorize]` attribute on hub
- User groups based on `ClaimTypes.NameIdentifier`
- Order rooms provide additional access control

### CORS
- Configured for specific origins
- Credentials allowed for authentication
- Must match between frontend and backend

## Monitoring & Debugging

### Enable Detailed Errors (Development)
```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});
```

### Client Logging
```typescript
.configureLogging(signalR.LogLevel.Information)
// Or for debugging:
.configureLogging(signalR.LogLevel.Debug)
```

### Connection State
```typescript
// Check connection state
if (wsService.isConnected()) {
  console.log('Connected to SignalR');
}

// Monitor reconnection
wsService.getReconnectingStatus().subscribe(isReconnecting => {
  console.log('Reconnecting:', isReconnecting);
});
```

## Performance Optimization

### Message Size
- Max message size: 32 KB (configurable)
- Use pagination for large datasets
- Send IDs instead of full objects when possible

### Connection Pooling
- Redis connection pooling handled by StackExchange.Redis
- SignalR manages WebSocket connections efficiently

### Batching
```typescript
// Instead of sending multiple small messages
for (const item of items) {
  await hub.send('ItemUpdate', item);  // BAD
}

// Send batch
await hub.send('ItemsUpdate', items);  // GOOD
```

## Troubleshooting

### Connection Fails
1. Check CORS configuration
2. Verify JWT token is valid
3. Ensure WebSocket is enabled on server
4. Check firewall/proxy settings

### Messages Not Received
1. Verify user is in correct group
2. Check event name matches exactly
3. Ensure connection is established before subscribing
4. Look for errors in browser console

### Reconnection Issues
1. Check network connectivity
2. Verify Redis is running (for backplane)
3. Review reconnection strategy settings
4. Check server logs for errors

## Testing

### Unit Tests
```csharp
[Fact]
public async Task NotifySlipVerified_SendsToUserGroup()
{
    // Arrange
    var mockHubContext = Mock.Of<IHubContext<NotificationHub>>();
    var service = new RealtimeNotificationService(mockHubContext, logger);
    
    // Act
    await service.NotifySlipVerifiedAsync(userId, result);
    
    // Assert
    Mock.Get(mockHubContext.Clients.Group($"user_{userId}"))
        .Verify(x => x.SendAsync("SlipVerified", It.IsAny<object>(), default));
}
```

### Integration Tests
- Test with actual SignalR connection
- Verify message delivery
- Test reconnection behavior

## Migration from Socket.io

The new SignalR implementation maintains backward compatibility:

```typescript
// Old Socket.io way (still works but deprecated)
wsService.on('custom-event').subscribe(data => { });

// New SignalR way (recommended)
wsService.onSlipVerified().subscribe(data => { });
```

## References

- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR JavaScript Client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [Redis Backplane](https://learn.microsoft.com/en-us/aspnet/core/signalr/redis-backplane)
- [@microsoft/signalr npm package](https://www.npmjs.com/package/@microsoft/signalr)
