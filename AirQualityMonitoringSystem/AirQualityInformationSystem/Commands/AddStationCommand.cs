using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using AirQualityInformationSystem.Services;

namespace AirQualityInformationSystem.Commands
{
    public class AddStationCommand : IUndoableCommand
    {
        private readonly MonitoringStationRepository repository;
        private readonly MonitoringStation station;

        public AddStationCommand(MonitoringStationRepository repository, MonitoringStation station)
        {
            this.repository = repository;
            this.station = station;
        }

        public void Execute()
        {
            repository.AddStation(station);
        }

        public void Undo()
        {
            repository.RemoveStation(station);
        }
    }
}