using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Application.Services;
using AppointmentManagementSystem.Infrastructure;

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

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();