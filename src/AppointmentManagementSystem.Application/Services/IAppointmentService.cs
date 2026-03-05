using AppointmentManagementSystem.Application.DTOs;

namespace AppointmentManagementSystem.Application.Services;

public interface IAppointmentService
{
    Task<AppointmentDetailsDto> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken);

    Task<AppointmentDetailsDto?> GetAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(
        DateTime date,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Guid id,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}

