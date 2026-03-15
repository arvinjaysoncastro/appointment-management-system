using AppointmentManagementSystem.Application.DTOs;
using AppointmentManagementSystem.Domain.Entities;

namespace AppointmentManagementSystem.Application.Mappers;

public sealed class AppointmentMapper
{
    public AppointmentDto ToDto(Appointment entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new AppointmentDto
        {
            Id = entity.Id,
            Title = entity.Title.Value,
            Description = entity.Description,
            Start = entity.TimeRange.Start,
            End = entity.TimeRange.End
        };
    }

    public Appointment FromRequest(CreateAppointmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new Appointment(request.Title, request.Description, request.Start, request.End);
    }
}