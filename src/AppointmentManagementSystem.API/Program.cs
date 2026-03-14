using AppointmentManagementSystem.Application;
using AppointmentManagementSystem.Application.Interfaces;
using AppointmentManagementSystem.Application.Services;
using AppointmentManagementSystem.API.Configuration;
using AppointmentManagementSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
