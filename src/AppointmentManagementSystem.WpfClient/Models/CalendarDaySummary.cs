using System;

namespace AppointmentManagementSystem.WpfClient.Models
{
    public sealed class CalendarDaySummary
    {
        public DateTime Date { get; set; }
        public int AppointmentCount { get; set; }
        public string TotalScheduledTimeText { get; set; }
        public bool IsInCurrentMonth { get; set; }
    }
}
