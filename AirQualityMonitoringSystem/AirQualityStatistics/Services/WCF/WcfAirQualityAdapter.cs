using System;
using System.Collections.Generic;
using System.ServiceModel;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;

namespace AirQualityStatistics.Services.WCF
{
    public class WcfAirQualityAdapter : IAirQualityDataProvider
    {
        private IAirQualityService service;
        private ChannelFactory<IAirQualityService> channelFactory;

        public WcfAirQualityAdapter()
        {
            try
            {
                var binding = new NetNamedPipeBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 2147483647,
                    OpenTimeout = TimeSpan.FromSeconds(10),
                    CloseTimeout = TimeSpan.FromSeconds(10),
                    SendTimeout = TimeSpan.FromSeconds(10),
                    ReceiveTimeout = TimeSpan.FromMinutes(10)
                };

                var endpoint = new EndpointAddress("net.pipe://localhost/AirQualityService");

                channelFactory = new ChannelFactory<IAirQualityService>(binding, endpoint);
                service = channelFactory.CreateChannel();

                System.Diagnostics.Debug.WriteLine("✓ WCF Client connected to net.pipe://localhost/AirQualityService");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ WCF Client connection failed: {ex.Message}");
                throw new Exception($"WCF connection error: {ex.Message}", ex);
            }
        }

        public List<AirQualityReading> GetReadings(Guid stationId, int month, int year)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Requesting readings for {stationId}, {month}/{year}");
                var result = service.GetReadingsForStation(stationId, month, year);
                System.Diagnostics.Debug.WriteLine($"Received {result.Count} readings");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to retrieve readings: {ex.Message}", ex);
            }
        }

        public List<MonitoringStation> GetStations()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Requesting all stations");
                var result = service.GetAllStations();
                System.Diagnostics.Debug.WriteLine($"Received {result.Count} stations");
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"WCF communication error: {ex.Message}.", ex);
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
