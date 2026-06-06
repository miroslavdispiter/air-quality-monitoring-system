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

            return new List<AirQualityReading>
            {
                new AirQualityReading
                {
                    Id = Guid.NewGuid(),
                    StationId = stations[0].Id,
                    ReadingTime = DateTime.Now.AddHours(-2),
                    PM25 = 12.5,
                    NO2Level = 35.2,
                    OzoneLevel = 45.1
                },
                new AirQualityReading
                {
                    Id = Guid.NewGuid(),
                    StationId = stations[1].Id,
                    ReadingTime = DateTime.Now.AddHours(-1),
                    PM25 = 28.3,
                    NO2Level = 65.7,
                    OzoneLevel = 52.3
                },
                new AirQualityReading
                {
                    Id = Guid.NewGuid(),
                    StationId = stations[2].Id,
                    ReadingTime = DateTime.Now,
                    PM25 = 82.6,
                    NO2Level = 95.4,
                    OzoneLevel = 78.9
                }
            };
        }
    }
}