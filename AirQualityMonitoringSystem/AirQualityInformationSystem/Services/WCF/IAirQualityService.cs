using System;
using System.Collections.Generic;
using System.ServiceModel;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Services.WCF
{
    [ServiceContract]
    public interface IAirQualityService
    {
        [OperationContract]
        List<AirQualityReading> GetReadingsForStation(Guid stationId, int month, int year);

        [OperationContract]
        List<MonitoringStation> GetAllStations();
    }
}