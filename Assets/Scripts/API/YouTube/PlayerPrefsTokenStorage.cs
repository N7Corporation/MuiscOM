using System;
using System.Threading.Tasks;
using UnityEngine;
using MusicOM.Core;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.API.YouTube
{
    public class PlayerPrefsTokenStorage : ITokenStorage
    {
        private const string TokenKey = "MusicOM_AuthToken";
        private readonly ILogger _logger;
        private AuthToken _cachedToken;

        public bool HasToken => !string.IsNullOrEmpty(PlayerPrefs.GetString(TokenKey, ""));

        public PlayerPrefsTokenStorage()
        {
            _logger = ServiceLocator.TryGet<ILogger>(out var logger) ? logger : null;
        }

        public Task<AuthToken> LoadTokenAsync()
        {
            if (_cachedToken != null)
            {
                return Task.FromResult(_cachedToken);
            }

            var json = PlayerPrefs.GetString(TokenKey, "");
            if (string.IsNullOrEmpty(json))
            {
                _logger?.Log("[TokenStorage] No stored token found");
                return Task.FromResult<AuthToken>(null);
            }

            try
            {
                _cachedToken = JsonUtility.FromJson<AuthToken>(json);
                _logger?.Log("[TokenStorage] Token loaded successfully");
                return Task.FromResult(_cachedToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[TokenStorage] Failed to load token: {ex.Message}");
                return Task.FromResult<AuthToken>(null);
            }
        }

        public Task SaveTokenAsync(AuthToken token)
        {
            if (token == null)
            {
                _logger?.LogWarning("[TokenStorage] Attempted to save null token");
                return Task.CompletedTask;
            }

            try
            {
                var json = JsonUtility.ToJson(token);
                PlayerPrefs.SetString(TokenKey, json);
                PlayerPrefs.Save();
                _cachedToken = token;
                _logger?.Log("[TokenStorage] Token saved successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError($"[TokenStorage] Failed to save token: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public Task DeleteTokenAsync()
        {
            PlayerPrefs.DeleteKey(TokenKey);
            PlayerPrefs.Save();
            _cachedToken = null;
            _logger?.Log("[TokenStorage] Token deleted");
            return Task.CompletedTask;
        }
    }
}
