using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Application.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(
        DateTimeOffset date,
        CancellationToken cancellationToken);
    // used for quick metadata queries

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);

    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken);

    Task DeleteAsync(Appointment appointment, CancellationToken cancellationToken);
}

