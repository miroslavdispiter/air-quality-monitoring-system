using AirQualityInformationSystem.Commands;
using AirQualityInformationSystem.Helpers;
using AirQualityInformationSystem.Models;
using AirQualityInformationSystem.Repositories;
using AirQualityInformationSystem.Services.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace AirQualityInformationSystem.ViewModels
{
    public class MonitoringStationsViewModel : BaseViewModel
    {
        private readonly MonitoringStationRepository stationRepo;
        private readonly CommandManager commandManager;
        private readonly LoggerService logger;

        #region Observable Collections

        public ObservableCollection<MonitoringStation> Stations { get; set; }

        private ObservableCollection<MonitoringStation> filteredStations;
        public ObservableCollection<MonitoringStation> FilteredStations
        {
            get => filteredStations;
            set { filteredStations = value; OnPropertyChanged(); }
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

        #region Commands

        public RelayCommand AddStationCommand { get; }
        public RelayCommand UpdateStationCommand { get; }
        public RelayCommand DeleteStationCommand { get; }

        #endregion

        public event Action OnDataChanged;

        public MonitoringStationsViewModel(
            MonitoringStationRepository stationRepo,
            CommandManager commandManager,
            LoggerService logger)
        {
            this.stationRepo = stationRepo;
            this.commandManager = commandManager;
            this.logger = logger;

            Stations = new ObservableCollection<MonitoringStation>();
            FilteredStations = new ObservableCollection<MonitoringStation>();

            AddStationCommand = new RelayCommand(AddStation);
            UpdateStationCommand = new RelayCommand(UpdateStation, () => SelectedStation != null);
            DeleteStationCommand = new RelayCommand(DeleteStation, () => SelectedStation != null);

            RefreshStations();
        }

        public void RefreshStations()
        {
            Stations.Clear();
            foreach (var station in stationRepo.GetAll())
                Stations.Add(station);

            FilterStations();
        }

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
                ClearStationInputs();

                logger.Log($"Station added: {station.Name}");
                MessageBox.Show("Station added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnDataChanged?.Invoke();
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

                RefreshStations();

                logger.Log($"Station updated: {updatedStation.Name}");
                MessageBox.Show("Station updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnDataChanged?.Invoke();
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
                ClearStationInputs();

                logger.Log($"Station deleted: {stationName}");
                MessageBox.Show("Station deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnDataChanged?.Invoke();
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
    }
}