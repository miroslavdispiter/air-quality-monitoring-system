using System.Runtime.Serialization;

namespace AirQualityInformationSystem.Models
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