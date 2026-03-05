using AppointmentManagementSystem.Application.Repositories;
using AppointmentManagementSystem.Infrastructure.Persistence;
using AppointmentManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AppointmentManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();

        return services;
    }
}
