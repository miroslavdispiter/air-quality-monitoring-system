using AirQualityInformationSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace AirQualityInformationSystem.Services
{
    public class PersistenceService : IPersistenceService
    {
        private readonly string _dataFilePath;

        public PersistenceService(string dataFilePath = "airquality_data.json")
        {
            _dataFilePath = dataFilePath;
        }

        public void SaveData(List<MonitoringStation> stations, List<AirQualityReading> readings)
        {
            try
            {
                var data = new
                {
                    Stations = stations,
                    Readings = readings
                };

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var jsonString = JsonSerializer.Serialize(data, options);
                File.WriteAllText(_dataFilePath, jsonString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save data: {ex.Message}", ex);
            }
        }

        public (List<MonitoringStation> Stations, List<AirQualityReading> Readings) LoadData()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return (new List<MonitoringStation>(), new List<AirQualityReading>());
                }

                var jsonString = File.ReadAllText(_dataFilePath);

                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return (new List<MonitoringStation>(), new List<AirQualityReading>());
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<DataContainer>(jsonString, options);

                return (data?.Stations ?? new List<MonitoringStation>(),
                        data?.Readings ?? new List<AirQualityReading>());
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load data: {ex.Message}", ex);
            }
        }

        private class DataContainer
        {
            public List<MonitoringStation> Stations { get; set; }
            public List<AirQualityReading> Readings { get; set; }
        }
    }
}