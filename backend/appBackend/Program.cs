using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using MyAppBackend.Repositories;
using MyAppBackend.Services;
using System.Data.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register services and repositories
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

var key = "my_secret_key_12345"; // 🔥 Change this to a secure key!

// Add services to the container
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
    });
builder.Services.AddAuthorization();
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
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
