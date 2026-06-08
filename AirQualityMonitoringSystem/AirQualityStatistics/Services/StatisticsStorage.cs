using System;
using System.Collections.Generic;
using System.Linq;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Services
{
    public class StatisticsStorage
    {
        private readonly Dictionary<string, List<AirQualityReading>> data;

        public StatisticsStorage()
        {
            data = new Dictionary<string, List<AirQualityReading>>();
        }

        public void Add(string key, List<AirQualityReading> readings)
        {
            if (data.ContainsKey(key))
            {
                data[key] = readings;
            }
            else
            {
                data.Add(key, readings);
            }
        }

        public List<AirQualityReading> Get(string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            return null;
        }

        public Dictionary<string, List<AirQualityReading>> GetAll()
        {
            return new Dictionary<string, List<AirQualityReading>>(data);
        }

        public List<StationReadingsGroup> GetAllAsGroups()
        {
            return data.Select(kvp =>
            {
                var parts = kvp.Key.Split('-');
                return new StationReadingsGroup
                {
                    Key = kvp.Key,
                    StationId = Guid.Parse(parts[0]),
                    Month = int.Parse(parts[1]),
                    Year = int.Parse(parts[2]),
                    Readings = kvp.Value
                };
            }).ToList();
        }

        public void Clear()
        {
            data.Clear();
        }

        public static string GenerateKey(Guid stationId, int month, int year)
        {
            return $"{stationId}-{month}-{year}";
        }
    }
}