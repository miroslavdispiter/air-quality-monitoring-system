using System;
using System.Collections.Generic;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Interfaces
{
    public interface IAirQualityDataProvider
    {
        List<AirQualityReading> GetReadings(Guid stationId, int month, int year);
        List<MonitoringStation> GetStations();
    }
}