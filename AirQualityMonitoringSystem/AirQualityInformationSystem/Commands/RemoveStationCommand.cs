using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Commands
{
    public class RemoveStationCommand : IUndoableCommand
    {
        private readonly MonitoringStationRepository repository;
        private readonly MonitoringStation station;

        public RemoveStationCommand(MonitoringStationRepository repository, MonitoringStation station)
        {
            this.repository = repository;
            this.station = station;
        }

        public void Execute()
        {
            repository.RemoveStation(station);
        }

        public void Undo()
        {
            repository.AddStation(station);
        }
    }
}