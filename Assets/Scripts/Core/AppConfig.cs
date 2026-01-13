using UnityEngine;

namespace MusicOM.Core
{
    [CreateAssetMenu(fileName = "AppConfig", menuName = "MusicOM/Config/App Config")]
    public class AppConfig : ScriptableObject
    {
        [Header("Debug")]
        [Tooltip("Enable debug mode for verbose logging and debug UI")]
        public bool debugMode = false;

        [Header("Performance")]
        [Tooltip("Target frame rate for Quest 3")]
        [Range(72, 120)]
        public int targetFrameRate = 90;

        [Tooltip("Minimum acceptable frame rate before quality reduction")]
        [Range(60, 90)]
        public int minFrameRate = 72;

        [Header("Audio")]
        [Tooltip("Maximum audio latency in milliseconds")]
        [Range(20, 100)]
        public int maxAudioLatencyMs = 50;

        [Tooltip("Audio buffer size in seconds")]
        [Range(1f, 5f)]
        public float audioBufferSeconds = 2f;

        [Header("Network")]
        [Tooltip("HTTP request timeout in seconds")]
        [Range(5, 30)]
        public int requestTimeoutSeconds = 10;

        [Tooltip("Maximum retry attempts for failed requests")]
        [Range(1, 5)]
        public int maxRetryAttempts = 3;

        [Header("Cache")]
        [Tooltip("Search results cache duration in seconds")]
        public int searchCacheDurationSeconds = 300;

        [Tooltip("Playlist cache duration in seconds")]
        public int playlistCacheDurationSeconds = 900;

        [Tooltip("User library cache duration in seconds")]
        public int libraryCacheDurationSeconds = 1800;

        [Tooltip("Maximum album art cache size in MB")]
        public int albumArtCacheSizeMB = 50;

        [Header("VR Settings")]
        [Tooltip("Enable hand tracking input")]
        public bool handTrackingEnabled = true;

        [Tooltip("Enable passthrough/AR mode")]
        public bool passthroughEnabled = true;

        [Tooltip("Enable spatial audio")]
        public bool spatialAudioEnabled = true;
    }
}
