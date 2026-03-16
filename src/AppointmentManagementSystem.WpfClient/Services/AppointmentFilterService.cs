using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AppointmentManagementSystem.WpfClient.Models;
using AppointmentManagementSystem.WpfClient.Enums;

namespace AppointmentManagementSystem.WpfClient.Services
{
    public sealed class AppointmentFilterService
    {
        public class FilterResult
        {
            public List<AppointmentModel> FilteredAppointments { get; set; } = new List<AppointmentModel>();
            public int TodayFilterCount { get; set; }
            public int UpcomingFilterCount { get; set; }
            public int PastFilterCount { get; set; }
            public double TotalMinutes { get; set; }
        }

        public FilterResult RefreshDashboardOverview(IEnumerable<AppointmentModel> appointments, string searchQuery, bool isSearching, CalendarViewMode selectedViewMode, ScheduleFilter selectedFilter, DateTime now, DateTime selectedDate)
        {
            var result = new FilterResult();
            var normalizedSearch = (searchQuery ?? string.Empty).Trim();
            var effectiveSearch = !string.IsNullOrWhiteSpace(normalizedSearch);

            IEnumerable<AppointmentModel> scope = effectiveSearch
                ? appointments
                : GetAppointmentsForSelectedPeriod(appointments, selectedViewMode, selectedDate);

            if (effectiveSearch)
            {
                scope = scope
                    .Where(appointment => MatchesSearchQuery(appointment, normalizedSearch, now))
                    .ToList();

                result.TodayFilterCount = FilterBySchedule(scope, ScheduleFilter.Today, now).Count();
                result.UpcomingFilterCount = FilterBySchedule(scope, ScheduleFilter.Upcoming, now).Count();
                result.PastFilterCount = FilterBySchedule(scope, ScheduleFilter.Past, now).Count();

                scope = FilterBySchedule(scope, selectedFilter, now);
            }
            else
            {
                result.TodayFilterCount = 0;
                result.UpcomingFilterCount = 0;
                result.PastFilterCount = 0;
            }

            var filteredAppointments = scope
                .OrderBy(appointment => appointment.Start)
                .ToList();

            result.FilteredAppointments = filteredAppointments;
            result.TotalMinutes = filteredAppointments.Sum(appointment => Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes));

            return result;
        }

        private static IEnumerable<AppointmentModel> GetAppointmentsForSelectedPeriod(IEnumerable<AppointmentModel> appointments, CalendarViewMode selectedViewMode, DateTime selectedDate)
        {
            switch (selectedViewMode)
            {
                case CalendarViewMode.Week:
                    var weekStart = GetStartOfWeek(selectedDate.Date);
                    var weekEnd = weekStart.AddDays(7);
                    return appointments.Where(appointment => appointment.Start >= weekStart && appointment.Start < weekEnd);

                case CalendarViewMode.Month:
                    var monthStart = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                    var monthEnd = monthStart.AddMonths(1);
                    return appointments.Where(appointment => appointment.Start >= monthStart && appointment.Start < monthEnd);

                case CalendarViewMode.Year:
                    var yearStart = new DateTime(selectedDate.Year, 1, 1);
                    var yearEnd = yearStart.AddYears(1);
                    return appointments.Where(appointment => appointment.Start >= yearStart && appointment.Start < yearEnd);

                default:
                    return appointments.Where(appointment => appointment.Start.Date == selectedDate.Date);
            }
        }

        public static bool MatchesSearchQuery(AppointmentModel appointment, string query, DateTime now)
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

        private static DateTime GetStartOfWeek(DateTime date)
        {
            var normalized = date.Date;
            var dayOfWeek = (int)normalized.DayOfWeek;
            var mondayOffset = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
            return normalized.AddDays(-mondayOffset);
        }
    }
}
