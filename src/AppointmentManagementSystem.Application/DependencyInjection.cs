using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Application.Mappers;
using AppointmentManagementSystem.Application.Services;
using AppointmentManagementSystem.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AppointmentMapper>();
        services.AddScoped<AppointmentSchedulingService>();
        services.AddScoped<IAppointmentService, AppointmentService>();

        return services;
    }
}