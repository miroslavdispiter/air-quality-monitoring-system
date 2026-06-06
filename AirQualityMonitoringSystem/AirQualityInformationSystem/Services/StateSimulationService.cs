using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using AirQualityInformationSystem.Models;

namespace AirQualityInformationSystem.Services
{
    public class StateSimulationService
    {
        private readonly DispatcherTimer timer;
        private readonly Random random = new Random();

        public event Action<AirQualityReading> OnStateChanged;

        public StateSimulationService()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            timer.Start();
        }

        public void Stop()
        {
            timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Simulacija ce se pokrenuti iz ViewModela za svaki reading
        }

        public void SimulateStateChange(AirQualityReading reading)
        {
            // Simulacija promene vrednosti
            reading.PM25 += random.Next(-5, 6);
            reading.NO2Level += random.Next(-3, 4);
            reading.OzoneLevel += random.Next(-2, 3);

            // Osiguravanje da su vrednosti pozitivne
            reading.PM25 = Math.Max(0, reading.PM25);
            reading.NO2Level = Math.Max(0, reading.NO2Level);
            reading.OzoneLevel = Math.Max(0, reading.OzoneLevel);

            // Evaluacija novog stanja
            reading.EvaluateState();

            OnStateChanged?.Invoke(reading);
        }
    }
}