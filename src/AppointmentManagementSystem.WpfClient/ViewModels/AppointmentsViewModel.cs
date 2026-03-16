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
using AppointmentManagementSystem.WpfClient.Enums;

namespace AppointmentManagementSystem.WpfClient.ViewModels
{
    public sealed class AppointmentsViewModel : ViewModelBase, IAsyncInitializable, INotifyDataErrorInfo
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
        private readonly DraftValidationService _draftValidationService;
        private readonly TimelineLayoutService _timelineLayoutService;
        private readonly CalendarSummaryService _calendarSummaryService;
        private readonly AppointmentFilterService _appointmentFilterService;
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
        private bool _isTimelineLoading;
        private CalendarViewMode _calendarViewMode;
        private double _timelineScrollOffset;
        private int _timelineScrollRequestId;
        private int _searchFocusRequestId;
        private string _searchQuery;
        private ScheduleFilter _selectedFilter;
        private int _todayFilterCount;
        private int _upcomingFilterCount;
        private int _pastFilterCount;
        private bool _hasValidationError;
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private bool _titleTouched;
        private bool _timeTouched;
        private bool _saveAttempted;

        public ObservableCollection<AppointmentModel> Appointments { get; }
        public ObservableCollection<AppointmentModel> TodayAppointments { get; }
        public ObservableCollection<TimelineHourViewModel> TimelineHours { get; }
        public ObservableCollection<TimelineAppointmentBlockViewModel> TimelineBlocks { get; }
        public ObservableCollection<string> TimeOptions { get; }
        public ObservableCollection<CalendarDaySummary> WeekDaySummaries { get; }
        public ObservableCollection<CalendarDaySummary> MonthDaySummaries { get; }
        public ObservableCollection<CalendarMonthSummary> YearMonthSummaries { get; }

        public bool IsTimelineLoading
        {
            get => _isTimelineLoading;
            set => SetProperty(ref _isTimelineLoading, value);
        }

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

        public bool HasValidationError
        {
            get => _hasValidationError;
            private set
            {
                if (_hasValidationError != value)
                {
                    _hasValidationError = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                    OnPropertyChanged(nameof(IsSaveAllowed));
                }
            }
        }

        // UI flags to control when per-field validation messages are shown
        public bool TitleTouched
        {
            get => _titleTouched;
            set
            {
                if (SetProperty(ref _titleTouched, value))
                {
                    OnPropertyChanged(nameof(ShowTitleError));
                }
            }
        }

        public bool TimeTouched
        {
            get => _timeTouched;
            set
            {
                if (SetProperty(ref _timeTouched, value))
                {
                    OnPropertyChanged(nameof(ShowTimeError));
                }
            }
        }

        public bool SaveAttempted
        {
            get => _saveAttempted;
            private set
            {
                if (SetProperty(ref _saveAttempted, value))
                {
                    OnPropertyChanged(nameof(ShowTitleError));
                    OnPropertyChanged(nameof(ShowTimeError));
                    OnPropertyChanged(nameof(TitleErrorMessage));
                    OnPropertyChanged(nameof(TimeErrorMessage));
                }
            }
        }

        public bool ShowTitleError =>
            ((GetErrors(nameof(Title)) as IEnumerable<string>)?.Any() == true) && (TitleTouched || SaveAttempted);

        public bool ShowTimeError
        {
            get
            {
                var hasTimeError = ((GetErrors(nameof(Start)) as IEnumerable<string>)?.Any() == true)
                                   || ((GetErrors(nameof(End)) as IEnumerable<string>)?.Any() == true)
                                   || ((GetErrors(nameof(SelectedStartDate)) as IEnumerable<string>)?.Any() == true)
                                   || ((GetErrors(nameof(SelectedEndDate)) as IEnumerable<string>)?.Any() == true)
                                   || ((GetErrors("TimeRange") as IEnumerable<string>)?.Any() == true);
                return hasTimeError && (TimeTouched || SaveAttempted);
            }
        }

        public bool IsSaveAllowed => CanSaveDraft();

        public string TitleErrorMessage => ((GetErrors(nameof(Title)) as IEnumerable<string>)?.FirstOrDefault()) ?? string.Empty;

