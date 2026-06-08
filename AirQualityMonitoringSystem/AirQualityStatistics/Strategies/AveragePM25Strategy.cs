using System.Collections.Generic;
using System.Linq;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Strategies
{
    public class AveragePM25Strategy : IStatisticsStrategy
    {
        public double Calculate(List<AirQualityReading> readings)
        {
            if (readings == null || !readings.Any())
                return 0;

            return readings.Average(r => r.PM25);
        }

        public string GetName()
        {
            return "Average PM2.5 Concentration";
        }
    }
}