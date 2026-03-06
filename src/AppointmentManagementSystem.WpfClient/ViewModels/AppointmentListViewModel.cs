using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Helpers;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Services;
using AppointmentManagementSystem.WpfClient.Views;

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
        private readonly AppointmentCreateViewModel _createViewModel;
        private ObservableCollection<AppointmentItemViewModel> _appointments;
        private DateTime _selectedDate;
        private bool _isLoading;
        private string _errorMessage;
        private bool _isDrawerOpen;
        private AppointmentItemViewModel _selectedAppointment;

        public ObservableCollection<AppointmentItemViewModel> Appointments
        {
            get => _appointments;
            set => SetProperty(ref _appointments, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    // Auto-reload appointments when date is changed
                    _ = LoadAppointmentsAsync();
                }
            }
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

        public bool IsDrawerOpen
        {
            get => _isDrawerOpen;
            set => SetProperty(ref _isDrawerOpen, value);
        }

        public AppointmentItemViewModel SelectedAppointment
        {
            get => _selectedAppointment;
            set => SetProperty(ref _selectedAppointment, value);
        }

        public AppointmentCreateViewModel CreateViewModel => _createViewModel;

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand AddAppointmentCommand { get; }
        public ICommand OpenCreateDrawerCommand { get; }
        public ICommand CloseDrawerCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }

        public AppointmentListViewModel(IAppointmentApiClient appointmentApiClient, AppointmentCreateViewModel createViewModel)
        {
            _appointmentApiClient = appointmentApiClient ?? throw new ArgumentNullException(nameof(appointmentApiClient));
            _createViewModel = createViewModel ?? throw new ArgumentNullException(nameof(createViewModel));
            Appointments = new ObservableCollection<AppointmentItemViewModel>();
            SelectedDate = DateTime.Today;
            LoadAppointmentsCommand = new AsyncRelayCommand(LoadAppointmentsAsync);
            AddAppointmentCommand = new RelayCommand(_ => OpenCreateDrawer());
            OpenCreateDrawerCommand = new RelayCommand(_ => OpenCreateDrawer());
            CloseDrawerCommand = new RelayCommand(_ => CloseDrawer());
            EditAppointmentCommand = new RelayCommand<AppointmentItemViewModel>(EditAppointment);
            DeleteAppointmentCommand = new RelayCommand<AppointmentItemViewModel>(DeleteAppointment);

            // Auto-load appointments for today on initialization
            _ = LoadAppointmentsAsync();
        }

        private async System.Threading.Tasks.Task LoadAppointmentsAsync()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var appointments = await _appointmentApiClient.GetAppointmentsAsync(SelectedDate);
                
                // Clear and repopulate collection
                Appointments.Clear();
                foreach (var appointment in appointments)
                {
                    // Resolve patient name from static lookup
                    var patientName = PatientLookup.GetName(appointment.PatientId);
                    
                    Appointments.Add(new AppointmentItemViewModel
                    {
                        Id = appointment.Id,
                        PatientId = appointment.PatientId,
                        PatientName = patientName,
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

        private void OpenCreateDrawer()
        {
            // Reset selected appointment for new creation
            SelectedAppointment = null;

            // Reset the create view model
            _createViewModel.SelectedPatient = null;
            _createViewModel.Title = string.Empty;
            _createViewModel.Notes = string.Empty;
            _createViewModel.StartTime = DateTimeOffset.Now.Date.AddHours(9);
            _createViewModel.EndTime = DateTimeOffset.Now.Date.AddHours(10);
            _createViewModel.ErrorMessage = string.Empty;

            // Open drawer
            IsDrawerOpen = true;
        }

        private void CloseDrawer()
        {
            IsDrawerOpen = false;
            SelectedAppointment = null;
        }

        private void EditAppointment(AppointmentItemViewModel appointment)
        {
            SelectedAppointment = appointment;

            // Populate the create view model with appointment data for editing
            _createViewModel.Title = appointment.Title;
            _createViewModel.Notes = string.Empty;
            _createViewModel.StartTime = appointment.StartTime;
            _createViewModel.EndTime = appointment.EndTime;
            _createViewModel.ErrorMessage = string.Empty;

            // Find and select the patient by ID
            var selectedPatient = _createViewModel.Patients.FirstOrDefault(p => p.Id == appointment.PatientId);
            _createViewModel.SelectedPatient = selectedPatient;

            // Open drawer for editing
            IsDrawerOpen = true;
        }

        private void DeleteAppointment(AppointmentItemViewModel appointment)
        {
            Appointments.Remove(appointment);
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
