using System.Collections.Generic;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Services.Persistence
{
    public class PersistenceData
    {
        public List<MonitoringStation> Stations { get; set; } = new List<MonitoringStation>();
        public List<AirQualityReading> Readings { get; set; } = new List<AirQualityReading>();
    }
}