using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Commands
{
    public class UpdateReadingCommand : IUndoableCommand
    {
        private readonly AirQualityRepository repository;
        private readonly AirQualityReading oldReading;
        private readonly AirQualityReading newReading;

        public UpdateReadingCommand(
            AirQualityRepository repository,
            AirQualityReading oldReading,
            AirQualityReading newReading)
        {
            this.repository = repository;
            this.oldReading = oldReading;
            this.newReading = newReading;
        }

        public void Execute()
        {
            repository.UpdateReading(newReading);
        }

        public void Undo()
        {
            repository.UpdateReading(oldReading);
        }
    }
}