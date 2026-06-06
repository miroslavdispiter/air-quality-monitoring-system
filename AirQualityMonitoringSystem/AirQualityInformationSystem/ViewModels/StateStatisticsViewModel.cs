using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace AirQualityInformationSystem.ViewModels
{
    public class StateStatisticsViewModel : BaseViewModel, IObserver
    {
        private readonly AirQualityRepository readingRepo;
        private readonly DispatcherTimer chartUpdateTimer;

        #region Chart Properties

        private ISeries[] stateSeries;
        public ISeries[] StateSeries
        {
            get => stateSeries;
            set { stateSeries = value; OnPropertyChanged(); }
        }

        private int goodCount;
        public int GoodCount
        {
            get => goodCount;
            private set { goodCount = value; OnPropertyChanged(); }
        }

        private int moderateCount;
        public int ModerateCount
        {
            get => moderateCount;
            private set { moderateCount = value; OnPropertyChanged(); }
        }

        private int unhealthyCount;
        public int UnhealthyCount
        {
            get => unhealthyCount;
            private set { unhealthyCount = value; OnPropertyChanged(); }
        }

        private int hazardousCount;
        public int HazardousCount
        {
            get => hazardousCount;
            private set { hazardousCount = value; OnPropertyChanged(); }
        }

        #endregion

        public StateStatisticsViewModel(AirQualityRepository readingRepo)
        {
            this.readingRepo = readingRepo;

            // Timer za periodicno azuriranje chart-a
            chartUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            chartUpdateTimer.Tick += (s, e) => Update();
            chartUpdateTimer.Start();

            // Inicijalno azuriranje
            Update();
        }

        /// <summary>
        /// IObserver implementacija - poziva se kada se promene readings
        /// </summary>
        public void Update()
        {
            var readings = readingRepo.GetAll().ToList();

            GoodCount = readings.Count(r => r.State == AirQualityState.Good);
            ModerateCount = readings.Count(r => r.State == AirQualityState.Moderate);
            UnhealthyCount = readings.Count(r => r.State == AirQualityState.Unhealthy);
            HazardousCount = readings.Count(r => r.State == AirQualityState.Hazardous);

            System.Diagnostics.Debug.WriteLine(
                $"Chart Update: Good={GoodCount}, Moderate={ModerateCount}, " +
                $"Unhealthy={UnhealthyCount}, Hazardous={HazardousCount}");

            UpdateChart();
        }

        /// <summary>
        /// Azurira LiveCharts prikaz
        /// </summary>
        public void UpdateChart()
        {
            var seriesList = new List<ISeries>();

            if (GoodCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { GoodCount },
                    Name = $"Good ({GoodCount})"
                });
            }

            if (ModerateCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { ModerateCount },
                    Name = $"Moderate ({ModerateCount})"
                });
            }

            if (UnhealthyCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { UnhealthyCount },
                    Name = $"Unhealthy ({UnhealthyCount})"
                });
            }

            if (HazardousCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { HazardousCount },
                    Name = $"Hazardous ({HazardousCount})"
                });
            }

            if (seriesList.Count == 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { 1 },
                    Name = "No Data"
                });
            }

            StateSeries = seriesList.ToArray();
        }

        /// <summary>
        /// Zaustavi timer kada ViewModel nije vise potreban
        /// </summary>
        public void StopTimer()
        {
            chartUpdateTimer?.Stop();
        }
    }
}