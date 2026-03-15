using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Application.Mappers;
using AppointmentManagementSystem.Domain.Errors;
using AppointmentManagementSystem.Domain.Exceptions;
using AppointmentManagementSystem.Domain.Interfaces;
using AppointmentManagementSystem.Domain.Services;
using Microsoft.Extensions.Logging;

namespace AppointmentManagementSystem.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly AppointmentMapper _appointmentMapper;
    private readonly AppointmentSchedulingService _schedulingService;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        AppointmentMapper appointmentMapper,
        AppointmentSchedulingService schedulingService,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _appointmentMapper = appointmentMapper;
        _schedulingService = schedulingService;
        _logger = logger;
    }

    public async Task<List<AppointmentDto>> GetAppointmentsAsync(CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return appointments.Select(_appointmentMapper.ToDto).ToList();
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        return appointment is null ? null : _appointmentMapper.ToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var appointment = _appointmentMapper.FromRequest(request);

        if (await HasOverlappingAppointmentAsync(request.Start, request.End, null, cancellationToken))
        {
            _logger.LogWarning(
                "Overlap validation failed while creating appointment {Title} from {Start} to {End}.",
                request.Title,
                request.Start,
                request.End);
            throw new DomainException(DomainErrors.AppointmentOverlap);
        }

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        _logger.LogInformation(
            "Appointment {AppointmentId} created from {Start} to {End}.",
            appointment.Id,
            appointment.TimeRange.Start,
            appointment.TimeRange.End);

        return _appointmentMapper.ToDto(appointment);
    }

    public async Task<AppointmentDto> UpdateAsync(Guid id, CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var existingAppointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (existingAppointment is null)
        {
            _logger.LogWarning("Appointment {AppointmentId} was not found for update.", id);
            throw new DomainException(DomainErrors.AppointmentNotFound);
        }

        existingAppointment.UpdateDetails(request.Title, request.Description);
        existingAppointment.Reschedule(request.Start, request.End);

        var hasOverlap = await HasOverlappingAppointmentAsync(
            existingAppointment.TimeRange.Start,
            existingAppointment.TimeRange.End,
            id,
            cancellationToken);

        if (hasOverlap)
        {
            _logger.LogWarning(
                "Overlap validation failed while updating appointment {AppointmentId} to {Start} - {End}.",
                id,
                request.Start,
                request.End);
            throw new DomainException(DomainErrors.AppointmentOverlap);
        }

        await _appointmentRepository.UpdateAsync(existingAppointment, cancellationToken);

        _logger.LogInformation(
            "Appointment {AppointmentId} updated to {Start} - {End}.",
            existingAppointment.Id,
            existingAppointment.TimeRange.Start,
            existingAppointment.TimeRange.End);

        return _appointmentMapper.ToDto(existingAppointment);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var existingAppointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (existingAppointment is null)
        {
            _logger.LogWarning("Appointment {AppointmentId} was not found for deletion.", id);
            throw new DomainException(DomainErrors.AppointmentNotFound);
        }

        await _appointmentRepository.DeleteAsync(id, cancellationToken);

        _logger.LogInformation("Appointment {AppointmentId} deleted.", id);
    }

    public Task<bool> HasOverlappingAppointment(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
        => HasOverlappingAppointmentAsync(start, end, null, cancellationToken);

    private async Task<bool> HasOverlappingAppointmentAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
        return _schedulingService.HasConflict(appointments, start, end, excludeId);
    }

}
