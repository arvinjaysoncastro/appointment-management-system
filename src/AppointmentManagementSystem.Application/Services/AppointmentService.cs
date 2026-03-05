using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Errors;
using AppointmentManagementSystem.Application.Exceptions;
using AppointmentManagementSystem.Application.Repositories;
using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
    }

    public async Task<AppointmentDetailsDto> CreateAsync(
        CreateAppointmentRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EndTime <= request.StartTime)
            throw new BusinessException(ErrorCodes.ValidationError, "End time must be after start time.");

        var patient = await _patientRepository.GetByIdAsync(request.PatientId, cancellationToken);
        if (patient is null)
        {
            throw new BusinessException(ErrorCodes.PatientNotFound, "Patient not found.");
        }

        var existing = await _appointmentRepository.SearchAsync(request.StartTime, cancellationToken);

        // prevent overlapping appointments for the same time slot
        if (existing.Any(e => request.StartTime < e.EndTime && request.EndTime > e.StartTime))
        {
            throw new BusinessException(
                ErrorCodes.AppointmentOverlap,
                "Appointment overlaps with an existing appointment.");
        }

        var appointment = new Appointment(
            Guid.NewGuid(),
            request.PatientId,
            request.Title,
            request.StartTime,
            request.EndTime,
            request.Notes);

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return new AppointmentDetailsDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = patient.FirstName + " " + patient.LastName,
            Title = appointment.Title,
            Notes = appointment.Notes,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            CreatedAt = appointment.CreatedAt
        };
    }

    public async Task<AppointmentDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            return null;
        }

        var patient = await _patientRepository.GetByIdAsync(appointment.PatientId, cancellationToken);

        return new AppointmentDetailsDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = patient is null ? string.Empty : patient.FirstName + " " + patient.LastName,
            Title = appointment.Title,
            Notes = appointment.Notes,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime,
            CreatedAt = appointment.CreatedAt
        };
    }

    public async Task<IReadOnlyList<AppointmentSummaryDto>> SearchAsync(DateTime date, CancellationToken cancellationToken)
    {
        var targetDate = new DateTimeOffset(date.Date, TimeSpan.Zero);
        return await _appointmentRepository.SearchAsync(targetDate, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (request.EndTime <= request.StartTime)
            throw new BusinessException(ErrorCodes.ValidationError, "End time must be after start time.");

        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            throw new BusinessException(ErrorCodes.AppointmentNotFound, "Appointment not found.");
        }

        var existing = await _appointmentRepository.SearchAsync(request.StartTime, cancellationToken);

        // prevent overlapping appointments excluding the current one
        if (existing.Any(e => e.Id != id && request.StartTime < e.EndTime && request.EndTime > e.StartTime))
        {
            throw new BusinessException(
                ErrorCodes.AppointmentOverlap,
                "Appointment overlaps with an existing appointment.");
        }

        appointment.SetTitle(request.Title);
        appointment.SetNotes(request.Notes);
        appointment.SetTimeRange(request.StartTime, request.EndTime);

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id, cancellationToken);
        if (appointment is null)
        {
            throw new BusinessException(ErrorCodes.AppointmentNotFound, "Appointment not found.");
        }

        await _appointmentRepository.DeleteAsync(appointment, cancellationToken);
    }
}
