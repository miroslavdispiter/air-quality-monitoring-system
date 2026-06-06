using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.States
{
    public class UnhealthyState : IAirQualityState
    {
        public void Handle(AirQualityReading reading)
        {
            reading.State = AirQualityState.Unhealthy;
        }
    }
}