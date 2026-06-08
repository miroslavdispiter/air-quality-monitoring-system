using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Services.WCF
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class AirQualityService : IAirQualityService
    {
        private static AirQualityRepository readingRepository;
        private static MonitoringStationRepository stationRepository;

        public static void Initialize(AirQualityRepository readingRepo, MonitoringStationRepository stationRepo)
        {
            readingRepository = readingRepo;
            stationRepository = stationRepo;
        }

        public AirQualityService()
        {
            
        }

        public List<AirQualityReading> GetReadingsForStation(Guid stationId, int month, int year)
        {

            if (readingRepository == null)
            {
                return new List<AirQualityReading>();
            }

            var readings = readingRepository.GetAll()
                .Where(r => r.StationId == stationId &&
                            r.ReadingTime.Month == month &&
                            r.ReadingTime.Year == year)
                .ToList();

            return readings;
        }

        public List<MonitoringStation> GetAllStations()
        {

            if (stationRepository == null)
            {
                return new List<MonitoringStation>();
            }

            var stations = stationRepository.GetAll().ToList();

            return stations;
        }
    }
}