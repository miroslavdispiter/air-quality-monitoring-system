using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.States
{
    public class HazardousState : IAirQualityState
    {
        public void Handle(AirQualityReading reading)
        {
            reading.State = AirQualityState.Hazardous;
        }
    }
}