using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Domain.Exceptions;
using AppointmentManagementSystem.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppointmentManagementSystem.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _logger = logger;
    }

    public async Task<List<AppointmentDto>> GetAppointmentsAsync(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        return appointment is null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Creating appointment {Title} from {Start} to {End}.",
            request.Title,
            request.Start,
            request.End);

        var appointment = new Appointment(request.Title, request.Description, request.Start, request.End);

        if (await HasOverlappingAppointment(request.Start, request.End, cancellationToken))
        {
            _logger.LogWarning(
                "Overlap validation failed while creating appointment {Title} from {Start} to {End}.",
                request.Title,
                request.Start,
                request.End);
            throw new DomainException("Appointment overlaps with an existing appointment.");
        }

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto> UpdateAsync(Guid id, CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "Updating appointment {AppointmentId} to {Title} from {Start} to {End}.",
            id,
            request.Title,
            request.Start,
            request.End);

        var existingAppointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (existingAppointment is null)
        {
            _logger.LogWarning("Appointment {AppointmentId} was not found for update.", id);
            throw new KeyNotFoundException($"Appointment '{id}' was not found.");
        }

        var updatedAppointment = new Appointment(id, request.Title, request.Description, request.Start, request.End);
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);

        var hasOverlap = appointments.Any(appointment =>
            appointment.Id != id &&
            appointment.Start < request.End &&
            appointment.End > request.Start);

        if (hasOverlap)
        {
            _logger.LogWarning(
                "Overlap validation failed while updating appointment {AppointmentId} to {Start} - {End}.",
                id,
                request.Start,
                request.End);
            throw new DomainException("Appointment overlaps with an existing appointment.");
        }

        await _appointmentRepository.UpdateAsync(updatedAppointment, cancellationToken);

        return MapToDto(updatedAppointment);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting appointment {AppointmentId}.", id);

        var existingAppointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (existingAppointment is null)
        {
            _logger.LogWarning("Appointment {AppointmentId} was not found for deletion.", id);
            throw new KeyNotFoundException($"Appointment '{id}' was not found.");
        }

        await _appointmentRepository.DeleteAsync(id, cancellationToken);
    }

    public Task<bool> HasOverlappingAppointment(DateTime start, DateTime end, CancellationToken cancellationToken)
    {
        return _appointmentRepository.HasOverlapAsync(start, end, cancellationToken);
    }

    private static AppointmentDto MapToDto(Appointment appointment)
    {
        return new AppointmentDto
        {
            Id = appointment.Id,
            Title = appointment.Title,
            Description = appointment.Description,
            Start = appointment.Start,
            End = appointment.End
        };
    }
}
