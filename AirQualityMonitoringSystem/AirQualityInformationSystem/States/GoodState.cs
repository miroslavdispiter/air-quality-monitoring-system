using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.States
{
    public class GoodState : IAirQualityState
    {
        public void Handle(AirQualityReading reading)
        {
            reading.State = AirQualityState.Good;
        }
    }
}