using AirQualityInformationSystem.Models;
using System.Collections.Generic;

namespace AirQualityInformationSystem.Services
{
    public interface IPersistenceService
    {
        void SaveData(List<MonitoringStation> stations, List<AirQualityReading> readings);
        (List<MonitoringStation> Stations, List<AirQualityReading> Readings) LoadData();
    }
}