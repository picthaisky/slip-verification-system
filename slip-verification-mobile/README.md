# Slip Verification Mobile App

React Native mobile application (iOS & Android) for slip verification and upload system.

## 📱 Features

### Core Features
- ✅ **Cross-platform**: iOS and Android support
- ✅ **Authentication**: Login/Register with JWT tokens
- ✅ **Slip Upload**: Camera integration and gallery picker
- ✅ **Real-time Updates**: WebSocket integration for live notifications
- ✅ **Transaction History**: View and filter past transactions
- ✅ **Push Notifications**: Firebase Cloud Messaging support
- ✅ **Offline Support**: Queue uploads when offline
- ✅ **Dark Mode**: Full theme support
- ✅ **Localization**: Thai and English languages
- ✅ **Biometric Auth**: Face ID, Touch ID, Fingerprint support (placeholder)

### Technical Features
- ✅ **TypeScript**: Fully typed codebase
- ✅ **Redux Toolkit**: State management
- ✅ **React Query**: Server state caching
- ✅ **React Navigation 6**: Type-safe navigation
- ✅ **React Native Paper**: Material Design components
- ✅ **Axios**: HTTP client with interceptors
- ✅ **Socket.io**: WebSocket client

## 🛠️ Tech Stack

- **React Native**: 0.75.4
- **TypeScript**: 5.6.3
- **React Navigation**: 6.x
- **React Native Paper**: 5.11.6
- **Redux Toolkit**: 2.0.1
- **React Query**: 5.17.9
- **Axios**: 1.6.5
- **Socket.io Client**: 4.6.1
- **React Native Image Picker**: 7.1.0
- **React Native Push Notification**: 8.1.1

## 📁 Project Structure

```
slip-verification-mobile/
├── src/
│   ├── api/                    # API client and endpoints
│   │   ├── client.ts           # Axios client with interceptors
│   │   └── endpoints/          # API endpoint definitions
│   │       ├── auth.ts         # Authentication endpoints
│   │       ├── slip.ts         # Slip upload endpoints
│   │       ├── order.ts        # Order management
│   │       └── notification.ts # Notification endpoints
│   │
│   ├── components/             # Reusable components
│   │   ├── common/             # Common UI components
│   │   ├── slip/               # Slip-related components
│   │   └── notifications/      # Notification components
│   │
│   ├── navigation/             # Navigation configuration
│   │   ├── AppNavigator.tsx    # Root navigator
│   │   ├── AuthNavigator.tsx   # Authentication flow
│   │   └── MainNavigator.tsx   # Main app tabs
│   │
│   ├── screens/                # Screen components
│   │   ├── Auth/               # Login, Register
│   │   ├── Home/               # Home dashboard
│   │   ├── SlipUpload/         # Slip upload flow
│   │   ├── History/            # Transaction history
│   │   └── Profile/            # User profile & settings
│   │
│   ├── store/                  # Redux store
│   │   ├── slices/             # Redux slices
│   │   │   ├── authSlice.ts    # Authentication state
│   │   │   ├── appSlice.ts     # App settings (theme, language)
│   │   │   └── notificationSlice.ts # Notifications
│   │   └── store.ts            # Store configuration
│   │
│   ├── services/               # Services
│   │   ├── storage.service.ts  # AsyncStorage wrapper
│   │   ├── websocket.service.ts # WebSocket client
│   │   ├── notification.service.ts # Push notifications
│   │   └── biometric.service.ts # Biometric auth
│   │
│   ├── hooks/                  # Custom hooks
│   │   ├── useRedux.ts         # Typed Redux hooks
│   │   └── useNetworkStatus.ts # Network monitoring
│   │
│   ├── utils/                  # Utilities
│   │   └── config.ts           # App configuration
│   │
│   ├── theme/                  # Theme configuration
│   │   └── index.ts            # Light & dark themes
│   │
│   ├── locales/                # Localization
│   │   ├── index.ts            # Translation helper
│   │   ├── en.ts               # English translations
│   │   └── th.ts               # Thai translations
│   │
│   ├── types/                  # TypeScript types
│   │   └── index.ts            # Type definitions
│   │
│   └── App.tsx                 # Root component
│
├── android/                    # Android native code
├── ios/                        # iOS native code
├── package.json                # Dependencies
├── tsconfig.json               # TypeScript config
├── babel.config.js             # Babel config
└── README.md                   # This file
```

