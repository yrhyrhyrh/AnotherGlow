using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using MyAppBackend.Repositories;
using MyAppBackend.Services;
using System.Data.Common;

var builder = WebApplication.CreateBuilder(args);

// Register services and repositories
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

// Add services to the container
builder.Services.AddControllers(); // Make sure controllers are registered here
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Swagger generation setup

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Get from appsettings.json

builder.Services.AddDbContext<SocialMediaDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptionsAction: sqlOptions =>
    {
        // Optional: Configure Npgsql options if needed
        // sqlOptions.EnableRetryOnFailure();
    })
    // Optional: Add logging in development
    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .EnableSensitiveDataLogging() // Only in development!
);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
