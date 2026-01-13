using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using MusicOM.Core;
using MusicOM.Infrastructure.ErrorHandling;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.API
{
    public class ApiClient
    {
        private readonly int _timeoutSeconds;
        private readonly IAppLogger _logger;
        private readonly Dictionary<string, string> _defaultHeaders = new();

        public ApiClient(int timeoutSeconds = 10)
        {
            _timeoutSeconds = timeoutSeconds;
            _logger = ServiceLocator.TryGet<IAppLogger>(out var logger) ? logger : null;
        }

        public void SetDefaultHeader(string key, string value)
        {
            _defaultHeaders[key] = value;
        }

        public void SetAuthorizationHeader(string token)
        {
            _defaultHeaders["Authorization"] = $"Bearer {token}";
        }

        public void ClearAuthorizationHeader()
        {
            _defaultHeaders.Remove("Authorization");
        }

        public IEnumerator GetAsync<T>(string url, Action<Result<T>> callback, Dictionary<string, string> headers = null)
        {
            yield return SendRequest(url, "GET", null, headers, callback);
        }

        public IEnumerator PostAsync<T>(string url, object body, Action<Result<T>> callback, Dictionary<string, string> headers = null)
        {
            var jsonBody = body != null ? JsonUtility.ToJson(body) : null;
            yield return SendRequest(url, "POST", jsonBody, headers, callback);
        }

        public IEnumerator PostFormAsync<T>(string url, Dictionary<string, string> formData, Action<Result<T>> callback, Dictionary<string, string> headers = null)
        {
            yield return SendFormRequest(url, formData, headers, callback);
        }

        private IEnumerator SendRequest<T>(string url, string method, string body, Dictionary<string, string> headers, Action<Result<T>> callback)
        {
            _logger?.Log($"[ApiClient] {method} {url}");

            using var request = new UnityWebRequest(url, method);
            request.timeout = _timeoutSeconds;
            request.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(body))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.SetRequestHeader("Content-Type", "application/json");
            }

            ApplyHeaders(request, headers);

            yield return request.SendWebRequest();

            HandleResponse(request, callback);
        }

        private IEnumerator SendFormRequest<T>(string url, Dictionary<string, string> formData, Dictionary<string, string> headers, Action<Result<T>> callback)
        {
            _logger?.Log($"[ApiClient] POST (form) {url}");

            var form = new WWWForm();
            foreach (var kvp in formData)
            {
                form.AddField(kvp.Key, kvp.Value);
            }

            using var request = UnityWebRequest.Post(url, form);
            request.timeout = _timeoutSeconds;

            ApplyHeaders(request, headers);

            yield return request.SendWebRequest();

            HandleResponse(request, callback);
        }

        private void ApplyHeaders(UnityWebRequest request, Dictionary<string, string> headers)
        {
            foreach (var header in _defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

        private void HandleResponse<T>(UnityWebRequest request, Action<Result<T>> callback)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseText = request.downloadHandler.text;
                _logger?.Log($"[ApiClient] Response: {request.responseCode}");

                try
                {
                    var data = JsonUtility.FromJson<T>(responseText);
                    callback(Result<T>.Success(data));
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"[ApiClient] JSON parse error: {ex.Message}");
                    callback(Result<T>.Failure($"Failed to parse response: {ex.Message}"));
                }
            }
            else
            {
                var error = $"{request.result}: {request.error} (HTTP {request.responseCode})";
                _logger?.LogError($"[ApiClient] Request failed: {error}");
                callback(Result<T>.Failure(error));
            }
        }

        public IEnumerator GetRawAsync(string url, Action<Result<string>> callback, Dictionary<string, string> headers = null)
        {
            _logger?.Log($"[ApiClient] GET (raw) {url}");

            using var request = UnityWebRequest.Get(url);
            request.timeout = _timeoutSeconds;

            ApplyHeaders(request, headers);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                callback(Result<string>.Success(request.downloadHandler.text));
            }
            else
            {
                var error = $"{request.result}: {request.error} (HTTP {request.responseCode})";
                callback(Result<string>.Failure(error));
            }
        }
    }
}
