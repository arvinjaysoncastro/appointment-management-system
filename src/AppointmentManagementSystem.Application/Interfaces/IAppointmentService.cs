using AppointmentManagementSystem.Application.DTOs;

namespace AppointmentManagementSystem.Application.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentDto>> GetAllAsync();

    Task<AppointmentDto?> GetByIdAsync(Guid id);

    Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request);

    Task<AppointmentDto> UpdateAsync(Guid id, CreateAppointmentRequest request);

    Task DeleteAsync(Guid id);
}