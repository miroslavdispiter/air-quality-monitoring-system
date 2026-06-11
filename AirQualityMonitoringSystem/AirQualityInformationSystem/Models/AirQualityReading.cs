using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.States;

namespace AirQualityInformationSystem.Models
{
    [DataContract(Namespace = "http://airquality.models/2024")]
    public class AirQualityReading : ISubject, INotifyPropertyChanged
    {
        private readonly List<IObserver> observers = new List<IObserver>();
        private IAirQualityState currentState;

        private double pm25;
        private double no2Level;
        private double ozoneLevel;
        private AirQualityState state;

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid StationId { get; set; }

        [DataMember]
        public DateTime ReadingTime { get; set; }

        [DataMember]
        public double PM25
        {
            get => pm25;
            set
            {
                if (pm25 != value)
                {
                    pm25 = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        public double NO2Level
        {
            get => no2Level;
            set
            {
                if (no2Level != value)
                {
                    no2Level = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        public double OzoneLevel
        {
            get => ozoneLevel;
            set
            {
                if (ozoneLevel != value)
                {
                    ozoneLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        [DataMember]
        public AirQualityState State
        {
            get => state;
            set
            {
                if (state != value)
                {
                    state = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public AirQualityReading()
        {
            Id = Guid.NewGuid();
            EvaluateState();
        }

        public void EvaluateState()
        {
            var previousState = State;

            if (PM25 < 15 && NO2Level < 40)
                currentState = new GoodState();
            else if (PM25 < 35 && NO2Level < 80)
                currentState = new ModerateState();
            else if (PM25 < 75)
                currentState = new UnhealthyState();
            else
                currentState = new HazardousState();

            currentState.Handle(this);

            if (previousState != State)
            {
                NotifyObservers();
            }
        }

        public void Register(IObserver observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
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