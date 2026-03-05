using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Repositories;
using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _dbContext;

    public PatientRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Patients.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<PatientSummaryDto>> SearchAsync(string search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.FirstName.Contains(search) || p.LastName.Contains(search));
        }

        return await query.Select(p => new PatientSummaryDto
        {
            Id = p.Id,
            FirstName = p.FirstName,
            LastName = p.LastName
        }).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        await _dbContext.Patients.AddAsync(patient, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken)
    {
        _dbContext.Patients.Update(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Patient patient, CancellationToken cancellationToken)
    {
        _dbContext.Patients.Remove(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
