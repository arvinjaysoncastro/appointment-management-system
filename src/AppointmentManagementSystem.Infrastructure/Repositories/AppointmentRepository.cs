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

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        _logger.LogDebug("Loading all appointments from database.");

        return await _context.Appointments
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        _logger.LogDebug("Loading appointment {AppointmentId}.", id);

        return await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Appointment appointment)
    {
        _logger.LogInformation("Creating appointment {AppointmentId}.", appointment.Id);

        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _logger.LogInformation("Updating appointment {AppointmentId}.", appointment.Id);

        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting appointment {AppointmentId}.", id);

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment is null)
        {
            _logger.LogDebug("Appointment {AppointmentId} was not found for deletion.", id);
            return;
        }

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
    }
}
