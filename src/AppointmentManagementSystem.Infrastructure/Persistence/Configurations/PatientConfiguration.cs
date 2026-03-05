using AppointmentManagementSystem.Domain.Entities;
using AppointmentManagementSystem.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppointmentManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName)
            .IsRequired();

        builder.Property(p => p.LastName)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.OwnsMany(p => p.Contacts)
            .ToTable("PatientContacts");

        builder.HasMany(p => p.Notes)
            .WithOne()
            .HasForeignKey(n => n.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // database schema configuration for appointment entity
        // used to enforce correct scheduling rules
    }
}
