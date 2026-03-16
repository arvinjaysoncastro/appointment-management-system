using System;

namespace AppointmentManagementSystem.WpfClient.DTOs
{
    public sealed class CreateAppointmentDto
    {
        public Guid PatientId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
