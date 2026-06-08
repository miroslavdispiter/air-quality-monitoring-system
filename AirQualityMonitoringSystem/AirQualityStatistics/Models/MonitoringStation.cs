using System;
using System.Runtime.Serialization;

namespace AirQualityStatistics.Models
{
    [DataContract(Namespace = "http://airquality.models/2024")]
    public class MonitoringStation
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string City { get; set; }

        [DataMember]
        public string District { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }
    }
}