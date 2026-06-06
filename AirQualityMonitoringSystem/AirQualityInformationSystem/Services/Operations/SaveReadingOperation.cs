using System;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Operations
{
    public class SaveReadingOperation : AbstractSaveOperation
    {
        private readonly AirQualityRepository repository;
        private readonly AirQualityReading reading;

        public SaveReadingOperation(
            AirQualityRepository repository,
            AirQualityReading reading,
            ILoggerService logger) : base(logger)
        {
            this.repository = repository;
            this.reading = reading;
        }

        protected override void Validate()
        {
            if (reading.PM25 < 0 || reading.NO2Level < 0 || reading.OzoneLevel < 0)
                throw new Exception("Pollution values cannot be negative.");
        }

        protected override void SaveToRepository()
        {
            repository.AddReading(reading);
        }

        protected override void Log()
        {
            logger.Log($"Reading added for StationId: {reading.StationId}");
        }
    }
}