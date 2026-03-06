using AppointmentManagementSystem.Application.Services;
using AppointmentManagementSystem.API.Configuration;
using AppointmentManagementSystem.Infrastructure;
using AppointmentManagementSystem.Infrastructure.Persistence;
using AppointmentManagementSystem.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Infrastructure (DbContext + Repositories)
builder.Services.AddInfrastructure(builder.Configuration);

// Application services
builder.Services.AddScoped<IAppointmentService, AppointmentService>();

// OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

// Initialize database and seed patients
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    // Seed patients if table is empty
    if (!dbContext.Patients.Any())
    {
        dbContext.Patients.AddRange(
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000001"), "John", "Smith"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Mary", "Johnson"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000003"), "David", "Lee"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000004"), "Sarah", "Miller"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000005"), "Michael", "Brown"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000006"), "Emily", "Davis"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000007"), "Daniel", "Wilson"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000008"), "Olivia", "Martinez"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000009"), "James", "Anderson"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000010"), "Sophia", "Taylor"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000011"), "Benjamin", "Thomas"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000012"), "Charlotte", "Moore"),
            new Patient(Guid.Parse("00000000-0000-0000-0000-000000000013"), "Lucas", "Jackson")
        );

        dbContext.SaveChanges();
    }
}

// Long term, use Database.Migrate();
// requiring: dotnet ef migrations add Initial
// for this exam: the implemation above is more practical: EnsureCreated

// Exception handling middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
