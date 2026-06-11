using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;

namespace AirQualityInformationSystem.Services
{
    public class StateSimulationService
    {
        private readonly DispatcherTimer timer;
        private readonly Random random = new Random();
        private readonly AirQualityRepository readingRepo;

        public StateSimulationService(AirQualityRepository readingRepo)
        {
            this.readingRepo = readingRepo;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3) // Svake 3 sekunde
            };
            timer.Tick += Timer_Tick;
        }

        public void Start()
        {
            timer.Start();
            System.Diagnostics.Debug.WriteLine("✓ Simulation started - will update every 3 seconds");
        }

        public void Stop()
        {
            timer.Stop();
            System.Diagnostics.Debug.WriteLine("✗ Simulation stopped");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            var readings = readingRepo.GetAll().ToList();

            if (readings.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("No readings to simulate");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"\n⟳ === SIMULATION TICK === {DateTime.Now:HH:mm:ss}");

            foreach (var reading in readings)
            {
                SimulateStateChange(reading);
            }

            System.Diagnostics.Debug.WriteLine($"⟳ Simulated changes for {readings.Count} readings\n");
        }

        private void SimulateStateChange(AirQualityReading reading)
        {
            var oldState = reading.State;
            var oldPM25 = reading.PM25;
            var oldNO2 = reading.NO2Level;

            reading.PM25 += random.Next(-10, 15);
            reading.NO2Level += random.Next(-8, 12);
            reading.OzoneLevel += random.Next(-5, 8);

            reading.PM25 = Math.Max(0, Math.Min(200, reading.PM25));
            reading.NO2Level = Math.Max(0, Math.Min(200, reading.NO2Level));
            reading.OzoneLevel = Math.Max(0, Math.Min(200, reading.OzoneLevel));

            reading.EvaluateState();

            System.Diagnostics.Debug.WriteLine(
                $"  Reading {reading.Id.ToString().Substring(0, 8)}... : " +
                $"PM2.5={oldPM25:F1}→{reading.PM25:F1}, " +
                $"NO2={oldNO2:F1}→{reading.NO2Level:F1}, " +
                $"State={oldState}→{reading.State}");
        }
    }
}