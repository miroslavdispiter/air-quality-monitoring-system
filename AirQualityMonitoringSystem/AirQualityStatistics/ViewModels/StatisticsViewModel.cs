using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using AirQualityStatistics.Helpers;
using AirQualityStatistics.Interfaces;
using AirQualityStatistics.Models;
using AirQualityStatistics.Services;
using AirQualityStatistics.Services.WCF;
using AirQualityStatistics.Strategies;

namespace AirQualityStatistics.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IAirQualityDataProvider provider;
        private readonly StatisticsContext context;
        private readonly StatisticsStorage storage;

        #region Properties

        private ObservableCollection<MonitoringStation> stations;
        public ObservableCollection<MonitoringStation> Stations
        {
            get => stations;
            set { stations = value; OnPropertyChanged(); }
        }

        private MonitoringStation selectedStation;
        public MonitoringStation SelectedStation
        {
            get => selectedStation;
            set
            {
                selectedStation = value;
                OnPropertyChanged();
                LoadDataCommand?.RaiseCanExecuteChanged();
            }
        }

        private int selectedMonth = DateTime.Now.Month;
        public int SelectedMonth
        {
            get => selectedMonth;
            set
            {
                selectedMonth = value;
                OnPropertyChanged();
            }
        }

        private int selectedYear = DateTime.Now.Year;
        public int SelectedYear
        {
            get => selectedYear;
            set
            {
                selectedYear = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<StationReadingsGroup> loadedDataGroups;
        public ObservableCollection<StationReadingsGroup> LoadedDataGroups
        {
            get => loadedDataGroups;
            set { loadedDataGroups = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> statisticsMethods;
        public ObservableCollection<string> StatisticsMethods
        {
            get => statisticsMethods;
            set { statisticsMethods = value; OnPropertyChanged(); }
        }

        private string selectedStatisticsMethod;
        public string SelectedStatisticsMethod
        {
            get => selectedStatisticsMethod;
            set
            {
                selectedStatisticsMethod = value;
                OnPropertyChanged();
                ApplyStatisticsCommand?.RaiseCanExecuteChanged();
            }
        }

        private string statisticsResult;
        public string StatisticsResult
        {
            get => statisticsResult;
            set { statisticsResult = value; OnPropertyChanged(); }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set { statusMessage = value; OnPropertyChanged(); }
        }

        public List<int> AvailableMonths { get; } = Enumerable.Range(1, 12).ToList();
        public List<int> AvailableYears { get; } = Enumerable.Range(2020, 11).ToList();

        #endregion

        #region Commands

        public RelayCommand LoadStationsCommand { get; }
        public RelayCommand LoadDataCommand { get; }
        public RelayCommand ApplyStatisticsCommand { get; }
        public RelayCommand ExportToCsvCommand { get; }
        public RelayCommand ClearDataCommand { get; }

        #endregion

        public StatisticsViewModel()
        {
            provider = new WcfAirQualityAdapter();
            context = new StatisticsContext();
            storage = new StatisticsStorage();

            Stations = new ObservableCollection<MonitoringStation>();
            LoadedDataGroups = new ObservableCollection<StationReadingsGroup>();
            StatisticsMethods = new ObservableCollection<string>
            {
                "Average PM2.5 Concentration",
                "Maximum NO2 Concentration",
                "Hazardous State Count"
            };

            LoadStationsCommand = new RelayCommand(LoadStations);
            LoadDataCommand = new RelayCommand(LoadData, () => SelectedStation != null);
            ApplyStatisticsCommand = new RelayCommand(ApplyStatistics, () => !string.IsNullOrEmpty(SelectedStatisticsMethod));
            ExportToCsvCommand = new RelayCommand(ExportToCsv, () => LoadedDataGroups.Any());
            ClearDataCommand = new RelayCommand(ClearData, () => LoadedDataGroups.Any());

            StatusMessage = "Ready. Please select a station and load data.";

            LoadStations();
        }

        #region Methods

        private void LoadStations()
        {
            try
            {
                StatusMessage = "Loading stations from WCF service...";

                var stationsList = provider.GetStations();

                Stations.Clear();
                foreach (var station in stationsList)
                {
                    Stations.Add(station);
                }

                StatusMessage = $"Loaded {stationsList.Count} station(s) successfully.";

                if (stationsList.Any())
                {
                    SelectedStation = stationsList.First();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading stations: {ex.Message}";
                MessageBox.Show(
                    $"Failed to load stations from WCF service.\n\n{ex.Message}\n\nMake sure Component 1 is running.",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void LoadData(Guid stationId, int month, int year)
        {
            try
            {
                StatusMessage = $"Loading readings for station {stationId}, {month:D2}/{year}...";

                var readings = provider.GetReadings(stationId, month, year);

                var key = StatisticsStorage.GenerateKey(stationId, month, year);
                storage.Add(key, readings);

                RefreshLoadedDataGroups();

                StatusMessage = $"Loaded {readings.Count} reading(s) for {month:D2}/{year}.";

                if (readings.Count == 0)
                {
                    MessageBox.Show(
                        $"No readings found for the selected station and period ({month:D2}/{year}).",
                        "No Data",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                MessageBox.Show(
                    $"Failed to load readings.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadData()
        {
            if (SelectedStation == null) return;

            LoadData(SelectedStation.Id, SelectedMonth, SelectedYear);
        }

        private void RefreshLoadedDataGroups()
        {
            LoadedDataGroups.Clear();

            var groups = storage.GetAllAsGroups()
                .OrderByDescending(g => g.Year)
                .ThenByDescending(g => g.Month);

            foreach (var group in groups)
            {
                LoadedDataGroups.Add(group);
            }

            ExportToCsvCommand?.RaiseCanExecuteChanged();
            ClearDataCommand?.RaiseCanExecuteChanged();
        }

        private void ApplyStatistics()
        {
            if (string.IsNullOrEmpty(SelectedStatisticsMethod))
            {
                MessageBox.Show("Please select a statistics method.", "No Method Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                switch (SelectedStatisticsMethod)
                {
                    case "Average PM2.5 Concentration":
                        context.SetStrategy(new AveragePM25Strategy());
                        break;
                    case "Maximum NO2 Concentration":
                        context.SetStrategy(new MaxNO2Strategy());
                        break;
                    case "Hazardous State Count":
                        context.SetStrategy(new HazardousCountStrategy());
                        break;
                    default:
                        MessageBox.Show("Unknown statistics method.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                }

                var allReadings = storage.GetAll()
                    .SelectMany(kvp => kvp.Value)
                    .ToList();

                if (!allReadings.Any())
                {
                    MessageBox.Show("No data available. Please load data first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                    StatisticsResult = "No data available";
                    return;
                }

                var result = context.Execute(allReadings);

                string formattedResult;
                string unit;

                if (SelectedStatisticsMethod.Contains("Count"))
                {
                    int count = (int)result;
                    formattedResult = count.ToString();
                    unit = count == 1 ? "occurrence" : "occurrences";
                }
                else
                {
                    formattedResult = result.ToString("F2");
                    unit = "µg/m³";
                }

                StatisticsResult = $"{SelectedStatisticsMethod}: {formattedResult} {unit}\n(Based on {allReadings.Count} total readings)";
                StatusMessage = $"Statistics calculated successfully: {formattedResult} {unit}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error calculating statistics: {ex.Message}";
                MessageBox.Show(
                    $"Failed to calculate statistics.\n\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public void ExportToCsv(string path)
        {
            try
            {
                var csv = new StringBuilder();

                csv.AppendLine("StationId,Month,Year,ReadingId,ReadingTime,PM2.5,NO2,Ozone,State");

                foreach (var group in storage.GetAllAsGroups())
                {
                    foreach (var reading in group.Readings)
                    {
                        csv.AppendLine($"{group.StationId},{group.Month},{group.Year}," +
                                     $"{reading.Id},{reading.ReadingTime:yyyy-MM-dd HH:mm:ss}," +
                                     $"{reading.PM25:F2},{reading.NO2Level:F2},{reading.OzoneLevel:F2},{reading.State}");
                    }
                }

                File.WriteAllText(path, csv.ToString(), Encoding.UTF8);

                StatusMessage = $"Data exported to {path}";
                MessageBox.Show($"Data successfully exported to:\n{path}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting to CSV: {ex.Message}";
                MessageBox.Show($"Failed to export data.\n\n{ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToCsv()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv",
                FileName = $"AirQualityStatistics_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                ExportToCsv(dialog.FileName);
            }
        }

        private void ClearData()
        {
            var result = MessageBox.Show(
                "Are you sure you want to clear all loaded data?",
                "Confirm Clear",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                storage.Clear();
                LoadedDataGroups.Clear();
                StatisticsResult = string.Empty;
                StatusMessage = "All data cleared.";
                ExportToCsvCommand?.RaiseCanExecuteChanged();
                ClearDataCommand?.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}
