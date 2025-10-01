# Android Configuration

This directory contains Android-specific native code and configuration.

## Setup

To initialize the Android project, you would typically run:

```bash
npx react-native init SlipVerificationMobile
```

This would generate the full Android project structure including:

```
android/
├── app/
│   ├── src/
│   │   └── main/
│   │       ├── java/
│   │       ├── res/
│   │       └── AndroidManifest.xml
│   └── build.gradle
├── build.gradle
├── gradle.properties
└── settings.gradle
```

## Required Configuration

### 1. Permissions (AndroidManifest.xml)

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.USE_FINGERPRINT" />
<uses-permission android:name="android.permission.USE_BIOMETRIC" />
```

### 2. Firebase Configuration

Add `google-services.json` to `android/app/` directory.

### 3. Build Configuration

In `android/app/build.gradle`:

```gradle
android {
    compileSdkVersion 34
    defaultConfig {
        applicationId "com.slipverificationmobile"
        minSdkVersion 21
        targetSdkVersion 34
        versionCode 1
        versionName "1.0.0"
    }
}
```

## Building

```bash
cd android
./gradlew assembleRelease
```

## Note

For a complete React Native project initialization, use the React Native CLI:

```bash
npx @react-native-community/cli init SlipVerificationMobile --template react-native-template-typescript
```

Then copy the source files from `src/` into the new project.
