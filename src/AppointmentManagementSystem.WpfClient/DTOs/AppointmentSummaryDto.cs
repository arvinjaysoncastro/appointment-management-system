using System;

namespace AppointmentManagementSystem.WpfClient.DTOs
{
    public sealed class AppointmentSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
    }
}
