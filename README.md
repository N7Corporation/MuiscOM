# MusicOM

A VR music streaming interface for Meta Quest 3, providing an immersive YouTube Music experience in virtual reality.

## Overview

MusicOM is a Unity-based application that brings YouTube Music to the Quest 3 headset through a spatial, intuitive VR interface. Users can browse, search, and play music from YouTube Music while immersed in customizable virtual environments.

## Features (Planned)

- **Spatial Music Player**: 3D audio visualization and playback controls
- **Voice Search**: Hands-free music search using Quest 3's voice input
- **Hand Tracking**: Natural gesture-based navigation and controls
- **Customizable Environments**: Multiple virtual spaces for listening
- **Playlist Management**: Create and manage playlists in VR
- **Music Discovery**: Browse recommendations, charts, and new releases

## Requirements

- Meta Quest 3 headset
- Unity 2022.3 LTS or newer
- Meta XR SDK
- YouTube Music API access

## Getting Started

### Prerequisites

1. Install Unity Hub and Unity 2022.3 LTS
2. Install Meta Quest Developer Hub
3. Enable Developer Mode on your Quest 3
4. Obtain YouTube Data API credentials from Google Cloud Console

### Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/MusicOM.git
   ```

2. Open the project in Unity

3. Import Meta XR SDK packages (if not already present)

4. Configure API credentials:
   - Create `Assets/Resources/Config/api_config.json`
   - Add your YouTube Data API key

5. Connect Quest 3 via USB or Wi-Fi

6. Build and Run to device

## Project Structure

```
MusicOM/
├── Assets/
│   ├── Oculus/          # Meta XR SDK assets
│   ├── XR/              # XR configuration
│   ├── Scripts/         # C# application code (to be created)
│   ├── Prefabs/         # Reusable VR UI components (to be created)
│   ├── Scenes/          # Unity scenes (to be created)
│   └── Resources/       # Runtime assets and config
├── Packages/            # Unity package dependencies
└── ProjectSettings/     # Unity project configuration
```

## Documentation

- [Project Roadmap](ROADMAP.md) - Development phases and milestones
- [Requirements](REQUIREMENTS.md) - Technical and functional specifications
- [Code of Conduct](CODE_OF_CONDUCT.md) - Community guidelines

## Contributing

We welcome contributions! Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the n7co Open License v1.0 - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Meta Quest platform and XR SDK
- YouTube Data API
- Unity Technologies

## Disclaimer

This project is not affiliated with, endorsed by, or sponsored by Google, YouTube, or Meta. YouTube Music is a trademark of Google LLC. Meta Quest is a trademark of Meta Platforms, Inc.
