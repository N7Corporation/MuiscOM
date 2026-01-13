# Config Setup

The config ScriptableObjects in this folder are git-ignored because they contain API keys.

## Setup Instructions

1. In Unity, right-click in this folder → Create → MusicOM → Config → **App Config**
2. Right-click again → Create → MusicOM → Config → **YouTube Config**

## YouTube Config Fields

| Field | Description |
|-------|-------------|
| `apiKey` | Your YouTube Data API v3 key from Google Cloud Console |
| `clientId` | OAuth 2.0 Client ID (Android type) |
| `clientSecret` | Leave empty (not needed for Android/PKCE) |

## Getting Credentials

1. Go to https://console.cloud.google.com/
2. Create a project and enable "YouTube Data API v3"
3. Create an API Key (Credentials → Create Credentials → API Key)
4. Create OAuth Client ID (Credentials → Create Credentials → OAuth client ID)
   - Type: Android
   - Package name: `com.yourcompany.musicom`
   - SHA-1: Run keytool command (see main README)
