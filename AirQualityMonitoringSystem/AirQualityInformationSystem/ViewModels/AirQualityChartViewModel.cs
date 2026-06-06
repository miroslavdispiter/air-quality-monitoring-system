using System.Collections.Generic;
using System.Linq;
using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.ViewModels
{
    public class AirQualityChartViewModel : IObserver
    {
        private readonly AirQualityRepository repository;

        public int GoodCount { get; private set; }
        public int ModerateCount { get; private set; }
        public int UnhealthyCount { get; private set; }
        public int HazardousCount { get; private set; }

        public AirQualityChartViewModel(AirQualityRepository repository)
        {
            this.repository = repository;
            Update();
        }

        public void Update()
        {
            var readings = repository.GetAll();

            GoodCount = readings.Count(r => r.State == AirQualityState.Good);
            ModerateCount = readings.Count(r => r.State == AirQualityState.Moderate);
            UnhealthyCount = readings.Count(r => r.State == AirQualityState.Unhealthy);
            HazardousCount = readings.Count(r => r.State == AirQualityState.Hazardous);
        }
    }
}