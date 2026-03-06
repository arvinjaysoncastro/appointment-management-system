using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Models;
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
        private PatientOption _selectedPatient;
        private string _title = string.Empty;
        private string _notes = string.Empty;
        private DateTimeOffset _startTime = DateTimeOffset.Now.Date.AddHours(9);
        private DateTimeOffset _endTime = DateTimeOffset.Now.Date.AddHours(10);
        private string _errorMessage = string.Empty;
        private bool _isSubmitting;

        public ObservableCollection<PatientOption> Patients { get; }

        public PatientOption SelectedPatient
        {
            get => _selectedPatient;
            set => SetProperty(ref _selectedPatient, value);
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
            
            // NOTE:
            // Due to time constraints for this technical assessment,
            // Patient CRUD and lookup UI were not implemented.
            // Instead a predefined patient list is used that matches
            // the seeded database patients.
            Patients = new ObservableCollection<PatientOption>
            {
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), Name = "John Smith" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), Name = "Mary Johnson" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), Name = "David Lee" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), Name = "Sarah Miller" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), Name = "Michael Brown" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), Name = "Emily Davis" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), Name = "Daniel Wilson" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000008"), Name = "Olivia Martinez" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000009"), Name = "James Anderson" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), Name = "Sophia Taylor" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000011"), Name = "Benjamin Thomas" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000012"), Name = "Charlotte Moore" },
                new PatientOption { Id = Guid.Parse("00000000-0000-0000-0000-000000000013"), Name = "Lucas Jackson" }
            };

            CreateCommand = new AsyncRelayCommand(CreateAppointmentAsync, CanCreateAppointment);
            CancelCommand = new RelayCommand(_ => OnCancelled());
        }

        private bool CanCreateAppointment()
        {
            return !string.IsNullOrWhiteSpace(Title) 
                && SelectedPatient != null
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

                if (SelectedPatient == null)
                {
                    ErrorMessage = "Please select a patient.";
                    return;
                }

                // Ensure proper DateTimeOffset conversion
                // If the DateTime is unspecified or UTC, treat as Local for user input
                var startOffset = ConvertToDateTimeOffset(StartTime);
                var endOffset = ConvertToDateTimeOffset(EndTime);

                var dto = new CreateAppointmentDto
                {
                    PatientId = SelectedPatient.Id,
                    Title = Title,
                    Notes = Notes,
                    StartTime = startOffset,
                    EndTime = endOffset
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

        private DateTimeOffset ConvertToDateTimeOffset(DateTimeOffset offset)
        {
            // If the offset is already properly configured (non-UTC or with correct offset), use it as-is
            if (offset.Offset != TimeSpan.Zero)
                return offset;

            // If it's UTC but originated from local input, convert it back to local time
            // This handles the case where DateTime.SpecifyKind(dt, DateTimeKind.Local) was used
            var dt = offset.DateTime;
            if (dt.Kind == DateTimeKind.Utc)
            {
                // Convert back to local time
                var localDt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
                return new DateTimeOffset(localDt);
            }

            return offset;
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
