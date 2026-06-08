using System;
using System.Runtime.Serialization;

namespace AirQualityStatistics.Models
{
    [DataContract(Namespace = "http://airquality.models/2024")]
    public class AirQualityReading
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid StationId { get; set; }

        [DataMember]
        public DateTime ReadingTime { get; set; }

        [DataMember]
        public double PM25 { get; set; }

        [DataMember]
        public double NO2Level { get; set; }

        [DataMember]
        public double OzoneLevel { get; set; }

        [DataMember]
        public AirQualityState State { get; set; }

        public override string ToString()
        {
            return $"[PM2.5: {PM25:F1}, NO2: {NO2Level:F1}, state: {State}]";
        }
    }
}