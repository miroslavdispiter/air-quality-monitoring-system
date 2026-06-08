using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using AirQualityInformationSystem.Interfaces;

namespace AirQualityInformationSystem.Services.Persistence
{
    public class XmlPersistenceStrategy : IPersistenceStrategy
    {
        private readonly string filePath = "airquality_data.xml";

        public void SaveData(PersistenceData data)
        {
            var serializer = new DataContractSerializer(typeof(PersistenceData));
            var settings = new XmlWriterSettings { Indent = true };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                serializer.WriteObject(writer, data);
            }
        }

        public PersistenceData LoadData()
        {
            if (!File.Exists(filePath))
                return new PersistenceData();

            var serializer = new DataContractSerializer(typeof(PersistenceData));

            using (var reader = XmlReader.Create(filePath))
            {
                return (PersistenceData)serializer.ReadObject(reader) ?? new PersistenceData();
            }
        }
    }
}