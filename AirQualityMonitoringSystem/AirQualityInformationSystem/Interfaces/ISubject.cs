using System;

namespace AirQualityInformationSystem.Interfaces
{
    public interface ISubject
    {
        void Register(IObserver observer);
        void Unregister(IObserver observer);
        void NotifyObservers();
    }
}