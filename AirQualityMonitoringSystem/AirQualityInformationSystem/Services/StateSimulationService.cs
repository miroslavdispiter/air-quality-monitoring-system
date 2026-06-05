using AirQualityInformationSystem.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirQualityInformationSystem.Services
{
    public class StateSimulationService
    {
        private readonly Random _random = new Random();

        public async Task SimulateStateChanges(AirQualityReading reading, Action<AirQualityState> onStateChanged, CancellationToken cancellationToken)
        {
            var states = new[] { AirQualityState.Good, AirQualityState.Moderate, AirQualityState.Unhealthy, AirQualityState.Hazardous };
            int currentIndex = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                reading.State = states[currentIndex];
                onStateChanged?.Invoke(states[currentIndex]);

                await Task.Delay(3000, cancellationToken);

                currentIndex = (currentIndex + 1) % states.Length;
            }
        }
    }
}