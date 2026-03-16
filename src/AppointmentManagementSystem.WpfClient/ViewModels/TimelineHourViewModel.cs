using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    public sealed class TimelineHourViewModel : ViewModelBase
    {
        private int _hour;
        private DateTime _start;
        private DateTime _end;

        public int Hour
        {
            get => _hour;
            set => SetProperty(ref _hour, value);
        }

        public DateTime Start
        {
            get => _start;
            set => SetProperty(ref _start, value);
        }

        public DateTime End
        {
            get => _end;
            set => SetProperty(ref _end, value);
        }

        public bool HasAppointments => Appointments.Count > 0;

        public ObservableCollection<AppointmentModel> Appointments { get; }

        public TimelineHourViewModel()
        {
            Appointments = new ObservableCollection<AppointmentModel>();
            Appointments.CollectionChanged += OnAppointmentsCollectionChanged;
        }

        private void OnAppointmentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasAppointments));
        }
    }
}
