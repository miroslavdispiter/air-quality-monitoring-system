using System;
using System.Collections.Generic;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Helpers
{
    public static class DataSeeder
    {
        public static List<MonitoringStation> GetDefaultStations()
        {
            return new List<MonitoringStation>
            {
                new MonitoringStation
                {
                    Id = Guid.NewGuid(),
                    Name = "Central Station",
                    City = "Belgrade",
                    District = "Stari Grad",
                    Latitude = 44.8176,
                    Longitude = 20.4569
                },
                new MonitoringStation
                {
                    Id = Guid.NewGuid(),
                    Name = "North Monitoring Point",
                    City = "Belgrade",
                    District = "Novi Beograd",
                    Latitude = 44.8203,
                    Longitude = 20.4170
                },
                new MonitoringStation
                {
                    Id = Guid.NewGuid(),
                    Name = "South Station",
                    City = "Belgrade",
                    District = "Vozdovac",
                    Latitude = 44.7622,
                    Longitude = 20.4711
                }
            };
        }

        public static List<AirQualityReading> GetDefaultReadings(List<MonitoringStation> stations)
        {
            if (stations.Count < 3) return new List<AirQualityReading>();

            var readings = new List<AirQualityReading>();

            for (int month = 1; month <= 3; month++)
            {
                readings.Add(new AirQualityReading
                {
                    Id = Guid.NewGuid(),
                    StationId = stations[0].Id,
                    ReadingTime = new DateTime(2024, month, 15, 10, 0, 0),
                    PM25 = 12.5 + month * 5,
                    NO2Level = 35.2 + month * 3,
                    OzoneLevel = 45.1
                });

                readings.Add(new AirQualityReading
                {
                    Id = Guid.NewGuid(),
                    StationId = stations[1].Id,
                    ReadingTime = new DateTime(2024, month, 15, 11, 0, 0),
                    PM25 = 28.3 + month * 4,
                    NO2Level = 65.7 + month * 2,
                    OzoneLevel = 52.3
                });

                readings.Add(new AirQualityReading
                {
                    Id = Guid.NewGuid(),
                    StationId = stations[2].Id,
                    ReadingTime = new DateTime(2024, month, 15, 12, 0, 0),
                    PM25 = 82.6 - month * 2,
                    NO2Level = 95.4 - month,
                    OzoneLevel = 78.9
                });
            }

            return readings;
        }
    }
}