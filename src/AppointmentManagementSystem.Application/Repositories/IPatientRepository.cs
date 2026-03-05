using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Application.Repositories;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(
        string search,
        CancellationToken cancellationToken);

    Task AddAsync(Patient patient, CancellationToken cancellationToken);

    Task UpdateAsync(Patient patient, CancellationToken cancellationToken);

    Task DeleteAsync(Patient patient, CancellationToken cancellationToken);
}

