using AppointmentManagementSystem.Application.DTOs;
using AutoMapper;
using Appointment = AppointmentManagementSystem.Domain.Entities.Appointment;

namespace AppointmentManagementSystem.Application.Mappings;

public sealed class AppointmentMappingProfile : Profile
{
    public AppointmentMappingProfile()
    {
        CreateMap<CreateAppointmentRequest, Appointment>()
            .ConstructUsing(source => new Appointment(source.Id, source.Title, source.Start, source.End));

        CreateMap<Appointment, AppointmentDto>();
    }
}