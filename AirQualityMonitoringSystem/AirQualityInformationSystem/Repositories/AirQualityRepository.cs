using System;
using System.Collections.Generic;
using System.Linq;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Repositories
{
    public class AirQualityRepository
    {
        private readonly List<AirQualityReading> readings = new List<AirQualityReading>();

        public IEnumerable<AirQualityReading> GetAll()
        {
            return readings.ToList();
        }

        public AirQualityReading GetById(Guid id)
        {
            return readings.FirstOrDefault(r => r.Id == id);
        }

        public void AddReading(AirQualityReading reading)
        {
            if (!readings.Any(r => r.Id == reading.Id))
            {
                readings.Add(reading);
            }
        }

        public void RemoveReading(AirQualityReading reading)
        {
            readings.Remove(reading);
        }

        public void UpdateReading(AirQualityReading updatedReading)
        {
            var existing = readings.FirstOrDefault(r => r.Id == updatedReading.Id);
            if (existing == null) return;

            existing.StationId = updatedReading.StationId;
            existing.ReadingTime = updatedReading.ReadingTime;
            existing.PM25 = updatedReading.PM25;
            existing.NO2Level = updatedReading.NO2Level;
            existing.OzoneLevel = updatedReading.OzoneLevel;
            existing.EvaluateState();
        }

        public void Clear()
        {
            readings.Clear();
        }

        public void LoadAll(IEnumerable<AirQualityReading> readingsToLoad)
        {
            readings.Clear();
            readings.AddRange(readingsToLoad);
        }
    }
}