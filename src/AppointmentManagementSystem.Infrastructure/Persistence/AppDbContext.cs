using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments { get; set; } = null!;
    public DbSet<Patient> Patients { get; set; } = null!;
    public DbSet<PatientNote> PatientNotes { get; set; } = null!;
    public DbSet<ClinicHoliday> ClinicHolidays { get; set; } = null!;
    public DbSet<ClinicClosedDay> ClinicClosedDays { get; set; } = null!;
    public DbSet<ClinicWorkingHours> ClinicWorkingHours { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());

        // other entities rely on EF Core conventions

        base.OnModelCreating(modelBuilder);
    }
}
