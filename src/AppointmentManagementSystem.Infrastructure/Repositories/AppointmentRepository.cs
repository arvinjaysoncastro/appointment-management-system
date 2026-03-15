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
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<bool> HasOverlapAsync(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Checking appointment overlap for {Start} to {End}.", start, end);

        return await _context.Appointments
            .AsNoTracking()
            .AnyAsync(appointment => appointment.Start < end && appointment.End > start, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating appointment {AppointmentId}.", appointment.Id);

        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating appointment {AppointmentId}.", appointment.Id);

        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting appointment {AppointmentId}.", id);

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
