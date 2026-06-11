using AirQualityInformationSystem.Commands;
using AirQualityInformationSystem.Helpers;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using AirQualityInformationSystem.Services;
using AirQualityInformationSystem.Services.Logging;
using AirQualityInformationSystem.Services.Persistence;
using AirQualityInformationSystem.Services.WCF;
using System;
using System.Linq;
using System.Windows;

namespace AirQualityInformationSystem.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly MonitoringStationRepository stationRepo;
        private readonly AirQualityRepository readingRepo;
        private readonly CommandManager commandManager;
        private readonly PersistenceContext persistenceContext;
        private readonly LoggerService logger;
        private readonly WcfServiceHost wcfServiceHost;
        private readonly StateSimulationService simulationService;

        #region Child ViewModels

        public MonitoringStationsViewModel MonitoringStationsVM { get; }
        public AirQualityReadingsViewModel AirQualityReadingsVM { get; }
        public StateStatisticsViewModel StateStatisticsVM { get; }

        #endregion

        #region Commands

        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand SaveDataCommand { get; }
        public RelayCommand LoadDataCommand { get; }
        public RelayCommand StartSimulationCommand { get; }
        public RelayCommand StopSimulationCommand { get; }

        #endregion

        private bool isSimulationRunning;
        public bool IsSimulationRunning
        {
            get => isSimulationRunning;
            set
            {
                isSimulationRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SimulationStatusText));
            }
        }

        public string SimulationStatusText => IsSimulationRunning ? "🟢 Running" : "🔴 Stopped";

        public MainViewModel()
        {
            stationRepo = new MonitoringStationRepository();
            readingRepo = new AirQualityRepository();
            commandManager = new CommandManager();
            persistenceContext = new PersistenceContext();
            logger = new LoggerService();

            persistenceContext.SetStrategy(new JsonPersistenceStrategy());

            InitializeData();

            MonitoringStationsVM = new MonitoringStationsViewModel(stationRepo, commandManager, logger);
            AirQualityReadingsVM = new AirQualityReadingsViewModel(readingRepo, commandManager, logger);
            StateStatisticsVM = new StateStatisticsViewModel(readingRepo);

            StateStatisticsVM.Initialize();

            simulationService = new StateSimulationService(readingRepo);

            MonitoringStationsVM.OnDataChanged += OnDataChanged;
            AirQualityReadingsVM.OnDataChanged += OnDataChanged;
            AirQualityReadingsVM.OnReadingAdded += (reading) => StateStatisticsVM.RegisterNewReading(reading);

            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            SaveDataCommand = new RelayCommand(SaveData);
            LoadDataCommand = new RelayCommand(LoadData);
            StartSimulationCommand = new RelayCommand(StartSimulation, () => !IsSimulationRunning);
            StopSimulationCommand = new RelayCommand(StopSimulation, () => IsSimulationRunning);

            wcfServiceHost = new WcfServiceHost(readingRepo, stationRepo);
            try
            {
                wcfServiceHost.Start();
                logger.Log("WCF Service started successfully");

                var stationCount = stationRepo.GetAll().Count();
                logger.Log($"WCF Service initialized with {stationCount} stations");
            }
            catch (Exception ex)
            {
                logger.Log($"Failed to start WCF Service: {ex.Message}");
                MessageBox.Show($"Warning: WCF Service failed to start.\n{ex.Message}",
                    "WCF Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            StartSimulation();

            logger.Log("Application started");
        }

        ~MainViewModel()
        {
            wcfServiceHost?.Stop();
            simulationService?.Stop();
        }

        private void InitializeData()
        {
            var loadedData = persistenceContext.Load();

            if (loadedData != null && loadedData.Stations.Count > 0)
            {
                stationRepo.LoadAll(loadedData.Stations);
                readingRepo.LoadAll(loadedData.Readings);
                logger?.Log($"Loaded {loadedData.Stations.Count} stations and {loadedData.Readings.Count} readings from file");
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

                logger?.Log($"Loaded {defaultStations.Count} default stations and {defaultReadings.Count} default readings");
            }

            RefreshAllViewModels();

            // Debug output
            var readings = readingRepo.GetAll().ToList();
            System.Diagnostics.Debug.WriteLine($"\n📋 Initial Data:");
            System.Diagnostics.Debug.WriteLine($"  Total Readings: {readings.Count}");
            System.Diagnostics.Debug.WriteLine($"  Good: {readings.Count(r => r.State == AirQualityState.Good)}");
            System.Diagnostics.Debug.WriteLine($"  Moderate: {readings.Count(r => r.State == AirQualityState.Moderate)}");
            System.Diagnostics.Debug.WriteLine($"  Unhealthy: {readings.Count(r => r.State == AirQualityState.Unhealthy)}");
            System.Diagnostics.Debug.WriteLine($"  Hazardous: {readings.Count(r => r.State == AirQualityState.Hazardous)}\n");
        }

        private void RefreshAllViewModels()
        {
            MonitoringStationsVM?.RefreshStations();
            AirQualityReadingsVM?.RefreshReadings();
            StateStatisticsVM?.UpdateChart();
        }

        private void OnDataChanged()
        {
            StateStatisticsVM?.UpdateChart();
        }

        private void StartSimulation()
        {
            if (IsSimulationRunning) return;

            simulationService.Start();
            IsSimulationRunning = true;
            StartSimulationCommand.RaiseCanExecuteChanged();
            StopSimulationCommand.RaiseCanExecuteChanged();

            logger.Log("Simulation started");
            MessageBox.Show("Real-time simulation started!\n\n" +
                "• Values will change every 3 seconds\n" +
                "• Watch the DataGrid update in real-time\n" +
                "• Chart updates when states change",
                "Simulation Started", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void StopSimulation()
        {
            if (!IsSimulationRunning) return;

            simulationService.Stop();
            IsSimulationRunning = false;
            StartSimulationCommand.RaiseCanExecuteChanged();
            StopSimulationCommand.RaiseCanExecuteChanged();

            logger.Log("Simulation stopped");
            MessageBox.Show("Simulation stopped.", "Simulation", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #region Undo/Redo

        private void Undo()
        {
            commandManager.Undo();
            RefreshAllViewModels();
            logger.Log("Undo executed");
        }

        private void Redo()
        {
            commandManager.Redo();
            RefreshAllViewModels();
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

                StateStatisticsVM.Initialize();

                RefreshAllViewModels();

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
    }
}