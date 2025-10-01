# Slip Verification Web Application

Modern Angular 20 web application for payment slip verification system with real-time features.

## 🚀 Features

### Core Features
- **Slip Upload & Verification**: Drag & drop interface with image preview and real-time validation
- **Dashboard**: Statistics overview with Material Design cards
- **Order Management**: Track and manage payment orders
- **Transaction History**: View and search transaction records
- **Real-time Notifications**: WebSocket-based notification system
- **Role-based Access Control**: Admin, User, and Guest roles

### Technical Features
- **Standalone Components**: Modern Angular architecture without NgModules
- **Signals API**: Reactive state management with Angular Signals
- **Lazy Loading**: Optimized route-based code splitting
- **HTTP Interceptors**: Automatic authentication, error handling, and loading states
- **Guards**: Authentication and role-based route protection
- **Reactive Forms**: Form validation with custom validators
- **Tailwind CSS**: Utility-first CSS framework
- **Angular Material**: Material Design components

## 📋 Prerequisites

- Node.js 18+ (recommended 20.x)
- npm 9+
- Angular CLI 20+

## 🛠️ Installation

```bash
# Install dependencies
npm install

# Install Angular CLI globally (if not already installed)
npm install -g @angular/cli@20
```

## 🏃 Development

```bash
# Start development server
npm start

# Or
ng serve

# Open browser to http://localhost:4200
```

## 🏗️ Build

```bash
# Production build
npm run build

# Development build
ng build

# Build with watch mode
ng build --watch
```

## 📁 Project Structure

```
src/
├── app/
│   ├── core/                      # Core functionality (singleton services)
│   │   ├── services/
│   │   │   ├── api.service.ts           # HTTP API wrapper
│   │   │   ├── auth.service.ts          # Authentication & JWT
│   │   │   ├── notification.service.ts  # Notification system
│   │   │   ├── websocket.service.ts     # Real-time WebSocket
│   │   │   └── loading.service.ts       # Global loading state
│   │   ├── guards/
│   │   │   ├── auth.guard.ts            # Authentication guard
│   │   │   └── role.guard.ts            # Role-based access guard
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts      # JWT token injection
│   │   │   ├── error.interceptor.ts     # Error handling
│   │   │   └── loading.interceptor.ts   # Loading state
│   │   └── models/
│   │       └── domain.models.ts         # Domain models & enums
│   │
│   ├── shared/                    # Shared components & utilities
│   │   ├── components/
│   │   │   ├── loading-spinner/         # Global loading spinner
│   │   │   ├── notification-panel/      # Notification panel
│   │   │   ├── not-found/               # 404 page
│   │   │   └── unauthorized/            # 403 page
│   │   └── pipes/
│   │       └── status-color.pipe.ts     # Status color mapping
│   │
│   ├── features/                  # Feature modules
│   │   ├── auth/
│   │   │   └── login/                   # Login component
│   │   ├── slip-upload/
│   │   │   ├── components/
│   │   │   │   └── slip-upload/         # Slip upload with drag & drop
│   │   │   └── services/
│   │   │       └── slip-upload.service.ts
│   │   ├── dashboard/                   # Dashboard with statistics
│   │   ├── order-management/            # Order management
│   │   ├── transaction-history/         # Transaction history
│   │   └── reports/                     # Reports (admin only)
│   │
│   ├── app.config.ts              # App configuration
│   ├── app.routes.ts              # Route definitions
│   └── app.ts                     # Root component
│
├── environments/
│   ├── environment.ts             # Development config
│   └── environment.prod.ts        # Production config
│
└── styles.scss                    # Global styles
```

## 🔧 Configuration

### Environment Variables

Update `src/environments/environment.ts` for development:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api/v1',
  wsUrl: 'http://localhost:5000',
  uploadMaxSize: 5 * 1024 * 1024, // 5MB
  supportedFileTypes: ['image/jpeg', 'image/png', 'image/jpg'],
};
```

### API Integration

The app connects to the backend API at the configured `apiUrl`. Ensure the backend is running before starting the frontend.

## 🎨 Styling

### Tailwind CSS

Configured with custom theme in `tailwind.config.js`:

```javascript
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        primary: { /* custom colors */ }
      }
    }
  }
}
```

### Angular Material

Using prebuilt indigo-pink theme. Can be customized in `src/styles.scss`.

## 🔐 Authentication

### JWT Token Flow

1. Login with credentials
2. Receive JWT token and refresh token
3. Token automatically added to all HTTP requests via interceptor
4. Auto-logout on 401 responses

### Protected Routes

```typescript
// Auth guard - requires login
{ path: 'dashboard', canActivate: [authGuard] }

// Role guard - requires specific role
{ path: 'reports', canActivate: [roleGuard], data: { roles: ['Admin'] } }
```

## 🔔 Real-time Features

### WebSocket Connection

```typescript
// Automatic connection on authentication
websocketService.connect();

// Listen to events
websocketService.on('notification').subscribe(data => {
  // Handle notification
});
```

## 📱 Responsive Design

- Mobile-first approach with Tailwind CSS
- Responsive breakpoints: sm (640px), md (768px), lg (1024px), xl (1280px)
- Adaptive layouts for all screen sizes

## 🧪 Testing

```bash
# Run unit tests
npm test

# Run e2e tests
npm run e2e
```

## 🚢 Deployment

### Production Build

```bash
npm run build
```

Output will be in `dist/slip-verification-web/` directory.

### Deploy to Server

```bash
# Copy dist files to web server
cp -r dist/slip-verification-web/* /var/www/html/

# Or use a static file server
npx http-server dist/slip-verification-web
```

## 📚 Key Technologies

- **Angular 20.3.3**: Latest Angular framework
- **TypeScript 5.6+**: Type-safe JavaScript
- **Tailwind CSS 3**: Utility-first CSS
- **Angular Material 20**: Material Design components
- **RxJS 7**: Reactive programming
- **Socket.io Client**: Real-time communication
- **Chart.js**: Data visualization

## 🔗 API Endpoints

The app communicates with the following backend endpoints:

- `POST /api/v1/auth/login` - User login
- `POST /api/v1/slips/verify` - Upload and verify slip
- `GET /api/v1/orders` - Get orders list
- `GET /api/v1/transactions` - Get transaction history

See backend API documentation for complete endpoint list.

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Follow Angular style guide
4. Run linter: `npm run lint`
5. Submit pull request

## 📝 Code Style

- Use standalone components
- Prefer Signals over traditional RxJS for state
- Use `inject()` function for dependency injection
- Follow Angular naming conventions
- Use async pipe in templates

## 🐛 Troubleshooting

### Port already in use
```bash
ng serve --port 4201
```

### Build errors
```bash
# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

## 📄 License

MIT License - See LICENSE file for details

## 👥 Support

For issues and questions:
- GitHub Issues: [Link to issues]
- Email: support@slipverification.com
