using AirQualityInformationSystem.Services.Persistence;

namespace AirQualityInformationSystem.Interfaces
{
    public interface IPersistenceStrategy
    {
        void SaveData(PersistenceData data);
        PersistenceData LoadData();
    }
}