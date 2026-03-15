using AppointmentManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Appointment> Appointments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");

            entity.HasKey(a => a.Id);
            entity.Ignore(a => a.DomainEvents);

            entity.OwnsOne(a => a.Title, title =>
            {
                title.Property(value => value.Value)
                    .HasColumnName("Title")
                    .IsRequired()
                    .HasMaxLength(200);
            });

            entity.Property(a => a.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.OwnsOne(a => a.TimeRange, timeRange =>
            {
                timeRange.Property(value => value.Start)
                    .HasColumnName("Start")
                    .IsRequired();

                timeRange.Property(value => value.End)
                    .HasColumnName("End")
                    .IsRequired();
            });

            entity.Navigation(a => a.Title).IsRequired();
            entity.Navigation(a => a.TimeRange).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}