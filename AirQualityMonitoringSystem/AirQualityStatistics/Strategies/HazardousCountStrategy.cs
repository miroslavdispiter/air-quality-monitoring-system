using System.Collections.Generic;
using System.Linq;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Strategies
{
    public class HazardousCountStrategy : IStatisticsStrategy
    {
        public double Calculate(List<AirQualityReading> readings)
        {
            if (readings == null || !readings.Any())
                return 0;

            return readings.Count(r => r.State == AirQualityState.Hazardous);
        }

        public string GetName()
        {
            return "Hazardous State Count";
        }
    }
}