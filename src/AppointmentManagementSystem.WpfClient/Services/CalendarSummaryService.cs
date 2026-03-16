using System;
using System.Collections.Generic;
using System.Linq;
using AppointmentManagementSystem.WpfClient.Models;

namespace AppointmentManagementSystem.WpfClient.Services
{
    public sealed class CalendarSummaryService
    {
        public List<CalendarDaySummary> BuildWeekDaySummaries(IEnumerable<AppointmentModel> appointments, DateTime selectedDate)
        {
            var summaries = new List<CalendarDaySummary>();
            var weekStart = GetStartOfWeek(selectedDate.Date);

            for (var i = 0; i < 7; i++)
            {
                var date = weekStart.AddDays(i);
                summaries.Add(CreateCalendarDaySummary(appointments, date, true));
            }

            return summaries;
        }

        public List<CalendarDaySummary> BuildMonthDaySummaries(IEnumerable<AppointmentModel> appointments, DateTime selectedDate)
        {
            var summaries = new List<CalendarDaySummary>();
            var firstOfMonth = new DateTime(selectedDate.Year, selectedDate.Month, 1);
            var gridStart = GetStartOfWeek(firstOfMonth);

            for (var i = 0; i < 42; i++)
            {
                var date = gridStart.AddDays(i);
                var isInCurrentMonth = date.Month == selectedDate.Month && date.Year == selectedDate.Year;
                summaries.Add(CreateCalendarDaySummary(appointments, date, isInCurrentMonth));
            }

            return summaries;
        }

        public List<CalendarMonthSummary> BuildYearMonthSummaries(IEnumerable<AppointmentModel> appointments, DateTime selectedDate)
        {
            var summaries = new List<CalendarMonthSummary>();

            for (var month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(selectedDate.Year, month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var monthAppointments = appointments
                    .Where(appointment => appointment.Start >= monthStart && appointment.Start < monthEnd)
                    .ToList();
                var totalMinutes = monthAppointments.Sum(appointment => Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes));

                summaries.Add(new CalendarMonthSummary
                {
                    MonthStart = monthStart,
                    MonthName = monthStart.ToString("MMMM"),
                    AppointmentCount = monthAppointments.Count,
                    TotalScheduledTimeText = FormatDuration(totalMinutes)
                });
            }

            return summaries;
        }

        private CalendarDaySummary CreateCalendarDaySummary(IEnumerable<AppointmentModel> appointments, DateTime date, bool isInCurrentMonth)
        {
            var dayAppointments = appointments.Where(appointment => appointment.Start.Date == date.Date).ToList();
            var totalMinutes = dayAppointments.Sum(appointment => Math.Max(0d, (appointment.End - appointment.Start).TotalMinutes));

            return new CalendarDaySummary
            {
                Date = date.Date,
                AppointmentCount = dayAppointments.Count,
                TotalScheduledTimeText = FormatDuration(totalMinutes),
                IsInCurrentMonth = isInCurrentMonth
            };
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
    }
}
