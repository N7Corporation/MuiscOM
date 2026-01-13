using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using MusicOM.Core;
using MusicOM.Infrastructure.Logging;

namespace MusicOM.Infrastructure.ErrorHandling
{
    public class RetryPolicy
    {
        private readonly int _maxAttempts;
        private readonly float _baseDelaySeconds;
        private readonly float _maxDelaySeconds;
        private readonly IAppLogger _logger;

        public RetryPolicy(int maxAttempts = 3, float baseDelaySeconds = 1f, float maxDelaySeconds = 30f)
        {
            _maxAttempts = maxAttempts;
            _baseDelaySeconds = baseDelaySeconds;
            _maxDelaySeconds = maxDelaySeconds;
            _logger = ServiceLocator.TryGet<IAppLogger>(out var logger) ? logger : null;
        }

        public async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> action, string operationName = "Operation")
        {
            int attempt = 0;
            Result<T> lastResult = Result<T>.Failure("No attempts made");

            while (attempt < _maxAttempts)
            {
                attempt++;

                try
                {
                    lastResult = await action();

                    if (lastResult.IsSuccess)
                    {
                        return lastResult;
                    }

                    _logger?.LogWarning($"[RetryPolicy] {operationName} attempt {attempt}/{_maxAttempts} failed: {lastResult.Error}");
                }
                catch (Exception ex)
                {
                    lastResult = Result<T>.Failure(ex.Message);
                    _logger?.LogError($"[RetryPolicy] {operationName} attempt {attempt}/{_maxAttempts} threw exception: {ex.Message}");
                }

                if (attempt < _maxAttempts)
                {
                    var delay = CalculateDelay(attempt);
                    _logger?.Log($"[RetryPolicy] Retrying in {delay:F1}s...");
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
            }

            _logger?.LogError($"[RetryPolicy] {operationName} failed after {_maxAttempts} attempts");
            return lastResult;
        }

        public IEnumerator ExecuteCoroutine<T>(
            Func<Action<Result<T>>, IEnumerator> action,
            Action<Result<T>> onComplete,
            string operationName = "Operation")
        {
            int attempt = 0;
            Result<T> lastResult = Result<T>.Failure("No attempts made");

            while (attempt < _maxAttempts)
            {
                attempt++;
                bool completed = false;

                yield return action((result) =>
                {
                    lastResult = result;
                    completed = true;
                });

                if (!completed)
                {
                    lastResult = Result<T>.Failure("Coroutine did not complete");
                }

                if (lastResult.IsSuccess)
                {
                    onComplete(lastResult);
                    yield break;
                }

                _logger?.LogWarning($"[RetryPolicy] {operationName} attempt {attempt}/{_maxAttempts} failed: {lastResult.Error}");

                if (attempt < _maxAttempts)
                {
                    var delay = CalculateDelay(attempt);
                    _logger?.Log($"[RetryPolicy] Retrying in {delay:F1}s...");
                    yield return new WaitForSeconds(delay);
                }
            }

            _logger?.LogError($"[RetryPolicy] {operationName} failed after {_maxAttempts} attempts");
            onComplete(lastResult);
        }

        private float CalculateDelay(int attempt)
        {
            var delay = _baseDelaySeconds * Mathf.Pow(2, attempt - 1);
            var jitter = UnityEngine.Random.Range(0f, delay * 0.1f);
            return Mathf.Min(delay + jitter, _maxDelaySeconds);
        }
    }
}
