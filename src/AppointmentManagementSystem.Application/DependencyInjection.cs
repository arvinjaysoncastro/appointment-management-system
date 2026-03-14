using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentService, AppointmentService>();

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}