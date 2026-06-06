using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Interfaces
{
    public interface IAirQualityState
    {
        void Handle(AirQualityReading reading);
    }
}