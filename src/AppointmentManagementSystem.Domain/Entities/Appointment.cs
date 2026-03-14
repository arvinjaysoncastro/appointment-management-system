using AppointmentManagementSystem.Domain.Exceptions;

namespace AppointmentManagementSystem.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; }
    public string Title { get; }
    public DateTime Start { get; }
    public DateTime End { get; }

    public Appointment(Guid id, string title, DateTime start, DateTime end)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Appointment id is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Appointment title is required.");
        }

        if (start >= end)
        {
            throw new DomainException("Appointment start must be before end.");
        }

        if (start < DateTime.UtcNow)
        {
            throw new DomainException("Appointment cannot be scheduled in the past.");
        }

        Id = id;
        Title = title.Trim();
        Start = start;
        End = end;
    }

    public void EnsureNoOverlap(IEnumerable<Appointment> existing)
    {
        ArgumentNullException.ThrowIfNull(existing);

        foreach (var appointment in existing)
        {
            if (appointment.Id == Id)
            {
                continue;
            }

            if (Start < appointment.End && End > appointment.Start)
            {
                throw new DomainException("Appointment overlaps with an existing appointment.");
            }
        }
    }
}

