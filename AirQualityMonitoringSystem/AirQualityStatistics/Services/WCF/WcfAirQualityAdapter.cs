using System;
using System.Collections.Generic;
using System.ServiceModel;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Services.WCF
{
    public class WcfAirQualityAdapter : IAirQualityDataProvider
    {
        private readonly IAirQualityService service;
        private readonly ChannelFactory<IAirQualityService> channelFactory;

        public WcfAirQualityAdapter()
        {
            var binding = new BasicHttpBinding();
            var endpoint = new EndpointAddress("http://localhost:8733/AirQualityService/");

            channelFactory = new ChannelFactory<IAirQualityService>(binding, endpoint);
            service = channelFactory.CreateChannel();
        }

        public List<AirQualityReading> GetReadings(Guid stationId, int month, int year)
        {
            try
            {
                return service.GetReadingsForStation(stationId, month, year);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve readings from WCF service: {ex.Message}", ex);
            }
        }

        public List<MonitoringStation> GetStations()
        {
            try
            {
                return service.GetAllStations();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve stations from WCF service: {ex.Message}", ex);
            }
        }

        public void Close()
        {
            try
            {
                if (service is ICommunicationObject commObj)
                {
                    if (commObj.State == CommunicationState.Faulted)
                        commObj.Abort();
                    else
                        commObj.Close();
                }

                channelFactory?.Close();
            }
            catch
            {
                channelFactory?.Abort();
            }
        }
    }
}