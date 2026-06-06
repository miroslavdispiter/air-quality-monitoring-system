using System;
using System.IO;
using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Services.Logging
{
    public class LoggerService : ILoggerService
    {
        private readonly string filePath = "log.txt";

        public void Log(string message)
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            File.AppendAllText(filePath, logMessage + Environment.NewLine);
        }
    }
}