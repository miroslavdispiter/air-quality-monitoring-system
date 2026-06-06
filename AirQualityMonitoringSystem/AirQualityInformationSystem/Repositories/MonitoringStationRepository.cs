using System;
using System.Collections.Generic;
using System.Linq;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Repositories
{
    public class MonitoringStationRepository
    {
        private readonly List<MonitoringStation> stations = new List<MonitoringStation>();

        public IEnumerable<MonitoringStation> GetAll()
        {
            return stations.ToList();
        }

        public MonitoringStation GetById(Guid id)
        {
            return stations.FirstOrDefault(s => s.Id == id);
        }

        public void AddStation(MonitoringStation station)
        {
            if (!stations.Any(s => s.Id == station.Id))
            {
                stations.Add(station);
            }
        }

        public void RemoveStation(MonitoringStation station)
        {
            stations.Remove(station);
        }

        public void UpdateStation(MonitoringStation updatedStation)
        {
            var existing = stations.FirstOrDefault(s => s.Id == updatedStation.Id);
            if (existing == null) return;

            existing.Name = updatedStation.Name;
            existing.City = updatedStation.City;
            existing.District = updatedStation.District;
            existing.Latitude = updatedStation.Latitude;
            existing.Longitude = updatedStation.Longitude;
        }

        public void Clear()
        {
            stations.Clear();
        }

        public void LoadAll(IEnumerable<MonitoringStation> stationsToLoad)
        {
            stations.Clear();
            stations.AddRange(stationsToLoad);
        }
    }
}