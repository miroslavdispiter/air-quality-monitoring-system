using System;
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
                var service = new AirQualityService(readingRepository, stationRepository);

                serviceHost = new ServiceHost(service, new Uri("http://localhost:8733/AirQualityService/"));

                var binding = new BasicHttpBinding();
                serviceHost.AddServiceEndpoint(
                    typeof(IAirQualityService),
                    binding,
                    "");

                var smb = new ServiceMetadataBehavior
                {
                    HttpGetEnabled = true
                };
                serviceHost.Description.Behaviors.Add(smb);

                serviceHost.Open();

                Console.WriteLine("WCF Service started at http://localhost:8733/AirQualityService/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting WCF service: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                if (serviceHost != null && serviceHost.State == CommunicationState.Opened)
                {
                    serviceHost.Close();
                    Console.WriteLine("WCF Service stopped.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping WCF service: {ex.Message}");
                serviceHost?.Abort();
            }
        }
    }
}