## 🚀 Getting Started

### Prerequisites

- Node.js 18+ (recommended 20.x)
- npm or yarn
- React Native development environment
  - For iOS: Xcode 14+ and CocoaPods
  - For Android: Android Studio and JDK 11+

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/picthaisky/slip-verification-system.git
cd slip-verification-system/slip-verification-mobile
```

2. **Install dependencies**
```bash
npm install
# or
yarn install
```

3. **Install iOS dependencies** (macOS only)
```bash
cd ios
pod install
cd ..
```

4. **Configure environment**

Create a `.env` file in the root directory:
```env
API_BASE_URL=http://localhost:5000/api/v1
WS_URL=http://localhost:5000
```

### Running the App

#### iOS
```bash
npm run ios
# or
npx react-native run-ios
```

#### Android
```bash
npm run android
# or
npx react-native run-android
```

#### Start Metro Bundler
```bash
npm start
# or
npx react-native start
```

## 📱 Screens

### Authentication
- **Login**: Email/password authentication
- **Register**: New user registration

### Main App
- **Home**: Dashboard with transaction summary and quick actions
- **Slip Upload**: Camera/gallery picker and upload flow
- **History**: Transaction list with filtering and search
- **Profile**: User settings, theme, language, logout

## 🔧 Configuration

### API Endpoints

Configure API endpoints in `src/utils/config.ts`:
```typescript
const config = {
  API_BASE_URL: 'http://localhost:5000/api/v1',
  WS_URL: 'http://localhost:5000',
  API_TIMEOUT: 30000,
};
```

### Theme

Customize themes in `src/theme/index.ts`:
```typescript
export const lightTheme = {
  ...MD3LightTheme,
  colors: {
    primary: '#6200EE',
    secondary: '#03DAC6',
    // ...
  },
};
```

### Localization

Add translations in `src/locales/`:
- `en.ts` - English translations
- `th.ts` - Thai translations

## 🧪 Testing

```bash
# Run tests
npm test

# Run with coverage
npm test -- --coverage

# Type checking
npm run type-check

# Linting
npm run lint
npm run lint:fix
```

## 📦 Building

### Android
```bash
# Generate APK
cd android
./gradlew assembleRelease

# Generate AAB (for Play Store)
./gradlew bundleRelease
```

### iOS
```bash
# Open Xcode
open ios/SlipVerificationMobile.xcworkspace

# Then build using Xcode or:
npx react-native run-ios --configuration Release
```

## 🔐 Security

- JWT tokens stored securely in AsyncStorage
- HTTPS communication with API
- Biometric authentication support
- Input validation and sanitization

## 🎨 UI/UX

- Material Design 3 (React Native Paper)
- Dark mode support
- Responsive layouts
- Smooth animations (React Native Reanimated)
- 60 FPS performance target

## 📊 Performance

- Optimized image compression
- Lazy loading of screens
- Memoization of components
- Efficient state management
- Background upload queue

## 🔄 Offline Support

- Queue slip uploads when offline
- Cache API responses
- Automatic sync when online
- Network status indicator

## 🔔 Push Notifications

- Firebase Cloud Messaging integration
- Local notifications
- Badge counter
- Deep linking support
- Notification history

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Run linter and tests
4. Submit pull request

## 📄 License

MIT License - See LICENSE file for details

## 📞 Support

For issues and questions:
- GitHub Issues: [Create issue](https://github.com/picthaisky/slip-verification-system/issues)
- Email: support@slipverification.com

---

**Version**: 1.0.0  
**Last Updated**: October 2024  
**Status**: Production Ready ✅
