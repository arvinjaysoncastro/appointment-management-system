using System;
using System.Windows.Input;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Services;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Manages navigation between sections via INavigationService.
    /// Views are resolved by DataTemplates, keeping view concerns out of the ViewModel.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private object _currentViewModel;

        public object CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ShowAppointmentsCommand { get; }
        public ICommand ShowPeopleCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

            ShowAppointmentsCommand = new RelayCommand(_ => ShowAppointments());
            ShowPeopleCommand = new RelayCommand(_ => ShowPeople());
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());

            // Set default view
            ShowAppointments();
        }

        private void OnCurrentViewModelChanged(object sender, EventArgs e)
        {
            CurrentViewModel = _navigationService.CurrentViewModel;
        }

        private void ShowAppointments()
        {
            _navigationService.NavigateTo<AppointmentsViewModel>();
        }

        private void ShowPeople()
        {
            _navigationService.NavigateTo<PeopleViewModel>();
        }

        private void ShowSettings()
        {
            _navigationService.NavigateTo<SettingsViewModel>();
        }
    }
}
