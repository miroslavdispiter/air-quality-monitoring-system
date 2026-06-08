using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Services.WCF
{
    public class WcfServiceHost
    {
        private ServiceHost serviceHost;
        private readonly AirQualityRepository readingRepository;
        private readonly MonitoringStationRepository stationRepository;

        public WcfServiceHost(
            AirQualityRepository readingRepository,
            MonitoringStationRepository stationRepository)
        {
            this.readingRepository = readingRepository;
            this.stationRepository = stationRepository;
        }

        public void Start()
        {
            try
            {
                var stationsCount = stationRepository.GetAll().Count();
                var readingsCount = readingRepository.GetAll().Count();
                System.Diagnostics.Debug.WriteLine($"Starting WCF with {stationsCount} stations and {readingsCount} readings");

                AirQualityService.Initialize(readingRepository, stationRepository);

                serviceHost = new ServiceHost(typeof(AirQualityService), new Uri("net.pipe://localhost"));

                var binding = new NetNamedPipeBinding
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    MaxBufferPoolSize = 2147483647
                };

                serviceHost.AddServiceEndpoint(
                    typeof(IAirQualityService),
                    binding,
                    "AirQualityService");

                var smb = new ServiceMetadataBehavior();
                serviceHost.Description.Behaviors.Add(smb);
                serviceHost.AddServiceEndpoint(
                    typeof(IMetadataExchange),
                    MetadataExchangeBindings.CreateMexNamedPipeBinding(),
                    "mex");

                serviceHost.Open();
                System.Diagnostics.Debug.WriteLine("✓ WCF Service started at net.pipe://localhost/AirQualityService");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"✗ WCF Service failed: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                if (serviceHost != null)
                {
                    if (serviceHost.State == CommunicationState.Opened)
                        serviceHost.Close();
                    else
                        serviceHost.Abort();
                }
            }
            catch
            {
                serviceHost?.Abort();
            }
        }
    }
}