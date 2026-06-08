using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.States;

namespace AirQualityInformationSystem.Models
{
    [DataContract(Namespace = "http://airquality.models/2024")]
    public class AirQualityReading : ISubject
    {
        private readonly List<IObserver> observers = new List<IObserver>();
        private IAirQualityState currentState;

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid StationId { get; set; }

        [DataMember]
        public DateTime ReadingTime { get; set; }

        [DataMember]
        public double PM25 { get; set; }

        [DataMember]
        public double NO2Level { get; set; }

        [DataMember]
        public double OzoneLevel { get; set; }

        [DataMember]
        public AirQualityState State { get; set; }

        public AirQualityReading()
        {
            Id = Guid.NewGuid();
            EvaluateState();
        }

        public void EvaluateState()
        {
            if (PM25 < 15 && NO2Level < 40)
                currentState = new GoodState();
            else if (PM25 < 35 && NO2Level < 80)
                currentState = new ModerateState();
            else if (PM25 < 75)
                currentState = new UnhealthyState();
            else
                currentState = new HazardousState();

            currentState.Handle(this);
            NotifyObservers();
        }

        public void Register(IObserver observer)
        {
            observers.Add(observer);
        }

        public void Unregister(IObserver observer)
        {
            observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update();
            }
        }
    }
}
