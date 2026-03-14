using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<IEnumerable<Appointment>> GetAllAsync();

    Task<Appointment?> GetByIdAsync(Guid id);

    Task AddAsync(Appointment appointment);

    Task UpdateAsync(Appointment appointment);

    Task DeleteAsync(Guid id);
}