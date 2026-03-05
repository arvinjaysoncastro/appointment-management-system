namespace AppointmentManagementSystem.Domain.Entities;

public sealed class ClinicWorkingHours
{
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly OpenTime { get; }
    public TimeOnly CloseTime { get; }

    public ClinicWorkingHours(DayOfWeek dayOfWeek, TimeOnly openTime, TimeOnly closeTime)
    {
        if (closeTime <= openTime)
        {
            throw new ArgumentException("Close time must be after open time.");
        }

        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    private ClinicWorkingHours() { }

    public bool Contains(DateTimeOffset dateTime)
    {
        if (dateTime.DayOfWeek != DayOfWeek)
        {
            return false;
        }

        var t = TimeOnly.FromDateTime(dateTime.LocalDateTime);
        return t >= OpenTime && t < CloseTime;
    }
}

