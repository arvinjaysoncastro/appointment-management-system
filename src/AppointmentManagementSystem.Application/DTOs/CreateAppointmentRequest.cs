namespace AppointmentManagementSystem.Application.DTOs;

public sealed class CreateAppointmentRequest
{
    public Guid PatientId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
}

