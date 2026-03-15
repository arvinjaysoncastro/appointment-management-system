namespace AppointmentManagementSystem.Domain.Events;

public sealed class AppointmentCreatedEvent : IDomainEvent
{
    public AppointmentCreatedEvent(Guid appointmentId)
    {
        AppointmentId = appointmentId;
        OccurredOnUtc = DateTime.UtcNow;
    }

    public Guid AppointmentId { get; }

    public DateTime OccurredOnUtc { get; }
}