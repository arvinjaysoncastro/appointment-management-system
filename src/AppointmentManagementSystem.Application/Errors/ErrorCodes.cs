namespace AppointmentManagementSystem.Application.Errors;

public static class ErrorCodes
{
    public const string AppointmentOverlap = "APPOINTMENT_OVERLAP";
    public const string PatientNotFound = "PATIENT_NOT_FOUND";
    public const string AppointmentNotFound = "APPOINTMENT_NOT_FOUND";
    public const string PatientAlreadyExists = "PATIENT_ALREADY_EXISTS";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string UnknownError = "UNKNOWN_ERROR";
}