        public string TimeErrorMessage
        {
            get
            {
                var tr = (GetErrors("TimeRange") as IEnumerable<string>)?.FirstOrDefault();
                if (!string.IsNullOrEmpty(tr)) return tr;
                var s = (GetErrors(nameof(Start)) as IEnumerable<string>)?.FirstOrDefault();
                if (!string.IsNullOrEmpty(s)) return s;
                var e = (GetErrors(nameof(End)) as IEnumerable<string>)?.FirstOrDefault();
                if (!string.IsNullOrEmpty(e)) return e;
                var sd = (GetErrors(nameof(SelectedStartDate)) as IEnumerable<string>)?.FirstOrDefault();
                if (!string.IsNullOrEmpty(sd)) return sd;
                var ed = (GetErrors(nameof(SelectedEndDate)) as IEnumerable<string>)?.FirstOrDefault();
                if (!string.IsNullOrEmpty(ed)) return ed;
                return string.Empty;
            }
        }

        // INotifyDataErrorInfo implementation
        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return Enumerable.Empty<string>();

            if (_errors.TryGetValue(propertyName, out var errors))
                return errors;

            return Enumerable.Empty<string>();
        }

        private void AddError(string property, string message)
        {
            if (!_errors.ContainsKey(property))
                _errors[property] = new List<string>();

            if (!_errors[property].Contains(message))
                _errors[property].Add(message);

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
            CommandManager.InvalidateRequerySuggested();
            OnPropertyChanged(nameof(IsSaveAllowed));
            OnPropertyChanged(nameof(ShowTitleError));
            OnPropertyChanged(nameof(ShowTimeError));
            OnPropertyChanged(nameof(TitleErrorMessage));
            OnPropertyChanged(nameof(TimeErrorMessage));
        }

        private void ClearErrors(string property)
        {
            if (_errors.Remove(property))
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(property));
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(nameof(IsSaveAllowed));
                OnPropertyChanged(nameof(ShowTitleError));
                OnPropertyChanged(nameof(ShowTimeError));
                OnPropertyChanged(nameof(TitleErrorMessage));
                OnPropertyChanged(nameof(TimeErrorMessage));
            }
        }

        private void ApplyValidationResults(Dictionary<string, List<string>> results)
        {
            // Clear existing and apply new errors
            var keys = _errors.Keys.Union(results?.Keys ?? Enumerable.Empty<string>()).ToList();
            foreach (var key in keys)
            {
                ClearErrors(key);
            }

            if (results == null)
                return;

            foreach (var kv in results)
            {
                foreach (var msg in kv.Value)
                {
                    AddError(kv.Key, msg);
                }
            }
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
                // Ensure validation is evaluated for the newly attached draft so commands update
                var day = _draftBlock?.Appointment?.Start.Date ?? SelectedDate;
                UpdateDraftState(day);
            }
        }

        public AppointmentModel DraftAppointment => DraftBlock?.Appointment;

        // Editor-facing properties used for binding/validation
        public string Title
        {
            get => DraftBlock?.Appointment?.Title ?? string.Empty;
            set
            {
                if (DraftBlock?.Appointment == null)
                    return;

                if (DraftBlock.Appointment.Title == value)
                    return;

                DraftBlock.Appointment.Title = value;
                OnPropertyChanged(nameof(Title));
                // validate title
                ClearErrors(nameof(Title));
                if (string.IsNullOrWhiteSpace(value))
                    AddError(nameof(Title), "Title is required");

                var day = DraftBlock?.Appointment?.Start.Date ?? SelectedDate;
                UpdateDraftState(day);
            }
        }

        public DateTime Start
        {
            get => DraftBlock?.Appointment?.Start ?? SelectedDate;
            set
            {
                if (DraftBlock?.Appointment == null)
                    return;

                if (DraftBlock.Appointment.Start == value)
                    return;

                DraftBlock.Appointment.Start = value;
                OnPropertyChanged(nameof(Start));

                ClearErrors(nameof(Start));
                ClearErrors(nameof(End));
                if (DraftBlock.Appointment.Start >= DraftBlock.Appointment.End)
                {
                    AddError(nameof(Start), "Start must be before End");
                    AddError(nameof(End), "End must be after Start");
                }

                var day = DraftBlock?.Appointment?.Start.Date ?? SelectedDate;
                UpdateDraftState(day);
            }
        }

        public DateTime End
        {
            get => DraftBlock?.Appointment?.End ?? SelectedDate.AddHours(1);
            set
            {
                if (DraftBlock?.Appointment == null)
                    return;

                if (DraftBlock.Appointment.End == value)
                    return;

                DraftBlock.Appointment.End = value;
                OnPropertyChanged(nameof(End));

                ClearErrors(nameof(Start));
                ClearErrors(nameof(End));
                if (DraftBlock.Appointment.Start >= DraftBlock.Appointment.End)
                {
                    AddError(nameof(Start), "Start must be before End");
                    AddError(nameof(End), "End must be after Start");
                }

                var day = DraftBlock?.Appointment?.Start.Date ?? SelectedDate;
                UpdateDraftState(day);
            }
        }

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
        public ICommand SaveDraftCommand { get; }
        public ICommand CreateDraftCommand { get; }
        public ICommand SaveAppointmentCommand { get; }
        public ICommand AttemptSaveCommand { get; }
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

        public AppointmentsViewModel(AppointmentApiService api,
            DraftValidationService draftValidationService,
            TimelineLayoutService timelineLayoutService,
            CalendarSummaryService calendarSummaryService,
            AppointmentFilterService appointmentFilterService)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _draftValidationService = draftValidationService ?? throw new ArgumentNullException(nameof(draftValidationService));
            _timelineLayoutService = timelineLayoutService ?? throw new ArgumentNullException(nameof(timelineLayoutService));
            _calendarSummaryService = calendarSummaryService ?? throw new ArgumentNullException(nameof(calendarSummaryService));
            _appointmentFilterService = appointmentFilterService ?? throw new ArgumentNullException(nameof(appointmentFilterService));
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
            SaveDraftCommand = new AsyncRelayCommand(SaveDraftAsync, CanSaveDraft);
            AttemptSaveCommand = new AsyncRelayCommand(async () =>
            {
                // Mark that the user attempted a save so validation messages become visible
                SaveAttempted = true;
                ValidateAll();
                if (HasErrors)
                {
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                await SaveDraftAsync();
            });
            SaveAppointmentCommand = SaveDraftCommand;
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
            IsTimelineLoading = true;
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
            finally
            {
                IsTimelineLoading = false;
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

        private bool CanSaveDraft()
        {
            if (DraftBlock == null)
                return false;

            // ViewModel-level errors take precedence
            if (HasErrors)
                return false;

            if (DraftBlock.HasValidationError)
                return false;

            var appt = DraftBlock.Appointment;
            if (appt == null)
                return false;

            if (appt.Start >= appt.End)
                return false;

            return true;
        }

        private async Task SaveDraftAsync()
        {
            // Force validation for all fields before attempting save
            ValidateAll();
            if (HasErrors)
            {
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            if (!CanSaveDraft())
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
            //catch (AppointmentManagementSystem.Domain.Exceptions.DomainException dex)
            //{
            //    // Domain exceptions (e.g. appointment overlap) should be presented to the user
            //    MessageBox.Show(dex.Message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    // Keep the draft open so the user can correct it; ensure commands re-evaluate
            //    CommandManager.InvalidateRequerySuggested();
            //}
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save appointment. {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                CommandManager.InvalidateRequerySuggested();

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

            var mapped = TimelineLayoutService.DragMode.None;
            switch (dragMode)
            {
                case DragMode.Move:
                    mapped = TimelineLayoutService.DragMode.Move;
                    break;
                case DragMode.ResizeStart:
                    mapped = TimelineLayoutService.DragMode.ResizeStart;
                    break;
                case DragMode.ResizeEnd:
                    mapped = TimelineLayoutService.DragMode.ResizeEnd;
                    break;
            }

            var draft = DraftBlock.Appointment;
            var updated = _timelineLayoutService.ApplyDragDelta(mapped, minuteDelta, draft);
            if (updated != null && DraftBlock?.Appointment != null)
            {
                DraftBlock.Appointment.Start = updated.Start;
                DraftBlock.Appointment.End = updated.End;
                var dayStart = DraftBlock.Appointment.Start.Date;
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
            var hours = _timelineLayoutService.GenerateTimelineHours(day);
            foreach (var h in hours)
                TimelineHours.Add(h);
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
            var blocks = _timelineLayoutService.BuildTimelineBlocks(Appointments, DraftBlock, day, DraftSourceAppointmentId, ShowAllDescriptions);
            foreach (var b in blocks)
                TimelineBlocks.Add(b);
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

            var updated = _timelineLayoutService.AssignAppointmentsToTimeline(TimelineHours.ToList(), TimelineBlocks.ToList(), day);
            TimelineHours.Clear();
            foreach (var h in updated)
                TimelineHours.Add(h);
        }

        private void UpdateDraftConflictState()
        {
            if (DraftBlock == null || DraftBlock.Appointment == null)
            {
                return;
            }

            var errors = _draftValidationService.ValidateDraft(DraftBlock.Appointment, DraftBlock, DraftSourceAppointmentId, Appointments);
            var hasConflict = errors.ContainsKey("TimeRange");
            DraftBlock.IsConflicting = hasConflict;
            CommandManager.InvalidateRequerySuggested();
        }

        private void UpdateDraftState(DateTime day)
        {
            UpdateDraftConflictState();
            AssignAppointmentsToTimeline(day);

            if (DraftBlock == null || DraftBlock.Appointment == null)
            {
                HasValidationError = false;
                CommandManager.InvalidateRequerySuggested();
                return;
            }

            var validationResults = _draftValidationService.ValidateDraft(DraftBlock.Appointment, DraftBlock, DraftSourceAppointmentId, Appointments);
            ApplyValidationResults(validationResults);

            DraftBlock.HasValidationError = DraftBlock.IsConflicting;
            HasValidationError = DraftBlock.HasValidationError || HasErrors;
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
                return;
            }

            if (e.PropertyName == nameof(TimelineAppointmentBlockViewModel.HasValidationError))
            {
                HasValidationError = DraftBlock?.HasValidationError ?? false;
                return;
            }

            // Re-evaluate validation when title or description change so commands update
            if (e.PropertyName == nameof(TimelineAppointmentBlockViewModel.Title) ||
                e.PropertyName == nameof(TimelineAppointmentBlockViewModel.Description))
            {
                var day = DraftBlock?.Appointment?.Start.Date ?? SelectedDate;
                UpdateDraftState(day);
                return;
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

        private void ValidateAll()
        {
            if (DraftBlock?.Appointment == null)
                return;

            // Force each validation to run by reassigning the same values
            Title = Title;
            Start = Start;
            End = End;
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
            var isSearching = !string.IsNullOrWhiteSpace((SearchQuery ?? string.Empty).Trim());

            var result = _appointmentFilterService.RefreshDashboardOverview(Appointments, SearchQuery, isSearching, SelectedViewMode, SelectedFilter, now, SelectedDate);

            TodayFilterCount = result.TodayFilterCount;
            UpcomingFilterCount = result.UpcomingFilterCount;
            PastFilterCount = result.PastFilterCount;

            foreach (var appointment in result.FilteredAppointments)
                TodayAppointments.Add(appointment);

            TodayTotalScheduledTimeText = FormatDuration(result.TotalMinutes);
            OnPropertyChanged(nameof(TodayAppointmentsCount));
            OnPropertyChanged(nameof(HasTodayAppointments));
        }

        

        

        

        

        private void RefreshCalendarSummaries()
        {
            var week = _calendarSummaryService.BuildWeekDaySummaries(Appointments, SelectedDate);
            WeekDaySummaries.Clear();
            foreach (var s in week) WeekDaySummaries.Add(s);

            var month = _calendarSummaryService.BuildMonthDaySummaries(Appointments, SelectedDate);
            MonthDaySummaries.Clear();
            foreach (var s in month) MonthDaySummaries.Add(s);

            var year = _calendarSummaryService.BuildYearMonthSummaries(Appointments, SelectedDate);
            YearMonthSummaries.Clear();
            foreach (var s in year) YearMonthSummaries.Add(s);
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
