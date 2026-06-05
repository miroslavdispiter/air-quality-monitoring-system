using AirQualityInformationSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AirQualityInformationSystem.Services
{
    public class DataService : IDataService
    {
        private List<MonitoringStation> _stations;
        private List<AirQualityReading> _readings;

        public DataService()
        {
            _stations = new List<MonitoringStation>();
            _readings = new List<AirQualityReading>();
        }

        public void SetData(List<MonitoringStation> stations, List<AirQualityReading> readings)
        {
            _stations = stations;
            _readings = readings;
        }

        public List<MonitoringStation> GetAllStations()
        {
            return _stations;
        }

        public List<AirQualityReading> GetAllReadings()
        {
            return _readings;
        }

        public List<AirQualityReading> GetReadingsByStationAndMonth(Guid stationId, int month, int year)
        {
            return _readings
                .Where(r => r.StationId == stationId
                         && r.ReadingTime.Month == month
                         && r.ReadingTime.Year == year)
                .ToList();
        }

        public void AddStation(MonitoringStation station)
        {
            _stations.Add(station);
        }

        public void AddReading(AirQualityReading reading)
        {
            _readings.Add(reading);
        }

        public void UpdateStation(MonitoringStation station)
        {
            var existing = _stations.FirstOrDefault(s => s.Id == station.Id);
            if (existing != null)
            {
                var index = _stations.IndexOf(existing);
                _stations[index] = station;
            }
        }

        public void UpdateReading(AirQualityReading reading)
        {
            var existing = _readings.FirstOrDefault(r => r.Id == reading.Id);
            if (existing != null)
            {
                var index = _readings.IndexOf(existing);
                _readings[index] = reading;
            }
        }

        public void RemoveStation(MonitoringStation station)
        {
            _stations.Remove(station);
        }

        public void RemoveReading(AirQualityReading reading)
        {
            _readings.Remove(reading);
        }
    }
}