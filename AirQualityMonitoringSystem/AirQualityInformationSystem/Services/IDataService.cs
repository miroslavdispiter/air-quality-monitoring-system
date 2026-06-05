using AirQualityInformationSystem.Models;
using System;
using System.Collections.Generic;

namespace AirQualityInformationSystem.Services
{
    public interface IDataService
    {
        void SetData(List<MonitoringStation> stations, List<AirQualityReading> readings);
        List<MonitoringStation> GetAllStations();
        List<AirQualityReading> GetAllReadings();
        List<AirQualityReading> GetReadingsByStationAndMonth(Guid stationId, int month, int year);
        void AddStation(MonitoringStation station);
        void AddReading(AirQualityReading reading);
        void UpdateStation(MonitoringStation station);
        void UpdateReading(AirQualityReading reading);
        void RemoveStation(MonitoringStation station);
        void RemoveReading(AirQualityReading reading);
    }
}