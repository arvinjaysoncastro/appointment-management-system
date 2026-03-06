using System;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Services;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for creating a new appointment.
    /// Handles form submission and API communication.
    /// </summary>
    public class AppointmentCreateViewModel : ViewModelBase
    {
        private readonly IAppointmentApiClient _appointmentApiClient;
        private Guid _patientId;
        private string _title = string.Empty;
        private string _notes = string.Empty;
        private DateTimeOffset _startTime = DateTimeOffset.Now.Date.AddHours(9);
        private DateTimeOffset _endTime = DateTimeOffset.Now.Date.AddHours(10);
        private string _errorMessage = string.Empty;
        private bool _isSubmitting;

        public Guid PatientId
        {
            get => _patientId;
            set => SetProperty(ref _patientId, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetProperty(ref _isSubmitting, value);
        }

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Event raised when appointment is successfully created.
        /// </summary>
        public event EventHandler<AppointmentCreatedEventArgs> AppointmentCreated;

        /// <summary>
        /// Event raised when user cancels the dialog.
        /// </summary>
        public event EventHandler Cancelled;

        public AppointmentCreateViewModel(IAppointmentApiClient appointmentApiClient)
        {
            _appointmentApiClient = appointmentApiClient ?? throw new ArgumentNullException(nameof(appointmentApiClient));
            CreateCommand = new AsyncRelayCommand(CreateAppointmentAsync, CanCreateAppointment);
            CancelCommand = new RelayCommand(_ => OnCancelled());
        }

        private bool CanCreateAppointment()
        {
            return !string.IsNullOrWhiteSpace(Title) 
                && PatientId != Guid.Empty 
                && EndTime > StartTime 
                && !IsSubmitting;
        }

        private async System.Threading.Tasks.Task CreateAppointmentAsync()
        {
            IsSubmitting = true;
            ErrorMessage = string.Empty;

            try
            {
                if (EndTime <= StartTime)
                {
                    ErrorMessage = "End time must be after start time.";
                    return;
                }

                var dto = new CreateAppointmentDto
                {
                    PatientId = PatientId,
                    Title = Title,
                    Notes = Notes,
                    StartTime = StartTime,
                    EndTime = EndTime
                };

                var result = await _appointmentApiClient.CreateAppointmentAsync(dto);
                OnAppointmentCreated(new AppointmentCreatedEventArgs { AppointmentId = result.Id });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error creating appointment: {ex.Message}";
            }
            finally
            {
                IsSubmitting = false;
            }
        }

        private void OnCancelled()
        {
            Cancelled?.Invoke(this, EventArgs.Empty);
        }

        private void OnAppointmentCreated(AppointmentCreatedEventArgs args)
        {
            AppointmentCreated?.Invoke(this, args);
        }
    }

    /// <summary>
    /// Event args for appointment created event.
    /// </summary>
    public class AppointmentCreatedEventArgs : EventArgs
    {
        public Guid AppointmentId { get; set; }
    }
}
