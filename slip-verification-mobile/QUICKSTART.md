# Quick Start Guide

## 🚀 Quick Setup (5 minutes)

### 1. Install Dependencies
```bash
cd slip-verification-mobile
npm install
```

### 2. iOS Setup (Mac only)
```bash
cd ios
pod install
cd ..
```

### 3. Run the App
```bash
# iOS
npm run ios

# Android
npm run android
```

## 📱 Test Login Credentials

For testing purposes, you can use these mock credentials:
- Email: `test@example.com`
- Password: `password123`

## 🎨 Key Features to Test

### 1. Authentication
- Login with email/password
- Register new account
- Remember user session

### 2. Home Dashboard
- View transaction summary
- Check recent activities
- Quick actions (Upload Slip, View History)

### 3. Slip Upload
- **Camera**: Tap "Take Photo" to use camera
- **Gallery**: Tap "Choose from Gallery" to select existing photo
- Enter Order ID
- Upload and view verification result

### 4. History
- View all transactions
- Filter by status (Pending, Verified, Rejected)
- Search by order ID or reference number

### 5. Profile
- View user information
- Toggle dark mode
- Change language (Thai/English)
- Logout

## 🔧 Common Commands

```bash
# Start Metro bundler
npm start

# Clear cache
npm start -- --reset-cache

# Type check
npm run type-check

# Lint
npm run lint

# Fix linting issues
npm run lint:fix
```

## 🎯 Project Structure Quick Reference

```
src/
├── api/                    # API client and endpoints
├── screens/                # All screen components
│   ├── Auth/              # Login, Register
│   ├── Home/              # Dashboard
│   ├── SlipUpload/        # Upload flow
│   ├── History/           # Transaction list
│   └── Profile/           # Settings
├── navigation/            # Navigation config
├── store/                 # Redux state
├── services/              # Services (storage, websocket, etc)
├── theme/                 # UI theme config
└── locales/               # Translations (TH/EN)
```

## 📝 Adding a New Screen

1. Create screen file in `src/screens/YourScreen/YourScreen.tsx`
2. Add route to navigator in `src/navigation/MainNavigator.tsx`
3. Update types if needed

## 🎨 Customizing Theme

Edit `src/theme/index.ts`:
```typescript
export const lightTheme = {
  colors: {
    primary: '#6200EE',  // Change this
    // ...
  },
};
```

## 🌍 Adding Translations

1. Add to `src/locales/en.ts`
2. Add to `src/locales/th.ts`
3. Use in components: `t('your.key')`

## 🔌 API Configuration

Edit `src/utils/config.ts`:
```typescript
const config = {
  API_BASE_URL: 'http://localhost:5000/api/v1',
  WS_URL: 'http://localhost:5000',
};
```

## 🐛 Troubleshooting

### Metro bundler not starting
```bash
npm start -- --reset-cache
```

### iOS build fails
```bash
cd ios
pod deintegrate
pod install
cd ..
```

### Android build fails
```bash
cd android
./gradlew clean
cd ..
```

### Type errors
```bash
npm run type-check
```

## 📚 Learn More

- [React Native Docs](https://reactnative.dev/docs/getting-started)
- [React Navigation](https://reactnavigation.org/)
- [React Native Paper](https://callstack.github.io/react-native-paper/)
- [Redux Toolkit](https://redux-toolkit.js.org/)

## 🎉 Next Steps

1. ✅ Run the app
2. ✅ Test all features
3. ✅ Customize theme and colors
4. ✅ Add your API endpoint
5. ✅ Test with real backend
6. ✅ Deploy to TestFlight (iOS) or Play Store (Android)

---

**Happy Coding! 🚀**
