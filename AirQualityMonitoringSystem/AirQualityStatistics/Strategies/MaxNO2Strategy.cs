using System.Collections.Generic;
using System.Linq;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Strategies
{
    public class MaxNO2Strategy : IStatisticsStrategy
    {
        public double Calculate(List<AirQualityReading> readings)
        {
            if (readings == null || !readings.Any())
                return 0;

            return readings.Max(r => r.NO2Level);
        }

        public string GetName()
        {
            return "Maximum NO2 Concentration";
        }
    }
}