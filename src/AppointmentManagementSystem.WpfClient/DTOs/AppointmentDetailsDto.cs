using System;

namespace AppointmentManagementSystem.WpfClient.DTOs
{
    public sealed class AppointmentDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
    }
}
