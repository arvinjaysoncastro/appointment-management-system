using AppointmentManagementSystem.Domain.Errors;
using AppointmentManagementSystem.Domain.Exceptions;

namespace AppointmentManagementSystem.Domain.ValueObjects;

public sealed class AppointmentTitle
{
    public string Value { get; private set; } = string.Empty;

    private AppointmentTitle()
    {
    }

    public AppointmentTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(DomainErrors.AppointmentTitleRequired);
        }

        Value = value.Trim();
    }

    public override string ToString() => Value;
}