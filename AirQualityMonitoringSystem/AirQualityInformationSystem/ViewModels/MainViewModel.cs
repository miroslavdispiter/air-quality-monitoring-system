using AirQualityInformationSystem.Commands;
using AirQualityInformationSystem.Helpers;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using AirQualityInformationSystem.Services;
using AirQualityInformationSystem.Services.Logging;
using AirQualityInformationSystem.Services.Persistence;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace AirQualityInformationSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        //using LiveChartsCore.SkiaSharpView.Painting;
        //using SkiaSharp;
        private readonly MonitoringStationRepository stationRepo;
        private readonly AirQualityRepository readingRepo;
        private readonly CommandManager commandManager;
        private readonly PersistenceContext persistenceContext;
        private readonly LoggerService logger;
        private readonly StateSimulationService simulationService;
        private readonly DispatcherTimer chartUpdateTimer;

        #region Observable Collections

        public ObservableCollection<MonitoringStation> Stations { get; set; }
        public ObservableCollection<AirQualityReading> Readings { get; set; }

        private ObservableCollection<MonitoringStation> filteredStations;
        public ObservableCollection<MonitoringStation> FilteredStations
        {
            get => filteredStations;
            set { filteredStations = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AirQualityReading> filteredReadings;
        public ObservableCollection<AirQualityReading> FilteredReadings
        {
            get => filteredReadings;
            set { filteredReadings = value; OnPropertyChanged(); }
        }

        #endregion

        #region Selected Items

        private MonitoringStation selectedStation;
        public MonitoringStation SelectedStation
        {
            get => selectedStation;
            set
            {
                selectedStation = value;
                OnPropertyChanged();
                DeleteStationCommand.RaiseCanExecuteChanged();
                UpdateStationCommand.RaiseCanExecuteChanged();

                if (value != null)
                {
                    NewStationName = value.Name;
                    NewStationCity = value.City;
                    NewStationDistrict = value.District;
                    NewStationLatitude = value.Latitude.ToString();
                    NewStationLongitude = value.Longitude.ToString();
                }
            }
        }

        private AirQualityReading selectedReading;
        public AirQualityReading SelectedReading
        {
            get => selectedReading;
            set
            {
                selectedReading = value;
                OnPropertyChanged();
                DeleteReadingCommand.RaiseCanExecuteChanged();
                UpdateReadingCommand.RaiseCanExecuteChanged();

                if (value != null)
                {
                    NewReadingStationId = value.StationId.ToString();
                    NewReadingTime = value.ReadingTime;
                    NewReadingPM25 = value.PM25.ToString();
                    NewReadingNO2 = value.NO2Level.ToString();
                    NewReadingOzone = value.OzoneLevel.ToString();
                }
            }
        }

        #endregion

        #region Search Properties

        private string stationSearchText;
        public string StationSearchText
        {
            get => stationSearchText;
            set
            {
                stationSearchText = value;
                OnPropertyChanged();
                FilterStations();
            }
        }

        private string readingSearchText;
        public string ReadingSearchText
        {
            get => readingSearchText;
            set
            {
                readingSearchText = value;
                OnPropertyChanged();
                FilterReadings();
            }
        }

        #endregion

        #region New Station Properties

        private string newStationName;
        public string NewStationName
        {
            get => newStationName;
            set { newStationName = value; OnPropertyChanged(); }
        }

        private string newStationCity;
        public string NewStationCity
        {
            get => newStationCity;
            set { newStationCity = value; OnPropertyChanged(); }
        }

        private string newStationDistrict;
        public string NewStationDistrict
        {
            get => newStationDistrict;
            set { newStationDistrict = value; OnPropertyChanged(); }
        }

        private string newStationLatitude;
        public string NewStationLatitude
        {
            get => newStationLatitude;
            set { newStationLatitude = value; OnPropertyChanged(); }
        }

        private string newStationLongitude;
        public string NewStationLongitude
        {
            get => newStationLongitude;
            set { newStationLongitude = value; OnPropertyChanged(); }
        }

        #endregion

        #region New Reading Properties

        private string newReadingStationId;
        public string NewReadingStationId
        {
            get => newReadingStationId;
            set { newReadingStationId = value; OnPropertyChanged(); }
        }

        private DateTime newReadingTime = DateTime.Now;
        public DateTime NewReadingTime
        {
            get => newReadingTime;
            set { newReadingTime = value; OnPropertyChanged(); }
        }

        private string newReadingPM25;
        public string NewReadingPM25
        {
            get => newReadingPM25;
            set { newReadingPM25 = value; OnPropertyChanged(); }
        }

        private string newReadingNO2;
        public string NewReadingNO2
        {
            get => newReadingNO2;
            set { newReadingNO2 = value; OnPropertyChanged(); }
        }

        private string newReadingOzone;
        public string NewReadingOzone
        {
            get => newReadingOzone;
            set { newReadingOzone = value; OnPropertyChanged(); }
        }

        #endregion

        #region Chart

        private ISeries[] stateSeries;
        public ISeries[] StateSeries
        {
            get => stateSeries;
            set { stateSeries = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public RelayCommand AddStationCommand { get; }
        public RelayCommand UpdateStationCommand { get; }
        public RelayCommand DeleteStationCommand { get; }

        public RelayCommand AddReadingCommand { get; }
        public RelayCommand UpdateReadingCommand { get; }
        public RelayCommand DeleteReadingCommand { get; }

        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }

        public RelayCommand SaveDataCommand { get; }
        public RelayCommand LoadDataCommand { get; }

        #endregion

        public MainViewModel()
        {
            stationRepo = new MonitoringStationRepository();
            readingRepo = new AirQualityRepository();
            commandManager = new CommandManager();
            persistenceContext = new PersistenceContext();
            logger = new LoggerService();
            simulationService = new StateSimulationService();

            persistenceContext.SetStrategy(new JsonPersistenceStrategy());

            Stations = new ObservableCollection<MonitoringStation>();
            Readings = new ObservableCollection<AirQualityReading>();
            FilteredStations = new ObservableCollection<MonitoringStation>();
            FilteredReadings = new ObservableCollection<AirQualityReading>();

            // Commands
            AddStationCommand = new RelayCommand(AddStation);
            UpdateStationCommand = new RelayCommand(UpdateStation, () => SelectedStation != null);
            DeleteStationCommand = new RelayCommand(DeleteStation, () => SelectedStation != null);

            AddReadingCommand = new RelayCommand(AddReading);
            UpdateReadingCommand = new RelayCommand(UpdateReading, () => SelectedReading != null);
            DeleteReadingCommand = new RelayCommand(DeleteReading, () => SelectedReading != null);

            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);

            SaveDataCommand = new RelayCommand(SaveData);
            LoadDataCommand = new RelayCommand(LoadData);

            // Chart update timer
            chartUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            chartUpdateTimer.Tick += (s, e) => UpdateChart();
            chartUpdateTimer.Start();

            // Simulation
            simulationService.OnStateChanged += OnReadingStateChanged;
            simulationService.Start();

            InitializeData();

            System.Diagnostics.Debug.WriteLine($"MainViewModel loaded. Readings count: {Readings.Count}");

            UpdateChart();

            logger.Log("Application started");
        }

        private void InitializeData()
        {
            var loadedData = persistenceContext.Load();

            if (loadedData != null && loadedData.Stations.Count > 0)
            {
                stationRepo.LoadAll(loadedData.Stations);
                readingRepo.LoadAll(loadedData.Readings);
            }
            else
            {
                var defaultStations = DataSeeder.GetDefaultStations();
                var defaultReadings = DataSeeder.GetDefaultReadings(defaultStations);

                stationRepo.LoadAll(defaultStations);
                readingRepo.LoadAll(defaultReadings);

                foreach (var reading in defaultReadings)
                {
                    reading.EvaluateState();
                }
            }

            RefreshCollections();
        }

        private void RefreshCollections()
        {
            Stations.Clear();
            foreach (var station in stationRepo.GetAll())
                Stations.Add(station);

            Readings.Clear();
            foreach (var reading in readingRepo.GetAll())
                Readings.Add(reading);

            FilterStations();
            FilterReadings();
        }

        #region Station Methods

        private void AddStation()
        {
            try
            {
                if (!ValidateStationInput(out var errorMessage))
                {
                    MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var station = new MonitoringStation
                {
                    Name = NewStationName,
                    City = NewStationCity,
                    District = NewStationDistrict,
                    Latitude = double.Parse(NewStationLatitude),
                    Longitude = double.Parse(NewStationLongitude)
                };

                var command = new AddStationCommand(stationRepo, station);
                commandManager.ExecuteCommand(command);

                Stations.Add(station);
                FilterStations();
                UpdateChart();

                ClearStationInputs();

                logger.Log($"Station added: {station.Name}");
                MessageBox.Show("Station added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding station: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error adding station: {ex.Message}");
            }
        }

        private void UpdateStation()
        {
            try
            {
                if (SelectedStation == null) return;

                if (!ValidateStationInput(out var errorMessage))
                {
                    MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var oldStation = new MonitoringStation
                {
                    Id = SelectedStation.Id,
                    Name = SelectedStation.Name,
                    City = SelectedStation.City,
                    District = SelectedStation.District,
                    Latitude = SelectedStation.Latitude,
                    Longitude = SelectedStation.Longitude
                };

                var updatedStation = new MonitoringStation
                {
                    Id = SelectedStation.Id,
                    Name = NewStationName,
                    City = NewStationCity,
                    District = NewStationDistrict,
                    Latitude = double.Parse(NewStationLatitude),
                    Longitude = double.Parse(NewStationLongitude)
                };

                var command = new UpdateStationCommand(stationRepo, oldStation, updatedStation);
                commandManager.ExecuteCommand(command);

                RefreshCollections();
                UpdateChart();

                logger.Log($"Station updated: {updatedStation.Name}");
                MessageBox.Show("Station updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating station: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error updating station: {ex.Message}");
            }
        }

        private void DeleteStation()
        {
            try
            {
                if (SelectedStation == null) return;

                var stationToDelete = SelectedStation;
                var stationName = stationToDelete.Name;

                var result = MessageBox.Show(
                    $"Are you sure you want to delete station '{stationName}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                var command = new RemoveStationCommand(stationRepo, stationToDelete);
                commandManager.ExecuteCommand(command);

                Stations.Remove(stationToDelete);
                FilterStations();
                UpdateChart();

                ClearStationInputs();

                logger.Log($"Station deleted: {stationName}");
                MessageBox.Show("Station deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting station: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error deleting station: {ex.Message}");
            }
        }

        private bool ValidateStationInput(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(NewStationName))
            {
                errorMessage = "Station name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewStationCity))
            {
                errorMessage = "City is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewStationDistrict))
            {
                errorMessage = "District is required.";
                return false;
            }

            if (!double.TryParse(NewStationLatitude, out var lat) || lat < -90 || lat > 90)
            {
                errorMessage = "Latitude must be a number between -90 and 90.";
                return false;
            }

            if (!double.TryParse(NewStationLongitude, out var lon) || lon < -180 || lon > 180)
            {
                errorMessage = "Longitude must be a number between -180 and 180.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ClearStationInputs()
        {
            NewStationName = string.Empty;
            NewStationCity = string.Empty;
            NewStationDistrict = string.Empty;
            NewStationLatitude = string.Empty;
            NewStationLongitude = string.Empty;
        }

        #endregion

        #region Reading Methods

        private void AddReading()
        {
            try
            {
                if (!ValidateReadingInput(out var errorMessage))
                {
                    MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var reading = new AirQualityReading
                {
                    StationId = Guid.Parse(NewReadingStationId),
                    ReadingTime = NewReadingTime,
                    PM25 = double.Parse(NewReadingPM25),
                    NO2Level = double.Parse(NewReadingNO2),
                    OzoneLevel = double.Parse(NewReadingOzone)
                };

                reading.EvaluateState();

                var command = new AddReadingCommand(readingRepo, reading);
                commandManager.ExecuteCommand(command);

                Readings.Add(reading);
                FilterReadings();
                UpdateChart();

                ClearReadingInputs();

                logger.Log($"Reading added for StationId: {reading.StationId}");
                MessageBox.Show("Reading added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding reading: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error adding reading: {ex.Message}");
            }
        }

        private void UpdateReading()
        {
            try
            {
                if (SelectedReading == null) return;

                if (!ValidateReadingInput(out var errorMessage))
                {
                    MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var oldReading = new AirQualityReading
                {
                    Id = SelectedReading.Id,
                    StationId = SelectedReading.StationId,
                    ReadingTime = SelectedReading.ReadingTime,
                    PM25 = SelectedReading.PM25,
                    NO2Level = SelectedReading.NO2Level,
                    OzoneLevel = SelectedReading.OzoneLevel,
                    State = SelectedReading.State
                };

                var updatedReading = new AirQualityReading
                {
                    Id = SelectedReading.Id,
                    StationId = Guid.Parse(NewReadingStationId),
                    ReadingTime = NewReadingTime,
                    PM25 = double.Parse(NewReadingPM25),
                    NO2Level = double.Parse(NewReadingNO2),
                    OzoneLevel = double.Parse(NewReadingOzone)
                };

                updatedReading.EvaluateState();

                var command = new UpdateReadingCommand(readingRepo, oldReading, updatedReading);
                commandManager.ExecuteCommand(command);

                RefreshCollections();
                UpdateChart();

                logger.Log($"Reading updated for StationId: {updatedReading.StationId}");
                MessageBox.Show("Reading updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating reading: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error updating reading: {ex.Message}");
            }
        }

        private void DeleteReading()
        {
            try
            {
                if (SelectedReading == null) return;

                var readingToDelete = SelectedReading;
                var stationId = readingToDelete.StationId;

                var result = MessageBox.Show(
                    "Are you sure you want to delete this reading?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                var command = new RemoveReadingCommand(readingRepo, readingToDelete);
                commandManager.ExecuteCommand(command);

                Readings.Remove(readingToDelete);
                FilterReadings();
                UpdateChart();

                ClearReadingInputs();

                logger.Log($"Reading deleted for StationId: {stationId}");
                MessageBox.Show("Reading deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting reading: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error deleting reading: {ex.Message}");
            }
        }

        private bool ValidateReadingInput(out string errorMessage)
        {
            if (!Guid.TryParse(NewReadingStationId, out _))
            {
                errorMessage = "Invalid Station ID. Must be a valid GUID.";
                return false;
            }

            if (!double.TryParse(NewReadingPM25, out var pm25) || pm25 < 0)
            {
                errorMessage = "PM2.5 must be a non-negative number.";
                return false;
            }

            if (!double.TryParse(NewReadingNO2, out var no2) || no2 < 0)
            {
                errorMessage = "NO2 Level must be a non-negative number.";
                return false;
            }

            if (!double.TryParse(NewReadingOzone, out var ozone) || ozone < 0)
            {
                errorMessage = "Ozone Level must be a non-negative number.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        private void ClearReadingInputs()
        {
            NewReadingStationId = string.Empty;
            NewReadingTime = DateTime.Now;
            NewReadingPM25 = string.Empty;
            NewReadingNO2 = string.Empty;
            NewReadingOzone = string.Empty;
        }

        #endregion

        #region Filter Methods

        private void FilterStations()
        {
            FilteredStations.Clear();

            var filtered = Stations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(StationSearchText))
            {
                var search = StationSearchText.ToLower();
                filtered = filtered.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    s.City.ToLower().Contains(search) ||
                    s.District.ToLower().Contains(search) ||
                    s.Latitude.ToString().Contains(search) ||
                    s.Longitude.ToString().Contains(search));
            }

            foreach (var station in filtered)
                FilteredStations.Add(station);
        }

        private void FilterReadings()
        {
            FilteredReadings.Clear();

            var filtered = Readings.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(ReadingSearchText))
            {
                var search = ReadingSearchText.ToLower();
                filtered = filtered.Where(r =>
                    r.StationId.ToString().ToLower().Contains(search) ||
                    r.ReadingTime.ToString().ToLower().Contains(search) ||
                    r.PM25.ToString().Contains(search) ||
                    r.NO2Level.ToString().Contains(search) ||
                    r.OzoneLevel.ToString().Contains(search) ||
                    r.State.ToString().ToLower().Contains(search));
            }

            foreach (var reading in filtered)
                FilteredReadings.Add(reading);
        }

        #endregion

        #region Undo/Redo

        private void Undo()
        {
            commandManager.Undo();
            RefreshCollections();
            UpdateChart();
            logger.Log("Undo executed");
        }

        private void Redo()
        {
            commandManager.Redo();
            RefreshCollections();
            UpdateChart();
            logger.Log("Redo executed");
        }

        #endregion

        #region Save/Load

        private void SaveData()
        {
            try
            {
                var data = new PersistenceData
                {
                    Stations = stationRepo.GetAll().ToList(),
                    Readings = readingRepo.GetAll().ToList()
                };

                persistenceContext.Save(data);

                logger.Log("Data saved to file");
                MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error saving data: {ex.Message}");
            }
        }

        private void LoadData()
        {
            try
            {
                var data = persistenceContext.Load();

                if (data == null || data.Stations.Count == 0)
                {
                    MessageBox.Show("No data found to load.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                stationRepo.LoadAll(data.Stations);
                readingRepo.LoadAll(data.Readings);

                RefreshCollections();
                UpdateChart();

                logger.Log("Data loaded from file");
                MessageBox.Show("Data loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                logger.Log($"Error loading data: {ex.Message}");
            }
        }

        #endregion

        #region Chart

        private void UpdateChart()
        {
            var goodCount = Readings.Count(r => r.State == AirQualityState.Good);
            var moderateCount = Readings.Count(r => r.State == AirQualityState.Moderate);
            var unhealthyCount = Readings.Count(r => r.State == AirQualityState.Unhealthy);
            var hazardousCount = Readings.Count(r => r.State == AirQualityState.Hazardous);

            System.Diagnostics.Debug.WriteLine($"Chart Update: Good={goodCount}, Moderate={moderateCount}, Unhealthy={unhealthyCount}, Hazardous={hazardousCount}");

            var seriesList = new List<ISeries>();

            if (goodCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { goodCount },
                    Name = $"Good ({goodCount})"
                });
            }

            if (moderateCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { moderateCount },
                    Name = $"Moderate ({moderateCount})"
                });
            }

            if (unhealthyCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { unhealthyCount },
                    Name = $"Unhealthy ({unhealthyCount})"
                });
            }

            if (hazardousCount > 0)
            {
                seriesList.Add(new PieSeries<int>
                {
                    Values = new[] { hazardousCount },
                    Name = $"Hazardous ({hazardousCount})"
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
            OnPropertyChanged(nameof(StateSeries));
        }

        #endregion

        #region State Simulation

        private void OnReadingStateChanged(AirQualityReading reading)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existingReading = Readings.FirstOrDefault(r => r.Id == reading.Id);
                if (existingReading != null)
                {
                    RefreshCollections();
                    UpdateChart();
                }
            });
        }

        #endregion
    }
}