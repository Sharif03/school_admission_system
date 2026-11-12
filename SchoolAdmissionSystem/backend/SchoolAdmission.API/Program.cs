
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.API.Middlewares;
using SchoolAdmission.Application.Interfaces;
using SchoolAdmission.Application.Services;
using SchoolAdmission.Infrastructure.Persistence;
using SchoolAdmission.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// CORS for the React development server
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// PostgreSQL
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository registration(Dependency injection)
builder.Services.AddScoped<IAdmissionRepository, AdmissionRepository>();

// Hold duration: default 600 seconds (10 minutes).
var holdDurationSeconds = int.TryParse(
    builder.Configuration["HOLD_DURATION_SECONDS"],
    out var configuredDuration)
        ? configuredDuration
        : 600;

if (holdDurationSeconds <= 0)
{
    throw new InvalidOperationException(
        "HOLD_DURATION_SECONDS must be greater than zero.");
}

// Application service registration
builder.Services.AddScoped<IAdmissionService>(serviceProvider =>
    new AdmissionService(
        serviceProvider.GetRequiredService<IAdmissionRepository>(),
        TimeSpan.FromSeconds(holdDurationSeconds)));

var app = builder.Build();

// Apply pending database migrations at startup
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");

app.MapControllers();

app.Run();
