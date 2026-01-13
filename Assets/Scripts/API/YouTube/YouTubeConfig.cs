using UnityEngine;

namespace MusicOM.API.YouTube
{
    [CreateAssetMenu(fileName = "YouTubeConfig", menuName = "MusicOM/Config/YouTube Config")]
    public class YouTubeConfig : ScriptableObject
    {
        [Header("API Credentials")]
        [Tooltip("Google Cloud Console API Key")]
        public string apiKey = "";

        [Tooltip("OAuth 2.0 Client ID")]
        public string clientId = "";

        [Tooltip("OAuth 2.0 Client Secret (keep empty for PKCE flow)")]
        public string clientSecret = "";

        [Header("API Endpoints")]
        [Tooltip("YouTube Data API base URL")]
        public string apiBaseUrl = "https://www.googleapis.com/youtube/v3";

        [Tooltip("OAuth authorization endpoint")]
        public string authEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";

        [Tooltip("OAuth token endpoint")]
        public string tokenEndpoint = "https://oauth2.googleapis.com/token";

        [Header("OAuth Settings")]
        [Tooltip("OAuth redirect URI for mobile app")]
        public string redirectUri = "com.musicom.app:/oauth2redirect";

        [Tooltip("OAuth scopes required")]
        public string[] scopes = new[]
        {
            "https://www.googleapis.com/auth/youtube.readonly"
        };

        [Header("Quota Management")]
        [Tooltip("Daily quota limit (default: 10000)")]
        public int dailyQuotaLimit = 10000;

        [Tooltip("Units cost for search request")]
        public int searchCost = 100;

        [Tooltip("Units cost for list request")]
        public int listCost = 1;

        public string ScopesString => string.Join(" ", scopes);

        public bool HasCredentials => !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(clientId);
    }
}
