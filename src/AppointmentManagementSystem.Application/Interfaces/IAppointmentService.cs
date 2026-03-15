using AppointmentManagementSystem.Application.DTOs;

namespace AppointmentManagementSystem.Application.Interfaces;

public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAppointmentsAsync(CancellationToken cancellationToken);

    Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);

    Task<AppointmentDto> UpdateAsync(Guid id, CreateAppointmentRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> HasOverlappingAppointment(DateTime start, DateTime end, CancellationToken cancellationToken);
}