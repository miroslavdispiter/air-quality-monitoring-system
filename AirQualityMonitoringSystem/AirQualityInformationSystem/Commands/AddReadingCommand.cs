using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Commands
{
    public class AddReadingCommand : IUndoableCommand
    {
        private readonly AirQualityRepository repository;
        private readonly AirQualityReading reading;

        public AddReadingCommand(AirQualityRepository repository, AirQualityReading reading)
        {
            this.repository = repository;
            this.reading = reading;
        }

        public void Execute()
        {
            repository.AddReading(reading);
        }

        public void Undo()
        {
            repository.RemoveReading(reading);
        }
    }
}