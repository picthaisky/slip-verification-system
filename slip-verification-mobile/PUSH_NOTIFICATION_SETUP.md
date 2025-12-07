# Firebase Push Notification Setup Guide

## Android Setup

1. **Create Firebase Project**
   - Go to [Firebase Console](https://console.firebase.google.com/)
   - Create a new project or select existing one
   - Add Android app with package name: `com.slipverification`

2. **Download google-services.json**
   - Download the `google-services.json` file from Firebase Console
   - Replace `android/app/google-services.json.example` with your file
   - Rename to `google-services.json`

3. **Add Firebase Plugin** (already configured in build.gradle)
   ```gradle
   // In android/build.gradle
   classpath("com.google.gms:google-services:4.4.0")
   
   // In android/app/build.gradle (add at bottom)
   apply plugin: 'com.google.gms.google-services'
   ```

4. **Initialize in App**
   ```javascript
   // In your App.js or index.js
   import messaging from '@react-native-firebase/messaging';
   
   messaging().getToken().then(token => {
     console.log('FCM Token:', token);
   });
   ```

---

## iOS Setup

1. **Enable Push Notifications in Xcode**
   - Open `ios/SlipVerificationMobile.xcworkspace` in Xcode
   - Select project > Signing & Capabilities
   - Add "Push Notifications" capability

2. **Configure APNs Key**
   - Go to Apple Developer > Certificates, IDs & Profiles
   - Create APNs Key
   - Upload to Firebase Console > Project Settings > Cloud Messaging

3. **Add Firebase iOS SDK**
   ```ruby
   # In ios/Podfile (add inside target)
   pod 'Firebase/Messaging'
   ```
   Then run: `cd ios && pod install`

4. **Update AppDelegate**
   ```objc
   // In AppDelegate.mm
   #import <Firebase.h>
   #import <UserNotifications/UserNotifications.h>
   
   - (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions {
     [FIRApp configure];
     // ... rest of code
   }
   ```

---

## Testing

```bash
# Android
cd android && ./gradlew assembleDebug

# iOS (macOS only)
cd ios && pod install
npx react-native run-ios
```
