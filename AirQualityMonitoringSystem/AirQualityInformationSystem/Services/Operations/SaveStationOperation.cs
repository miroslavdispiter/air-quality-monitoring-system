using System;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Operations
{
    public class SaveStationOperation : AbstractSaveOperation
    {
        private readonly MonitoringStationRepository repository;
        private readonly MonitoringStation station;

        public SaveStationOperation(
            MonitoringStationRepository repository,
            MonitoringStation station,
            ILoggerService logger) : base(logger)
        {
            this.repository = repository;
            this.station = station;
        }

        protected override void Validate()
        {
            if (string.IsNullOrWhiteSpace(station.Name))
                throw new Exception("Station name cannot be empty.");
        }

        protected override void SaveToRepository()
        {
            repository.AddStation(station);
        }

        protected override void Log()
        {
            logger.Log($"Station added: {station.Name}");
        }
    }
}