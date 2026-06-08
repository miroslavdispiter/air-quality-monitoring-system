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
    public class AirQualityReadingsViewModel : BaseViewModel
    {
        private readonly AirQualityRepository readingRepo;
        private readonly CommandManager commandManager;
        private readonly LoggerService logger;

        #region Observable Collections

        public ObservableCollection<AirQualityReading> Readings { get; set; }

        private ObservableCollection<AirQualityReading> filteredReadings;
        public ObservableCollection<AirQualityReading> FilteredReadings
        {
            get => filteredReadings;
            set { filteredReadings = value; OnPropertyChanged(); }
        }

        #endregion

        #region Selected Items

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
                    NewReadingDate = value.ReadingTime.Date;
                    NewReadingTimeString = value.ReadingTime.ToString("HH:mm");
                    NewReadingPM25 = value.PM25.ToString();
                    NewReadingNO2 = value.NO2Level.ToString();
                    NewReadingOzone = value.OzoneLevel.ToString();
                }
            }
        }

        #endregion

        #region Search Properties

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

        private DateTime newReadingDate = DateTime.Now.Date;
        public DateTime NewReadingDate
        {
            get => newReadingDate;
            set
            {
                newReadingDate = value;
                UpdateNewReadingTime();
                OnPropertyChanged();
            }
        }

        private string newReadingTimeString = DateTime.Now.ToString("HH:mm");
        public string NewReadingTimeString
        {
            get => newReadingTimeString;
            set
            {
                newReadingTimeString = value;
                UpdateNewReadingTime();
                OnPropertyChanged();
            }
        }

        private void UpdateNewReadingTime()
        {
            if (TimeSpan.TryParse(NewReadingTimeString, out var time))
            {
                NewReadingTime = NewReadingDate.Date.Add(time);
            }
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

        #region Commands

        public RelayCommand AddReadingCommand { get; }
        public RelayCommand UpdateReadingCommand { get; }
        public RelayCommand DeleteReadingCommand { get; }

        #endregion

        public event Action OnDataChanged;

        public AirQualityReadingsViewModel(
            AirQualityRepository readingRepo,
            CommandManager commandManager,
            LoggerService logger)
        {
            this.readingRepo = readingRepo;
            this.commandManager = commandManager;
            this.logger = logger;

            Readings = new ObservableCollection<AirQualityReading>();
            FilteredReadings = new ObservableCollection<AirQualityReading>();

            AddReadingCommand = new RelayCommand(AddReading);
            UpdateReadingCommand = new RelayCommand(UpdateReading, () => SelectedReading != null);
            DeleteReadingCommand = new RelayCommand(DeleteReading, () => SelectedReading != null);

            RefreshReadings();
        }

        public void RefreshReadings()
        {
            Readings.Clear();
            foreach (var reading in readingRepo.GetAll())
                Readings.Add(reading);

            FilterReadings();
        }

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
                ClearReadingInputs();

                logger.Log($"Reading added for StationId: {reading.StationId}");
                MessageBox.Show("Reading added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnDataChanged?.Invoke();
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

                RefreshReadings();

                logger.Log($"Reading updated for StationId: {updatedReading.StationId}");
                MessageBox.Show("Reading updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnDataChanged?.Invoke();
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
                ClearReadingInputs();

                logger.Log($"Reading deleted for StationId: {stationId}");
                MessageBox.Show("Reading deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnDataChanged?.Invoke();
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
            NewReadingDate = DateTime.Now.Date;
            NewReadingTimeString = DateTime.Now.ToString("HH:mm");
            NewReadingPM25 = string.Empty;
            NewReadingNO2 = string.Empty;
            NewReadingOzone = string.Empty;
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
                    r.ReadingTime.ToString("yyyy-MM-dd HH:mm:ss").Contains(search) ||
                    r.ReadingTime.ToString("dd/MM/yyyy").Contains(search) ||
                    r.ReadingTime.ToString("MM/yyyy").Contains(search) ||
                    r.PM25.ToString().Contains(search) ||
                    r.NO2Level.ToString().Contains(search) ||
                    r.OzoneLevel.ToString().Contains(search) ||
                    r.State.ToString().ToLower().Contains(search));
            }

            foreach (var reading in filtered)
                FilteredReadings.Add(reading);
        }
    }
}