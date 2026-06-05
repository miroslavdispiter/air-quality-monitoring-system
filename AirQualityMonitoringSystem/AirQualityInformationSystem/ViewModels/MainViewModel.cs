using AirQualityInformationSystem.Helpers;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AirQualityInformationSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IDataService _dataService;
        private readonly IPersistenceService _persistenceService;
        private readonly ILoggingService _loggingService;
        private readonly UndoRedoManager _undoRedoManager;
        private readonly StateSimulationService _stateSimulationService;
        private readonly Dictionary<Guid, CancellationTokenSource> _simulationTokens;

        private ObservableCollection<MonitoringStation> _stations;
        private ObservableCollection<AirQualityReading> _readings;
        private ObservableCollection<MonitoringStation> _filteredStations;
        private ObservableCollection<AirQualityReading> _filteredReadings;

        private MonitoringStation _selectedStation;
        private AirQualityReading _selectedReading;

        private string _newStationName;
        private string _newStationCity;
        private string _newStationDistrict;
        private string _newStationLatitude;
        private string _newStationLongitude;

        private string _newReadingStationId;
        private DateTime _newReadingTime = DateTime.Now;
        private string _newReadingPM25;
        private string _newReadingNO2;
        private string _newReadingOzone;

        // Search filters
        private string _stationSearchText;
        private string _readingSearchText;

        // Charts
        private ObservableCollection<ISeries> _stateSeries;

        // Commands
        public ICommand AddStationCommand { get; private set; }
        public ICommand UpdateStationCommand { get; private set; }
        public ICommand DeleteStationCommand { get; private set; }
        public ICommand AddReadingCommand { get; private set; }
        public ICommand UpdateReadingCommand { get; private set; }
        public ICommand DeleteReadingCommand { get; private set; }
        public ICommand UndoCommand { get; private set; }
        public ICommand RedoCommand { get; private set; }
        public ICommand SaveDataCommand { get; private set; }
        public ICommand LoadDataCommand { get; private set; }
        public ICommand SearchStationsCommand { get; private set; }
        public ICommand SearchReadingsCommand { get; private set; }

        public MainViewModel()
        {
            _dataService = new DataService();
            _persistenceService = new PersistenceService();
            _loggingService = new LoggingService();
            _undoRedoManager = new UndoRedoManager();
            _stateSimulationService = new StateSimulationService();
            _simulationTokens = new Dictionary<Guid, CancellationTokenSource>();

            _stations = new ObservableCollection<MonitoringStation>();
            _readings = new ObservableCollection<AirQualityReading>();
            _filteredStations = new ObservableCollection<MonitoringStation>();
            _filteredReadings = new ObservableCollection<AirQualityReading>();

            InitializeCommands();
            LoadInitialData();
            InitializeChart();
        }

        #region Properties

        public ObservableCollection<MonitoringStation> Stations
        {
            get => _stations;
            set => SetProperty(ref _stations, value);
        }

        public ObservableCollection<AirQualityReading> Readings
        {
            get => _readings;
            set => SetProperty(ref _readings, value);
        }

        public ObservableCollection<MonitoringStation> FilteredStations
        {
            get => _filteredStations;
            set => SetProperty(ref _filteredStations, value);
        }

        public ObservableCollection<AirQualityReading> FilteredReadings
        {
            get => _filteredReadings;
            set => SetProperty(ref _filteredReadings, value);
        }

        public MonitoringStation SelectedStation
        {
            get => _selectedStation;
            set => SetProperty(ref _selectedStation, value);
        }

        public AirQualityReading SelectedReading
        {
            get => _selectedReading;
            set => SetProperty(ref _selectedReading, value);
        }

        public string NewStationName
        {
            get => _newStationName;
            set => SetProperty(ref _newStationName, value);
        }

        public string NewStationCity
        {
            get => _newStationCity;
            set => SetProperty(ref _newStationCity, value);
        }

        public string NewStationDistrict
        {
            get => _newStationDistrict;
            set => SetProperty(ref _newStationDistrict, value);
        }

        public string NewStationLatitude
        {
            get => _newStationLatitude;
            set => SetProperty(ref _newStationLatitude, value);
        }

        public string NewStationLongitude
        {
            get => _newStationLongitude;
            set => SetProperty(ref _newStationLongitude, value);
        }

        public string NewReadingStationId
        {
            get => _newReadingStationId;
            set => SetProperty(ref _newReadingStationId, value);
        }

        public DateTime NewReadingTime
        {
            get => _newReadingTime;
            set => SetProperty(ref _newReadingTime, value);
        }

        public string NewReadingPM25
        {
            get => _newReadingPM25;
            set => SetProperty(ref _newReadingPM25, value);
        }

        public string NewReadingNO2
        {
            get => _newReadingNO2;
            set => SetProperty(ref _newReadingNO2, value);
        }

        public string NewReadingOzone
        {
            get => _newReadingOzone;
            set => SetProperty(ref _newReadingOzone, value);
        }

        public string StationSearchText
        {
            get => _stationSearchText;
            set
            {
                SetProperty(ref _stationSearchText, value);
                FilterStations();
            }
        }

        public string ReadingSearchText
        {
            get => _readingSearchText;
            set
            {
                SetProperty(ref _readingSearchText, value);
                FilterReadings();
            }
        }

        public ObservableCollection<ISeries> StateSeries
        {
            get => _stateSeries;
            set => SetProperty(ref _stateSeries, value);
        }

        #endregion

        #region Initialization

        private void InitializeCommands()
        {
            AddStationCommand = new RelayCommand(ExecuteAddStation, CanExecuteAddStation);
            UpdateStationCommand = new RelayCommand(ExecuteUpdateStation, CanExecuteUpdateStation);
            DeleteStationCommand = new RelayCommand(ExecuteDeleteStation, CanExecuteDeleteStation);
            AddReadingCommand = new RelayCommand(ExecuteAddReading, CanExecuteAddReading);
            UpdateReadingCommand = new RelayCommand(ExecuteUpdateReading, CanExecuteUpdateReading);
            DeleteReadingCommand = new RelayCommand(ExecuteDeleteReading, CanExecuteDeleteReading);
            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
            SaveDataCommand = new RelayCommand(ExecuteSaveData);
            LoadDataCommand = new RelayCommand(ExecuteLoadData);
            SearchStationsCommand = new RelayCommand(_ => FilterStations());
            SearchReadingsCommand = new RelayCommand(_ => FilterReadings());
        }

        private void LoadInitialData()
        {
            try
            {
                var (stations, readings) = _persistenceService.LoadData();

                if (stations.Count == 0)
                {
                    stations = GetDefaultStations();
                    readings = GetDefaultReadings(stations);
                }

                _dataService.SetData(stations, readings);

                foreach (var station in stations)
                {
                    _stations.Add(station);
                    _filteredStations.Add(station);
                }

                foreach (var reading in readings)
                {
                    _readings.Add(reading);
                    _filteredReadings.Add(reading);
                    StartStateSimulation(reading);
                }

                _loggingService.LogActivity("Application started and data loaded successfully");
                UpdateChart();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _loggingService.LogActivity($"Error loading data: {ex.Message}");
            }
        }

        private List<MonitoringStation> GetDefaultStations()
        {
            return new List<MonitoringStation>
            {
                new MonitoringStation
                {
                    Id = Guid.NewGuid(),
                    Name = "Central Station",
                    City = "Belgrade",
                    District = "Stari Grad",
                    Latitude = 44.8176,
                    Longitude = 20.4569
                },
                new MonitoringStation
                {
                    Id = Guid.NewGuid(),
                    Name = "North Station",
                    City = "Novi Sad",
                    District = "Center",
                    Latitude = 45.2671,
                    Longitude = 19.8335
                },
                new MonitoringStation
                {
                    Id = Guid.NewGuid(),
                    Name = "South Station",
                    City = "Niš",
                    District = "Medijana",
                    Latitude = 43.3209,
                    Longitude = 21.8958
                }
            };
        }

        private List<AirQualityReading> GetDefaultReadings(List<MonitoringStation> stations)
        {
            var readings = new List<AirQualityReading>();
            var random = new Random();

            foreach (var station in stations)
            {
                for (int i = 0; i < 3; i++)
                {
                    readings.Add(new AirQualityReading
                    {
                        Id = Guid.NewGuid(),
                        StationId = station.Id,
                        ReadingTime = DateTime.Now.AddHours(-i * 2),
                        PM25 = random.Next(10, 100),
                        NO2Level = random.Next(20, 90),
                        OzoneLevel = random.Next(30, 120),
                        State = (AirQualityState)random.Next(0, 4)
                    });
                }
            }

            return readings;
        }

        private void InitializeChart()
        {
            StateSeries = new ObservableCollection<ISeries>();
            UpdateChart();
        }

        #endregion

        #region Station Commands

        private bool CanExecuteAddStation(object parameter)
        {
            return !string.IsNullOrWhiteSpace(NewStationName) &&
                   !string.IsNullOrWhiteSpace(NewStationCity) &&
                   !string.IsNullOrWhiteSpace(NewStationDistrict) &&
                   double.TryParse(NewStationLatitude, out _) &&
                   double.TryParse(NewStationLongitude, out _);
        }

        private void ExecuteAddStation(object parameter)
        {
            if (!double.TryParse(NewStationLatitude, out double latitude) ||
                !double.TryParse(NewStationLongitude, out double longitude))
            {
                MessageBox.Show("Invalid latitude or longitude values", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (latitude < -90 || latitude > 90)
            {
                MessageBox.Show("Latitude must be between -90 and 90", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (longitude < -180 || longitude > 180)
            {
                MessageBox.Show("Longitude must be between -180 and 180", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var station = new MonitoringStation
            {
                Id = Guid.NewGuid(),
                Name = NewStationName,
                City = NewStationCity,
                District = NewStationDistrict,
                Latitude = latitude,
                Longitude = longitude
            };

            var action = new AddAction<MonitoringStation>(_stations, station);
            _undoRedoManager.ExecuteAction(action);
            _dataService.AddStation(station);
            FilteredStations.Add(station);

            _loggingService.LogActivity($"Added new station: {station.Name} in {station.City}");

            ClearStationForm();
        }

        private bool CanExecuteUpdateStation(object parameter)
        {
            return SelectedStation != null;
        }

        private void ExecuteUpdateStation(object parameter)
        {
            if (SelectedStation == null) return;

            var oldStation = new MonitoringStation
            {
                Id = SelectedStation.Id,
                Name = SelectedStation.Name,
                City = SelectedStation.City,
                District = SelectedStation.District,
                Latitude = SelectedStation.Latitude,
                Longitude = SelectedStation.Longitude
            };

            _dataService.UpdateStation(SelectedStation);
            _loggingService.LogActivity($"Updated station: {SelectedStation.Name}");

            UpdateChart();
        }

        private bool CanExecuteDeleteStation(object parameter)
        {
            return SelectedStation != null;
        }

        private void ExecuteDeleteStation(object parameter)
        {
            if (SelectedStation == null) return;

            var stationToDelete = SelectedStation;
            var stationName = stationToDelete.Name;

            var result = MessageBox.Show($"Are you sure you want to delete station '{SelectedStation.Name}'?",
                                       "Confirm Delete",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var action = new RemoveAction<MonitoringStation>(_stations, stationToDelete);
                _undoRedoManager.ExecuteAction(action);
                _dataService.RemoveStation(stationToDelete);
                FilteredStations.Remove(stationToDelete);

                _loggingService.LogActivity($"Deleted station: {stationName}");
                SelectedStation = null;
            }
        }

        private void ClearStationForm()
        {
            NewStationName = string.Empty;
            NewStationCity = string.Empty;
            NewStationDistrict = string.Empty;
            NewStationLatitude = string.Empty;
            NewStationLongitude = string.Empty;
        }

        #endregion

        #region Reading Commands

        private bool CanExecuteAddReading(object parameter)
        {
            return Guid.TryParse(NewReadingStationId, out _) &&
                   double.TryParse(NewReadingPM25, out _) &&
                   double.TryParse(NewReadingNO2, out _) &&
                   double.TryParse(NewReadingOzone, out _);
        }

        private void ExecuteAddReading(object parameter)
        {
            if (!Guid.TryParse(NewReadingStationId, out Guid stationId) ||
                !double.TryParse(NewReadingPM25, out double pm25) ||
                !double.TryParse(NewReadingNO2, out double no2) ||
                !double.TryParse(NewReadingOzone, out double ozone))
            {
                MessageBox.Show("Invalid input values", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (pm25 < 0 || no2 < 0 || ozone < 0)
            {
                MessageBox.Show("Concentration values must be positive", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_stations.Any(s => s.Id == stationId))
            {
                MessageBox.Show("Station with given ID does not exist", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reading = new AirQualityReading
            {
                Id = Guid.NewGuid(),
                StationId = stationId,
                ReadingTime = NewReadingTime,
                PM25 = pm25,
                NO2Level = no2,
                OzoneLevel = ozone,
                State = AirQualityState.Good
            };

            var action = new AddAction<AirQualityReading>(_readings, reading);
            _undoRedoManager.ExecuteAction(action);
            _dataService.AddReading(reading);
            FilteredReadings.Add(reading);

            StartStateSimulation(reading);

            _loggingService.LogActivity($"Added new reading for station {stationId} at {reading.ReadingTime}");

            ClearReadingForm();
            UpdateChart();
        }

        private bool CanExecuteUpdateReading(object parameter)
        {
            return SelectedReading != null;
        }

        private void ExecuteUpdateReading(object parameter)
        {
            if (SelectedReading == null) return;

            _dataService.UpdateReading(SelectedReading);
            _loggingService.LogActivity($"Updated reading with ID: {SelectedReading.Id}");

            UpdateChart();
        }

        private bool CanExecuteDeleteReading(object parameter)
        {
            return SelectedReading != null;
        }

        private void ExecuteDeleteReading(object parameter)
        {
            if (SelectedReading == null) return;

            var readingToDelete = SelectedReading;
            var readingId = readingToDelete.Id;

            var result = MessageBox.Show($"Are you sure you want to delete this reading?",
                                       "Confirm Delete",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (result == MessageBoxResult.Yes)
                {
                    if (_simulationTokens.ContainsKey(readingId))
                    {
                        _simulationTokens[readingId].Cancel();
                        _simulationTokens.Remove(readingId);
                    }

                    var action = new RemoveAction<AirQualityReading>(_readings, readingToDelete);
                    _undoRedoManager.ExecuteAction(action);
                    _dataService.RemoveReading(readingToDelete);
                    FilteredReadings.Remove(readingToDelete);

                    _loggingService.LogActivity($"Deleted reading with ID: {readingId}");
                    SelectedReading = null;

                    UpdateChart();
                }
            }
        }

        private void ClearReadingForm()
        {
            NewReadingStationId = string.Empty;
            NewReadingTime = DateTime.Now;
            NewReadingPM25 = string.Empty;
            NewReadingNO2 = string.Empty;
            NewReadingOzone = string.Empty;
        }

        #endregion

        #region Undo/Redo Commands

        private bool CanExecuteUndo(object parameter)
        {
            return _undoRedoManager.CanUndo;
        }

        private void ExecuteUndo(object parameter)
        {
            _undoRedoManager.Undo();
            _loggingService.LogActivity("Undo action executed");
            FilterStations();
            FilterReadings();
            UpdateChart();
        }

        private bool CanExecuteRedo(object parameter)
        {
            return _undoRedoManager.CanRedo;
        }

        private void ExecuteRedo(object parameter)
        {
            _undoRedoManager.Redo();
            _loggingService.LogActivity("Redo action executed");
            FilterStations();
            FilterReadings();
            UpdateChart();
        }

        #endregion

        #region Persistence Commands

        private void ExecuteSaveData(object parameter)
        {
            try
            {
                _persistenceService.SaveData(_stations.ToList(), _readings.ToList());
                _loggingService.LogActivity("Data saved successfully");
                MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _loggingService.LogActivity($"Error saving data: {ex.Message}");
            }
        }

        private void ExecuteLoadData(object parameter)
        {
            try
            {
                var (stations, readings) = _persistenceService.LoadData();

                // Cancel all simulations
                foreach (var token in _simulationTokens.Values)
                {
                    token.Cancel();
                }
                _simulationTokens.Clear();

                _stations.Clear();
                _readings.Clear();
                _filteredStations.Clear();
                _filteredReadings.Clear();

                _dataService.SetData(stations, readings);

                foreach (var station in stations)
                {
                    _stations.Add(station);
                    _filteredStations.Add(station);
                }

                foreach (var reading in readings)
                {
                    _readings.Add(reading);
                    _filteredReadings.Add(reading);
                    StartStateSimulation(reading);
                }

                _loggingService.LogActivity("Data loaded successfully");
                UpdateChart();
                MessageBox.Show("Data loaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _loggingService.LogActivity($"Error loading data: {ex.Message}");
            }
        }

        #endregion

        #region Search/Filter

        private void FilterStations()
        {
            FilteredStations.Clear();

            if (string.IsNullOrWhiteSpace(StationSearchText))
            {
                foreach (var station in _stations)
                {
                    FilteredStations.Add(station);
                }
            }
            else
            {
                var searchLower = StationSearchText.ToLower();
                foreach (var station in _stations)
                {
                    if (station.Name.ToLower().Contains(searchLower) ||
                        station.City.ToLower().Contains(searchLower) ||
                        station.District.ToLower().Contains(searchLower) ||
                        station.Latitude.ToString().Contains(searchLower) ||
                        station.Longitude.ToString().Contains(searchLower))
                    {
                        FilteredStations.Add(station);
                    }
                }
            }
        }

        private void FilterReadings()
        {
            FilteredReadings.Clear();

            if (string.IsNullOrWhiteSpace(ReadingSearchText))
            {
                foreach (var reading in _readings)
                {
                    FilteredReadings.Add(reading);
                }
            }
            else
            {
                var searchLower = ReadingSearchText.ToLower();
                foreach (var reading in _readings)
                {
                    if (reading.StationId.ToString().ToLower().Contains(searchLower) ||
                        reading.ReadingTime.ToString().ToLower().Contains(searchLower) ||
                        reading.PM25.ToString().Contains(searchLower) ||
                        reading.NO2Level.ToString().Contains(searchLower) ||
                        reading.OzoneLevel.ToString().Contains(searchLower) ||
                        reading.State.ToString().ToLower().Contains(searchLower))
                    {
                        FilteredReadings.Add(reading);
                    }
                }
            }
        }

        #endregion

        #region State Simulation

        private async void StartStateSimulation(AirQualityReading reading)
        {
            var cts = new CancellationTokenSource();
            _simulationTokens[reading.Id] = cts;

            try
            {
                await _stateSimulationService.SimulateStateChanges(
                    reading,
                    state =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateChart();
                            _loggingService.LogActivity($"Reading {reading.Id} state changed to {state}");
                        });
                    },
                    cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Simulation was cancelled
            }
        }

        #endregion

        #region Chart

        private void UpdateChart()
        {
            var goodCount = _readings.Count(r => r.State == AirQualityState.Good);
            var moderateCount = _readings.Count(r => r.State == AirQualityState.Moderate);
            var unhealthyCount = _readings.Count(r => r.State == AirQualityState.Unhealthy);
            var hazardousCount = _readings.Count(r => r.State == AirQualityState.Hazardous);

            StateSeries = new ObservableCollection<ISeries>
            {
                new PieSeries<int>
                {
                    Values = new[] { goodCount },
                    Name = "Good",
                    Fill = new SolidColorPaint(SKColors.Green)
                },
                new PieSeries<int>
                {
                    Values = new[] { moderateCount },
                    Name = "Moderate",
                    Fill = new SolidColorPaint(SKColors.Yellow)
                },
                new PieSeries<int>
                {
                    Values = new[] { unhealthyCount },
                    Name = "Unhealthy",
                    Fill = new SolidColorPaint(SKColors.Orange)
                },
                new PieSeries<int>
                {
                    Values = new[] { hazardousCount },
                    Name = "Hazardous",
                    Fill = new SolidColorPaint(SKColors.Red)
                }
            };
        }

        #endregion
    }
}