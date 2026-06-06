using System;

namespace AirQualityInformationSystem.Models
{
    public class MonitoringStation
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string City { get; set; }

        public string District { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public MonitoringStation()
        {
            Id = Guid.NewGuid();
        }
    }
}