using AppointmentManagementSystem.Application.Services;
using AppointmentManagementSystem.API.Configuration;
using AppointmentManagementSystem.Infrastructure;
using AppointmentManagementSystem.Infrastructure.Persistence;

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

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
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
