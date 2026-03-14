using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentService, AppointmentService>();

        return services;
    }
}