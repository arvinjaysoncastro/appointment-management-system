namespace AppointmentManagementSystem.Domain.Events;

public sealed class AppointmentRescheduledEvent : IDomainEvent
{
    public AppointmentRescheduledEvent(Guid appointmentId, DateTimeOffset start, DateTimeOffset end)
    {
        AppointmentId = appointmentId;
        Start = start;
        End = end;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid AppointmentId { get; }

    public DateTimeOffset Start { get; }

    public DateTimeOffset End { get; }

    public DateTime OccurredOnUtc { get; }
}