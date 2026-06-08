using System.Runtime.Serialization;

namespace AirQualityStatistics.Models
{
    [DataContract]
    public enum AirQualityState
    {
        [EnumMember]
        Good,
        [EnumMember]
        Moderate,
        [EnumMember]
        Unhealthy,
        [EnumMember]
        Hazardous
    }
}