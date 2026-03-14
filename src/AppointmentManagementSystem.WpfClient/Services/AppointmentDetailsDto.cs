using System;

namespace AppointmentManagementSystem.WpfClient.Services
{
    /// <summary>
    /// DTO for appointment details response.
    /// </summary>
    public class AppointmentDetailsDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
