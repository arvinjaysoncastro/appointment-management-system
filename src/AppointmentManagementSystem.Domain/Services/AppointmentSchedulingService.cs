using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Domain.Services;

public sealed class AppointmentSchedulingService
{
    public bool HasConflict(
        IEnumerable<Appointment> appointments,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludeId = null)
    {
        ArgumentNullException.ThrowIfNull(appointments);

        return appointments.Any(appointment =>
            appointment.Id != excludeId &&
            start < appointment.TimeRange.End &&
            end > appointment.TimeRange.Start);
    }
}