using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Views;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Manages navigation between different views (Appointments, People, Settings).
    /// Uses command pattern to switch between views.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly AppointmentListViewModel _appointmentListViewModel;
        private readonly PeopleViewModel _peopleViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly AppointmentsView _appointmentsView;
        private readonly PeopleView _peopleView;
        private readonly SettingsView _settingsView;
        private object _currentView;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ShowAppointmentsCommand { get; }
        public ICommand ShowPeopleCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainWindowViewModel(AppointmentListViewModel appointmentListViewModel)
        {
            _appointmentListViewModel = appointmentListViewModel ?? throw new ArgumentNullException(nameof(appointmentListViewModel));
            _peopleViewModel = new PeopleViewModel();
            _settingsViewModel = new SettingsViewModel();

            // Create AppointmentsView with ViewModel binding
            _appointmentsView = new AppointmentsView();
            _appointmentsView.DataContext = _appointmentListViewModel;

            // Create PeopleView with ViewModel binding
            _peopleView = new PeopleView();
            _peopleView.DataContext = _peopleViewModel;

            // Create SettingsView with ViewModel binding
            _settingsView = new SettingsView();
            _settingsView.DataContext = _settingsViewModel;

            ShowAppointmentsCommand = new RelayCommand(_ => ShowAppointments());
            ShowPeopleCommand = new RelayCommand(_ => ShowPeople());
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());

            // Set default view
            ShowAppointments();
        }

        private void ShowAppointments()
        {
            CurrentView = _appointmentsView;
        }

        private void ShowPeople()
        {
            CurrentView = _peopleView;
        }

        private void ShowSettings()
        {
            CurrentView = _settingsView;
        }
    }
}
