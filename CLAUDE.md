# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MusicOM is a VR music streaming interface for Meta Quest 3, built in Unity 6000.3.2f1, providing YouTube Music playback with spatial audio, hand tracking, and AR passthrough support.

**Status**: Core infrastructure and VR environment foundation complete.

## Build & Development Commands

**Unity Editor Build**:
- Open project in Unity 2022.3+ / Unity 6
- Build target: Android (Quest 3)
- File → Build and Run (or Ctrl+B)

**Command-line build** (if Unity installed):
```bash
Unity -batchmode -projectPath . -buildTarget Android -executeMethod BuildScript.Build -quit
```

**Run tests**:
```bash
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults results.xml
```

**Deploy to Quest 3**:
- Connect via USB or enable wireless ADB
- Use Meta Quest Developer Hub or `adb install` for APK

## Architecture

### Unity Package Dependencies
- **Meta XR SDK v83.0.1**: Core VR functionality, spatial audio, haptics, hand tracking, voice input
- **Universal Render Pipeline 17.3.0**: Graphics pipeline
- **Input System 1.17.0**: Hand tracking and controller input
- **OpenXR 1.16.1**: XR runtime

### Code Structure
```
Assets/
├── Scripts/
│   ├── API/               # YouTube API client, auth
│   │   └── YouTube/       # YouTubeAuthManager, YouTubeConfig
│   ├── Audio/             # AudioManager (spatial audio, playback)
│   ├── Input/             # InputManager, HandTrackingController, HapticManager
│   ├── Infrastructure/    # Logging (IAppLogger), ErrorHandling (Result, RetryPolicy)
│   └── Core/              # AppConfig, AppInitializer, ServiceLocator, SceneController, XRSetup
├── Haptics/               # Custom .haptic files (ERROR, minor action, major action)
├── Prefabs/               # VR UI components
├── Scenes/                # Unity scenes
└── Resources/Config/      # ScriptableObject configs (git-ignored)
```

### Key Configuration Files
- `Assets/Plugins/Android/AndroidManifest.xml` - Quest 3 VR setup, permissions
- `Assets/XR/Settings/` - OpenXR loader and settings
- `Assets/Resources/OculusRuntimeSettings.asset` - Meta SDK runtime config
- `Packages/manifest.json` - Unity package dependencies

## Platform Targets
- **Primary**: Quest 3, Quest 3S
- **Supported**: Quest 2, Quest Pro
- **Minimum Horizon OS**: v60
- **Android API**: 32+ (Android 12L)

## Performance Constraints
- Target frame rate: 90Hz (72Hz minimum)
- UI response latency: <100ms
- Audio latency: <50ms
- Memory budget: <2GB RAM
- Thermal-aware: avoid sustained high GPU load

## API Integration Notes

**YouTube Data API v3**:
- OAuth 2.0 with PKCE for authentication
- Quota: 10,000 units/day (search costs 100 units)
- Implement aggressive caching (album art 50MB LRU, search results 5min, playlists 15min)
- Store tokens in Android Keystore (encrypted)

**Known limitation**: YouTube API provides metadata only, not direct audio streams. Alternative streaming approach required.

## Custom Haptics

**IMPORTANT**: Use the custom haptic clips in `Assets/Haptics/` for feedback:
- `ERROR.haptic` - Error/failure feedback
- `minor action.haptic` - Light interactions (hover, scroll, minor UI)
- `major action.haptic` - Significant actions (select, confirm, play/pause)

Access via `HapticManager`:
```csharp
var haptics = ServiceLocator.Get<HapticManager>();
haptics.PlayMinorAction(HandSide.Right);  // UI interactions
haptics.PlayMajorAction(Controller.Both); // Confirmations
haptics.PlayError(HandSide.Left);         // Errors
```

## C# Conventions
- Target: .NET Standard 2.1, C# 9.0
- Suppress warnings: 0169 (unused fields), USG0001
- Use MonoBehaviour patterns for Unity components
- Separate API clients from UI logic
