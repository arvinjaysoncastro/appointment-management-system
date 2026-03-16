using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AppointmentManagementSystem.WpfClient.Infrastructure;
using AppointmentManagementSystem.WpfClient.Models;
using AppointmentManagementSystem.WpfClient.Services;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    public enum CalendarViewMode
    {
        Day,
        Week,
        Month,
        Year
    }

    public enum ScheduleFilter
    {
        Today,
        Upcoming,
        Past
    }

    public sealed class CalendarDaySummary
    {
        public DateTime Date { get; set; }
        public int AppointmentCount { get; set; }
        public string TotalScheduledTimeText { get; set; }
        public bool IsInCurrentMonth { get; set; }
    }

    public sealed class CalendarMonthSummary
    {
        public DateTime MonthStart { get; set; }
        public string MonthName { get; set; }
        public int AppointmentCount { get; set; }
        public string TotalScheduledTimeText { get; set; }
    }

    public sealed class AppointmentsViewModel : ViewModelBase, IAsyncInitializable
    {
        private const int MinimumDurationMinutes = 5;

        private enum DragMode
        {
            None,
            Move,
            ResizeStart,
            ResizeEnd
        }

        private readonly AppointmentApiService _api;
        private readonly DispatcherTimer _currentTimeTimer;
        private AppointmentModel _selectedListAppointment;
        private TimelineAppointmentBlockViewModel _draftBlock;
        private Guid? _draftSourceAppointmentId;
        private DateTime _selectedDate;
        private DateTime _currentTimelineDay;
        private bool _isCreatingDraft;
        private bool _isEditingDraft;
        private bool _showAllDescriptions;
        private string _currentDateTimeDisplay;
        private bool _isCurrentTimeMarkerVisible;
        private double _currentTimeMarkerTopOffset;
        private string _currentTimeMarkerLabel;
        private string _todayTotalScheduledTimeText;
        private DateTime? _selectedStartDate;
        private string _selectedStartTime;
        private DateTime? _selectedEndDate;
        private string _selectedEndTime;
        private bool _isSyncingEditorDateTimeFields;
        private bool _isReloadingForSelectedDate;
        private CalendarViewMode _calendarViewMode;
        private double _timelineScrollOffset;
        private int _timelineScrollRequestId;
        private int _searchFocusRequestId;
        private string _searchQuery;
        private ScheduleFilter _selectedFilter;
        private int _todayFilterCount;
        private int _upcomingFilterCount;
        private int _pastFilterCount;

        public ObservableCollection<AppointmentModel> Appointments { get; }
        public ObservableCollection<AppointmentModel> TodayAppointments { get; }
        public ObservableCollection<TimelineHourViewModel> TimelineHours { get; }
        public ObservableCollection<TimelineAppointmentBlockViewModel> TimelineBlocks { get; }
        public ObservableCollection<string> TimeOptions { get; }
        public ObservableCollection<CalendarDaySummary> WeekDaySummaries { get; }
        public ObservableCollection<CalendarDaySummary> MonthDaySummaries { get; }
        public ObservableCollection<CalendarMonthSummary> YearMonthSummaries { get; }

        public double HourRowHeight => 60d * TimelineAppointmentBlockViewModel.PixelsPerMinute;
        public double TimelineHeight => 24d * 60d * TimelineAppointmentBlockViewModel.PixelsPerMinute;
        public bool IsDashboardMode => DraftBlock == null;
        public bool IsEditorMode => DraftBlock != null;
        public int TodayAppointmentsCount => TodayAppointments.Count;
        public bool IsDayView => CalendarViewMode == CalendarViewMode.Day;
        public bool IsWeekView => CalendarViewMode == CalendarViewMode.Week;
        public bool IsMonthView => CalendarViewMode == CalendarViewMode.Month;
        public bool IsYearView => CalendarViewMode == CalendarViewMode.Year;
        public bool IsTodayQuickFilter => SelectedFilter == ScheduleFilter.Today;
        public bool IsUpcomingQuickFilter => SelectedFilter == ScheduleFilter.Upcoming;
        public bool IsPastQuickFilter => SelectedFilter == ScheduleFilter.Past;
        public bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);
        public bool HasTodayAppointments => TodayAppointments.Count > 0;
        public int TodayFilterCount
        {
            get => _todayFilterCount;
            private set => SetProperty(ref _todayFilterCount, value);
        }
        public int UpcomingFilterCount
        {
            get => _upcomingFilterCount;
            private set => SetProperty(ref _upcomingFilterCount, value);
        }
        public int PastFilterCount
        {
            get => _pastFilterCount;
            private set => SetProperty(ref _pastFilterCount, value);
        }
        public CalendarViewMode SelectedViewMode
        {
            get => CalendarViewMode;
            set => CalendarViewMode = value;
        }
        public string OverviewTitle
        {
            get
            {
                if (IsSearching)
                {
                    return "Search Results";
                }

                switch (SelectedViewMode)
                {
                    case CalendarViewMode.Week:
                        return "Week Overview";
                    case CalendarViewMode.Month:
                        return "Month Overview";
                    case CalendarViewMode.Year:
                        return "Year Overview";
                    default:
                        return "Day Overview";
                }
            }
        }
        public string SearchFilterSummary => "Filter: " + SelectedFilter;
        public string OverviewAppointmentsLabel => IsSearching ? "Matching Appointments" : "Appointments";
        public string OverviewScheduleLabel
        {
            get
            {
                if (IsSearching)
                {
                    return "Matching Schedule";
                }

                switch (SelectedViewMode)
                {
                    case CalendarViewMode.Week:
                        return "Week Schedule";
                    case CalendarViewMode.Month:
                        return "Month Schedule";
                    case CalendarViewMode.Year:
                        return "Year Schedule";
                    default:
                        return "Day Schedule";
                }
            }
        }
        public string CurrentMonthTitle => SelectedDate.ToString("MMMM yyyy");
        public string CurrentMonthText => CurrentMonthTitle;
        public string SelectedDateText => SelectedDate.ToString("MMM dd, yyyy");

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    OnPropertyChanged(nameof(IsSearching));
                    OnPropertyChanged(nameof(OverviewTitle));
                    OnPropertyChanged(nameof(OverviewAppointmentsLabel));
                    OnPropertyChanged(nameof(OverviewScheduleLabel));
                    RefreshDashboardOverview();
                }
            }
        }
        public AppointmentModel SelectedAppointment
        {
            get => SelectedListAppointment;
            set => SelectedListAppointment = value;
        }

        public double TimelineScrollOffset
        {
            get => _timelineScrollOffset;
            private set => SetProperty(ref _timelineScrollOffset, value);
        }

        public int TimelineScrollRequestId
        {
            get => _timelineScrollRequestId;
            private set => SetProperty(ref _timelineScrollRequestId, value);
        }

        public int SearchFocusRequestId
        {
            get => _searchFocusRequestId;
            private set => SetProperty(ref _searchFocusRequestId, value);
        }

        public ScheduleFilter SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    OnPropertyChanged(nameof(IsTodayQuickFilter));
                    OnPropertyChanged(nameof(IsUpcomingQuickFilter));
                    OnPropertyChanged(nameof(IsPastQuickFilter));
                    OnPropertyChanged(nameof(SearchFilterSummary));
                    RefreshDashboardOverview();
                }
            }
        }

        public CalendarViewMode CalendarViewMode
        {
            get => _calendarViewMode;
            set
            {
                if (SetProperty(ref _calendarViewMode, value))
                {
                    OnPropertyChanged(nameof(SelectedViewMode));
                    OnPropertyChanged(nameof(IsDayView));
                    OnPropertyChanged(nameof(IsWeekView));
                    OnPropertyChanged(nameof(IsMonthView));
                    OnPropertyChanged(nameof(IsYearView));
                    OnPropertyChanged(nameof(OverviewTitle));
                    OnPropertyChanged(nameof(OverviewScheduleLabel));
                    RefreshDashboardOverview();
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                var normalized = value.Date;
                if (SetProperty(ref _selectedDate, normalized))
                {
                    OnPropertyChanged(nameof(SelectedDateDisplayText));
                    OnPropertyChanged(nameof(SelectedDateText));
                    OnPropertyChanged(nameof(CurrentMonthTitle));
                    OnPropertyChanged(nameof(CurrentMonthText));
                    _currentTimelineDay = normalized;
                    UpdateCurrentTimeState();
                    RefreshDashboardOverview();
                    RefreshCalendarSummaries();
                    _ = ReloadForSelectedDateAsync();
                }
            }
        }

        public string SelectedDateDisplayText =>
            SelectedDate.Date == DateTime.Today
                ? "Today — " + SelectedDate.ToString("MMM dd, yyyy")
                : SelectedDate.ToString("MMM dd, yyyy");

        public string EditorDurationText
        {
            get
            {
                if (DraftBlock?.Appointment == null)
                {
                    return "0h 00m";
                }

                var totalMinutes = Math.Max(0, (int)Math.Round((DraftBlock.Appointment.End - DraftBlock.Appointment.Start).TotalMinutes, MidpointRounding.AwayFromZero));
                var hours = totalMinutes / 60;
                var minutes = totalMinutes % 60;
                return hours + "h " + minutes.ToString("00") + "m";
            }
        }

        public string TodayTotalScheduledTimeText
        {
            get => _todayTotalScheduledTimeText;
            private set => SetProperty(ref _todayTotalScheduledTimeText, value);
        }

        public string CurrentDateTimeDisplay
        {
            get => _currentDateTimeDisplay;
            private set => SetProperty(ref _currentDateTimeDisplay, value);
        }

        public bool IsCurrentTimeMarkerVisible
        {
            get => _isCurrentTimeMarkerVisible;
            private set => SetProperty(ref _isCurrentTimeMarkerVisible, value);
        }

        public double CurrentTimeMarkerTopOffset
        {
            get => _currentTimeMarkerTopOffset;
            private set => SetProperty(ref _currentTimeMarkerTopOffset, value);
        }

        public string CurrentTimeMarkerLabel
        {
            get => _currentTimeMarkerLabel;
            private set => SetProperty(ref _currentTimeMarkerLabel, value);
        }

        public DateTime? SelectedStartDate
        {
            get => _selectedStartDate;
            set
            {
                if (SetProperty(ref _selectedStartDate, value))
                {
                    UpdateDraftDateTimeFromSelectors(true);
                }
            }
        }

        public string SelectedStartTime
        {
            get => _selectedStartTime;
            set
            {
                if (SetProperty(ref _selectedStartTime, value))
                {
                    UpdateDraftDateTimeFromSelectors(true);
                }
            }
        }

        public DateTime? SelectedEndDate
        {
            get => _selectedEndDate;
            set
            {
                if (SetProperty(ref _selectedEndDate, value))
                {
                    UpdateDraftDateTimeFromSelectors(false);
                }
            }
        }

        public string SelectedEndTime
        {
            get => _selectedEndTime;
            set
            {
                if (SetProperty(ref _selectedEndTime, value))
                {
                    UpdateDraftDateTimeFromSelectors(false);
                }
            }
        }

        public AppointmentModel SelectedListAppointment
        {
            get => _selectedListAppointment;
            set
            {
                if (SetProperty(ref _selectedListAppointment, value))
                {
                    OnPropertyChanged(nameof(SelectedAppointment));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public TimelineAppointmentBlockViewModel DraftBlock
        {
            get => _draftBlock;
            set
            {
                if (ReferenceEquals(_draftBlock, value))
                {
                    return;
                }

                DetachDraftBlock(_draftBlock);
                _draftBlock = value;
                AttachDraftBlock(_draftBlock);
                SyncEditorDateTimeSelectors();

                OnPropertyChanged();
                OnPropertyChanged(nameof(DraftAppointment));
                OnPropertyChanged(nameof(IsDashboardMode));
                OnPropertyChanged(nameof(IsEditorMode));
                OnPropertyChanged(nameof(EditorDurationText));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public AppointmentModel DraftAppointment => DraftBlock?.Appointment;

        public Guid? DraftSourceAppointmentId
        {
            get => _draftSourceAppointmentId;
            set => SetProperty(ref _draftSourceAppointmentId, value);
        }

        public bool IsCreatingDraft
        {
            get => _isCreatingDraft;
            set => SetProperty(ref _isCreatingDraft, value);
        }

        public bool IsEditingDraft
        {
            get => _isEditingDraft;
            set => SetProperty(ref _isEditingDraft, value);
        }

        public bool ShowAllDescriptions
        {
            get => _showAllDescriptions;
            set
            {
                if (SetProperty(ref _showAllDescriptions, value))
                {
                    OnPropertyChanged(nameof(GlobalDetailsToggleText));
                }
            }
        }

        public string GlobalDetailsToggleText => ShowAllDescriptions ? "Hide all details" : "Show all details";

        public ICommand LoadAppointmentsCommand { get; }
        public ICommand CreateDraftCommand { get; }
        public ICommand SaveAppointmentCommand { get; }
        public ICommand CancelDraftCommand { get; }
        public ICommand SelectAppointmentCommand { get; }
        public ICommand EditAppointmentCommand { get; }
        public ICommand DeleteAppointmentCommand { get; }
        public ICommand DragAppointmentBlockCommand { get; }
        public ICommand ResizeAppointmentStartCommand { get; }
        public ICommand ResizeAppointmentEndCommand { get; }
        public ICommand SelectTimeSlotCommand { get; }
        public ICommand ToggleBlockDetailsCommand { get; }
        public ICommand ToggleAllDetailsCommand { get; }
        public ICommand PreviousDateCommand { get; }
        public ICommand NextDateCommand { get; }
        public ICommand SetTodayCommand { get; }
        public ICommand SetDayViewCommand { get; }
        public ICommand SetWeekViewCommand { get; }
        public ICommand SetMonthViewCommand { get; }
        public ICommand SetYearViewCommand { get; }
        public ICommand SelectCalendarDateCommand { get; }
        public ICommand SelectCalendarMonthCommand { get; }
        public ICommand SetSelectedFilterCommand { get; }
        public ICommand FocusSearchCommand { get; }

        // Backward-compatible aliases for existing bindings.
        public ICommand CreateAppointmentCommand { get; }
        public ICommand UpdateAppointmentCommand { get; }

        public AppointmentsViewModel(AppointmentApiService api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _selectedDate = DateTime.Today;
            Appointments = new ObservableCollection<AppointmentModel>();
            TodayAppointments = new ObservableCollection<AppointmentModel>();
            TimelineHours = new ObservableCollection<TimelineHourViewModel>();
            TimelineBlocks = new ObservableCollection<TimelineAppointmentBlockViewModel>();
            TimeOptions = new ObservableCollection<string>(BuildTimeOptions());
            WeekDaySummaries = new ObservableCollection<CalendarDaySummary>();
            MonthDaySummaries = new ObservableCollection<CalendarDaySummary>();
            YearMonthSummaries = new ObservableCollection<CalendarMonthSummary>();
            _currentTimelineDay = _selectedDate;
            _calendarViewMode = CalendarViewMode.Day;
            _currentDateTimeDisplay = string.Empty;
            _currentTimeMarkerLabel = string.Empty;
            _todayTotalScheduledTimeText = "0m";
            _selectedStartTime = string.Empty;
            _selectedEndTime = string.Empty;
            _searchQuery = string.Empty;
            _selectedFilter = ScheduleFilter.Today;
            _selectedListAppointment = null;
            _draftBlock = null;

            LoadAppointmentsCommand = new AsyncRelayCommand(LoadAppointmentsAsync);
            CreateDraftCommand = new RelayCommand(_ => CreateDraft());
            SaveAppointmentCommand = new AsyncRelayCommand(SaveDraftAsync, () => DraftBlock != null);
            CancelDraftCommand = new RelayCommand(_ => CancelDraft(), _ => DraftBlock != null);
            SelectAppointmentCommand = new RelayCommand(SelectAppointment);
            EditAppointmentCommand = new RelayCommand(EditAppointment, _ => SelectedListAppointment != null);
            DeleteAppointmentCommand = new AsyncRelayCommand(DeleteAppointmentAsync);
            DragAppointmentBlockCommand = new RelayCommand(DragAppointmentBlock, CanDragAppointmentBlock);
            ResizeAppointmentStartCommand = new RelayCommand(ResizeAppointmentStart, CanResizeAppointmentBlock);
            ResizeAppointmentEndCommand = new RelayCommand(ResizeAppointmentEnd, CanResizeAppointmentBlock);
            SelectTimeSlotCommand = new RelayCommand(SelectTimeSlot);
            ToggleBlockDetailsCommand = new RelayCommand(ToggleBlockDetails);
            ToggleAllDetailsCommand = new RelayCommand(_ => ToggleAllDetails());
            PreviousDateCommand = new RelayCommand(_ => SelectedDate = SelectedDate.AddDays(-1));
            NextDateCommand = new RelayCommand(_ => SelectedDate = SelectedDate.AddDays(1));
            SetTodayCommand = new RelayCommand(_ => SelectedDate = DateTime.Today);
            SetDayViewCommand = new RelayCommand(_ => CalendarViewMode = CalendarViewMode.Day);
            SetWeekViewCommand = new RelayCommand(_ => CalendarViewMode = CalendarViewMode.Week);
            SetMonthViewCommand = new RelayCommand(_ => CalendarViewMode = CalendarViewMode.Month);
            SetYearViewCommand = new RelayCommand(_ => CalendarViewMode = CalendarViewMode.Year);
            SelectCalendarDateCommand = new RelayCommand(SelectCalendarDate);
            SelectCalendarMonthCommand = new RelayCommand(SelectCalendarMonth);
            SetSelectedFilterCommand = new RelayCommand(SetSelectedFilter);
            FocusSearchCommand = new RelayCommand(_ => SearchFocusRequestId = SearchFocusRequestId + 1);

            CreateAppointmentCommand = CreateDraftCommand;
            UpdateAppointmentCommand = EditAppointmentCommand;

            GenerateTimelineHours(SelectedDate);
            _currentTimeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _currentTimeTimer.Tick += OnCurrentTimeTimerTick;
            _currentTimeTimer.Start();
            UpdateCurrentTimeState();
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

                RefreshDashboardOverview();
                RefreshCalendarSummaries();
                RebuildTimeline(SelectedDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load appointments. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateDraft()
        {
            BeginCreateDraft(SelectedDate.Date.AddHours(9));
        }

        private void SelectTimeSlot(object parameter)
        {
            if (parameter is DateTime slotStart)
            {
                BeginCreateDraft(slotStart);
                return;
            }

            BeginCreateDraft(SelectedDate.Date.AddHours(9));
        }

        private void SelectAppointment(object parameter)
        {
            var selected = parameter as AppointmentModel;
            if (selected == null || selected.Id == Guid.Empty)
            {
                return;
            }

            var existing = Appointments.FirstOrDefault(a => a.Id == selected.Id);
            if (existing == null)
            {
                return;
            }

            if (IsEditingDraft && DraftSourceAppointmentId.HasValue && DraftSourceAppointmentId.Value == existing.Id)
            {
                RequestTimelineScrollTo(existing.Start);
                return;
            }

            CalendarViewMode = CalendarViewMode.Day;
            SelectedDate = existing.Start.Date;
            SelectedAppointment = existing;
            BeginEditDraft();
            RequestTimelineScrollTo(existing.Start);
        }

        private void EditAppointment(object parameter)
        {
            var clicked = parameter as AppointmentModel;
            if (clicked == null)
            {
                return;
            }

            SelectAppointment(clicked);
        }

        private void BeginCreateDraft(DateTime start)
        {
            var appointmentStart = start;
            var appointment = new AppointmentModel
            {
                Id = Guid.Empty,
                Title = string.Empty,
                Description = string.Empty,
                Start = appointmentStart,
                End = appointmentStart.AddHours(1)
            };

            DraftSourceAppointmentId = null;
            IsCreatingDraft = true;
            IsEditingDraft = false;
            DraftBlock = CreateDraftBlock(appointment);
            SelectedListAppointment = appointment;

            RebuildTimeline(appointmentStart.Date);
        }

        private void BeginEditDraft()
        {
            if (SelectedListAppointment == null)
            {
                return;
            }

            DraftSourceAppointmentId = SelectedListAppointment.Id;
            IsCreatingDraft = false;
            IsEditingDraft = true;
            DraftBlock = CreateDraftBlock(Clone(SelectedListAppointment));

            RebuildTimeline(SelectedListAppointment.Start.Date);
            RequestTimelineScrollTo(SelectedListAppointment.Start);
        }

        private async Task SaveDraftAsync()
        {
            if (DraftBlock == null)
            {
                return;
            }

            try
            {
                var payload = Clone(DraftBlock.Appointment);

                if (DraftSourceAppointmentId.HasValue)
                {
                    await _api.UpdateAppointmentAsync(DraftSourceAppointmentId.Value, payload);
                }
                else
                {
                    await _api.CreateAppointmentAsync(payload);
                }

                ClearDraftState();
                await LoadAppointmentsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save appointment. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelDraft()
        {
            ClearDraftState();
            RebuildTimeline(SelectedDate);
        }

        private async Task DeleteAppointmentAsync()
        {
            var appointmentToDelete = SelectedListAppointment;
            if (appointmentToDelete == null && DraftSourceAppointmentId.HasValue)
            {
                appointmentToDelete = Appointments.FirstOrDefault(a => a.Id == DraftSourceAppointmentId.Value);
            }

            if (appointmentToDelete == null || appointmentToDelete.Id == Guid.Empty)
            {
                return;
            }

            var result = MessageBox.Show(
                "Delete this appointment?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _api.DeleteAppointmentAsync(appointmentToDelete.Id);
                ClearDraftState();
                await LoadAppointmentsAsync();
                SelectedListAppointment = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete appointment. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DragAppointmentBlock(object parameter)
        {
            var drag = parameter as ThumbDragCommandArgs;
            var block = drag?.Parameter as TimelineAppointmentBlockViewModel;
            if (block == null || DraftBlock == null || !ReferenceEquals(block, DraftBlock))
            {
                return;
            }

            var minuteDelta = (int)Math.Round(drag.VerticalChange / TimelineAppointmentBlockViewModel.PixelsPerMinute, MidpointRounding.AwayFromZero);
            if (minuteDelta == 0)
            {
                return;
            }

            ApplyDragDelta(DragMode.Move, minuteDelta);
        }

        private void ResizeAppointmentStart(object parameter)
        {
            var drag = parameter as ThumbDragCommandArgs;
            var block = drag?.Parameter as TimelineAppointmentBlockViewModel;
            if (block == null || DraftBlock == null || !ReferenceEquals(block, DraftBlock))
            {
                return;
            }

            var minuteDelta = (int)Math.Round(drag.VerticalChange / TimelineAppointmentBlockViewModel.PixelsPerMinute, MidpointRounding.AwayFromZero);
            if (minuteDelta == 0)
            {
                return;
            }

            ApplyDragDelta(DragMode.ResizeStart, minuteDelta);
        }

        private void ResizeAppointmentEnd(object parameter)
        {
            var drag = parameter as ThumbDragCommandArgs;
            var block = drag?.Parameter as TimelineAppointmentBlockViewModel;
            if (block == null || DraftBlock == null || !ReferenceEquals(block, DraftBlock))
            {
                return;
            }

            var minuteDelta = (int)Math.Round(drag.VerticalChange / TimelineAppointmentBlockViewModel.PixelsPerMinute, MidpointRounding.AwayFromZero);
            if (minuteDelta == 0)
            {
                return;
            }

            ApplyDragDelta(DragMode.ResizeEnd, minuteDelta);
        }

        private void ApplyDragDelta(DragMode dragMode, int minuteDelta)
        {
            if (dragMode == DragMode.None || minuteDelta == 0 || DraftBlock == null)
            {
                return;
            }

            var draft = DraftBlock.Appointment;
            var dayStart = draft.Start.Date;
            var dayEnd = dayStart.AddDays(1).AddMinutes(-1);
            var duration = EnsureMinimumDuration(draft.End - draft.Start);

            if (dragMode == DragMode.Move)
            {
                var proposedStart = draft.Start.AddMinutes(minuteDelta);
                if (proposedStart < dayStart)
                {
                    proposedStart = dayStart;
                }

                var latestStart = dayEnd.Subtract(duration);
                if (proposedStart > latestStart)
                {
                    proposedStart = latestStart;
                }

                draft.Start = proposedStart;
                draft.End = proposedStart.Add(duration);
                UpdateDraftState(dayStart);
                return;
            }

            if (dragMode == DragMode.ResizeStart)
            {
                var maxStart = draft.End.AddMinutes(-MinimumDurationMinutes);
                var proposedStart = draft.Start.AddMinutes(minuteDelta);

                if (proposedStart < dayStart)
                {
                    proposedStart = dayStart;
                }

                if (proposedStart > maxStart)
                {
                    proposedStart = maxStart;
                }

                draft.Start = proposedStart;
                UpdateDraftState(dayStart);
                return;
            }

            if (dragMode == DragMode.ResizeEnd)
            {
                var minEnd = draft.Start.AddMinutes(MinimumDurationMinutes);
                var proposedEnd = draft.End.AddMinutes(minuteDelta);

                if (proposedEnd > dayEnd)
                {
                    proposedEnd = dayEnd;
                }

                if (proposedEnd < minEnd)
                {
                    proposedEnd = minEnd;
                }

                draft.End = proposedEnd;
                UpdateDraftState(dayStart);
            }
        }

        private bool CanDragAppointmentBlock(object parameter)
        {
            var drag = parameter as ThumbDragCommandArgs;
            var block = drag?.Parameter as TimelineAppointmentBlockViewModel;
            return block != null && block.IsDraft && DraftBlock != null && ReferenceEquals(block, DraftBlock);
        }

        private bool CanResizeAppointmentBlock(object parameter)
        {
            return CanDragAppointmentBlock(parameter);
        }

        private void GenerateTimelineHours(DateTime day)
        {
            _currentTimelineDay = day.Date;
            TimelineHours.Clear();

            for (var hour = 0; hour <= 23; hour++)
            {
                var start = day.Date.AddHours(hour);
                var end = start.AddHours(1).AddMinutes(-1);

                TimelineHours.Add(new TimelineHourViewModel
                {
                    Hour = hour,
                    Start = start,
                    End = end
                });
            }
        }

        private void RebuildTimeline(DateTime day)
        {
            _currentTimelineDay = day.Date;
            BuildTimelineBlocks(day);
            AssignAppointmentsToTimeline(day);
            UpdateDraftConflictState();
            UpdateCurrentTimeState();
        }

        private void OnCurrentTimeTimerTick(object sender, EventArgs e)
        {
            UpdateCurrentTimeState();
        }

        private void UpdateCurrentTimeState()
        {
            var now = DateTime.Now;
            CurrentDateTimeDisplay = "Today — " + now.ToString("MMM dd, yyyy hh:mm tt");

            var isTodayTimeline = now.Date == _currentTimelineDay.Date;
            if (!isTodayTimeline)
            {
                IsCurrentTimeMarkerVisible = false;
                CurrentTimeMarkerLabel = string.Empty;
                return;
            }

            var minutesSinceMidnight = (now.Hour * 60d) + now.Minute;
            CurrentTimeMarkerTopOffset = minutesSinceMidnight * TimelineAppointmentBlockViewModel.PixelsPerMinute;
            CurrentTimeMarkerLabel = "Now " + now.ToString("HH:mm");
            IsCurrentTimeMarkerVisible = CurrentTimeMarkerTopOffset >= 0d && CurrentTimeMarkerTopOffset <= TimelineHeight;
        }

        private void BuildTimelineBlocks(DateTime day)
        {
            TimelineBlocks.Clear();

            foreach (var appointment in Appointments)
            {
                if (appointment.Start.Date != day.Date)
                {
                    continue;
                }

                if (DraftSourceAppointmentId.HasValue && appointment.Id == DraftSourceAppointmentId.Value)
                {
                    continue;
                }

                TimelineBlocks.Add(CreateSavedBlock(appointment));
            }

            if (DraftBlock != null && DraftBlock.Appointment.Start.Date == day.Date)
            {
                DraftBlock.IsDraft = true;
                DraftBlock.ShowDetails = ShowAllDescriptions;
                DraftBlock.RecalculateLayout();
                TimelineBlocks.Add(DraftBlock);
            }
        }

        private void ToggleBlockDetails(object parameter)
        {
            var block = parameter as TimelineAppointmentBlockViewModel;
            if (block == null)
            {
                return;
            }

            block.ShowDetails = !block.ShowDetails;
            RefreshGlobalDetailsState();
        }

        private void ToggleAllDetails()
        {
            ShowAllDescriptions = !ShowAllDescriptions;

            foreach (var block in TimelineBlocks)
            {
                block.ShowDetails = ShowAllDescriptions;
            }

            if (DraftBlock != null)
            {
                DraftBlock.ShowDetails = ShowAllDescriptions;
            }
        }

        private void RefreshGlobalDetailsState()
        {
            ShowAllDescriptions = TimelineBlocks.Count > 0 && TimelineBlocks.All(block => block.ShowDetails);
        }

        private void AssignAppointmentsToTimeline(DateTime day)
        {
            if (TimelineHours.Count != 24)
            {
                GenerateTimelineHours(day);
            }

            foreach (var slot in TimelineHours)
            {
                slot.Appointments.Clear();
            }

            foreach (var block in TimelineBlocks)
            {
                var appointment = block.Appointment;
                if (appointment == null || appointment.Start.Date != day.Date)
                {
                    continue;
                }

                TimelineHours[appointment.Start.Hour].Appointments.Add(appointment);
            }
        }

        private void UpdateDraftConflictState()
        {
            if (DraftBlock == null || DraftBlock.Appointment == null)
            {
                return;
            }

            var draft = DraftBlock.Appointment;
            var hasConflict = Appointments.Any(existing =>
                existing.Start.Date == draft.Start.Date &&
                (!DraftSourceAppointmentId.HasValue || existing.Id != DraftSourceAppointmentId.Value) &&
                draft.Start < existing.End &&
                draft.End > existing.Start);

            DraftBlock.IsConflicting = hasConflict;
        }

        private void UpdateDraftState(DateTime day)
        {
            UpdateDraftConflictState();
            AssignAppointmentsToTimeline(day);
            CommandManager.InvalidateRequerySuggested();
        }

        private void RequestTimelineScrollTo(DateTime dateTime)
        {
            var minutesSinceMidnight = (dateTime.Hour * 60d) + dateTime.Minute;
            var targetOffset = (minutesSinceMidnight * TimelineAppointmentBlockViewModel.PixelsPerMinute) - 120d;
            if (targetOffset < 0d)
            {
                targetOffset = 0d;
            }

            TimelineScrollOffset = targetOffset;
            TimelineScrollRequestId = TimelineScrollRequestId + 1;
        }

        private TimelineAppointmentBlockViewModel CreateDraftBlock(AppointmentModel appointment)
        {
            var block = new TimelineAppointmentBlockViewModel
            {
                Appointment = appointment,
                IsDraft = true,
                ShowDetails = ShowAllDescriptions
            };

            block.RecalculateLayout();
            return block;
        }

        private TimelineAppointmentBlockViewModel CreateSavedBlock(AppointmentModel appointment)
        {
            return new TimelineAppointmentBlockViewModel
            {
                Appointment = appointment,
                IsDraft = false,
                IsConflicting = false,
                ShowDetails = ShowAllDescriptions
            };
        }

        private void AttachDraftBlock(TimelineAppointmentBlockViewModel block)
        {
            if (block == null)
            {
                return;
            }

            block.PropertyChanged += OnDraftBlockPropertyChanged;
        }

        private void DetachDraftBlock(TimelineAppointmentBlockViewModel block)
        {
            if (block == null)
            {
                return;
            }

            block.PropertyChanged -= OnDraftBlockPropertyChanged;
        }

        private void OnDraftBlockPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimelineAppointmentBlockViewModel.Start) ||
                e.PropertyName == nameof(TimelineAppointmentBlockViewModel.End))
            {
                SyncEditorDateTimeSelectors();
                OnPropertyChanged(nameof(EditorDurationText));
                var day = DraftBlock?.Appointment?.Start.Date ?? SelectedDate;
                UpdateDraftState(day);
            }
        }

        private void ClearDraftState()
        {
            DraftBlock = null;
            DraftSourceAppointmentId = null;
            IsCreatingDraft = false;
            IsEditingDraft = false;
            SelectedListAppointment = null;
        }

        private void SyncEditorDateTimeSelectors()
        {
            _isSyncingEditorDateTimeFields = true;

            if (DraftBlock?.Appointment == null)
            {
                SelectedStartDate = null;
                SelectedStartTime = string.Empty;
                SelectedEndDate = null;
                SelectedEndTime = string.Empty;
                _isSyncingEditorDateTimeFields = false;
                return;
            }

            SelectedStartDate = DraftBlock.Appointment.Start.Date;
            SelectedStartTime = FormatTimeOption(DraftBlock.Appointment.Start);
            SelectedEndDate = DraftBlock.Appointment.End.Date;
            SelectedEndTime = FormatTimeOption(DraftBlock.Appointment.End);

            _isSyncingEditorDateTimeFields = false;
        }

        private void UpdateDraftDateTimeFromSelectors(bool isStart)
        {
            if (_isSyncingEditorDateTimeFields || DraftBlock?.Appointment == null)
            {
                return;
            }

            if (isStart)
            {
                var updatedStart = CombineDateAndTime(SelectedStartDate, SelectedStartTime, DraftBlock.Appointment.Start);
                if (updatedStart != DraftBlock.Appointment.Start)
                {
                    DraftBlock.Appointment.Start = updatedStart;
                }

                return;
            }

            var updatedEnd = CombineDateAndTime(SelectedEndDate, SelectedEndTime, DraftBlock.Appointment.End);
            if (updatedEnd != DraftBlock.Appointment.End)
            {
                DraftBlock.Appointment.End = updatedEnd;
            }
        }

        private void SetSelectedFilter(object parameter)
        {
            if (!IsSearching)
            {
                return;
            }

            if (parameter is ScheduleFilter selected)
            {
                SelectedFilter = selected;
                return;
            }

            if (parameter is string raw && Enum.TryParse(raw, true, out ScheduleFilter parsed))
            {
                SelectedFilter = parsed;
            }
        }

        private void RefreshDashboardOverview()
        {
            TodayAppointments.Clear();

            var now = DateTime.Now;
            var normalizedSearch = (SearchQuery ?? string.Empty).Trim();
            var isSearching = !string.IsNullOrWhiteSpace(normalizedSearch);

            IEnumerable<AppointmentModel> scope = isSearching
                ? Appointments
                : GetAppointmentsForSelectedPeriod();

            if (isSearching)
            {
                scope = scope
                    .Where(appointment => MatchesSearchQuery(appointment, normalizedSearch, now))
                    .ToList();

                TodayFilterCount = FilterBySchedule(scope, ScheduleFilter.Today, now).Count();
                UpcomingFilterCount = FilterBySchedule(scope, ScheduleFilter.Upcoming, now).Count();
                PastFilterCount = FilterBySchedule(scope, ScheduleFilter.Past, now).Count();

                scope = FilterBySchedule(scope, SelectedFilter, now);
            }
            else
            {
                TodayFilterCount = 0;
                UpcomingFilterCount = 0;
                PastFilterCount = 0;
            }

            var filteredAppointments = scope
                .OrderBy(appointment => appointment.Start)
                .ToList();

            foreach (var appointment in filteredAppointments)
            {
                TodayAppointments.Add(appointment);
            }

            var totalMinutes = filteredAppointments.Sum(appointment => Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes));
            TodayTotalScheduledTimeText = FormatDuration(totalMinutes);
            OnPropertyChanged(nameof(TodayAppointmentsCount));
            OnPropertyChanged(nameof(HasTodayAppointments));
        }

        private static IEnumerable<AppointmentModel> FilterBySchedule(IEnumerable<AppointmentModel> appointments, ScheduleFilter filter, DateTime now)
        {
            switch (filter)
            {
                case ScheduleFilter.Upcoming:
                    return appointments.Where(appointment => appointment.Start > now);
                case ScheduleFilter.Past:
                    return appointments.Where(appointment => appointment.End < now);
                default:
                    return appointments.Where(appointment => appointment.Start.Date == now.Date);
            }
        }

        private IEnumerable<AppointmentModel> GetAppointmentsForSelectedPeriod()
        {
            switch (SelectedViewMode)
            {
                case CalendarViewMode.Week:
                    var weekStart = GetStartOfWeek(SelectedDate.Date);
                    var weekEnd = weekStart.AddDays(7);
                    return Appointments.Where(appointment => appointment.Start >= weekStart && appointment.Start < weekEnd);

                case CalendarViewMode.Month:
                    var monthStart = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);
                    return Appointments.Where(appointment => appointment.Start >= monthStart && appointment.Start < monthEnd);

                case CalendarViewMode.Year:
                    var yearStart = new DateTime(SelectedDate.Year, 1, 1);
                    var yearEnd = yearStart.AddYears(1);
                    return Appointments.Where(appointment => appointment.Start >= yearStart && appointment.Start < yearEnd);

                default:
                    return Appointments.Where(appointment => appointment.Start.Date == SelectedDate.Date);
            }
        }

        private static bool MatchesSearchQuery(AppointmentModel appointment, string query, DateTime now)
        {
            if (appointment == null)
            {
                return false;
            }

            var trimmedQuery = (query ?? string.Empty).Trim();
            if (trimmedQuery.Length == 0)
            {
                return true;
            }

            var comparison = StringComparison.OrdinalIgnoreCase;
            var title = appointment.Title ?? string.Empty;
            var description = appointment.Description ?? string.Empty;
            var notes = ReadOptionalSearchProperty(appointment, "Notes");
            var metadata = ReadOptionalSearchProperty(appointment, "Metadata") + " " +
                           ReadOptionalSearchProperty(appointment, "MetadataText");
            var startTimeText = appointment.Start.ToString("HH:mm");
            var endTimeText = appointment.End.ToString("HH:mm");

            var titleMatches = title.IndexOf(trimmedQuery, comparison) >= 0;
            var descriptionMatches = description.IndexOf(trimmedQuery, comparison) >= 0;
            var notesMatches = notes.IndexOf(trimmedQuery, comparison) >= 0;
            var metadataMatches = metadata.IndexOf(trimmedQuery, comparison) >= 0;
            var startMatches = startTimeText.IndexOf(trimmedQuery, comparison) >= 0;
            var endMatches = endTimeText.IndexOf(trimmedQuery, comparison) >= 0;

            if (titleMatches || descriptionMatches || notesMatches || metadataMatches || startMatches || endMatches)
            {
                return true;
            }

            var loweredQuery = trimmedQuery.ToLowerInvariant();
            if (ContainsDateKeyword(loweredQuery, appointment, now))
            {
                return true;
            }

            if (ContainsDayPeriodKeyword(loweredQuery, appointment.Start))
            {
                return true;
            }

            if (TryParseSearchTime(trimmedQuery, out var parsedTime, out var toleranceMinutes))
            {
                var minutesFromMidnight = appointment.Start.Hour * 60 + appointment.Start.Minute;
                var delta = Math.Abs(minutesFromMidnight - parsedTime);
                return delta <= toleranceMinutes;
            }

            return false;
        }

        private static string ReadOptionalSearchProperty(AppointmentModel appointment, string propertyName)
        {
            if (appointment == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return string.Empty;
            }

            var property = appointment.GetType().GetProperty(propertyName);
            if (property == null)
            {
                return string.Empty;
            }

            var value = property.GetValue(appointment, null);
            return value == null ? string.Empty : value.ToString();
        }

        private static bool ContainsDateKeyword(string loweredQuery, AppointmentModel appointment, DateTime now)
        {
            if (loweredQuery.IndexOf("today", StringComparison.Ordinal) >= 0)
            {
                return appointment.Start.Date == now.Date;
            }

            if (loweredQuery.IndexOf("tomorrow", StringComparison.Ordinal) >= 0)
            {
                return appointment.Start.Date == now.Date.AddDays(1);
            }

            return false;
        }

        private static bool ContainsDayPeriodKeyword(string loweredQuery, DateTime start)
        {
            var hour = start.Hour;

            if (loweredQuery.IndexOf("morning", StringComparison.Ordinal) >= 0)
            {
                return hour >= 5 && hour < 12;
            }

            if (loweredQuery.IndexOf("afternoon", StringComparison.Ordinal) >= 0)
            {
                return hour >= 12 && hour < 17;
            }

            if (loweredQuery.IndexOf("evening", StringComparison.Ordinal) >= 0)
            {
                return hour >= 17 && hour < 23;
            }

            return false;
        }

        private static bool TryParseSearchTime(string query, out int minutesFromMidnight, out int toleranceMinutes)
        {
            minutesFromMidnight = 0;
            toleranceMinutes = 0;

            if (string.IsNullOrWhiteSpace(query))
            {
                return false;
            }

            var normalized = query.Trim().ToLowerInvariant();

            DateTime parsed;
            if (DateTime.TryParseExact(normalized, new[] { "h:mmtt", "htt", "h tt", "h:mm tt", "h:mm", "HH:mm" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                minutesFromMidnight = parsed.Hour * 60 + parsed.Minute;
                toleranceMinutes = normalized.Contains(":") ? 15 : 59;
                return true;
            }

            int hour;
            if (int.TryParse(normalized, out hour) && hour >= 0 && hour <= 23)
            {
                minutesFromMidnight = hour * 60;
                toleranceMinutes = 59;
                return true;
            }

            return false;
        }

        private void RefreshCalendarSummaries()
        {
            BuildWeekDaySummaries();
            BuildMonthDaySummaries();
            BuildYearMonthSummaries();
        }

        private void BuildWeekDaySummaries()
        {
            WeekDaySummaries.Clear();
            var weekStart = GetStartOfWeek(SelectedDate.Date);

            for (var i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                WeekDaySummaries.Add(CreateCalendarDaySummary(date, true));
            }
        }

        private void BuildMonthDaySummaries()
        {
            MonthDaySummaries.Clear();

            var firstOfMonth = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var gridStart = GetStartOfWeek(firstOfMonth);

            for (var i = 0; i < 42; i++)
            {
                var date = gridStart.AddDays(i);
                var isInCurrentMonth = date.Month == SelectedDate.Month && date.Year == SelectedDate.Year;
                MonthDaySummaries.Add(CreateCalendarDaySummary(date, isInCurrentMonth));
            }
        }

        private void BuildYearMonthSummaries()
        {
            YearMonthSummaries.Clear();

            for (var month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(SelectedDate.Year, month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var monthAppointments = Appointments
                    .Where(appointment => appointment.Start >= monthStart && appointment.Start < monthEnd)
                    .ToList();
                var totalMinutes = monthAppointments.Sum(appointment => Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes));

                YearMonthSummaries.Add(new CalendarMonthSummary
                {
                    MonthStart = monthStart,
                    MonthName = monthStart.ToString("MMMM"),
                    AppointmentCount = monthAppointments.Count,
                    TotalScheduledTimeText = FormatDuration(totalMinutes)
                });
            }
        }

        private CalendarDaySummary CreateCalendarDaySummary(DateTime date, bool isInCurrentMonth)
        {
            var dayAppointments = Appointments.Where(appointment => appointment.Start.Date == date.Date).ToList();
            var totalMinutes = dayAppointments.Sum(appointment => Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes));

            return new CalendarDaySummary
            {
                Date = date.Date,
                AppointmentCount = dayAppointments.Count,
                TotalScheduledTimeText = FormatDuration(totalMinutes),
                IsInCurrentMonth = isInCurrentMonth
            };
        }

        private void SelectCalendarDate(object parameter)
        {
            if (!(parameter is DateTime date))
            {
                return;
            }

            CalendarViewMode = CalendarViewMode.Day;
            SelectedDate = date.Date;
        }

        private void SelectCalendarMonth(object parameter)
        {
            if (parameter is CalendarMonthSummary monthSummary)
            {
                CalendarViewMode = CalendarViewMode.Month;
                SelectedDate = monthSummary.MonthStart.Date;
                return;
            }

            if (parameter is DateTime date)
            {
                CalendarViewMode = CalendarViewMode.Month;
                SelectedDate = new DateTime(date.Year, date.Month, 1);
            }
        }

        private static DateTime GetStartOfWeek(DateTime date)
        {
            var normalized = date.Date;
            var dayOfWeek = (int)normalized.DayOfWeek;
            var mondayOffset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            return normalized.AddDays(-mondayOffset);
        }

        private static string FormatDuration(double totalMinutes)
        {
            var roundedMinutes = Math.Max(0, (int)Math.Round(totalMinutes, MidpointRounding.AwayFromZero));
            var hours = roundedMinutes / 60;
            var minutes = roundedMinutes % 60;

            if (hours > 0 && minutes > 0)
            {
                return hours + "h " + minutes + "m";
            }

            if (hours > 0)
            {
                return hours + "h";
            }

            return minutes + "m";
        }

        private static string[] BuildTimeOptions()
        {
            var options = new string[96];
            for (var i = 0; i < options.Length; i++)
            {
                options[i] = TimeSpan.FromMinutes(i * 15).ToString(@"hh\:mm");
            }

            return options;
        }

        private static string FormatTimeOption(DateTime value)
        {
            var roundedMinutes = (int)(Math.Round(value.Minute / 15d, MidpointRounding.AwayFromZero) * 15d);
            var rounded = new DateTime(value.Year, value.Month, value.Day, value.Hour, 0, 0).AddMinutes(roundedMinutes);
            return rounded.ToString("HH:mm");
        }

        private static DateTime CombineDateAndTime(DateTime? selectedDate, string selectedTime, DateTime fallback)
        {
            var date = selectedDate ?? fallback.Date;
            if (!TimeSpan.TryParseExact(selectedTime, @"hh\:mm", CultureInfo.InvariantCulture, out var time))
            {
                time = fallback.TimeOfDay;
            }

            return date.Date.Add(time);
        }

        private async Task ReloadForSelectedDateAsync()
        {
            if (_isReloadingForSelectedDate)
            {
                return;
            }

            _isReloadingForSelectedDate = true;
            try
            {
                var hasCreateDraft = DraftBlock != null && !DraftSourceAppointmentId.HasValue;
                if (hasCreateDraft)
                {
                    var currentStart = DraftBlock.Appointment.Start;
                    var duration = DraftBlock.Appointment.End - DraftBlock.Appointment.Start;
                    var newStart = SelectedDate.Date.Add(currentStart.TimeOfDay);
                    DraftBlock.Appointment.Start = newStart;
                    DraftBlock.Appointment.End = newStart.Add(duration);
                }
                else
                {
                    ClearDraftState();
                }

                await LoadAppointmentsAsync();
            }
            finally
            {
                _isReloadingForSelectedDate = false;
            }
        }

        private static AppointmentModel Clone(AppointmentModel source)
        {
            return new AppointmentModel
            {
                Id = source.Id,
                Title = source.Title,
                Description = source.Description,
                Start = source.Start,
                End = source.End
            };
        }

        private static TimeSpan EnsureMinimumDuration(TimeSpan duration)
        {
            if (duration.TotalMinutes < MinimumDurationMinutes)
            {
                return TimeSpan.FromMinutes(MinimumDurationMinutes);
            }

            return duration;
        }
    }
}
