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

    public async Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(
        DateTimeOffset date,
        CancellationToken cancellationToken)
    {
        var start = new DateTimeOffset(date.DateTime.Date, date.Offset);
        var end = start.AddDays(1);

        var appointments = await _dbContext.Appointments
            .AsNoTracking()
            //.Include(a => a.Patient)
            .ToListAsync(cancellationToken);

        return appointments
            .Where(a => a.StartTime >= start && a.StartTime < end)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id,
                PatientId = a.PatientId,
                //PatientName = a.Patient != null
                //    ? a.Patient.FirstName + " " + a.Patient.LastName
                //    : string.Empty,
                Title = a.Title,
                StartTime = a.StartTime,
                EndTime = a.EndTime
            })
            .OrderBy(a => a.StartTime)
            .ToList();
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
