using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Application.Interfaces;
using AutoMapper;
using FluentValidation;
using Appointment = AppointmentManagementSystem.Domain.Entities.Appointment;
using IAppointmentRepository = AppointmentManagementSystem.Domain.Interfaces.IAppointmentRepository;

namespace AppointmentManagementSystem.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateAppointmentRequest> _createAppointmentValidator;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IMapper mapper,
        IValidator<CreateAppointmentRequest> createAppointmentValidator)
    {
        _appointmentRepository = appointmentRepository;
        _mapper = mapper;
        _createAppointmentValidator = createAppointmentValidator;
    }

    public async Task<IEnumerable<AppointmentDto>> GetAllAsync()
    {
        var appointments = await _appointmentRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AppointmentDto>>(appointments);
    }

    public async Task<AppointmentDto?> GetByIdAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id);
        return appointment is null ? null : _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequest request)
    {
        await _createAppointmentValidator.ValidateAndThrowAsync(request);

        var appointment = _mapper.Map<Appointment>(request);
        var existing = await _appointmentRepository.GetAllAsync();

        appointment.EnsureNoOverlap(existing);

        await _appointmentRepository.AddAsync(appointment);

        return _mapper.Map<AppointmentDto>(appointment);
    }

    public async Task<AppointmentDto> UpdateAsync(Guid id, CreateAppointmentRequest request)
    {
        await _createAppointmentValidator.ValidateAndThrowAsync(request);

        var existingAppointment = await _appointmentRepository.GetByIdAsync(id);
        if (existingAppointment is null)
        {
            throw new KeyNotFoundException($"Appointment '{id}' was not found.");
        }

        var updatedAppointment = new Appointment(id, request.Title, request.Start, request.End);
        var allAppointments = await _appointmentRepository.GetAllAsync();

        updatedAppointment.EnsureNoOverlap(allAppointments);

        await _appointmentRepository.UpdateAsync(updatedAppointment);

        return _mapper.Map<AppointmentDto>(updatedAppointment);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _appointmentRepository.DeleteAsync(id);
    }
}
