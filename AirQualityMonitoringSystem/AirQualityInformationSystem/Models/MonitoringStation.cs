using System;

using System;
using System.Runtime.Serialization;

namespace AirQualityInformationSystem.Models
{
    [DataContract]
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

        public MonitoringStation()
        {
            Id = Guid.NewGuid();
        }
    }
}