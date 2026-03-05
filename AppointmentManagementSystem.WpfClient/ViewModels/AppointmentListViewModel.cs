using System.Collections.ObjectModel;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Services;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for displaying and managing the list of appointments.
    /// Receives AppointmentApiClient via constructor injection.
    /// </summary>
    public class AppointmentListViewModel : ViewModelBase
    {
        private readonly IAppointmentApiClient _appointmentApiClient;
        private ObservableCollection<AppointmentItemViewModel> _appointments;
        private bool _isLoading;

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

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand AddAppointmentCommand { get; }

        public AppointmentListViewModel(IAppointmentApiClient appointmentApiClient)
        {
            _appointmentApiClient = appointmentApiClient ?? throw new System.ArgumentNullException(nameof(appointmentApiClient));
            Appointments = new ObservableCollection<AppointmentItemViewModel>();
            LoadAppointmentsCommand = new RelayCommand(_ => LoadAppointments());
            AddAppointmentCommand = new RelayCommand(_ => AddAppointment());
        }

        private async void LoadAppointments()
        {
            // TODO: Call AppointmentApiClient to fetch appointments from API
            // Scaffold only - no API calls implemented yet
            IsLoading = true;
            try
            {
                // var response = await _appointmentApiClient.GetAppointmentsAsync(DateTime.Now);
                // MapToViewModels(response);
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
        private int _id;
        private string _patientName;
        private string _appointmentTime;
        private string _status;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string PatientName
        {
            get => _patientName;
            set => SetProperty(ref _patientName, value);
        }

        public string AppointmentTime
        {
            get => _appointmentTime;
            set => SetProperty(ref _appointmentTime, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
    }
}
