using System.Collections.Generic;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Services
{
    public class StatisticsContext
    {
        private IStatisticsStrategy strategy;

        public void SetStrategy(IStatisticsStrategy strategy)
        {
            this.strategy = strategy;
        }

        public double Execute(List<AirQualityReading> readings)
        {
            if (strategy == null)
                return 0;

            return strategy.Calculate(readings);
        }

        public string GetCurrentStrategyName()
        {
            return strategy?.GetName() ?? "No strategy selected";
        }
    }
}