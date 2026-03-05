using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Domain.ValueObjects;
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
        // Aggregate roots with explicit IEntityTypeConfiguration
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new PatientConfiguration());

        // Entity configurations via Fluent API
        // PatientNote: aggregate entity with Id key
        modelBuilder.Entity<PatientNote>()
            .HasKey(x => x.Id);

        // ClinicHoliday: calendar entity with DateOnly key
        modelBuilder.Entity<ClinicHoliday>()
            .HasKey(x => x.Date);

        // ClinicClosedDay: day-of-week entity with DayOfWeek key
        modelBuilder.Entity<ClinicClosedDay>()
            .HasKey(x => x.DayOfWeek);

        // ClinicWorkingHours: composite key (DayOfWeek, OpenTime)
        // ensures one working hour record per day with unique opening time
        modelBuilder.Entity<ClinicWorkingHours>()
            .HasKey(x => new { x.DayOfWeek, x.OpenTime });

        // PatientContact is configured as owned by Patient in PatientConfiguration
        // using OwnsMany() for the contacts collection

        base.OnModelCreating(modelBuilder);
    }
}
