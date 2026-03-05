namespace AppointmentManagementSystem.Application.Exceptions;

public sealed class BusinessException : Exception
{
    public string ErrorCode { get; }

    public BusinessException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }

    public BusinessException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
    }
}

