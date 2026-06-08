using System.Collections.Generic;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Interfaces
{
    public interface IStatisticsStrategy
    {
        double Calculate(List<AirQualityReading> readings);
        string GetName();
    }
}