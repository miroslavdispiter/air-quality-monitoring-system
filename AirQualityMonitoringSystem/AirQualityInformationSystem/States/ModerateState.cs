using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.States
{
    public class ModerateState : IAirQualityState
    {
        public void Handle(AirQualityReading reading)
        {
            reading.State = AirQualityState.Moderate;
        }
    }
}