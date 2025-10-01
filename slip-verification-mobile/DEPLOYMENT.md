# Deployment Guide

This guide covers deploying the Slip Verification Mobile App to production.

## 📋 Pre-Deployment Checklist

- [ ] Test app thoroughly on iOS and Android
- [ ] Configure production API endpoints
- [ ] Set up Firebase project for push notifications
- [ ] Configure app icons and splash screens
- [ ] Update version numbers
- [ ] Generate production certificates and provisioning profiles
- [ ] Test on physical devices
- [ ] Prepare app store assets (screenshots, descriptions)

## 🔧 Configuration

### 1. Environment Setup

Create production environment configuration:

```typescript
// src/utils/config.ts
const config = {
  API_BASE_URL: 'https://api.slipverification.com/api/v1',
  WS_URL: 'https://api.slipverification.com',
  API_TIMEOUT: 30000,
};
```

### 2. Update Version

Update version in `package.json`:
```json
{
  "version": "1.0.0"
}
```

Update version in `ios/SlipVerificationMobile/Info.plist`:
```xml
<key>CFBundleShortVersionString</key>
<string>1.0.0</string>
<key>CFBundleVersion</key>
<string>1</string>
```

Update version in `android/app/build.gradle`:
```gradle
versionCode 1
versionName "1.0.0"
```

## 🍎 iOS Deployment

### 1. Prerequisites

- Apple Developer Account ($99/year)
- Xcode 14+
- Valid certificates and provisioning profiles

### 2. Build for Release

```bash
# Clean build
cd ios
rm -rf build
xcodebuild clean

# Install pods
pod install

# Build archive
cd ..
npx react-native run-ios --configuration Release
```

### 3. Archive in Xcode

1. Open `ios/SlipVerificationMobile.xcworkspace` in Xcode
2. Select "Any iOS Device" as target
3. Product → Archive
4. Distribute App → App Store Connect
5. Upload to App Store Connect

### 4. App Store Connect

1. Create app in App Store Connect
2. Fill in app information:
   - Name: Slip Verification
   - Category: Finance
   - Privacy Policy URL
   - Support URL
3. Upload screenshots (required sizes)
4. Write app description
5. Submit for review

### Screenshots Required
- 6.7" Display (iPhone 15 Pro Max): 1290 x 2796 pixels
- 6.5" Display (iPhone 14 Pro Max): 1284 x 2778 pixels
- 5.5" Display (iPhone 8 Plus): 1242 x 2208 pixels

## 🤖 Android Deployment

### 1. Prerequisites

- Google Play Developer Account ($25 one-time)
- Android Studio
- Signing key for release builds

### 2. Generate Signing Key

```bash
cd android/app
keytool -genkeypair -v -storetype PKCS12 -keystore slip-verification-release.keystore -alias slip-verification -keyalg RSA -keysize 2048 -validity 10000
```

Save the keystore file and credentials securely.

### 3. Configure Signing

Edit `android/gradle.properties`:
```properties
MYAPP_UPLOAD_STORE_FILE=slip-verification-release.keystore
MYAPP_UPLOAD_KEY_ALIAS=slip-verification
MYAPP_UPLOAD_STORE_PASSWORD=***
MYAPP_UPLOAD_KEY_PASSWORD=***
```

Edit `android/app/build.gradle`:
```gradle
android {
    signingConfigs {
        release {
            if (project.hasProperty('MYAPP_UPLOAD_STORE_FILE')) {
                storeFile file(MYAPP_UPLOAD_STORE_FILE)
                storePassword MYAPP_UPLOAD_STORE_PASSWORD
                keyAlias MYAPP_UPLOAD_KEY_ALIAS
                keyPassword MYAPP_UPLOAD_KEY_PASSWORD
            }
        }
    }
    buildTypes {
        release {
            signingConfig signingConfigs.release
            minifyEnabled true
            proguardFiles getDefaultProguardFile('proguard-android.txt'), 'proguard-rules.pro'
        }
    }
}
```

### 4. Build Release APK/AAB

```bash
cd android

# For AAB (recommended for Play Store)
./gradlew bundleRelease

# For APK (for testing)
./gradlew assembleRelease
```

Output:
- AAB: `android/app/build/outputs/bundle/release/app-release.aab`
- APK: `android/app/build/outputs/apk/release/app-release.apk`

### 5. Google Play Console

1. Create app in Google Play Console
2. Fill in app details:
   - App name: Slip Verification
   - Category: Finance
   - Content rating
   - Privacy Policy
3. Upload AAB file
4. Create release notes
5. Submit for review

### Screenshots Required
- Phone: 1080 x 1920 pixels (minimum 2 screenshots)
- 7-inch Tablet: 1920 x 1200 pixels
- 10-inch Tablet: 2560 x 1536 pixels

## 🔔 Firebase Setup

### 1. Create Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Create new project
3. Add iOS app and Android app

### 2. iOS Configuration

1. Download `GoogleService-Info.plist`
2. Add to `ios/SlipVerificationMobile/` directory
3. Install pods: `cd ios && pod install`

### 3. Android Configuration

1. Download `google-services.json`
2. Add to `android/app/` directory
3. Sync Gradle

## 📊 Monitoring

### 1. Crashlytics

Enable Firebase Crashlytics for crash reporting:

```bash
npm install @react-native-firebase/crashlytics
```

### 2. Analytics

Enable Firebase Analytics:

```bash
npm install @react-native-firebase/analytics
```

## 🔄 Updates

### Over-the-Air Updates (Optional)

Consider using CodePush for instant updates:

```bash
npm install react-native-code-push
appcenter-cli login
```

## 🐛 Troubleshooting

### iOS Build Fails

```bash
cd ios
rm -rf Pods Podfile.lock
pod install
cd ..
```

### Android Build Fails

```bash
cd android
./gradlew clean
cd ..
```

### Missing Dependencies

```bash
npm install
cd ios && pod install && cd ..
```

## 📝 Release Checklist

### Before Release
- [ ] All features tested
- [ ] No console errors
- [ ] App icons added
- [ ] Splash screen configured
- [ ] Production API configured
- [ ] Push notifications tested
- [ ] Deep linking tested
- [ ] App signed with release certificates

### After Release
- [ ] Monitor crash reports
- [ ] Monitor user reviews
- [ ] Track analytics
- [ ] Prepare hotfix if needed
- [ ] Plan next version features

## 🚀 Continuous Deployment

### Fastlane Setup (Recommended)

Install Fastlane:
```bash
gem install fastlane
```

Initialize:
```bash
cd ios
fastlane init

cd ../android
fastlane init
```

Create lanes for automated builds and deployments.

## 📞 Support

For deployment issues:
- iOS: [Apple Developer Forums](https://developer.apple.com/forums/)
- Android: [Google Play Console Help](https://support.google.com/googleplay/android-developer)
- Firebase: [Firebase Support](https://firebase.google.com/support)

---

**Good luck with your deployment! 🚀**
