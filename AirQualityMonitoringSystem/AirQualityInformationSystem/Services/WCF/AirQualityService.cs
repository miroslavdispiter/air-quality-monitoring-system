using System;
using System.Collections.Generic;
using System.Linq;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Services.WCF
{
    public class AirQualityService : IAirQualityService
    {
        private readonly AirQualityRepository readingRepository;
        private readonly MonitoringStationRepository stationRepository;

        public AirQualityService(
            AirQualityRepository readingRepository,
            MonitoringStationRepository stationRepository)
        {
            this.readingRepository = readingRepository;
            this.stationRepository = stationRepository;
        }

        public List<AirQualityReading> GetReadingsForStation(Guid stationId, int month, int year)
        {
            var readings = readingRepository.GetAll()
                .Where(r => r.StationId == stationId &&
                            r.ReadingTime.Month == month &&
                            r.ReadingTime.Year == year)
                .ToList();

            return readings;
        }

        public List<MonitoringStation> GetAllStations()
        {
            return stationRepository.GetAll().ToList();
        }
    }
}