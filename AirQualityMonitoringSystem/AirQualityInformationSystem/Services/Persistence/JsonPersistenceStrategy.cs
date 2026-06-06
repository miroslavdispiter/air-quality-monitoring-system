using System.IO;
using System.Text.Json;
using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Services.Persistence
{
    public class JsonPersistenceStrategy : IPersistenceStrategy
    {
        private readonly string filePath = "airquality_data.json";

        public void SaveData(PersistenceData data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        public PersistenceData LoadData()
        {
            if (!File.Exists(filePath))
                return new PersistenceData();

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<PersistenceData>(json) ?? new PersistenceData();
        }
    }
}