namespace MusicOM.Infrastructure.Logging
{
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 4
    }

    public interface ILogger
    {
        LogLevel MinimumLevel { get; set; }

        void Log(string message);
        void Log(string message, UnityEngine.Object context);

        void LogWarning(string message);
        void LogWarning(string message, UnityEngine.Object context);

        void LogError(string message);
        void LogError(string message, UnityEngine.Object context);

        void LogException(System.Exception exception);
        void LogException(System.Exception exception, UnityEngine.Object context);
    }
}
