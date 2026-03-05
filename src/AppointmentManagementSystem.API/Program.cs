using AppointmentManagementSystem.Application.Services;
using AppointmentManagementSystem.API.Configuration;
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
