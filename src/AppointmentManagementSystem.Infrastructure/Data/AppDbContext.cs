using AppointmentManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(a => a.Start)
                .IsRequired();

            entity.Property(a => a.End)
                .IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}