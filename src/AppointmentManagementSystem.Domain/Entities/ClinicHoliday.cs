namespace AppointmentManagementSystem.Domain.Entities;

public sealed class ClinicHoliday
{
    public DateOnly Date { get; }
    public string Name { get; }

    public ClinicHoliday(DateOnly date, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Holiday name is required.", nameof(name));
        }

        Date = date;
        Name = name.Trim();
    }

    private ClinicHoliday() { }
}

