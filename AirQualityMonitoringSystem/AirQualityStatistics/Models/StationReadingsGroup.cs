using System;
using System.Collections.Generic;
using System.Linq;

namespace AirQualityStatistics.Models
{
    public class StationReadingsGroup
    {
        public string Key { get; set; }
        public Guid StationId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public List<AirQualityReading> Readings { get; set; }

        public string DisplayText
        {
            get
            {
                if (Readings == null || !Readings.Any())
                    return $"({StationId}, {Month:D2}/{Year}) -> No data";

                var readingsPreview = string.Join(", ", Readings.Take(3).Select(r => r.ToString()));
                var remaining = Readings.Count > 3 ? $", ... (+{Readings.Count - 3} more)" : "";

                return $"({StationId}, {Month:D2}/{Year}) -> {readingsPreview}{remaining}";
            }
        }
    }
}