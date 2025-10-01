# iOS Configuration

This directory contains iOS-specific native code and configuration.

## Setup

To initialize the iOS project, you would typically run:

```bash
npx react-native init SlipVerificationMobile
cd ios
pod install
```

This would generate the full iOS project structure including:

```
ios/
├── SlipVerificationMobile/
│   ├── AppDelegate.mm
│   ├── Info.plist
│   ├── LaunchScreen.storyboard
│   └── Images.xcassets/
├── SlipVerificationMobile.xcodeproj/
├── SlipVerificationMobile.xcworkspace/
├── Podfile
└── Pods/
```

## Required Configuration

### 1. Permissions (Info.plist)

```xml
<key>NSCameraUsageDescription</key>
<string>We need access to your camera to take photos of payment slips</string>
<key>NSPhotoLibraryUsageDescription</key>
<string>We need access to your photo library to upload payment slips</string>
<key>NSFaceIDUsageDescription</key>
<string>We use Face ID for secure authentication</string>
```

### 2. Firebase Configuration

Add `GoogleService-Info.plist` to the iOS project in Xcode.

### 3. CocoaPods Dependencies

In `Podfile`:

```ruby
platform :ios, '13.0'

target 'SlipVerificationMobile' do
  config = use_native_modules!
  use_react_native!(:path => config[:reactNativePath])
  
  # Required pods
  pod 'RNVectorIcons', :path => '../node_modules/react-native-vector-icons'
  pod 'react-native-image-picker', :path => '../node_modules/react-native-image-picker'
end
```

### 4. Capabilities

Enable in Xcode:
- Push Notifications
- Face ID / Touch ID
- Camera
- Photo Library

## Building

### Debug
```bash
npx react-native run-ios
```

### Release
```bash
npx react-native run-ios --configuration Release
```

Or build in Xcode:
```bash
open ios/SlipVerificationMobile.xcworkspace
```

## Requirements

- Xcode 14+
- CocoaPods 1.11+
- iOS 13.0+

## Note

For a complete React Native project initialization, use the React Native CLI:

```bash
npx @react-native-community/cli init SlipVerificationMobile --template react-native-template-typescript
```

Then copy the source files from `src/` into the new project.
