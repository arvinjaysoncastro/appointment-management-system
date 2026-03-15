namespace AppointmentManagementSystem.Domain.Errors;

public static class DomainErrors
{
    public const string AppointmentOverlap = nameof(AppointmentOverlap);
    public const string AppointmentNotFound = nameof(AppointmentNotFound);
    public const string InvalidAppointmentTime = nameof(InvalidAppointmentTime);
    public const string AppointmentTitleRequired = nameof(AppointmentTitleRequired);
    public const string AppointmentIdRequired = nameof(AppointmentIdRequired);
}