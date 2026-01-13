using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using MusicOM.Core;
using MusicOM.Infrastructure.ErrorHandling;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.API.YouTube
{
    public class YouTubeAuthManager : MonoBehaviour
    {
        [SerializeField] private YouTubeConfig _config;

        private ITokenStorage _tokenStorage;
        private IAppLogger _logger;
        private ApiClient _apiClient;
        private string _codeVerifier;
        private string _state;

        public event Action OnAuthenticationRequired;
        public event Action OnAuthenticationSuccess;
        public event Action<string> OnAuthenticationFailed;

        public bool IsAuthenticated => _tokenStorage?.HasToken ?? false;

        private void Start()
        {
            _logger = ServiceLocator.Get<IAppLogger>();
            _tokenStorage = new PlayerPrefsTokenStorage();
            _apiClient = new ApiClient();

            if (_config == null)
            {
                _config = Resources.Load<YouTubeConfig>("Config/YouTubeConfig");
            }

            ServiceLocator.Register(this);
            _logger?.Log("[YouTubeAuth] Initialized");
        }

        public string GetAuthorizationUrl()
        {
            if (_config == null || !_config.HasCredentials)
            {
                _logger?.LogError("[YouTubeAuth] Missing API credentials in YouTubeConfig");
                return null;
            }

            _codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(_codeVerifier);
            _state = GenerateState();

            var authUrl = $"{_config.authEndpoint}?" +
                $"client_id={Uri.EscapeDataString(_config.clientId)}&" +
                $"redirect_uri={Uri.EscapeDataString(_config.redirectUri)}&" +
                $"response_type=code&" +
                $"scope={Uri.EscapeDataString(_config.ScopesString)}&" +
                $"code_challenge={codeChallenge}&" +
                $"code_challenge_method=S256&" +
                $"state={_state}&" +
                $"access_type=offline&" +
                $"prompt=consent";

            _logger?.Log("[YouTubeAuth] Authorization URL generated");
            return authUrl;
        }

        public void HandleAuthCallback(string callbackUrl)
        {
            _logger?.Log("[YouTubeAuth] Processing auth callback");

            var uri = new Uri(callbackUrl);
            var queryParams = ParseQueryString(uri.Query);

            queryParams.TryGetValue("state", out var state);
            queryParams.TryGetValue("code", out var code);
            queryParams.TryGetValue("error", out var error);

            if (!string.IsNullOrEmpty(error))
            {
                _logger?.LogError($"[YouTubeAuth] Auth error: {error}");
                OnAuthenticationFailed?.Invoke(error);
                return;
            }

            if (state != _state)
            {
                _logger?.LogError("[YouTubeAuth] State mismatch - possible CSRF attack");
                OnAuthenticationFailed?.Invoke("Security validation failed");
                return;
            }

            if (string.IsNullOrEmpty(code))
            {
                _logger?.LogError("[YouTubeAuth] No authorization code received");
                OnAuthenticationFailed?.Invoke("No authorization code");
                return;
            }

            StartCoroutine(ExchangeCodeForToken(code));
        }

        private IEnumerator ExchangeCodeForToken(string code)
        {
            _logger?.Log("[YouTubeAuth] Exchanging code for token");

            var formData = new System.Collections.Generic.Dictionary<string, string>
            {
                { "client_id", _config.clientId },
                { "code", code },
                { "code_verifier", _codeVerifier },
                { "grant_type", "authorization_code" },
                { "redirect_uri", _config.redirectUri }
            };

            yield return _apiClient.PostFormAsync<TokenResponse>(
                _config.tokenEndpoint,
                formData,
                (result) =>
                {
                    result.Match(
                        onSuccess: async (response) =>
                        {
                            var token = new AuthToken
                            {
                                accessToken = response.access_token,
                                refreshToken = response.refresh_token,
                                tokenType = response.token_type,
                                expiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + response.expires_in
                            };

                            await _tokenStorage.SaveTokenAsync(token);
                            _logger?.Log("[YouTubeAuth] Authentication successful");
                            OnAuthenticationSuccess?.Invoke();
                        },
                        onFailure: (error) =>
                        {
                            _logger?.LogError($"[YouTubeAuth] Token exchange failed: {error}");
                            OnAuthenticationFailed?.Invoke(error);
                        }
                    );
                }
            );
        }

        public IEnumerator RefreshTokenIfNeeded(Action<Result<string>> callback)
        {
            var token = _tokenStorage.LoadTokenAsync().Result;

            if (token == null)
            {
                OnAuthenticationRequired?.Invoke();
                callback(Result<string>.Failure("Not authenticated"));
                yield break;
            }

            if (!token.IsExpired)
            {
                callback(Result<string>.Success(token.accessToken));
                yield break;
            }

            _logger?.Log("[YouTubeAuth] Refreshing expired token");

            var formData = new System.Collections.Generic.Dictionary<string, string>
            {
                { "client_id", _config.clientId },
                { "refresh_token", token.refreshToken },
                { "grant_type", "refresh_token" }
            };

            yield return _apiClient.PostFormAsync<TokenResponse>(
                _config.tokenEndpoint,
                formData,
                (result) =>
                {
                    result.Match(
                        onSuccess: async (response) =>
                        {
                            token.accessToken = response.access_token;
                            token.expiresAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + response.expires_in;

                            if (!string.IsNullOrEmpty(response.refresh_token))
                            {
                                token.refreshToken = response.refresh_token;
                            }

                            await _tokenStorage.SaveTokenAsync(token);
                            _logger?.Log("[YouTubeAuth] Token refreshed successfully");
                            callback(Result<string>.Success(token.accessToken));
                        },
                        onFailure: (error) =>
                        {
                            _logger?.LogError($"[YouTubeAuth] Token refresh failed: {error}");
                            OnAuthenticationRequired?.Invoke();
                            callback(Result<string>.Failure(error));
                        }
                    );
                }
            );
        }

        public async void Logout()
        {
            await _tokenStorage.DeleteTokenAsync();
            _logger?.Log("[YouTubeAuth] User logged out");
        }

        private string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        private string GenerateCodeChallenge(string verifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.ASCII.GetBytes(verifier);
                var hash = sha256.ComputeHash(bytes);
                return Base64UrlEncode(hash);
            }
        }

        private string GenerateState()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlEncode(bytes);
        }

        private string Base64UrlEncode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private System.Collections.Generic.Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new System.Collections.Generic.Dictionary<string, string>();
            if (string.IsNullOrEmpty(query)) return result;

            query = query.TrimStart('?');
            var pairs = query.Split('&');

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = Uri.UnescapeDataString(keyValue[1]);
                    result[key] = value;
                }
            }

            return result;
        }

        [Serializable]
        private class TokenResponse
        {
            public string access_token;
            public string refresh_token;
            public string token_type;
            public int expires_in;
        }
    }
}
