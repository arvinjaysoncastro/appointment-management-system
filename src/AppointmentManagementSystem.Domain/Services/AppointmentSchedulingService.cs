using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Domain.Exceptions;

namespace AppointmentManagementSystem.Domain.Services;

public sealed class AppointmentSchedulingService
{
    public void EnsureNoOverlap(
        Appointment newAppointment,
        IEnumerable<Appointment> existingAppointments)
    {
        ArgumentNullException.ThrowIfNull(newAppointment);
        ArgumentNullException.ThrowIfNull(existingAppointments);

        foreach (var appointment in existingAppointments)
        {
            if (appointment.Id == newAppointment.Id)
            {
                continue;
            }

            if (newAppointment.Start < appointment.End &&
                newAppointment.End > appointment.Start)
            {
                throw new DomainException("Appointment overlaps with an existing appointment.");
            }
        }
    }
}
