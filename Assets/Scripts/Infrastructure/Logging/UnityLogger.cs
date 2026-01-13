using System;
using UnityEngine;

namespace MusicOM.Infrastructure.Logging
{
    public class UnityLogger : ILogger
    {
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        public void Log(string message)
        {
            if (MinimumLevel <= LogLevel.Info)
            {
                Debug.Log(FormatMessage(message));
            }
        }

        public void Log(string message, UnityEngine.Object context)
        {
            if (MinimumLevel <= LogLevel.Info)
            {
                Debug.Log(FormatMessage(message), context);
            }
        }

        public void LogWarning(string message)
        {
            if (MinimumLevel <= LogLevel.Warning)
            {
                Debug.LogWarning(FormatMessage(message));
            }
        }

        public void LogWarning(string message, UnityEngine.Object context)
        {
            if (MinimumLevel <= LogLevel.Warning)
            {
                Debug.LogWarning(FormatMessage(message), context);
            }
        }

        public void LogError(string message)
        {
            if (MinimumLevel <= LogLevel.Error)
            {
                Debug.LogError(FormatMessage(message));
            }
        }

        public void LogError(string message, UnityEngine.Object context)
        {
            if (MinimumLevel <= LogLevel.Error)
            {
                Debug.LogError(FormatMessage(message), context);
            }
        }

        public void LogException(Exception exception)
        {
            if (MinimumLevel <= LogLevel.Error)
            {
                Debug.LogException(exception);
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            if (MinimumLevel <= LogLevel.Error)
            {
                Debug.LogException(exception, context);
            }
        }

        private string FormatMessage(string message)
        {
            return $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        }
    }
}
