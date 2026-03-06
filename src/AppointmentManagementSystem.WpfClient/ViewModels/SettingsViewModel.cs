using System;
using System.Collections.ObjectModel;
using AppointmentManagementSystem.WpfClient.Infrastructure;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for clinic settings.
    /// NOTE:
    /// Placeholder settings UI.
    /// Persistence and configuration management were not implemented
    /// due to time constraints in the technical assessment.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private string _startTime;
        private string _endTime;
        private string _closedDay;
        private string _holidayDates;

        public string StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public string EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public string ClosedDay
        {
            get => _closedDay;
            set => SetProperty(ref _closedDay, value);
        }

        public string HolidayDates
        {
            get => _holidayDates;
            set => SetProperty(ref _holidayDates, value);
        }

        public ObservableCollection<string> DaysOfWeek { get; }

        public SettingsViewModel()
        {
            // Initialize with default values
            _startTime = "08:00";
            _endTime = "18:00";
            _closedDay = "Sunday";
            _holidayDates = "Enter holiday dates here (comma-separated)";

            // Days of week for dropdown
            DaysOfWeek = new ObservableCollection<string>
            {
                "Monday",
                "Tuesday",
                "Wednesday",
                "Thursday",
                "Friday",
                "Saturday",
                "Sunday"
            };
        }
    }
}
