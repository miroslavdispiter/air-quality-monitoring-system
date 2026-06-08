using AirQualityInformationSystem.Commands;
using AirQualityInformationSystem.Helpers;
using AirQualityInformationSystem.Repositories;
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

        #endregion

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

            MonitoringStationsVM.OnDataChanged += OnDataChanged;
            AirQualityReadingsVM.OnDataChanged += OnDataChanged;

            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            SaveDataCommand = new RelayCommand(SaveData);
            LoadDataCommand = new RelayCommand(LoadData);

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

            logger.Log("Application started");
        }

        ~MainViewModel()
        {
            wcfServiceHost?.Stop();
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