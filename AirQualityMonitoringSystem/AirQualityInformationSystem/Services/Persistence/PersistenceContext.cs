using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Services.Persistence
{
    public class PersistenceContext
    {
        private IPersistenceStrategy strategy;

        public void SetStrategy(IPersistenceStrategy strategy)
        {
            this.strategy = strategy;
        }

        public void Save(PersistenceData data)
        {
            strategy?.SaveData(data);
        }

        public PersistenceData Load()
        {
            return strategy?.LoadData();
        }
    }
}