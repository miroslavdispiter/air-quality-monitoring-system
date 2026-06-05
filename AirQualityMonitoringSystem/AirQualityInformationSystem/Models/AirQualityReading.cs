using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AirQualityInformationSystem.Models
{
    public class AirQualityReading : INotifyPropertyChanged
    {
        private Guid _id;
        private Guid _stationId;
        private DateTime _readingTime;
        private double _pm25;
        private double _no2Level;
        private double _ozoneLevel;
        private AirQualityState _state;

        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public Guid StationId
        {
            get => _stationId;
            set
            {
                _stationId = value;
                OnPropertyChanged();
            }
        }

        public DateTime ReadingTime
        {
            get => _readingTime;
            set
            {
                _readingTime = value;
                OnPropertyChanged();
            }
        }

        public double PM25
        {
            get => _pm25;
            set
            {
                _pm25 = value;
                OnPropertyChanged();
            }
        }

        public double NO2Level
        {
            get => _no2Level;
            set
            {
                _no2Level = value;
                OnPropertyChanged();
            }
        }

        public double OzoneLevel
        {
            get => _ozoneLevel;
            set
            {
                _ozoneLevel = value;
                OnPropertyChanged();
            }
        }

        public AirQualityState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}