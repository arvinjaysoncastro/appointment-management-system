using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Models;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Services;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    public sealed class AppointmentsViewModel : ViewModelBase, IAsyncInitializable
    {
        private readonly AppointmentApiService _api;
        private AppointmentModel _selectedAppointment;

        public ObservableCollection<AppointmentModel> Appointments { get; }

        public AppointmentModel SelectedAppointment
        {
            get => _selectedAppointment;
            set => SetProperty(ref _selectedAppointment, value);
        }

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand CreateAppointmentCommand { get; }
        public ICommand UpdateAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }

        public AppointmentsViewModel(AppointmentApiService api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            Appointments = new ObservableCollection<AppointmentModel>();
            _selectedAppointment = null;

            LoadAppointmentsCommand = new AsyncRelayCommand(LoadAppointmentsAsync);
            CreateAppointmentCommand = new AsyncRelayCommand(CreateAppointmentAsync);
            UpdateAppointmentCommand = new AsyncRelayCommand(UpdateAppointmentAsync);
            DeleteAppointmentCommand = new AsyncRelayCommand(DeleteAppointmentAsync);
        }

        public Task InitializeAsync()
        {
            return LoadAppointmentsAsync();
        }

        private async Task LoadAppointmentsAsync()
        {
            try
            {
                var data = await _api.GetAppointmentsAsync();

                Appointments.Clear();
                foreach (var item in data)
                {
                    Appointments.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load appointments. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateAppointmentAsync()
        {
            try
            {
                var now = DateTime.Now;
                var newAppointment = new AppointmentModel
                {
                    Id = Guid.NewGuid(),
                    Title = "New Appointment",
                    Start = now.AddMinutes(30),
                    End = now.AddMinutes(90)
                };

                await _api.CreateAppointmentAsync(newAppointment);
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create appointment. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAppointmentAsync()
        {
            if (SelectedAppointment == null)
            {
                return;
            }

            try
            {
                var updatedAppointment = new AppointmentModel
                {
                    Id = SelectedAppointment.Id,
                    Title = SelectedAppointment.Title,
                    Start = SelectedAppointment.Start,
                    End = SelectedAppointment.End
                };

                await _api.UpdateAppointmentAsync(updatedAppointment.Id, updatedAppointment);
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to update appointment. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteAppointmentAsync()
        {
            if (SelectedAppointment == null)
            {
                return;
            }

            try
            {
                await _api.DeleteAppointmentAsync(SelectedAppointment.Id);
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete appointment. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}