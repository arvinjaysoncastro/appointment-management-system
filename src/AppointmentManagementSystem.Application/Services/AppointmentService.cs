using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Domain.Interfaces;

namespace AppointmentManagementSystem.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return appointments.Select(MapToDto);
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment is null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request)
    {
        ValidateRequest(request);

        var appointment = new Appointment(request.Title, request.Description, request.Start, request.End);
        var existing = await _appointmentRepository.GetAllAsync();

        appointment.EnsureNoOverlap(existing);

        await _appointmentRepository.AddAsync(appointment);

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto> UpdateAsync(Guid id, CreateAppointmentRequest request)
    {
        ValidateRequest(request);

        var existingAppointment = await _appointmentRepository.GetByIdAsync(id);
        if (existingAppointment is null)
        {
            throw new KeyNotFoundException($"Appointment '{id}' was not found.");
        }

        var updatedAppointment = new Appointment(id, request.Title, request.Description, request.Start, request.End);
        var allAppointments = await _appointmentRepository.GetAllAsync();

        updatedAppointment.EnsureNoOverlap(allAppointments);

        await _appointmentRepository.UpdateAsync(updatedAppointment);

        return MapToDto(updatedAppointment);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _appointmentRepository.DeleteAsync(id);
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

    private static void ValidateRequest(CreateAppointmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(request.Title));
        }

        if (request.End <= request.Start)
        {
            throw new ArgumentException("End must be after Start.");
        }
    }
}
