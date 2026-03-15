using AppointmentManagementSystem.Domain.Errors;
using AppointmentManagementSystem.Domain.Exceptions;

namespace AppointmentManagementSystem.Domain.ValueObjects;

public sealed class AppointmentTimeRange
{
    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }

    private AppointmentTimeRange()
    {
    }

    public AppointmentTimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            throw new DomainException(DomainErrors.InvalidAppointmentTime);
        }

        Start = start;
        End = end;
    }
}