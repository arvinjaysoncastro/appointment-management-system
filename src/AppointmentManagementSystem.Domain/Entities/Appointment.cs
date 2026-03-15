using AppointmentManagementSystem.Domain.Exceptions;

namespace AppointmentManagementSystem.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }

    private Appointment()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Appointment(string title, string description, DateTime start, DateTime end)
        : this(Guid.NewGuid(), title, description, start, end)
    {
    }

    public Appointment(Guid id, string title, string description, DateTime start, DateTime end)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Appointment id is required.");
        }

        Id = id;
        SetDetails(title, description, start, end);
    }

    public void SetDetails(string title, string description, DateTime start, DateTime end)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Appointment title is required.");
        }

        if (end <= start)
        {
            throw new DomainException("Appointment end time must be after start time.");
        }

        Title = title.Trim();
        Description = (description ?? string.Empty).Trim();
        Start = start;
        End = end;
    }
}

