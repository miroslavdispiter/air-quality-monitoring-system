using System;
using System.IO;

namespace AirQualityInformationSystem.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public LoggingService(string logFilePath = "activity_log.txt")
        {
            _logFilePath = logFilePath;
        }

        public void LogActivity(string message)
        {
            lock (_lockObject)
            {
                try
                {
                    var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
                }
            }
        }
    }
}