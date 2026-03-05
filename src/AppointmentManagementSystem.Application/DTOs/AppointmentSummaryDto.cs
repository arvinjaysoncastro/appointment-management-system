namespace AppointmentManagementSystem.Application.DTOs;

public sealed class AppointmentSummaryDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public string Title { get; init; } = string.Empty;
}

