
using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Infrastructure.Persistence;
using SchoolAdmission.Application.Interfaces;
using SchoolAdmission.Application.Services;
using SchoolAdmission.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Database connection string 'DefaultConnection' was not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository registration
builder.Services.AddScoped<
    IAdmissionRepository,
    AdmissionRepository>();

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

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
