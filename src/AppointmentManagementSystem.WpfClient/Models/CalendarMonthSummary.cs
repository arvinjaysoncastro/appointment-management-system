using System;

namespace AppointmentManagementSystem.WpfClient.Models
{
    public sealed class CalendarMonthSummary
    {
        public DateTime MonthStart { get; set; }
        public string MonthName { get; set; }
        public int AppointmentCount { get; set; }
        public string TotalScheduledTimeText { get; set; }
    }
}
