using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Services;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing the list of appointments.
    /// Receives AppointmentApiClient via constructor injection.
    /// No business logic - only orchestrates service calls and updates UI state.
    /// </summary>
    public class AppointmentListViewModel : ViewModelBase
    {
        private readonly IAppointmentApiClient _appointmentApiClient;
        private ObservableCollection<AppointmentItemViewModel> _appointments;
        private bool _isLoading;
        private string _errorMessage;

        public ObservableCollection<AppointmentItemViewModel> Appointments
        {
            get => _appointments;
            set => SetProperty(ref _appointments, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand AddAppointmentCommand { get; }

        public AppointmentListViewModel(IAppointmentApiClient appointmentApiClient)
        {
            _appointmentApiClient = appointmentApiClient ?? throw new ArgumentNullException(nameof(appointmentApiClient));
            Appointments = new ObservableCollection<AppointmentItemViewModel>();
            LoadAppointmentsCommand = new AsyncRelayCommand(LoadAppointmentsAsync);
            AddAppointmentCommand = new RelayCommand(_ => AddAppointment());
        }

        private async System.Threading.Tasks.Task LoadAppointmentsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var appointments = await _appointmentApiClient.GetAppointmentsAsync(DateTime.Now.Date);
                
                // Clear and repopulate collection
                Appointments.Clear();
                foreach (var appointment in appointments)
                {
                    Appointments.Add(new AppointmentItemViewModel
                    {
                        Id = appointment.Id,
                        PatientId = appointment.PatientId,
                        PatientName = appointment.PatientName,
                        StartTime = appointment.StartTime,
                        EndTime = appointment.EndTime,
                        Title = appointment.Title
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading appointments: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddAppointment()
        {
            // TODO: Navigate to appointment creation view
            // Implementation deferred
        }
    }

    /// <summary>
    /// ViewModel for a single appointment item in the list.
    /// </summary>
    public class AppointmentItemViewModel : ViewModelBase
    {
        private Guid _id;
        private Guid _patientId;
        private string _patientName;
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        private string _title;

        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public Guid PatientId
        {
            get => _patientId;
            set => SetProperty(ref _patientId, value);
        }

        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        public DateTimeOffset StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTimeOffset EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}
