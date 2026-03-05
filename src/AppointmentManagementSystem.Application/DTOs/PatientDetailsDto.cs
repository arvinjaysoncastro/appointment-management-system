namespace AppointmentManagementSystem.Application.DTOs;

public sealed class PatientDetailsDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public IReadOnlyCollection<string> Contacts { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Notes { get; init; } = Array.Empty<string>();
}

