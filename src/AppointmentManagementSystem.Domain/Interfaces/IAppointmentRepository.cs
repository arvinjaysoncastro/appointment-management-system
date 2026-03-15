using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> HasOverlapAsync(DateTime start, DateTime end, CancellationToken cancellationToken);

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);

    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}