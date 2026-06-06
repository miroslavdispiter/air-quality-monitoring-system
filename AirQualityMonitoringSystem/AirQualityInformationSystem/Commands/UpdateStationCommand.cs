using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Commands
{
    public class UpdateStationCommand : IUndoableCommand
    {
        private readonly MonitoringStationRepository repository;
        private readonly MonitoringStation oldStation;
        private readonly MonitoringStation newStation;

        public UpdateStationCommand(
            MonitoringStationRepository repository,
            MonitoringStation oldStation,
            MonitoringStation newStation)
        {
            this.repository = repository;
            this.oldStation = oldStation;
            this.newStation = newStation;
        }

        public void Execute()
        {
            repository.UpdateStation(newStation);
        }

        public void Undo()
        {
            repository.UpdateStation(oldStation);
        }
    }
}