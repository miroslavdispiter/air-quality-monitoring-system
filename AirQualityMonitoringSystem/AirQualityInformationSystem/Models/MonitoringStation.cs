using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AirQualityInformationSystem.Models
{
    public class MonitoringStation : INotifyPropertyChanged
    {
        private Guid _id;
        private string _name;
        private string _city;
        private string _district;
        private double _latitude;
        private double _longitude;

        public Guid Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged();
            }
        }

        public string District
        {
            get => _district;
            set
            {
                _district = value;
                OnPropertyChanged();
            }
        }

        public double Latitude
        {
            get => _latitude;
            set
            {
                _latitude = value;
                OnPropertyChanged();
            }
        }

        public double Longitude
        {
            get => _longitude;
            set
            {
                _longitude = value;
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