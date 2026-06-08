using System.Collections.Generic;
using System.Runtime.Serialization;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Services.Persistence
{
    [DataContract(Namespace = "http://airquality.persistence/2024")]
    public class PersistenceData
    {
        [DataMember]
        public List<MonitoringStation> Stations { get; set; } = new List<MonitoringStation>();

        [DataMember]
        public List<AirQualityReading> Readings { get; set; } = new List<AirQualityReading>();
    }
}