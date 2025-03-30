using appBackend.Repositories;
using Microsoft.EntityFrameworkCore;
using MyAppBackend.Repositories;
using MyAppBackend.Services;
using System.Data.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using appBackend.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register services and repositories
builder.Services.AddScoped<IWeatherForecastService, WeatherForecastService>();
builder.Services.AddScoped<IWeatherForecastRepository, WeatherForecastRepository>();

builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IPollRepository, PollRepository>();  // ✅ Ensure this is registered

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = jwtSettings["Secret"] ?? throw new ArgumentNullException("JWT Secret is missing!");

// Register JWT Authentication
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

// Enable CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy => policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200") // Allow both HTTP and HTTPS
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<UserService>();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SocialMediaDbContext>(options =>
    options.UseNpgsql(connectionString)
    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
    .EnableSensitiveDataLogging()
);

var app = builder.Build();

// Enable Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS before Authentication
app.UseCors("AllowLocalhost");

// ⚠️ Remove if frontend is HTTP only
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
