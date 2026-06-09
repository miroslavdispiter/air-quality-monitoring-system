using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace AirQualityInformationSystem.ViewModels
{
    public class StateStatisticsViewModel : BaseViewModel, IObserver
    {
        private readonly AirQualityRepository readingRepo;
        private readonly DispatcherTimer chartUpdateTimer;

        #region Chart Properties

        private ObservableCollection<ISeries> stateSeries;
        public ObservableCollection<ISeries> StateSeries
        {
            get => stateSeries;
            set
            {
                stateSeries = value;
                OnPropertyChanged();
                System.Diagnostics.Debug.WriteLine($"StateSeries property changed, Count={value?.Count ?? 0}");
            }
        }

        #endregion

        public StateStatisticsViewModel(AirQualityRepository readingRepo)
        {
            this.readingRepo = readingRepo;

            // Initialize collection first
            StateSeries = new ObservableCollection<ISeries>();

            // Timer
            chartUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            chartUpdateTimer.Tick += (s, e) => Update();
            chartUpdateTimer.Start();

            // Initial update
            Update();
        }

        public void Update()
        {
            var readings = readingRepo.GetAll().ToList();

            var goodCount = readings.Count(r => r.State == AirQualityState.Good);
            var moderateCount = readings.Count(r => r.State == AirQualityState.Moderate);
            var unhealthyCount = readings.Count(r => r.State == AirQualityState.Unhealthy);
            var hazardousCount = readings.Count(r => r.State == AirQualityState.Hazardous);

            System.Diagnostics.Debug.WriteLine(
                $"Chart Update: Good={goodCount}, Moderate={moderateCount}, " +
                $"Unhealthy={unhealthyCount}, Hazardous={hazardousCount}");

            UpdateChart(goodCount, moderateCount, unhealthyCount, hazardousCount);
        }

        // Overload bez parametara
        public void UpdateChart()
        {
            Update();
        }

        private void UpdateChart(int good, int moderate, int unhealthy, int hazardous)
        {
            StateSeries.Clear();

            if (good > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { good },
                    Name = $"Good ({good})"
                });
            }

            if (moderate > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { moderate },
                    Name = $"Moderate ({moderate})"
                });
            }

            if (unhealthy > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { unhealthy },
                    Name = $"Unhealthy ({unhealthy})"
                });
            }

            if (hazardous > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { hazardous },
                    Name = $"Hazardous ({hazardous})"
                });
            }

            // If no data
            if (StateSeries.Count == 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { 1 },
                    Name = "No Data"
                });
            }

            System.Diagnostics.Debug.WriteLine($"✓ Chart series updated: {StateSeries.Count} series");
            foreach (var series in StateSeries)
            {
                System.Diagnostics.Debug.WriteLine($"  - {series.Name}");
            }
        }

        public void StopTimer()
        {
            chartUpdateTimer?.Stop();
        }
    }
}