namespace AppointmentManagementSystem.Application.DTOs;

public sealed class AppointmentDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime Start { get; init; }
    public DateTime End { get; init; }
}