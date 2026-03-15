using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Domain.Interfaces;
using AppointmentManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AppointmentManagementSystem.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<AppointmentRepository> _logger;

    public AppointmentRepository(AppDbContext context, ILogger<AppointmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Appointment>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading all appointments from database.");

        return await _context.Appointments
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading appointment {AppointmentId}.", id);

        return await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsOverlapAsync(DateTimeOffset start, DateTimeOffset end, Guid? excludeId, CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Checking appointment overlap for {Start} to {End} excluding {ExcludedAppointmentId}.",
            start,
            end,
            excludeId);

        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(
                appointment => appointment.Id != excludeId &&
                               start < appointment.TimeRange.End &&
                               end > appointment.TimeRange.Start,
                cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        if (_context.Entry(appointment).State == EntityState.Detached)
        {
            _context.Appointments.Update(appointment);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments.FindAsync(new object[] { id }, cancellationToken);
        if (appointment is null)
        {
            _logger.LogDebug("Appointment {AppointmentId} was not found for deletion.", id);
            return;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
