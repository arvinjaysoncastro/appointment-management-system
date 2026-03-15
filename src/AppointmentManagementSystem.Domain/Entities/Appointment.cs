using AppointmentManagementSystem.Domain.Errors;
using AppointmentManagementSystem.Domain.Events;
using AppointmentManagementSystem.Domain.Exceptions;
using AppointmentManagementSystem.Domain.ValueObjects;

namespace AppointmentManagementSystem.Domain.Entities;

public sealed class Appointment
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public AppointmentTitle Title { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public AppointmentTimeRange TimeRange { get; private set; } = null!;
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    private Appointment()
    {
        Description = string.Empty;
    }

    public Appointment(string title, string description, DateTimeOffset start, DateTimeOffset end)
        : this(Guid.NewGuid(), title, description, start, end)
    {
    }

    public Appointment(Guid id, string title, string description, DateTimeOffset start, DateTimeOffset end)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException(DomainErrors.AppointmentIdRequired);
        }

        Id = id;
        UpdateDetails(title, description);
        SetTimeRange(start, end);
        AddDomainEvent(new AppointmentCreatedEvent(Id));
    }

    public void UpdateDetails(string title, string description)
    {
        Title = new AppointmentTitle(title);
        Description = (description ?? string.Empty).Trim();
    }

    public void Reschedule(DateTimeOffset start, DateTimeOffset end)
    {
        SetTimeRange(start, end);
        AddDomainEvent(new AppointmentRescheduledEvent(Id, TimeRange.Start, TimeRange.End));
    }

    private void SetTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        TimeRange = new AppointmentTimeRange(start, end);
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj is Appointment other && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}

