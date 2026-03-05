using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Repositories;
using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _dbContext;

    public AppointmentRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Appointments.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(DateTimeOffset date, CancellationToken cancellationToken)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var query = from a in _dbContext.Appointments.AsNoTracking()
                    join p in _dbContext.Patients.AsNoTracking() on a.PatientId equals p.Id
                    where a.StartTime >= start && a.StartTime < end
                    orderby a.StartTime
                    select new AppointmentSummaryDto
                    {
                        Id = a.Id,
                        PatientId = a.PatientId,
                        PatientName = p.FirstName + " " + p.LastName,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        Title = a.Title
                    };

        return await query.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _dbContext.Appointments.Update(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _dbContext.Appointments.Remove(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
