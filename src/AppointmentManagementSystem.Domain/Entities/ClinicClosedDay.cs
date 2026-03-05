namespace AppointmentManagementSystem.Domain.Entities;

public sealed class ClinicClosedDay
{
    public DayOfWeek DayOfWeek { get; }

    public ClinicClosedDay(DayOfWeek dayOfWeek)
    {
        DayOfWeek = dayOfWeek;
    }

    private ClinicClosedDay() { }
}

