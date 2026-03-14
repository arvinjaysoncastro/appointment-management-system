using System.ComponentModel.DataAnnotations;

namespace AppointmentManagementSystem.Application.DTOs;

public sealed class UpdateAppointmentRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    public string? Notes { get; init; }

    [Required]
    public DateTimeOffset StartTime { get; init; }

    [Required]
    public DateTimeOffset EndTime { get; init; }
}

