using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Commands
{
    public class RemoveReadingCommand : IUndoableCommand
    {
        private readonly AirQualityRepository repository;
        private readonly AirQualityReading reading;

        public RemoveReadingCommand(AirQualityRepository repository, AirQualityReading reading)
        {
            this.repository = repository;
            this.reading = reading;
        }

        public void Execute()
        {
            repository.RemoveReading(reading);
        }

        public void Undo()
        {
            repository.AddReading(reading);
        }
    }
}