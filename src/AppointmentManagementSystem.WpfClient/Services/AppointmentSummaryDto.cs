using System;

namespace AppointmentManagementSystem.WpfClient.Services
{
    /// <summary>
    /// DTO matching API AppointmentSummaryDto.
    /// </summary>
    public class AppointmentSummaryDto
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
