using AppointmentManagementSystem.Domain.Enums;

namespace AppointmentManagementSystem.Domain.ValueObjects;

public sealed record PatientContact
{
    public PatientContactType Type { get; }
    public string Value { get; }

    public PatientContact(PatientContactType type, string value)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Invalid contact type.");
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Contact value is required.", nameof(value));
        }

        Type = type;
        Value = value.Trim();
    }

    private PatientContact() { }
}

