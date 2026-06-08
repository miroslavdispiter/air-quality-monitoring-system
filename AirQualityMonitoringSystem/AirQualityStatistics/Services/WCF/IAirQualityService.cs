using System;
using System.Collections.Generic;
using System.ServiceModel;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Services.WCF
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