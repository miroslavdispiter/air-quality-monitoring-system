using AirQualityInformationSystem.Interfaces;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AirQualityInformationSystem.ViewModels
{
    public class StateStatisticsViewModel : BaseViewModel, IObserver
    {
        private readonly AirQualityRepository readingRepo;

        #region Chart Properties

        private ObservableCollection<ISeries> stateSeries;
        public ObservableCollection<ISeries> StateSeries
        {
            get => stateSeries;
            set
            {
                stateSeries = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public StateStatisticsViewModel(AirQualityRepository readingRepo)
        {
            this.readingRepo = readingRepo;
            StateSeries = new ObservableCollection<ISeries>();

            System.Diagnostics.Debug.WriteLine("✓ StateStatisticsViewModel created");
        }

        public void Initialize()
        {
            SubscribeToReadings();

            Update();

            System.Diagnostics.Debug.WriteLine("✓ StateStatisticsViewModel initialized as Observer");
        }

        private void SubscribeToReadings()
        {
            var readings = readingRepo.GetAll().ToList();
            foreach (var reading in readings)
            {
                reading.Register(this);
            }
            System.Diagnostics.Debug.WriteLine($"✓ Subscribed to {readings.Count} readings");
        }

        public void Update()
        {
            var readings = readingRepo.GetAll().ToList();

            var goodCount = readings.Count(r => r.State == AirQualityState.Good);
            var moderateCount = readings.Count(r => r.State == AirQualityState.Moderate);
            var unhealthyCount = readings.Count(r => r.State == AirQualityState.Unhealthy);
            var hazardousCount = readings.Count(r => r.State == AirQualityState.Hazardous);

            System.Diagnostics.Debug.WriteLine(
                $"📊 Chart Update: Good={goodCount}, Moderate={moderateCount}, " +
                $"Unhealthy={unhealthyCount}, Hazardous={hazardousCount}");

            UpdateChart(goodCount, moderateCount, unhealthyCount, hazardousCount);
        }

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
                    Name = $"Good ({good})",
                    DataLabelsSize = 14,
                    DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(
                        new SkiaSharp.SKColor(50, 50, 50)),
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                });
            }

            if (moderate > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { moderate },
                    Name = $"Moderate ({moderate})",
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                });
            }

            if (unhealthy > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { unhealthy },
                    Name = $"Unhealthy ({unhealthy})",
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                });
            }

            if (hazardous > 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { hazardous },
                    Name = $"Hazardous ({hazardous})",
                    DataLabelsSize = 14,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                });
            }

            if (StateSeries.Count == 0)
            {
                StateSeries.Add(new PieSeries<double>
                {
                    Values = new double[] { 1 },
                    Name = "No Data"
                });
            }

            System.Diagnostics.Debug.WriteLine($"✓ Chart updated with {StateSeries.Count} series");
        }

        public void RegisterNewReading(AirQualityReading reading)
        {
            reading.Register(this);
            Update();
            System.Diagnostics.Debug.WriteLine($"✓ Registered new reading as observer");
        }
    }
}