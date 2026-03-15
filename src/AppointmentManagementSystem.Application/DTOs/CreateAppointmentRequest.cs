namespace AppointmentManagementSystem.Application.DTOs;

public sealed class CreateAppointmentRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
}

