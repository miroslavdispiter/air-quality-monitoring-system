using System.Runtime.Serialization;

namespace AirQualityInformationSystem.Models
{
    [DataContract(Namespace = "http://airquality.models/2024")]
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