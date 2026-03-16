using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    public sealed class TimelineAppointmentBlockViewModel : ViewModelBase
    {
        public const double PixelsPerMinute = 1.5;

        private string _title;
        private string _description;
        private DateTime _start;
        private DateTime _end;
        private double _topOffset;
        private double _blockHeight;
        private bool _isDraft;
        private bool _isConflicting;
        private bool _showDetails;
        private readonly ObservableCollection<double> _timerCircleOpacities;
        private AppointmentModel _appointment;
        private bool _syncingFromAppointment;

        public AppointmentModel Appointment
        {
            get => _appointment;
            set
            {
                if (ReferenceEquals(_appointment, value))
                {
                    return;
                }

                if (_appointment != null)
                {
                    _appointment.PropertyChanged -= OnAppointmentPropertyChanged;
                }

                _appointment = value ?? new AppointmentModel();
                _appointment.PropertyChanged += OnAppointmentPropertyChanged;

                _syncingFromAppointment = true;
                Title = _appointment.Title;
                Description = _appointment.Description;
                Start = _appointment.Start;
                End = _appointment.End;
                _syncingFromAppointment = false;

                RecalculateLayout();
                RefreshComputedPresentation();
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    if (!_syncingFromAppointment && _appointment != null && _appointment.Description != value)
                    {
                        _appointment.Description = value;
                    }

                    OnPropertyChanged(nameof(HasDescription));
                }
            }
        }

        public DateTime Start
        {
            get => _start;
            set
            {
                if (SetProperty(ref _start, value))
                {
                    if (!_syncingFromAppointment && _appointment != null && _appointment.Start != value)
                    {
                        _appointment.Start = value;
                    }

                    RecalculateLayout();
                    RefreshComputedPresentation();
                }
            }
        }

        public DateTime End
        {
            get => _end;
            set
            {
                if (SetProperty(ref _end, value))
                {
                    if (!_syncingFromAppointment && _appointment != null && _appointment.End != value)
                    {
                        _appointment.End = value;
                    }

                    RecalculateLayout();
                    RefreshComputedPresentation();
                }
            }
        }

        public bool ShowDetails
        {
            get => _showDetails;
            set => SetProperty(ref _showDetails, value);
        }

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);

        public ObservableCollection<double> TimerCircleOpacities => _timerCircleOpacities;

        public bool IsDraft
        {
            get => _isDraft;
            set => SetProperty(ref _isDraft, value);
        }

        public bool IsConflicting
        {
            get => _isConflicting;
            set => SetProperty(ref _isConflicting, value);
        }

        public double TopOffset
        {
            get => _topOffset;
            set => SetProperty(ref _topOffset, value);
        }

        public double BlockHeight
        {
            get => _blockHeight;
            set => SetProperty(ref _blockHeight, value);
        }

        public string TimeRange => Start.ToString("HH:mm") + " -> " + End.ToString("HH:mm");

        public string DurationText
        {
            get
            {
                var minutes = Math.Max(0, (int)Math.Round((End - Start).TotalMinutes, MidpointRounding.AwayFromZero));
                var hours = minutes / 60;
                var remainderMinutes = minutes % 60;

                if (hours > 0 && remainderMinutes > 0)
                {
                    return hours + "h " + remainderMinutes + "m";
                }

                if (hours > 0)
                {
                    return hours + "h";
                }

                return remainderMinutes + "m";
            }
        }

        public TimelineAppointmentBlockViewModel()
        {
            _title = string.Empty;
            _description = string.Empty;
            _timerCircleOpacities = new ObservableCollection<double>();
            _appointment = new AppointmentModel();
            _appointment.PropertyChanged += OnAppointmentPropertyChanged;
            RecalculateLayout();
            RefreshComputedPresentation();
        }

        public void RecalculateLayout()
        {
            var minutesFromMidnight = (Start.Hour * 60d) + Start.Minute;
            TopOffset = minutesFromMidnight * PixelsPerMinute;

            var durationMinutes = Math.Max((End - Start).TotalMinutes, 0d);
            BlockHeight = Math.Max(durationMinutes * PixelsPerMinute, 10d);
        }

        private void RefreshComputedPresentation()
        {
            OnPropertyChanged(nameof(TimeRange));
            OnPropertyChanged(nameof(DurationText));

            var totalMinutes = Math.Max(0d, (End - Start).TotalMinutes);
            var circleCount = Math.Max(1, (int)Math.Ceiling(totalMinutes / 60d));
            _timerCircleOpacities.Clear();

            for (var i = 0; i < circleCount; i++)
            {
                var remainingForCircle = Math.Max(0d, totalMinutes - (i * 60d));
                var circleMinutes = Math.Min(60d, remainingForCircle);
                _timerCircleOpacities.Add(circleMinutes / 60d);
            }

            OnPropertyChanged(nameof(TimerCircleOpacities));
        }

        private void OnAppointmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_appointment == null)
            {
                return;
            }

            if (e.PropertyName == nameof(AppointmentModel.Title))
            {
                Title = _appointment.Title;
                return;
            }

            if (e.PropertyName == nameof(AppointmentModel.Description))
            {
                Description = _appointment.Description;
                return;
            }

            if (e.PropertyName == nameof(AppointmentModel.Start) || e.PropertyName == nameof(AppointmentModel.End))
            {
                _syncingFromAppointment = true;
                Start = _appointment.Start;
                End = _appointment.End;
                _syncingFromAppointment = false;
            }
        }
    }
}
