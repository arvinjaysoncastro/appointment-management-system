namespace AppointmentManagementSystem.Application.DTOs;

public sealed class AppointmentDetailsDto
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public string PatientName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

